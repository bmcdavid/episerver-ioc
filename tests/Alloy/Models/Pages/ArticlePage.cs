using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace Alloy.Models.Pages
{
    /// <summary>
    /// Used primarily for publishing news articles on the website
    /// </summary>
    [SiteContentType(
        GroupName = Global.GroupNames.News,
        GUID = "AEECADF2-3E89-4117-ADEB-F8D43565D2F4")]
    [SiteImageUrl(Global.StaticGraphicsFolderPath + "page-type-thumbnail-article.png")]
    [AvailableContentTypes(
        Availability.Specific,
        IncludeOn = new[] { typeof(StartPage) })]
    public class ArticlePage : StandardPage
    {

    }
}
