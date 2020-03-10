using EPiServer.ServiceLocation;

namespace DryIocEpi
{
    public interface IServiceLocatorCreateScope
    {
        IServiceLocator CreateScope();
    }
}
