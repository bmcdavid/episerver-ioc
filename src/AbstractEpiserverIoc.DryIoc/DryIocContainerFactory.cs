using DryIoc;
using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.DryIocEpi
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