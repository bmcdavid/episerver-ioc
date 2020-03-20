using System.Web.Mvc;
using EpiserverSite1.Models.Pages;
using EpiserverSite1.Models.ViewModels;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using EPiServer.ServiceLocation;
using AbstractEpiserverIoc.Abstractions;

namespace EpiserverSite1.Controllers
{
    [ServiceConfiguration(typeof(ScopeTest), Lifecycle = ServiceInstanceScope.HttpContext)]
    public class ScopeTest
    {
        public List<string> ListTest { get; set; } = new List<string>();
    }

    public class StartPageController : PageControllerBase<StartPage>
    {
        private readonly ScopeTest _scopeTest;

        public StartPageController(ScopeTest scopeTest)//, IEpiserverEnvironment episerverEnvironment)
        {
            _scopeTest = scopeTest;
            _scopeTest.ListTest.Add(nameof(StartPageController));
        }

        public async Task<ActionResult> Index(StartPage currentPage)
        {
            var c = await Task.FromResult(0);
            if (_scopeTest.ListTest.Count != 1) { throw new System.Exception("Bad"); }
            var model = PageViewModel.Create(currentPage);

            if (SiteDefinition.Current.StartPage.CompareToIgnoreWorkID(currentPage.ContentLink)) // Check if it is the StartPage or just a page of the StartPage type.
            {
                //Connect the view models logotype property to the start page's to make it editable
                var editHints = ViewData.GetEditHints<PageViewModel<StartPage>, StartPage>();
                editHints.AddConnection(m => m.Layout.Logotype, p => p.SiteLogotype);
                editHints.AddConnection(m => m.Layout.ProductPages, p => p.ProductPageLinks);
                editHints.AddConnection(m => m.Layout.CompanyInformationPages, p => p.CompanyInformationPageLinks);
                editHints.AddConnection(m => m.Layout.NewsPages, p => p.NewsPageLinks);
                editHints.AddConnection(m => m.Layout.CustomerZonePages, p => p.CustomerZonePageLinks);
            }

            return View(model);
        }

    }
}
