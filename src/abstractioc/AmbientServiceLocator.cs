using AbstractEpiserverIoc.Abstractions;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AbstractEpiserverIoc.Core
{
    public class AmbientServiceLocator : IDisposable, IServiceLocator, IServiceLocatorCreateScope
    {
        private static readonly AsyncLocal<Stack<IServiceLocator>> _stack = new AsyncLocal<Stack<IServiceLocator>>();

        public AmbientServiceLocator(IServiceLocator unscopedContext) => UnScopedContext = unscopedContext;

        protected IServiceLocator UnScopedContext { get; }

        public IServiceLocatorScoped CreateScope()
        {
            var scoped = (AmbientContext() as IServiceLocatorCreateScope).CreateScope();
            AddScope(scoped);

            return scoped;
        }

        public void Dispose()
        {
            var ambientContext = AmbientContext();
            if (ReferenceEquals(ambientContext, UnScopedContext)) { return; } // would need a special processing
            if (ambientContext is null) { return; }
            (ambientContext as IServiceLocatorScoped).Dispose();
            AddScope(null);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => AmbientContext().GetAllInstances(serviceType);

        public object GetInstance(Type serviceType) => AmbientContext().GetInstance(serviceType);

        public TService GetInstance<TService>() => AmbientContext().GetInstance<TService>();

        public object GetService(Type serviceType) => AmbientContext().GetService(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            if (AmbientContext().TryGetExistingInstance(serviceType, out instance))
            {
                return true;
            }

            return false;
        }

        public static void ClearScope() => AddScope(null);

        private static void AddScope(IServiceLocator scopeContext)
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

        public IServiceLocator AmbientContext()
        {
            var stack = GetStack();
            var scoped = stack?.Count > 0 ? stack.Peek() : null;

            return scoped ?? UnScopedContext;
        }

        private static Stack<IServiceLocator> GetStack() => _stack.Value;

        private static void SetStack(Stack<IServiceLocator> stack)
        {
            if (stack is object)
            {
                _stack.Value = null;
            }
            _stack.Value = stack;
        }
    }
}