using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Serialization;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EpiserverSite1.Business.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace EpiserverSite1.Business.Initialization
{
    internal class AppSerializer : IObjectSerializer
    {
        EPiServer.Framework.Serialization.Json.Internal.JsonObjectSerializer _epi;

        public AppSerializer(IEnumerable<JsonConverter> converters, IContractResolver resolver)//IEnumerable<JsonConverter> converters
        {
            //var test2 = serviceLocator.GetInstance<EPiServer.Construction.Internal.ContentDataFactory<EPiServer.Core.BlockData>>();
            //var test3 = serviceLocator.GetInstance<EPiServer.Construction.IContentDataBuilder>();
            //var test4 = serviceLocator.GetInstance<IContentTypeRepository<BlockType>>();
            //var test = serviceLocator.GetInstance<EPiServer.Cms.Shell.Json.Internal.BlockDataConverter>();
            //var converters = serviceLocator.GetAllInstances<JsonConverter>();
            _epi = new EPiServer.Framework.Serialization.Json.Internal.JsonObjectSerializer(converters, resolver);
        }

        public IEnumerable<string> HandledContentTypes => _epi.HandledContentTypes;

        public object Deserialize(TextReader reader, Type objectType) => _epi.Deserialize(reader, objectType);

        public T Deserialize<T>(TextReader reader) => _epi.Deserialize<T>(reader);

        public void Serialize(TextWriter textWriter, object value)
        {
            try
            {
                _epi.Serialize(textWriter, value);
            }
            catch (Exception e)
            {

            }
        }
    }

    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IContentTypeRepository<BlockType>, BlockTypeRepository>();            
            context.Services.AddSingleton<IObjectSerializer, AppSerializer>();


            //context.Services.Intercept<IObjectSerializer>(
            //    (locator, existing) => new AppSerializer(existing));
            // context.Services
            //// project specific
            //.AddSingleton<StandardResolution>()
            //.AddSingleton<IpadHorizontalResolution>()
            //.AddSingleton<IphoneVerticalResolution>()
            //.AddSingleton<AndroidVerticalResolution>()
            //.AddSingleton<PageContextActionFilter>()
            //.AddSingleton<PageViewContextFactory>()
            //.AddSingleton<WebChannel>()
            //.AddSingleton<MobileChannel>()

            //// Epi specific
            //.AddEpiInternalType(ServiceInstanceScope.Singleton, "EPiServer.Cms.Shell.UI.Approvals.Notifications.UserNotificationFormatter")
            //.AddEpiInternalType(ServiceInstanceScope.Singleton, ("EPiServer.Cms.Shell.UI.Approvals.Notifications.ApprovalViewModelFactory"))
            //.AddEpiInternalType(ServiceInstanceScope.Singleton, ("EPiServer.Cms.Shell.UI.Approvals.Notifications.EmailViewModelFactory"))
            //.AddEpiInternalType(ServiceInstanceScope.Singleton, "EPiServer.Cms.Shell.PropertyListItemsDefinitionsLoader")
            //.AddTransient<EPiServer.Cms.Shell.UI.ObjectEditing.Internal.FileExtensionsResolver>()
            //.AddTransient<EPiServer.Security.CreatorRole>()
            //.AddTransient<EPiServer.Security.MappedRole>()
            // ;

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
