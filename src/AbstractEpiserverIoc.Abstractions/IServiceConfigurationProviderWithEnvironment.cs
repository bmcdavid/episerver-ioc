using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Abstractions
{
    public interface IServiceConfigurationProviderWithEnvironment : IServiceConfigurationProvider
    {
        IEpiserverEnvironment Environment { get; }
    }
}