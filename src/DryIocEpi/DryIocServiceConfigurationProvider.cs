﻿using DryIoc;
using EPiServer.ServiceLocation;
using System;

namespace DryIocEpi
{
    public class DryIocServiceConfigurationProvider : IServiceConfigurationProvider, IRegisteredService, EPiServer.ServiceLocation.Internal.IInterceptorRegister // todo: internal
    {
        private Type _latestType;

        public DryIocServiceConfigurationProvider(IContainer container) => Container = container;

        public IContainer Container { get; }

        public IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (implementationType is null) { throw new ArgumentNullException(nameof(implementationType), $"{serviceType?.FullName ?? "no service type"} was not given an implementation type!"); }
            Container.Register(serviceType, implementationType, ConvertLifeTime(lifetime));
            _latestType = serviceType;
            return this;
        }

        public IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {
            Container.RegisterDelegate(serviceType, r => implementationFactory(new DryIocServiceLocator(r)), ConvertLifeTime(lifetime));

            _latestType = serviceType;
            return this;
        }

        public IRegisteredService Add(Type serviceType, object instance)
        {
            Container.RegisterInstance(serviceType, instance);
            _latestType = serviceType;

            return this;
        }

        public IServiceConfigurationProvider AddServiceAccessor()
        {
            // todo: internal
            EPiServer.ServiceLocation.Internal.
                ReflectiveServiceConfigurationHelper.RegisterServiceAccessorDelegates(this, _latestType);

            return this;
        }

        public void Verify()
        {
            (Container as Container).Validate();
        }

        public bool Contains(Type serviceType) => Container.IsRegistered(serviceType);

        public void Intercept<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class =>
            Container.RegisterDelegateDecorator<T>(r => (t) => interceptorFactory(new DryIocServiceLocator(r), t));

        public IServiceConfigurationProvider RemoveAll(Type serviceType)
        {
            Container.Unregister(serviceType, null, FactoryType.Service, (f) => true);

            return this;
        }

        private static IReuse ConvertLifeTime(ServiceInstanceScope lifetime)
        {
            switch (lifetime)
            {
                case ServiceInstanceScope.Singleton:
                    return Reuse.Singleton;
                case ServiceInstanceScope.PerRequest:
                case ServiceInstanceScope.Transient:
                    return Reuse.Transient;
                case ServiceInstanceScope.HttpContext:
                    return Reuse.InWebRequest;
                case ServiceInstanceScope.Hybrid:
                    return Reuse.Scoped;
            }

            throw new NotSupportedException(lifetime.ToString() + " is not supported!");
        }
    }
}