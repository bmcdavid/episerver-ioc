using EpiserverIoc.Abstractions;
using EPiServer.Data;
using EPiServer.Data.Internal;
using EPiServer.Data.Providers;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using Alloy.Business.Rendering;
using System.Web.Mvc;

namespace Alloy.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [ModuleDependency(typeof(DataInitialization))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public static string EnvironmentName { get; private set; } = "notset";

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            // note: violation of options, needs empty constructor
            // Microsoft.Extensions.Options.IOptions<EPiServer.Cms.TinyMce.Core.TinyMceConfiguration> a = null;

            // changes for Grace
            var services = context.Services;
            services.RemoveAll<IAsyncDatabaseExecutorFactory>();
            services.RemoveAll<ServiceAccessor<IAsyncDatabaseExecutorFactory>>();

            //services.Forward<IDatabaseExecutorFactory, IAsyncDatabaseExecutorFactory>(); // this is bad, must be a singleton....
            services.AddSingleton(locator => (IAsyncDatabaseExecutorFactory)locator.GetInstance<IDatabaseExecutorFactory>());
            services.AddTransient(s => s.GetInstance<IAsyncDatabaseExecutorFactory>().CreateDefaultHandler());
            services.AddTransient(s => (ServiceAccessor<IAsyncDatabaseExecutor>)(() => s.GetInstance<IAsyncDatabaseExecutor>()));
            // end for Grace

            // changes for DryIoc
#pragma warning disable CS0618 // Type or member is obsolete
            context.Services.AddSingleton<IContentTypeRepository<BlockType>, BlockTypeRepository>();
#pragma warning restore CS0618 // Type or member is obsolete
            // end for DryIoc

            // Example of enviornment specific configuration, for example change logging
            EnvironmentName = context.Services.EnvironmentName();

            if (context.Services.IsIntegrationEnvironment())
            {
                // could do something specific to integration here...
            }

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

        public void Uninitialize(InitializationEngine context) { }
    }
}
