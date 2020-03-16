using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Core
{
    public interface IServiceLocatorCreateScope : IServiceLocator
    {
        IServiceLocatorScoped CreateScope();
    }
}