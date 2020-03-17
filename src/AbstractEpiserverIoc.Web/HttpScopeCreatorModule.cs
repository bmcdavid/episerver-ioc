using AbstractEpiserverIoc.Abstractions;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Web;

namespace AbstractEpiserverIoc.Web
{
    [InitializableModule]
    public class HttpScopeCreatorModule : IInitializableHttpModule
    {
        private IServiceLocator _serviceLocator;
        private const string _itemKey = "servicelocator";

        public void Initialize(InitializationEngine context) =>
            _serviceLocator = context.Locate.Advanced;

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var scope = app.Context.Items[_itemKey];
            (scope as IDisposable)?.Dispose();
            // todo: EPiServer.Framework.Web.Resources.ClientResources.RenderResources in UI Admin throws null reference exception
            //  [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Hybrid, ServiceType = typeof (IClientResourceService))] // called from a static.....
            //public class ClientResourceService : IClientResourceService
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            app.Context.Items[_itemKey] =
                (_serviceLocator as IServiceLocatorCreateScope).CreateScope();
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}