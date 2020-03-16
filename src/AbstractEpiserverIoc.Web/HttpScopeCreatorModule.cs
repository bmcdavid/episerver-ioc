using AbstractEpiserverIoc.Core;
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
            (app.Context.Items[_itemKey] as IDisposable)?.Dispose();
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            app.Context.Items[_itemKey] =
                (_serviceLocator as IServiceLocatorCreateScope)?.CreateScope();
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}