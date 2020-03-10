using DryIoc;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

[assembly: EPiServer.ServiceLocation.AutoDiscovery.ServiceLocatorFactory(typeof(DryIocEpi.DryIocLocatorFactory))]

namespace DryIocEpi
{
    public interface IServiceLocatorCreateScope
    {
        IServiceLocator CreateScope();
    }

    public class DryIocServiceLocator : IServiceLocator, IDisposable, IServiceLocatorCreateScope
    {
        public static Action<string, string> CheckType;
        private static readonly AsyncLocal<Stack<IResolverContext>> _stack = new AsyncLocal<Stack<IResolverContext>>();

        [JsonIgnore]
        private readonly IResolverContext _resolveContext;

        public DryIocServiceLocator(IResolverContext context) => _resolveContext = context;

        public ICollection<string> Debug() => (AmbientContext() as Container)?
            .GetServiceRegistrations()
            .Select(x => x.ServiceType.FullName + " " + x.ImplementationType?.FullName ?? "n/a" + " " + x.Factory.Reuse.GetType().FullName)
            .ToList();

        public IResolverContext AmbientContext()
        {
            var stack = GetStack();
            return stack?.Count > 0 ? stack.Peek() : _resolveContext;
        }

        public IServiceLocator CreateScope()
        {
            var scope = AmbientContext().OpenScope();
            AddScope(scope);

            return new DryIocServiceLocator(scope);
        }

        public void Dispose()
        {
            if (_resolveContext.IsDisposed) { return; }
            _resolveContext.Dispose();
            SetStack(null);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            AmbientContext().ResolveMany(serviceType);

        public object GetInstance(Type serviceType)
        {
            try
            {
                return AmbientContext().Resolve(serviceType, ifUnresolvedReturnDefault: false);
            }
            catch (Exception e)
            {
                var issues = (AmbientContext() as IContainer)?
                    .Validate();
                var issueList = issues
                    .Select(kvp => kvp.Key.ToString() + " = " + kvp.Value.ToString())
                    .ToList();

                CheckType?.Invoke(string.Join(Environment.NewLine, Debug()), "registrations.txt");
                CheckType?.Invoke(string.Join(Environment.NewLine, issueList), "issues.txt");

                throw new Exception("Unable to resolve: " + serviceType.FullName, e);
            }
        }

        public TService GetInstance<TService>() =>
            (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) =>
            AmbientContext().Resolve(serviceType, IfUnresolved.Throw);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = AmbientContext().Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }

        private static void AddScope(IResolverContext scopedLocator)
        {
            var stack = GetStack() ?? new Stack<IResolverContext>();
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
