using AbstractEpiserverIoc.Abstractions;
using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DryIocEpi
{
    public class DryIocServiceLocator : IServiceLocator, IServiceLocatorCreateScope, IServiceLocatorAmbientPreferredStorage
    {
        private const string _storageKey = nameof(IServiceLocatorAmbientPreferredStorage);
        // For Ambient Async State
        private static readonly System.Threading.AsyncLocal<Stack<IResolverContext>> _stack =
            new System.Threading.AsyncLocal<Stack<IResolverContext>>();
        private static Func<IDictionary> _storageGetter;
        private readonly IResolverContext _resolveContext;

        public DryIocServiceLocator(IResolverContext context)
        {
            _resolveContext = context;
        }

        public IResolverContext AmbientContext()
        {
            var stack = GetStack();
            var scoped = stack?.Count > 0 ? stack.Peek() : null;

            return scoped ?? _resolveContext;
        }

        public IServiceLocatorScoped CreateScope()
        {
            var scope = AmbientContext().OpenScope();
            AddScope(scope);

            return new DryIocServiceLocatorScoped(scope, null);
        }

        public ICollection<string> Debug() => (AmbientContext() as Container)?
            .GetServiceRegistrations()
            .Select(x => x.ServiceType.FullName + " " + x.ImplementationType?.FullName ?? "n/a" + " " + x.Factory.Reuse.GetType().FullName)
            .ToList();

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            AmbientContext().ResolveMany(serviceType).ToList();

        public object GetInstance(Type serviceType) => AmbientContext().Resolve(serviceType, ifUnresolved: IfUnresolved.Throw);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) =>
            AmbientContext().Resolve(serviceType, IfUnresolved.Throw);

        public void SetStorage(Func<IDictionary> storageGetter) => _storageGetter = storageGetter;
        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = AmbientContext().Resolve(serviceType, IfUnresolved.ReturnDefaultIfNotRegistered);

            return instance is object;
        }

        internal static void AddScope(IResolverContext scopedLocator)
        {
            var stack = GetStack();
            if (stack is null)
            {
                stack = new Stack<IResolverContext>();
            }
            // clears on scope disposing
            if (scopedLocator is null && stack.Count > 0) { stack.Pop(); }
            // push newest scope to top
            else if (scopedLocator is object) { stack.Push(scopedLocator); }
            SetStack(stack);
        }

        internal static void ClearAmbientScope() => AddScope(null);

        private static Stack<IResolverContext> GetStack()
        {
            var preferredStorage = _storageGetter?.Invoke();
            if (preferredStorage is null) { return _stack.Value; }

            return preferredStorage[_storageKey] as Stack<IResolverContext>;
        }

        private static void SetStack(Stack<IResolverContext> stack)
        {
            var preferredStorage = _storageGetter?.Invoke();

            if (preferredStorage is object)
            {
                preferredStorage[_storageKey] = stack;

                return;
            }

            if (stack is object)
            {
                _stack.Value = null;
            }
            _stack.Value = stack;
        }
    }
}