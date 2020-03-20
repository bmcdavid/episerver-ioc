using System.Web.Mvc;

namespace EpiserverSite1
{
    public class EPiServerApplication : EPiServer.Global
    {
        protected void Application_Start()
        {
            // todo: violation of options, needs empty constructor
            // Microsoft.Extensions.Options.IOptions<EPiServer.Cms.TinyMce.Core.TinyMceConfiguration> a = null;
            AreaRegistration.RegisterAllAreas();
            //Tip: Want to call the EPiServer API on startup? Add an initialization module instead (Add -> New Item.. -> EPiServer -> Initialization Module)
        }
    }
}