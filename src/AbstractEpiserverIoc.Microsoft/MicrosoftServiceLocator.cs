using AbstractEpiserverIoc.Abstractions;
using AbstractEpiserverIoc.Core;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbstractEpiserverIoc.MicrosoftEpi
{
    public class MicrosoftServiceLocator : IServiceLocatorCreateScope, IServiceLocator, IServiceLocatorWireupCollection
    {
        private bool _isWiredUp;
        private ServiceProvider _container;

        public MicrosoftServiceLocator() { }

        public IServiceLocatorScoped CreateScope() =>
            new MicrosoftServiceLocatorScoped(_container.CreateScope(), null);

        public IEnumerable<object> GetAllInstances(Type serviceType) => _container.GetServices(serviceType).ToList();

        public object GetInstance(Type serviceType) => _container.GetService(serviceType);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetService(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = null;

            try
            {
                instance = _container.GetService(serviceType);
            }
            catch { }

            return instance is object;
        }

        public void WireupServices(IServiceCollectionExtended services)
        {
            if (_isWiredUp) { return; }
            services.ServiceCollectionChanged = HandleCollectionChanges;
            _container = services.BuildServiceProvider();
            _isWiredUp = true;
        }

        private void HandleCollectionChanges(ServiceCollectionChangedArgs args)
        {
            if (args.IsRemove)
            {
                // todo: should this throw?
                return;
            }
        }
    }
}