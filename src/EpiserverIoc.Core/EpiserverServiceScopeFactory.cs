using EPiServer.ServiceLocation;
using EpiserverIoc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EpiserverIoc.Core
{
    public class EpiserverServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceLocatorCreateScope _serviceLocator;

        public EpiserverServiceScopeFactory(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator as IServiceLocatorCreateScope;
        }

        public IServiceScope CreateScope() => new ServiceScope(_serviceLocator.CreateScope());

        private class ServiceScope : IServiceScope
        {
            private readonly IServiceLocator _scopedLocator;

            public ServiceScope(IServiceLocator scopedLocator)
            {
                _scopedLocator = scopedLocator;
            }

            public IServiceProvider ServiceProvider => _scopedLocator;

            public void Dispose()
            {
                (_scopedLocator as IDisposable).Dispose();
            }
        }
    }
}
