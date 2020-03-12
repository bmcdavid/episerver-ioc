using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Core
{
    public interface IServiceLocatorScoped : IServiceLocator, IServiceLocatorCreateScope
    {
        IServiceLocator Parent { get; }
    }
}