using AbstractEpiserverIoc.Core;
using AbstractEpiserverIoc.MicrosoftEpi;
using EPiServer.ServiceLocation;

[assembly: AbstractLocatorFactoryCreator(typeof(MicrosoftContainerFactory), nameof(MicrosoftContainerFactory.Create))]

namespace AbstractEpiserverIoc.MicrosoftEpi
{
    public static class MicrosoftContainerFactory
    {
        public static IServiceLocator Create() => new MicrosoftServiceLocator();
    }
}