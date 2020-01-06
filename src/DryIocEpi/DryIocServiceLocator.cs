using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;

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

        public object GetInstance(Type serviceType) => _resolveContext.Resolve(serviceType);

        public TService GetInstance<TService>() =>
            (TService)_resolveContext.Resolve<TService>(ifUnresolvedReturnDefault: false);

        public object GetService(Type serviceType) => _resolveContext.Resolve(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _resolveContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }
    }
}
