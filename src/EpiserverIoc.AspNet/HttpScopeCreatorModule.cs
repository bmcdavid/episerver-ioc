using EpiserverIoc.Abstractions;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Web;

namespace EpiserverIoc.AspNet
{
    [InitializableModule]
    public class HttpScopeCreatorModule : IInitializableHttpModule, IConfigurableModule
    {
        internal const string _itemKey = nameof(HttpScopeCreatorModule);
        private static IServiceLocator _serviceLocator;

        public HttpScopeCreatorModule()
        {
            EpiserverEnvironment.EnvironmentNameProvider =
                () => System.Web.Configuration.WebConfigurationManager.AppSettings["episerver:EnvironmentName"];
        }

        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            if (context.Services is IServiceConfigurationProviderWithEnvironment env && env.Environment is object)
            {
                context.Services.Add(typeof(IEpiserverEnvironment), env.Environment);
            }
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
                    (_serviceLocator as IServiceLocatorCreateScope)?.CreateScope();
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