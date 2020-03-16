using AbstractEpiserverIoc.Core;
using DryIoc;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace AbstractEpiserverIoc.DryIocEpi
{
    public class DryIocServiceLocator : IServiceLocatorCreateScope, IServiceLocator, IServiceLocatorWireupCollection
    {
        private readonly IContainer _container;
        private bool _isWiredUp;

        public DryIocServiceLocator(IContainer rootContainer) => _container = rootContainer as Container;

        public IServiceLocatorScoped CreateScope() => new DryIocServiceLocatorScoped(_container.OpenScope(), null);

        public IEnumerable<object> GetAllInstances(Type serviceType) => _container.ResolveMany(serviceType).ToList();

        public object GetInstance(Type serviceType) => _container.Resolve(serviceType, IfUnresolved.Throw);

        public TService GetInstance<TService>() => _container.Resolve<TService>(ifUnresolved: IfUnresolved.Throw);

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _container.Resolve(serviceType, ifUnresolved: IfUnresolved.ReturnDefaultIfNotRegistered);

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

        private static IReuse ConvertLifeTime(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return Reuse.Singleton;

                case ServiceLifetime.Transient:
                    return Reuse.Transient;

                case ServiceLifetime.Scoped:
                    return Reuse.Scoped;
            }

            throw new NotSupportedException(lifetime.ToString() + " is not supported!");
        }

        private void AddDelegate(Type serviceType, Func<IServiceLocator, object> implementationFactory, IReuse lifetime)
        {
            if (implementationFactory is null) { throw new ArgumentNullException(nameof(implementationFactory)); }

            object checkedDelegate(IResolverContext r)
            {
                var obj = implementationFactory(r.Resolve<IServiceLocator>());
                if (obj == null)
                {
                    var lf = lifetime;
                }
                return obj
                    .ThrowIfNotInstanceOf(serviceType, Error.RegisteredDelegateResultIsNotOfServiceType, r);
            }

            var factory = new DelegateFactory(checkedDelegate, lifetime, null);

            _container.Register(factory, serviceType, null, null, isStaticallyChecked: false);
        }

        private void AddInstance(Type serviceType, object instance) => _container.RegisterInstance(serviceType, instance);

        private void AddType(Type serviceType, Type implementationType, IReuse lifetime)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (implementationType is null) { throw new ArgumentNullException(nameof(implementationType), $"{serviceType?.FullName ?? "no service type"} was not given an implementation type!"); }

            _container.Register(serviceType, implementationType, lifetime);
        }

        private void HandleCollectionChanges(ServiceCollectionChangedArgs args)
        {
            if (args.IsRemove)
            {
                _container.Unregister(args.ServiceDescriptor.ServiceType, condition: f => true);
                return;
            }

            HandleServiceRegistration(args.ServiceDescriptor);
        }

        private void HandleServiceRegistration(MicrosoftServiceDescriptor service)
        {
            if (service.ImplementationType is object)
            {
                AddType(service.ServiceType, service.ImplementationType, ConvertLifeTime(service.Lifetime));
                return;
            }

            if (service.ImplementationInstance is object)
            {
                AddInstance(service.ServiceType, service.ImplementationInstance);
                return;
            }

            AddDelegate(service.ServiceType, service.ImplementationFactory, ConvertLifeTime(service.Lifetime));
        }
    }
}