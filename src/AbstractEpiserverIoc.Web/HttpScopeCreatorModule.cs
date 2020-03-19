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
        internal const string _itemKey = nameof(HttpScopeCreatorModule);// "servicelocator";
        private static IServiceLocator _serviceLocator;

        public HttpScopeCreatorModule()
        {
            EpiserverEnvironment.EnvironmentNameProvider =
                () => System.Web.Configuration.WebConfigurationManager.AppSettings["episerver:EnvironmentName"];
        }

        public void Initialize(InitializationEngine context)
        {
            _serviceLocator = context.Locate.Advanced;

            if (_serviceLocator is IServiceLocatorAmbientPreferredStorage preferred)
            {
                preferred.SetStorage(() => HttpContext.Current?.Items);
            }
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
        }

        public void Uninitialize(InitializationEngine context) { }

        private static void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!(sender is HttpApplication app)) { return; }
            if (!(app.Context.Items[_itemKey] is IServiceLocatorScoped))
            {
                app.Context.Items[_itemKey] =
                    (_serviceLocator as IServiceLocatorCreateScope).CreateScope();
            }
        }

        private static void Application_EndRequest(object sender, EventArgs e)
        {
            if (!(sender is HttpApplication app)) { return; }

            var scope = app.Context.Items[_itemKey];
            (scope as IServiceLocatorScoped)?.Dispose();
        }
    }
}