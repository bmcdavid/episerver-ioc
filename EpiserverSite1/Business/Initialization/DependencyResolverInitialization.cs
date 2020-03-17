using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EpiserverSite1.Business.Rendering;
using System.Web.Mvc;

namespace EpiserverSite1.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]    
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            context.Services.AddSingleton<IContentTypeRepository<BlockType>, BlockTypeRepository>();
#pragma warning restore CS0618 // Type or member is obsolete
            context.Services.AddSingleton<EPiServer.Web.IDisplayChannelService, EPiServer.Web.Internal.DefaultDisplayChannelService>();

            context.ConfigurationComplete += (o, e) =>
            {
                //Register custom implementations that should be used in favour of the default implementations
                e.Services
                    .AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                    .AddTransient<ContentAreaRenderer, AlloyContentAreaRenderer>();
            };
        }

        public void Initialize(InitializationEngine context)
        {
            DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
