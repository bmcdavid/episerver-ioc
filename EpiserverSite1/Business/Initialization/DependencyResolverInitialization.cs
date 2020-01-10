using System.Web.Mvc;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EpiserverSite1.Business.Rendering;
using EPiServer.Web.Mvc;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web;
using System;
using System.Collections.Generic;
using EPiServer.DataAbstraction;
using EPiServer.Core;
using StructureMap;

namespace EpiserverSite1.Business.Initialization
{
    public class Test : EPiServer.Cms.Shell.UI.Rest.Projects.IProjectService
    {
        public bool IsInProjectMode => false;

        public bool IsProjectModeEnabled => false;

        public IEnumerable<ProjectItem> AddToCurrentProject(IEnumerable<ContentReference> contentLinks)
        {
            return new ProjectItem[] { };
        }
    }

    [InitializableModule]
    public class DependencyResolverInitialization : IConfigurableModule
    {
        private IContainer _container;

        private static ITemplateResolverEvents GetEvents(IServiceLocator l)
        {
            return l.GetInstance<ITemplateResolver>() as ITemplateResolverEvents;
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            //_container = context.StructureMap();
            //Implementations for custom interfaces can be registered here.
            context.Services.AddSingleton<EPiServer.DataAccess.Internal.LanguageBranchDB>();
            context.Services.Add<EPiServer.Cms.Shell.UI.Rest.Projects.IProjectService, Test>(ServiceInstanceScope.Singleton);
            context.Services.Add(typeof(ITemplateResolver), typeof(EPiServer.Web.Internal.DefaultTemplateResolver), ServiceInstanceScope.Singleton);
            context.Services.Add(GetEvents, ServiceInstanceScope.Singleton);
            context.Services.AddTransient<IPropertyDefinitionRepository,
                EPiServer.DataAbstraction.Internal.DefaultPropertyDefinitionRepository>();

            (context.Services as DryIocEpi.DryIocServiceConfigurationProvider).Verify();

            //        //.AddSingleton<DisplayChannel,Channels.WebChannel>()
            //        //.AddSingleton<DisplayChannel,Channels.MobileChannel>()
            //        ;
            context.ConfigurationComplete += (o, e) =>
            {
                //Register custom implementations that should be used in favour of the default implementations
                context.Services
                    .AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                    .AddTransient<ContentAreaRenderer, AlloyContentAreaRenderer>();

            };
        }

        public void Initialize(InitializationEngine context)
        {
            context.InitComplete += Context_InitComplete;

            DependencyResolver.SetResolver(new ServiceLocatorDependencyResolver(context.Locate.Advanced));
        }

        private void Context_InitComplete(object sender, EventArgs e)
        {
            //_container.AssertConfigurationIsValid();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
