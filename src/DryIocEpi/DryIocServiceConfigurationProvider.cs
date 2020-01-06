using DryIoc;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.Internal;
using System;

namespace DryIocEpi
{
    public class DryIocServiceConfigurationProvider : IServiceConfigurationProvider, IRegisteredService, IInterceptorRegister
    {
        private Type _latestType;

        public DryIocServiceConfigurationProvider(IContainer container) => Container = container;

        public IContainer Container { get; }

        public IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
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
            ReflectiveServiceConfigurationHelper.RegisterServiceAccessorDelegates(this, _latestType);

            return this;
        }

        public bool Contains(Type serviceType) => Container.IsRegistered(serviceType);

        public void Intercept<T>(Func<IServiceLocator, T, T> interceptorFactory) where T : class => 
            Container.RegisterDelegateDecorator<T>(r => (t) => interceptorFactory(new DryIocServiceLocator(r), r.Resolve<T>()));

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
                case ServiceInstanceScope.Transient:
                    return Reuse.Transient;
                case ServiceInstanceScope.ThreadLocal:
                    return Reuse.Scoped;
                case ServiceInstanceScope.HttpContext:
                    return Reuse.InWebRequest;
                case ServiceInstanceScope.Hybrid:
                    return Reuse.ScopedOrSingleton;
            }

            return Reuse.Transient;
        }
    }
}