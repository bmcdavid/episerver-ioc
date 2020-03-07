using DryIoc;
using EPiServer.ServiceLocation;
using System;

namespace DryIocEpi
{
    public class DryIocServiceConfigurationProvider : IServiceConfigurationProvider, IRegisteredService, EPiServer.ServiceLocation.Internal.IInterceptorRegister // todo: internal
    {
        private Type _latestType;

        public static Action<Type,Type,ServiceInstanceScope> Inspector { get; set; }
        public DryIocServiceConfigurationProvider(IContainer container) => Container = container;

        public IContainer Container { get; }

        public IRegisteredService Add(Type serviceType, Type implementationType, ServiceInstanceScope lifetime)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (implementationType is null) { throw new ArgumentNullException(nameof(implementationType), $"{serviceType?.FullName ?? "no service type"} was not given an implementation type!"); }

            Inspector?.Invoke(serviceType, implementationType, lifetime);
            Container.Register(serviceType, implementationType, ConvertLifeTime(lifetime));
            
            //if (implementationType.GetInterfaces() is Type[] interfaces && interfaces.Length > 1)
            //{
            //    foreach (var t in interfaces)
            //    {
            //        if (t == serviceType) { continue; }

            //        try
            //        {
            //            Container.RegisterMapping(t, serviceType, factoryType: FactoryType.Service);
            //        }
            //        catch 
            //        {
            //        }// todo: bad
            //    }
            //}

            _latestType = serviceType;
            return this;
        }

        public IRegisteredService Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, ServiceInstanceScope lifetime)
        {

            Inspector?.Invoke(serviceType, null, lifetime);
            Container.RegisterDelegate(serviceType, r => implementationFactory(r.Resolve<IServiceLocator>()), ConvertLifeTime(lifetime));

            _latestType = serviceType;
            return this;
        }

        public IRegisteredService Add(Type serviceType, object instance)
        {

            Inspector?.Invoke(serviceType, instance.GetType(), ServiceInstanceScope.Singleton);
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
            Container.RegisterDelegateDecorator<T>(r => (t) => interceptorFactory(r.Resolve<IServiceLocator>(), t));

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
                case ServiceInstanceScope.Hybrid:
                    return Reuse.Scoped;
            }

            throw new NotSupportedException(lifetime.ToString() + " is not supported!");
        }
    }
}