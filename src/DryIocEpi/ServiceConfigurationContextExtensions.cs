using DryIoc;

namespace EPiServer.ServiceLocation
{
    public static class ServiceConfigurationContextExtensions
    {
        public static IContainer DryIoc(this ServiceConfigurationContext context)
        {
            return (context.Services as DryIocEpi.DryIocServiceConfigurationProvider)?.Container;
        }
    }
}