using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Core.Tests
{
    internal static class TestServiceLocatorFactory
    {
        internal static IServiceLocator CreateServiceLocator()
        {
            return DryIocEpi.DryIocContainerFactory.Create();
        }
    }
}
