using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Abstractions
{
    public interface IServiceLocatorCreateScope : IServiceLocator
    {
        IServiceLocatorScoped CreateScope();
    }
}