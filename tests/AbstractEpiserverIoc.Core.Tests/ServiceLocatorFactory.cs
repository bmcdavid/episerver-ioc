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
#elif MICROSOFT_LOCATOR
            LocatorName = nameof(MicrosoftEpi);
            return MicrosoftEpi.MicrosoftContainerFactory.Create();
#else
            throw new System.InvalidOperationException();
#endif
        }
    }
}