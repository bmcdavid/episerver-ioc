using EPiServer.ServiceLocation;

namespace EpiserverIoc.Abstractions
{
    public static class ServiceConfigurationProviderExtensions
    {
        public static string EnvironmentName(this IServiceConfigurationProvider services)
        {
            if (!(services is IServiceConfigurationProviderWithEnvironment env)) { return string.Empty; }

            return env.Environment.Name;
        }

        public static bool IsIntegrationEnvironment(this IServiceConfigurationProvider services)
        {
            if (!(services is IServiceConfigurationProviderWithEnvironment env)) { return false; }

            return env.Environment.IsIntegration;
        }

        public static bool IsLocalEnvironment(this IServiceConfigurationProvider services)
        {
            if (!(services is IServiceConfigurationProviderWithEnvironment env)) { return false; }

            return env.Environment.IsLocal;
        }

        public static bool IsPreproductionEnvironment(this IServiceConfigurationProvider services)
        {
            if (!(services is IServiceConfigurationProviderWithEnvironment env)) { return false; }

            return env.Environment.IsPreproduction;
        }

        public static bool IsProductionEnvironment(this IServiceConfigurationProvider services)
        {
            if (!(services is IServiceConfigurationProviderWithEnvironment env)) { return false; }

            return env.Environment.IsProduction;
        }

        public static bool IsUnitTestEnvironment(this IServiceConfigurationProvider services)
        {
            if (!(services is IServiceConfigurationProviderWithEnvironment env)) { return false; }

            return env.Environment.IsIntegration;
        }
    }
}