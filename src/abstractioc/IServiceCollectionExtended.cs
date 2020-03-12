using Microsoft.Extensions.DependencyInjection;
using System;

namespace AbstractEpiserverIoc.Core
{
    public interface IServiceCollectionExtended : IServiceCollection
    {
        bool IsConfigured { get; }

        void AddInterceptor<T>(Func<EPiServer.ServiceLocation.IServiceLocator, T,T> interceptor) where T : class;

        Action<ServiceDescriptor> AfterConfigurationAddedDescriptor { get; set; }

        Action<ServiceDescriptor> AfterConfigurationRemovedDescriptor { get; set; }
    }
}