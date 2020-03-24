using EpiserverIoc.Core;
using EpiserverIoc.Locators.DryIoc;
using DryIoc;
using EPiServer.ServiceLocation;

[assembly: AbstractLocatorFactoryCreator(typeof(DryIocContainerFactory), nameof(DryIocContainerFactory.Create))]

namespace EpiserverIoc.Locators.DryIoc
{
    public static class DryIocContainerFactory
    {
        public static IServiceLocator Create()
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

            return new DryIocServiceLocator(new Container(rules));
        }
    }
}