using DryIoc;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;

namespace DryIocEpi
{
    public class DryIocServiceLocator : IServiceLocator
    {
        private readonly IContainer _container;

        public DryIocServiceLocator(IContainer container) => _container = container;

        public IEnumerable<object> GetAllInstances(Type serviceType) =>
            _container.ResolveMany(serviceType);

        public object GetInstance(Type serviceType) => _container.Resolve(serviceType);

        public TService GetInstance<TService>() =>
            (TService)_container.Resolve<TService>(ifUnresolvedReturnDefault: false);

        public object GetService(Type serviceType) => _container.Resolve(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _container.Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }
    }
}
