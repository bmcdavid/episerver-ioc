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
        private static readonly object _lock = new object();
        public HttpIocModule()
        {
            DryIocEpi.DryIocServiceLocator.CheckType += CheckType;
        }
        private void CheckType(string serviceType, string filename = "log.txt")
        {
            var file = System.Web.Hosting.HostingEnvironment.MapPath($"~/App_Data/{filename}");

            lock (_lock)
            {
                if (System.IO.File.Exists(file)) { System.IO.File.Delete(file); }
                System.IO.File.WriteAllText(file, serviceType);

            }
        }

        private IServiceLocator serviceLocator;

        public void Initialize(InitializationEngine context)
        {
            serviceLocator = context.Locate.Advanced;
        }

        public void InitializeHttpEvents(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var context = app.Context;
            (app.Context.Items[nameof(HttpIocModule)] as IDisposable)?.Dispose();
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var context = app.Context;
            app.Context.Items[nameof(HttpIocModule)] = (serviceLocator as IServiceLocatorCreateScope).CreateScope();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}
