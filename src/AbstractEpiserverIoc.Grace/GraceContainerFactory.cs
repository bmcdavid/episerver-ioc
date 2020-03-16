using EPiServer.ServiceLocation;
using Grace.DependencyInjection;

namespace AbstractEpiserverIoc.GraceEpi
{
    public static class GraceContainerFactory
    {
        public static IServiceLocator Create() => new GraceServiceLocator(new DependencyInjectionContainer());
    }
}