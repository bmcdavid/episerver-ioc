using EPiServer.ServiceLocation;

namespace EpiserverIoc.Core.Tests
{
    internal static class TestServiceLocatorFactory
    {
        internal static string LocatorName { get; private set; } = "NotSet";

        internal static IServiceLocator CreateServiceLocator()
        {
#if GRACE_LOCATOR
            LocatorName = nameof(Locators.Grace);
            return Locators.Grace.GraceContainerFactory.Create();
#elif DRYIOC_LOCATOR
            LocatorName = nameof(Locators.DryIoc);
            return Locators.DryIoc.DryIocContainerFactory.Create();
#elif MICROSOFT_LOCATOR
            LocatorName = nameof(Locators.MicrosoftDI);
            return Locators.MicrosoftDI.MicrosoftContainerFactory.Create();
#else
            throw new System.InvalidOperationException();
#endif
        }
    }
}