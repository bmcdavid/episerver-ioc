using EPiServer.ServiceLocation;

namespace EpiserverIoc.Abstractions
{
    public interface IServiceConfigurationProviderWithEnvironment : IServiceConfigurationProvider
    {
        IEpiserverEnvironment Environment { get; }
    }
}