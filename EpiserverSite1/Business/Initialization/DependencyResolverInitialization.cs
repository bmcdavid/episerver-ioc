using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EpiserverSite1.Business.Channels;
using EpiserverSite1.Business.Rendering;
using System;
using System.Web.Mvc;

namespace EpiserverSite1.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.ConfigurationComplete += Context_ConfigurationComplete;

            context.Services
                // project specific
                .AddSingleton<StandardResolution>()
                .AddSingleton<IpadHorizontalResolution>()
                .AddSingleton<IphoneVerticalResolution>()
                .AddSingleton<AndroidVerticalResolution>()
                .AddSingleton<PageContextActionFilter>()
                .AddSingleton<PageViewContextFactory>()
                .AddSingleton<WebChannel>()
                .AddSingleton<MobileChannel>()

                // Epi specific
                .AddEpiInternalType(ServiceInstanceScope.Singleton, "EPiServer.Cms.Shell.UI.Approvals.Notifications.UserNotificationFormatter")
                .AddEpiInternalType(ServiceInstanceScope.Singleton, ("EPiServer.Cms.Shell.UI.Approvals.Notifications.ApprovalViewModelFactory"))
                .AddEpiInternalType(ServiceInstanceScope.Singleton, ("EPiServer.Cms.Shell.UI.Approvals.Notifications.EmailViewModelFactory"))
                .AddEpiInternalType(ServiceInstanceScope.Singleton, "EPiServer.Cms.Shell.PropertyListItemsDefinitionsLoader")
                .AddTransient<EPiServer.Cms.Shell.UI.ObjectEditing.Internal.FileExtensionsResolver>()
                .AddTransient<EPiServer.Security.CreatorRole>()
                .AddTransient<EPiServer.Security.MappedRole>()
            ;

            context.ConfigurationComplete += (o, e) =>
            {
                //Register custom implementations that should be used in favour of the default implementations
                e.Services
                    .AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                    .AddTransient<ContentAreaRenderer, AlloyContentAreaRenderer>();
            };
        }

        private void Context_ConfigurationComplete(object sender, ServiceConfigurationEventArgs e)
        {

        }

        public void Initialize(InitializationEngine context)
        {
            DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }

    internal static class BuildExtensions
    {
        public static IRegisteredService AddEpiInternalType(this IServiceConfigurationProvider p, ServiceInstanceScope scope, string typeString)
        {
            var type = GetEpiInternalType(typeString);
            return p.Add(type, type, lifetime: scope);
        }

        private static Type GetEpiInternalType(string typeString)
        {
            return System.Web.Compilation.BuildManager.GetType(typeString, throwOnError: true, ignoreCase: false);
        }

    }
}
