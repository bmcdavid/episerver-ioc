using System;
using System.Web.Optimization;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace EpiserverSite1.Business.Initialization
{
    [InitializableModule]
    public class BundleConfig : IInitializableModule
    {
        public BundleConfig()
        {
            DryIocEpi.DryIocServiceConfigurationProvider.ExtendedCheck += CheckType; 
        }

        private Type CheckType(Type serviceType)
        {
            if (serviceType == typeof(EPiServer.Web.ITemplateResolver))
            {
                return typeof(EPiServer.Web.ITemplateResolverEvents);
                //                EPiServer.Web.Internal.DefaultTemplateResolver

                //container.RegisterMapping<IBar, IFoo>(); // maps to the IBar registration

                //.ITemplateResolverEvents
            }
            return null;
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
