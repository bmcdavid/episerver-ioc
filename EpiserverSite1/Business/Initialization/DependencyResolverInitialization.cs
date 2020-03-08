using EPiServer;
using EPiServer.Core;
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
    public static class TestExt
    {
        /// <summary>
        /// Register <typeparamref name="T2" /> as a service where actual instance is delegated to <typeparamref name="T1" /></summary>
        /// <typeparam name="T1">An existing service</typeparam>
        /// <typeparam name="T2">An additional service</typeparam>
        /// <param name="services">The service provider that is extended</param>
        /// <returns>The service configuration provider</returns>
        public static IRegisteredService Forward2<T1, T2>(
          this IServiceConfigurationProvider services)
          where T1 : class, T2
          where T2 : class
        {
            return services.AddTransient<T2>((Func<IServiceLocator, T2>)(s => (object)s.GetInstance<T1>() as T2));
        }

    }

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
            //            e.Services.AddSingleton<EPiServer.Web.ITemplateResolverEvents>(locator => locator.GetInstance<EPiServer.Web.Internal.DefaultTemplateResolver>()
            //as EPiServer.Web.ITemplateResolverEvents);

            e.Services.RemoveAll<EPiServer.Web.ITemplateResolverEvents>();
            e.Services
                .Forward<EPiServer.Web.Internal.DefaultTemplateResolver,EPiServer.Web.ITemplateResolverEvents>();
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
        public static IRegisteredService AddEpiInternalType(this IServiceConfigurationProvider p, ServiceInstanceScope scope, string typeString, Type serviceType = null)
        {
            var type = GetEpiInternalType(typeString);
            return p.Add(serviceType ?? type, type, lifetime: scope);
        }

        private static Type GetEpiInternalType(string typeString)
        {
            return System.Web.Compilation.BuildManager.GetType(typeString, throwOnError: true, ignoreCase: false);
        }

    }
}
