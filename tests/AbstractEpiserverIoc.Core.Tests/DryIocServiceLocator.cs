using DryIoc;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AbstractEpiserverIoc.Core.Tests
{
    public class DryIocServiceLocator : IServiceLocatorCreateScope, IServiceLocator
    {
        private readonly IServiceCollectionExtended _services;
        private bool isWiredUp;

        public IContainer Container { get; }

        public DryIocServiceLocator(IServiceCollectionExtended services)
        {
            _services = services;

            var rules = Rules.Default
                .WithAutoConcreteTypeResolution()
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .WithTrackingDisposableTransients()
                .WithCaptureContainerDisposeStackTrace()
                //.With(propertiesAndFields: InjectedProperties)
                .WithFuncAndLazyWithoutRegistration()

                ; //used in transient delegate cases
            Container = new Container(rules);
            
        }

        private void WireUp()
        {
            if (isWiredUp) { return; }

            foreach (var service in _services)
            {
                if (service.ImplementationType is object)
                {
                    Add(service.ServiceType, service.ImplementationType, ConvertLifeTime(service.Lifetime));
                    continue;
                }

                if (service.ImplementationInstance is object)
                {
                    Add(service.ServiceType, service.ImplementationInstance);
                    continue;
                }

                Add(service.ServiceType, service.ImplementationFactory, ConvertLifeTime(service.Lifetime));
            }

            isWiredUp = true;
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

        private void Add(Type serviceType, Type implementationType, IReuse lifetime)
        {
            if (serviceType is null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (implementationType is null) { throw new ArgumentNullException(nameof(implementationType), $"{serviceType?.FullName ?? "no service type"} was not given an implementation type!"); }

            Container.Register(serviceType, implementationType, lifetime);
        }

        private void Add(Type serviceType, Func<IServiceLocator, object> implementationFactory, IReuse lifetime)
        {
            if (implementationFactory is null) { throw new ArgumentNullException(nameof(implementationFactory)); }

            object checkedDelegate(IResolverContext r)
            {
                var obj = (object)implementationFactory(r.Resolve<IServiceLocator>());
                if (obj == null)
                {


                    var lf = lifetime;
                }
                return obj
                    .ThrowIfNotInstanceOf(serviceType, Error.RegisteredDelegateResultIsNotOfServiceType, r);
            }

            var factory = new DelegateFactory(checkedDelegate, lifetime, null);

            Container.Register(factory, serviceType, null, null, isStaticallyChecked: false);
        }

        private void Add(Type serviceType, object instance)
        {            
            Container.UseInstance(serviceType, instance);
        }

        public IServiceLocatorScoped CreateScope()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType)
        {
            WireUp();
            return Container.Resolve(serviceType);
        }

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            throw new NotImplementedException();
        }
    }
}
