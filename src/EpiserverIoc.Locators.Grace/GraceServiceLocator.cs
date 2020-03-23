using EpiserverIoc.Abstractions;
using EpiserverIoc.Core;
using EPiServer.ServiceLocation;
using Grace.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace EpiserverIoc.Locators.Grace
{
    public class GraceServiceLocator : IServiceLocatorCreateScope, IServiceLocator, IServiceLocatorWireupCollection
    {
        private bool _isWiredUp;
        private readonly DependencyInjectionContainer _container;

        public GraceServiceLocator(DependencyInjectionContainer rootContainer) => _container = rootContainer;

        public IServiceLocatorScoped CreateScope() =>
            new GraceServiceLocatorScoped(_container.BeginLifetimeScope(), null);

        public IEnumerable<object> GetAllInstances(Type serviceType) => _container.LocateAll(serviceType);//.ToList();

        public object GetInstance(Type serviceType) => _container.Locate(serviceType);

        public TService GetInstance<TService>() => _container.Locate<TService>();

        public object GetService(Type serviceType) => _container.Locate(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance) => _container.TryLocate(serviceType, out instance);

        public void WireupServices(IServiceCollectionExtended services)
        {
            if (_isWiredUp) { return; }
            services.ServiceCollectionChanged = HandleCollectionChanges;

            foreach (var service in services)
            {
                HandleServiceRegistration(service);
            }

            _isWiredUp = true;
        }

        private void AddInstance(Type serviceType, object instance)
        {
            _container.Configure(c =>
            {
                c.ExportInstance(instance)
                .As(serviceType)
                .ConfigureLifetime(ServiceLifetime.Singleton);
            });
        }

        private void AddType(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (implementationType is null) { throw new ArgumentNullException(nameof(implementationType), $"{serviceType?.FullName ?? "no service type"} was not given an implementation type!"); }

            _container.Configure(c =>
            {
                c.Export(implementationType)
                .As(serviceType)
                .ConfigureLifetime(lifetime);
            });
        }

        private void HandleCollectionChanges(ServiceCollectionChangedArgs args)
        {
            if (args.IsRemove)
            {
                // todo: determine best course of action for Grace when service is removed after wireup
                return;
            }

            HandleServiceRegistration(args.ServiceDescriptor);
        }

        private void HandleServiceRegistration(MicrosoftServiceDescriptor service)
        {
            if (service.ImplementationType is object)
            {
                AddType(service.ServiceType, service.ImplementationType, service.Lifetime);
                return;
            }

            if (service.ImplementationInstance is object)
            {
                AddInstance(service.ServiceType, service.ImplementationInstance);
                return;
            }

            _container.Configure(c =>
            {
                c.ExportFactory(() => service.ImplementationFactory(_container.Locate<IServiceLocator>()))
                .As(service.ServiceType)
                .ConfigureLifetime(service.Lifetime);
            });
        }
    }
}