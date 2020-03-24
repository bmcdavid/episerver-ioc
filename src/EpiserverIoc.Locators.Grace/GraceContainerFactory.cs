using EpiserverIoc.Core;
using EpiserverIoc.Locators.Grace;
using EPiServer.ServiceLocation;
using Grace.DependencyInjection;

[assembly: AbstractLocatorFactoryCreator(typeof(GraceContainerFactory), nameof(GraceContainerFactory.Create))]

namespace EpiserverIoc.Locators.Grace
{
    public static class GraceContainerFactory
    {
        public static IServiceLocator Create() => new GraceServiceLocator(new DependencyInjectionContainer());
    }
}