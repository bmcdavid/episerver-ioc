using EPiServer.Data;
using EPiServer.Data.Internal;
using EPiServer.Data.Providers;
using EPiServer.ServiceLocation;

namespace EpiserverIoc.Locators.Grace.Extensions
{
    public static class ServiceFixExtensions
    {
        public static void ResolveGraceDependencyIssues(this IServiceConfigurationProvider services)
        {
            services.RemoveAll<IAsyncDatabaseExecutorFactory>();
            services.RemoveAll<ServiceAccessor<IAsyncDatabaseExecutorFactory>>();

            //services.Forward<IDatabaseExecutorFactory, IAsyncDatabaseExecutorFactory>(); // this is bad, must be a singleton....
            services.AddSingleton(locator => (IAsyncDatabaseExecutorFactory)locator.GetInstance<IDatabaseExecutorFactory>());
            services.AddTransient(s => s.GetInstance<IAsyncDatabaseExecutorFactory>().CreateDefaultHandler());
            services.AddTransient(s => (ServiceAccessor<IAsyncDatabaseExecutor>)(() => s.GetInstance<IAsyncDatabaseExecutor>()));
        }
    }
}
