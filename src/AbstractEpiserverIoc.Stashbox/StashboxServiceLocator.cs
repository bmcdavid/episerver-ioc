using AbstractEpiserverIoc.Abstractions;
using AbstractEpiserverIoc.Core;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Stashbox;
using Stashbox.Lifetime;
using Stashbox.Registration.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace AbstractEpiserverIoc.StashboxEpi
{
    public class StashboxServiceLocator : IServiceLocatorCreateScope, IServiceLocator, IServiceLocatorWireupCollection
    {
        private bool _isWiredUp;
        private readonly IStashboxContainer _container;

        public StashboxServiceLocator(IStashboxContainer rootContainer) => _container = rootContainer;

        public IServiceLocatorScoped CreateScope() => new StashboxServiceLocatorScoped(_container.BeginScope(), null);

        public IEnumerable<object> GetAllInstances(Type serviceType) => _container.ResolveAll(serviceType).ToList();

        public object GetInstance(Type serviceType) => _container.Resolve(serviceType, nullResultAllowed: false);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetService(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = null;

            try
            {
                instance = _container.Resolve(serviceType, nullResultAllowed: true);
            }
            catch { }

            return instance is object;
        }

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

        private void AddInstance(Type serviceType, object instance) => _container.RegisterInstance(serviceType, instance);

        private void AddType(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (implementationType is null) { throw new ArgumentNullException(nameof(implementationType), $"{serviceType?.FullName ?? "no service type"} was not given an implementation type!"); }


        }

        private void HandleCollectionChanges(ServiceCollectionChangedArgs args)
        {
            if (args.IsRemove)
            {
                // todo: should this throw?
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

            _container.Register
            (
                service.ServiceType,
                c => c
                .ConvertLifetime(service.Lifetime)
                .WithFactory(r => service.ImplementationFactory(r.Resolve<IServiceLocator>()))
            );
        }
    }

    internal static class StashboxExtensions
    {
        public static RegistrationConfigurator ConvertLifetime(this RegistrationConfigurator r, ServiceLifetime l)
        {
            switch (l)
            {
                case ServiceLifetime.Scoped: return r.WithLifetime(new ScopedLifetime());
                case ServiceLifetime.Singleton: return r.WithLifetime(new SingletonLifetime());
                case ServiceLifetime.Transient:
                default:
                    return r;
            }
        }
    }
}