using AbstractEpiserverIoc.Abstractions;
using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DryIocEpi
{
    public class DryIocServiceLocator : IServiceLocator, IServiceLocatorCreateScope
    {
        // For Ambient Async State
        private static readonly AsyncLocal<Stack<IResolverContext>> _stack = new AsyncLocal<Stack<IResolverContext>>();
        private readonly IResolverContext _resolveContext;

        public DryIocServiceLocator(IResolverContext context) => _resolveContext = context;

        public ICollection<string> Debug() => (AmbientContext() as Container)?
            .GetServiceRegistrations()
            .Select(x => x.ServiceType.FullName + " " + x.ImplementationType?.FullName ?? "n/a" + " " + x.Factory.Reuse.GetType().FullName)
            .ToList();

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

        public void Dispose()
        {
            if (_resolveContext is null || _resolveContext.IsDisposed) { return; }
            _resolveContext.Dispose();
            AddScope(null);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            AmbientContext().ResolveMany(serviceType).ToList();

        public object GetInstance(Type serviceType)
        {
            return AmbientContext().Resolve(serviceType, ifUnresolved: IfUnresolved.ReturnDefaultIfNotRegistered);
        }

        public TService GetInstance<TService>() =>
            (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) =>
            AmbientContext().Resolve(serviceType, IfUnresolved.ReturnDefaultIfNotRegistered);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = AmbientContext().Resolve(serviceType, IfUnresolved.ReturnDefaultIfNotRegistered);

            return instance is object;
        }

        internal static void ClearAmbientScope() => AddScope(null);

        private static void AddScope(IResolverContext scopedLocator)
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

        private static Stack<IResolverContext> GetStack() => _stack.Value;

        private static void SetStack(Stack<IResolverContext> stack)
        {
            if (stack is object)
            {
                _stack.Value = null;
            }
            _stack.Value = stack;
        }
    }
}
