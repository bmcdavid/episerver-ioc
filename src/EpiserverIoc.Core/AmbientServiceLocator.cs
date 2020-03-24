using EpiserverIoc.Abstractions;
using EPiServer.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace EpiserverIoc.Core
{
    public class AmbientServiceLocator : IServiceLocator, IServiceLocatorCreateScope, IServiceLocatorAmbientPreferredStorage
    {
        private const string _storageKey = nameof(_storageGetter);
        private static readonly AsyncLocal<Stack<IServiceLocatorScoped>> _stack = new AsyncLocal<Stack<IServiceLocatorScoped>>();
        private static Func<IDictionary> _storageGetter;

        public AmbientServiceLocator(IServiceLocator rootLocator)
        {
            RootLocator = rootLocator;
            RootLocatorType = rootLocator?.GetType();
        }

        public bool IsScoped => AmbientContext() is IServiceLocatorScoped;

        public Type RootLocatorType { get; }

        protected IServiceLocator RootLocator { get; }

        public IServiceLocatorScoped CreateScope()
        {
            var scoped = (RootLocator as IServiceLocatorCreateScope).CreateScope();
            AddScope(scoped);

            return scoped;
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => AmbientContext().GetAllInstances(serviceType);

        public object GetInstance(Type serviceType) => AmbientContext().GetInstance(serviceType);

        public TService GetInstance<TService>() => AmbientContext().GetInstance<TService>();

        public object GetService(Type serviceType) => AmbientContext().GetService(serviceType);

        public void SetStorage(Func<IDictionary> contextStorage) => _storageGetter = contextStorage;

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            if (AmbientContext().TryGetExistingInstance(serviceType, out instance))
            {
                return true;
            }

            return false;
        }

        internal static void AddScope(IServiceLocatorScoped scopeContext)
        {
            var stack = GetStack();
            if (stack is null)
            {
                stack = new Stack<IServiceLocatorScoped>();
            }
            // clears on scope disposing
            if (scopeContext is null && stack.Count > 0) { stack.Pop(); }
            // push newest scope to top
            else if (scopeContext is object) { stack.Push(scopeContext); }
            SetStack(stack);
        }

        internal static void ClearScope() => AddScope(null);

        private static Stack<IServiceLocatorScoped> GetStack()
        {
            var preferredStorage = _storageGetter?.Invoke();
            if (preferredStorage is null) { return _stack.Value; }

            return preferredStorage[_storageKey] as Stack<IServiceLocatorScoped>;
        }

        private static void SetStack(Stack<IServiceLocatorScoped> stack)
        {
            var preferredStorage = _storageGetter?.Invoke();

            if (preferredStorage is object)
            {
                preferredStorage[_storageKey] = stack;
                //return;
            }

            if (stack is object)
            {
                _stack.Value = null;
            }
            _stack.Value = stack;
        }

        private IServiceLocator AmbientContext()
        {
            var stack = GetStack();
            var scoped = stack?.Count > 0 ? stack.Peek() : null;

            return scoped ?? RootLocator;
        }
    }
}