using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IResolverContext _resolveContext;

        public DryIocServiceLocator(IResolverContext context)
        {
            _resolveContext = context;
        }

        public ICollection<string> Debug => (_resolveContext as Container)?
            .GetServiceRegistrations()
            .Select(x => x.ServiceType.FullName + " " + x.ImplementationType?.FullName ?? "n/a" + " " + x.Factory.Reuse.GetType().FullName)
            .ToList();

        public IServiceLocator CreateScope() => new DryIocServiceLocator(_resolveContext.OpenScope());

        public void Dispose()
        {
            if (_resolveContext.IsDisposed) { return; }
            _resolveContext.Dispose();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            _resolveContext.ResolveMany(serviceType);

        private static Type _iServiceLocatorType = typeof(IServiceLocator);

        public object GetInstance(Type serviceType)
        {
            try
            {
                //if (_iServiceLocatorType.IsAssignableFrom(serviceType)) { return this; }

                return _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);
            }
            catch (Exception e)
            {
                var issues = (_resolveContext as IContainer)?
                    .Validate();
                var issueList = issues
                    .Select(kvp => kvp.Key.ToString() + " = " + kvp.Value.ToString())
                    .ToList();

                CheckType?.Invoke(string.Join(Environment.NewLine, Debug), "registrations.txt");
                CheckType?.Invoke(string.Join(Environment.NewLine, issueList), "issues.txt");

                throw new Exception("Unable to resolve: " + serviceType.FullName, e);
            }
        }

        public TService GetInstance<TService>() =>
            (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => _resolveContext.Resolve(serviceType, IfUnresolved.ReturnDefault);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }
    }
}
