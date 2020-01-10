using DryIocEpi;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using System;
using System.Web;
using System.Web.Optimization;

namespace EpiserverSite1.Business.Initialization
{
    [InitializableModule]
    public class HttpIoc : IInitializableHttpModule
    {
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
            (app.Context.Items[nameof(HttpIoc)] as IDisposable)?.Dispose();
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            var context = app.Context;
            app.Context.Items[nameof(HttpIoc)] = (serviceLocator as IServiceLocatorCreateScope).CreateScope();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }

    [InitializableModule]
    public class BundleConfig : IInitializableModule
    {
        public BundleConfig()
        {
            DryIocEpi.DryIocServiceLocator.CheckType += CheckType;
        }
        private void CheckType(string serviceType)
        {
            var file = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/log.txt");
            if (System.IO.File.Exists(file)) { System.IO.File.Delete(file); }
            System.IO.File.WriteAllText(file, serviceType);

            //if (serviceType == typeof(EPiServer.Web.ITemplateResolver))
            {
                //container.RegisterMapping<IBar, IFoo>(); // maps to the IBar registration

            }
        }

        public void Initialize(InitializationEngine context)
        {
            if (context.HostType == HostType.WebApplication)
            {
                RegisterBundles(BundleTable.Bundles);
            }
        }

        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                        "~/Static/js/jquery.js", //jquery.js can be removed and linked from CDN instead, we use a local one for demo purposes without internet connectionzz
                        "~/Static/js/bootstrap.js"));

            bundles.Add(new StyleBundle("~/bundles/css")
                .Include("~/Static/css/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Static/css/bootstrap-responsive.css")
                .Include("~/Static/css/media.css")
                .Include("~/Static/css/style.css", new CssRewriteUrlTransform())
                .Include("~/Static/css/editmode.css"));
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }
}
