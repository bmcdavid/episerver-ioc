using AbstractEpiserverIoc.Core;
using AbstractEpiserverIoc.StashboxEpi;
using EPiServer.ServiceLocation;

[assembly: AbstractLocatorFactoryCreator(typeof(StashboxContainerFactory), nameof(StashboxContainerFactory.Create))]

namespace AbstractEpiserverIoc.StashboxEpi
{
    public static class StashboxContainerFactory
    {
        public static IServiceLocator Create() => new StashboxServiceLocator(new Stashbox.StashboxContainer());
    }
}