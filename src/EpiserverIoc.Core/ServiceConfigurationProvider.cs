using EpiserverIoc.Abstractions;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.Internal; // todo: requires internal
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace EpiserverIoc.Core
{
    public class ServiceConfigurationProvider : IServiceConfigurationProviderWithEnvironment, IRegisteredService, IInterceptorRegister
    {
        private readonly IServiceCollectionExtended _serviceCollection;
        private MicrosoftServiceDescriptor _latestType;

        public ServiceConfigurationProvider(IServiceCollectionExtended serviceCollection, IEpiserverEnvironment episerverEnvironment)
        {
            _serviceCollection = serviceCollection;
            Environment = episerverEnvironment;
        }

        public IEpiserverEnvironment Environment { get; }
        
        public virtual IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, implementationType, ConvertLifeTime(lifetime));

            return AddInternal(descriptor);
        }

        public virtual IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, provider =>
            {
                var ambient = GetAmbientLocator(provider);
                return implementationFactory(ambient);
            },
            ConvertLifeTime(lifetime));

            return AddInternal(descriptor);
        }

        public virtual IRegisteredService Add(Type serviceType, object instance)
        {
            var descriptor = new MicrosoftServiceDescriptor(serviceType, instance);

            return AddInternal(descriptor);
        }

        public virtual IServiceConfigurationProvider AddServiceAccessor()
        {
            ReflectiveServiceConfigurationHelper.RegisterServiceAccessorDelegates(this, _latestType.ServiceType);

            return this;
        }

        public virtual bool Contains(Type serviceType) => _serviceCollection.Any(sd => sd.ServiceType == serviceType);

        public void Intercept<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class
        {
            //To change as little as possible we replace in same place in list
            var toBeReplacedIndexes = new List<int>();
            for (int i = 0; i < _serviceCollection.Count; i++)
            {
                if (_serviceCollection[i].ServiceType == typeof(T))
                {
                    toBeReplacedIndexes.Add(i);
                }
            }

            foreach (var index in toBeReplacedIndexes)
            {
                var existing = _serviceCollection[index];
                var decorated = new MicrosoftServiceDescriptor(
                    existing.ServiceType,
                    s => interceptorFactory(GetAmbientLocator(s), (T)GetDefaultFactory(typeof(T), existing).Invoke(s)),
                    existing.Lifetime);
                _serviceCollection[index] = decorated;
            }
        }

        public virtual IServiceConfigurationProvider RemoveAll(Type serviceType)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }

            var toRemove = _serviceCollection
                .Where(sd => serviceType == sd.ServiceType)
                .ToList();

            foreach (var serviceToRemove in toRemove)
            {
                _serviceCollection.Remove(serviceToRemove);

                if (_serviceCollection.IsConfigured)
                {
                    var args = new ServiceCollectionChangedArgs(serviceToRemove, isRemove: true);
                    _serviceCollection.ServiceCollectionChanged?.Invoke(args);
                }
            }

            return this;
        }

        protected virtual IRegisteredService AddInternal(MicrosoftServiceDescriptor descriptor)
        {
            if (_serviceCollection.IsConfigured)
            {
                var args = new ServiceCollectionChangedArgs(descriptor, isRemove: false);
                _serviceCollection.ServiceCollectionChanged?.Invoke(args);
            }

            _serviceCollection.Add(descriptor);
            _latestType = descriptor;
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

        private static AmbientServiceLocator GetAmbientLocator(IServiceProvider provider)
        {
            if (!(provider is AmbientServiceLocator ambient))
            {
                ambient = provider.GetService<IServiceLocator>() as AmbientServiceLocator;
            }

            return ambient;
        }

        private Func<IServiceProvider, object> GetDefaultFactory(Type _, MicrosoftServiceDescriptor descriptor)
        {
            // https://github.com/dotnet/extensions/blob/5bcbaa2cd1b54da4d919841df815820185189a5c/src/Shared/src/ActivatorUtilities/ActivatorUtilities.cs
            if (descriptor.ImplementationInstance is object)
                return s => descriptor.ImplementationInstance;
            else if (descriptor.ImplementationType is object)
                return s =>
                {

                    //return ActivatorUtilities.GetServiceOrCreateInstance(s, descriptor.ImplementationType); // recursive and bad for Options
                    return ActivatorUtilities.CreateInstance(s, descriptor.ImplementationType);
                };
            else
                return s => descriptor.ImplementationFactory(s);
        }
    }

    internal static class FuncFactory
    {
        // This method is invoked via reflection from ServiceConfigurationScanner.
        [System.Diagnostics.DebuggerStepThrough]
        public static Func<IServiceLocator, TService> Get<TService>(Func<IServiceLocator, object> untyped)
        {
            return (s) => (TService)untyped(s);
        }
    }
}