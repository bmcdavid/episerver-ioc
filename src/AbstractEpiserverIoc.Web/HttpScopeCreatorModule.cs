using AbstractEpiserverIoc.Abstractions;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Collections;
using System.Web;

namespace AbstractEpiserverIoc.Web
{
    [InitializableModule]
    public class HttpScopeCreatorModule : IInitializableHttpModule
    {
        private const string _itemKey = nameof(HttpScopeCreatorModule);
        private IServiceLocator _serviceLocator;
        // "servicelocator";

        public void Initialize(InitializationEngine context) =>
            _serviceLocator = context.Locate.Advanced;

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
            //application.PreRequestHandlerExecute += Application_PreRequestHandlerExecute;
            //application.MapRequestHandler += Application_MapRequestHandler;
            //System.Web.HttpApplication.CallHandlerExecutionStep.System.Web.HttpApplication.IExecutionStep.Execute
        }

        public void Uninitialize(InitializationEngine context) { }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            CreateRequestScope(app.Context.Items, nameof(HttpApplication.BeginRequest));
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var scope = app.Context.Items[_itemKey];
            (scope as IServiceLocatorScoped)?.Dispose();
        }

        private void CreateRequestScope(IDictionary httpContextItem, string _)
        {
            if (!(httpContextItem[_itemKey] is IServiceLocatorScoped))
            {
                httpContextItem[_itemKey] =
                    (_serviceLocator as IServiceLocatorCreateScope).CreateScope();
            }
        }
    }
}