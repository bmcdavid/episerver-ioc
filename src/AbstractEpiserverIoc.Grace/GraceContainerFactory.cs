using AbstractEpiserverIoc.Core;
using AbstractEpiserverIoc.GraceEpi;
using EPiServer.ServiceLocation;
using Grace.DependencyInjection;

[assembly: AbstractLocatorFactoryCreator(typeof(GraceContainerFactory), nameof(GraceContainerFactory.Create))]

namespace AbstractEpiserverIoc.GraceEpi
{
    public static class GraceContainerFactory
    {
        public static IServiceLocator Create() => new GraceServiceLocator(new DependencyInjectionContainer());
    }
}