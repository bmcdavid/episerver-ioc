using DryIoc;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.AutoDiscovery;
using EpiserverIoc.Abstractions;

[assembly: ServiceLocatorFactory(typeof(EpiserverIoc.Locators.DryIocPure.DryIocLocatorFactory))]

namespace EpiserverIoc.Locators.DryIocPure
{
    public class DryIocLocatorFactory : IServiceLocatorFactory
    {
        private readonly IContainer _container;
        private readonly IEpiserverEnvironment _episerverEnvironment;

        public DryIocLocatorFactory()
        {
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

            _container = new Container(rules);
            _episerverEnvironment = new EpiserverEnvironment(EpiserverEnvironment.EnvironmentNameProvider?.Invoke());
        }

        public DryIocLocatorFactory(IContainer container) => _container = container;

        public IServiceLocator CreateLocator()
        {
            var dsl = new DryIocServiceLocator(_container);
            _container.
                RegisterInstance(typeof(IServiceLocator), dsl, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            return dsl;
        }

        public IServiceConfigurationProvider CreateProvider() => new DryIocServiceConfigurationProvider(_container, _episerverEnvironment);
    }
}