using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.Internal; // todo: requires internal
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace AbstractEpiserverIoc.Core
{
    // todo needs event to broadcast registrations past configuration complete since some things do utilize this....
    public class ServiceConfigurationProvider : IServiceConfigurationProvider, IRegisteredService, IInterceptorRegister
    {
        private readonly IServiceCollectionExtended _serviceCollection;
        private Type _latestType;

        public ServiceConfigurationProvider(IServiceCollectionExtended serviceCollection) => _serviceCollection = serviceCollection;

        public IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, implementationType, ConvertLifeTime(lifetime));

            return AddInternal(descriptor);
        }

        public IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, provider => implementationFactory(provider as IServiceLocator), ConvertLifeTime(lifetime));

            return AddInternal(descriptor);
        }

        public IRegisteredService Add(Type serviceType, object instance)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, instance);

            return AddInternal(descriptor);
        }

        public IServiceConfigurationProvider AddServiceAccessor()
        {
            ReflectiveServiceConfigurationHelper.RegisterServiceAccessorDelegates(this, _latestType);

            return this;
        }

        public bool Contains(Type serviceType) => _serviceCollection.Any(sd => sd.ServiceType == serviceType);

        public void Intercept<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class => _serviceCollection.AddInterceptor(interceptorFactory);

        public IServiceConfigurationProvider RemoveAll(Type serviceType)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }            

            var toRemove = _serviceCollection.Where(sd => serviceType == sd.ServiceType);

            foreach (var serviceToRemove in toRemove)
            {
                _serviceCollection.Remove(serviceToRemove);

                if (_serviceCollection.IsConfigured)
                {
                    _serviceCollection.AfterConfigurationRemovedDescriptor?.Invoke(serviceToRemove);
                }
            }

            return this;
        }

        private static ServiceLifetime ConvertLifeTime(ServiceInstanceScope lifetime)
        {
            switch (lifetime)
            {
                case ServiceInstanceScope.Singleton:
                    return ServiceLifetime.Singleton;
#pragma warning disable CS0618 // Type or member is obsolete
                case ServiceInstanceScope.Unique:
                case ServiceInstanceScope.PerRequest:
#pragma warning restore CS0618 // Type or member is obsolete
                case ServiceInstanceScope.Transient:
                    return ServiceLifetime.Transient;
                case ServiceInstanceScope.HttpContext:
                case ServiceInstanceScope.Hybrid:
                    return ServiceLifetime.Scoped;
            }

            throw new NotSupportedException(lifetime.ToString() + " is not supported!");
        }

        private IRegisteredService AddInternal(MicrosoftServiceDescriptor descriptor)
        {
            if (_serviceCollection.IsConfigured)
            {
                _serviceCollection.AfterConfigurationAddedDescriptor?.Invoke(descriptor);
            }

            _serviceCollection.Add(descriptor);
            _latestType = descriptor.ServiceType;
            return this;
        }
    }
}