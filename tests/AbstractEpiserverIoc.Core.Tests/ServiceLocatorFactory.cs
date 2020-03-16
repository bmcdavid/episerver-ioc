using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Core.Tests
{
    internal static class TestServiceLocatorFactory
    {
        internal static IServiceLocator CreateServiceLocator(string key)
        {
            return key switch
            {
                "dryioc" => DryIocEpi.DryIocContainerFactory.Create(),
                "grace" => GraceEpi.GraceContainerFactory.Create(),
                _ => throw new System.InvalidOperationException(key),
            };
        }
    }
}
