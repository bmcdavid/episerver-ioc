using EPiServer.ServiceLocation;

namespace EpiserverIoc.Abstractions
{
    public interface IServiceLocatorCreateScope : IServiceLocator
    {
        IServiceLocatorScoped CreateScope();
    }
}