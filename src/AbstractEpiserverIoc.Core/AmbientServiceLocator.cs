using AbstractEpiserverIoc.Abstractions;
using EPiServer.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace AbstractEpiserverIoc.Core
{
    public class AmbientServiceLocator : IServiceLocator, IServiceLocatorCreateScope, IServiceLocatorAmbientPreferredStorage
    {
        private const string _storageKey = nameof(_storageGetter);
        private static readonly AsyncLocal<Stack<IServiceLocator>> _stack =
            new AsyncLocal<Stack<IServiceLocator>>();
        private static Func<IDictionary> _storageGetter;

        public AmbientServiceLocator(IServiceLocator unscopedContext) =>
            UnScopedContext = unscopedContext;

        public bool IsScoped => AmbientContext() is IServiceLocatorScoped;

        protected IServiceLocator UnScopedContext { get; }

        public static void AddScope(IServiceLocator scopeContext)
        {
            var stack = GetStack();
            if (stack is null)
            {
                stack = new Stack<IServiceLocator>();
            }
            // clears on scope disposing
            if (scopeContext is null && stack.Count > 0) { stack.Pop(); }
            // push newest scope to top
            else if (scopeContext is object) { stack.Push(scopeContext); }
            SetStack(stack);
        }

        public static void ClearScope() => AddScope(null);

        public IServiceLocator AmbientContext()
        {
            var stack = GetStack();
            var scoped = stack?.Count > 0 ? stack.Peek() : null;

            return scoped ?? UnScopedContext;
        }

        public IServiceLocatorScoped CreateScope()
        {
            var scoped = (UnScopedContext as IServiceLocatorCreateScope).CreateScope();
            AddScope(scoped);

            return scoped;
        }

        //public void Dispose()
        //{
        //    var ambientContext = AmbientContext();
        //    if (ReferenceEquals(ambientContext, UnScopedContext)) { return; } // would need a special processing
        //    if (ambientContext is null) { return; }
        //    (ambientContext as IServiceLocatorScoped).Dispose();
        //    AddScope(null);
        //}

        public IEnumerable<object> GetAllInstances(Type serviceType) => AmbientContext().GetAllInstances(serviceType);

        public object GetInstance(Type serviceType) => AmbientContext().GetInstance(serviceType);

        public TService GetInstance<TService>() => AmbientContext().GetInstance<TService>();

        public object GetService(Type serviceType) => AmbientContext().GetService(serviceType);

        public void SetStorage(Func<IDictionary> contextStorage)
        {
            _storageGetter = contextStorage;
        }

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            if (AmbientContext().TryGetExistingInstance(serviceType, out instance))
            {
                return true;
            }

            return false;
        }

        private static Stack<IServiceLocator> GetStack()
        {
            var preferredStorage = _storageGetter?.Invoke();
            if (preferredStorage is null) { return _stack.Value; }

            return preferredStorage[_storageKey] as Stack<IServiceLocator>;
        }

        private static void SetStack(Stack<IServiceLocator> stack)
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
    }
}