using EPiServer.ServiceLocation;

namespace AbstractEpiserverIoc.Core.Tests
{
    internal static class TestServiceLocatorFactory
    {
        internal static string LocatorName = "NotSet";

        internal static IServiceLocator CreateServiceLocator()
        {
#if GRACE_LOCATOR
            LocatorName = nameof(GraceEpi);
            return GraceEpi.GraceContainerFactory.Create();
#elif DRYIOC_LOCATOR
            LocatorName = nameof(DryIocEpi);
            return DryIocEpi.DryIocContainerFactory.Create();
#elif STASHBOX_LOCATOR
            LocatorName = nameof(StashboxEpi);
            return StashboxEpi.StashboxContainerFactory.Create();
#else
            throw new System.InvalidOperationException();
#endif
        }
    }
}
