using EpiserverIoc.Core;
using EpiserverIoc.Locators.DryIoc;
using EPiServer.ServiceLocation;
using StructureMap;

[assembly: AbstractLocatorFactoryCreator(typeof(StructureMapContainerFactory), nameof(StructureMapContainerFactory.Create))]

namespace EpiserverIoc.Locators.DryIoc
{
    public static class StructureMapContainerFactory
    {
        public static IServiceLocator Create()
        {
            var registry = new Registry();

            return new StructureMapServiceLocator(new Container(registry));
        }
    }
}