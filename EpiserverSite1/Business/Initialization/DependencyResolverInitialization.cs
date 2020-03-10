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
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace EpiserverSite1.Business.Initialization
{
    public class AppContractResolver :
        //DefaultContractResolver
        EPiServer.Framework.Serialization.Json.Internal.DefaultNewtonsoftContractResolver
    {
        protected static Type[] IgnoredPropertyTypes =
        {
            typeof(ServiceLocationHelper)
        };

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var propertyType = (member as PropertyInfo)?.PropertyType;
            if (member.MemberType == MemberTypes.Property
                && IgnoredPropertyTypes.Contains(propertyType))
            {
                return new JsonProperty { ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            }

            return base.CreateProperty(member, memberSerialization);
        }
    }

    internal class AppSerializer : IObjectSerializer
    {
        private readonly IObjectSerializer _existing;

        public AppSerializer(IObjectSerializer existing) => _existing = existing;

        public IEnumerable<string> HandledContentTypes => _existing.HandledContentTypes;

        public object Deserialize(TextReader reader, Type objectType) =>
            _existing.Deserialize(reader, objectType);

        public T Deserialize<T>(TextReader reader) => _existing.Deserialize<T>(reader);

        public void Serialize(TextWriter textWriter, object value)
        {
            try
            {
                _existing.Serialize(textWriter, value);
            }
            catch (Exception e)
            {
                // EPiServer.Cms.Shell.UI.ObjectEditing.InternalMetadata.ExpireBlock
            }
        }
    }

    [InitializableModule]
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            //EPiServer.Framework.Serialization.Json.Internal.JsonObjectSerializer
            context.Services.AddTransient<IContractResolver,AppContractResolver>();
            context.Services.Intercept<IObjectSerializer>(
                (locator, existing) => new AppSerializer(existing));
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
