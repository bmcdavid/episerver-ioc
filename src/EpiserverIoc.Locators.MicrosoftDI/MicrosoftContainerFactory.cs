using EpiserverIoc.Core;
using EpiserverIoc.Locators.MicrosoftDI;
using EPiServer.ServiceLocation;

[assembly: AbstractLocatorFactoryCreator(typeof(MicrosoftContainerFactory), nameof(MicrosoftContainerFactory.Create))]

namespace EpiserverIoc.Locators.MicrosoftDI
{
    public static class MicrosoftContainerFactory
    {
        public static IServiceLocator Create() => new MicrosoftServiceLocator();
    }
}