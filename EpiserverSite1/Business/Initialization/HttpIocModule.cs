using DryIocEpi;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Web;

namespace EpiserverSite1.Business.Initialization
{
    [InitializableModule]
    public class HttpIocModule : IInitializableHttpModule
    {
        private IServiceLocator serviceLocator;
        private const string itemKey = "servicelocator";

        public void Initialize(InitializationEngine context) =>
            serviceLocator = context.Locate.Advanced;

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.PreSendRequestContent += Application_EndRequest;
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            //(app.Context.Items[itemKey] as IDisposable)?.Dispose();
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            app.Context.Items[itemKey] =
                (serviceLocator as IServiceLocatorCreateScope)?.CreateScope();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
