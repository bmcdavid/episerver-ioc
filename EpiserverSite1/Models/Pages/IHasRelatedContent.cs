using EPiServer.Core;

namespace EpiserverSite1.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}
