using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: EPiServer.ServiceLocation.AutoDiscovery.ServiceLocatorFactory(typeof(DryIocEpi.DryIocLocatorFactory))]

namespace DryIocEpi
{
    public class DryIocServiceLocator : IServiceLocator, IDisposable
    {
        private readonly IResolverContext _resolveContext;

        public DryIocServiceLocator(IResolverContext context) => _resolveContext = context;

        public void Dispose()
        {
            if (_resolveContext.IsDisposed) { return; }
            _resolveContext.Dispose();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            _resolveContext.ResolveMany(serviceType);

        public object GetInstance(Type serviceType) => _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

        public TService GetInstance<TService>() =>
            (TService)_resolveContext.Resolve<TService>(ifUnresolvedReturnDefault: true);

        public ICollection<string> Debug => (_resolveContext as Container)?
            .GetServiceRegistrations()
            .Select(x => x.ToString())
            .ToList();

        public object GetService(Type serviceType) => _resolveContext.Resolve(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }
    }
}
