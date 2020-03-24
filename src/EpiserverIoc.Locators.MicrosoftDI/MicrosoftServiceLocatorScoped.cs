using EpiserverIoc.Abstractions;
using EpiserverIoc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EpiserverIoc.Locators.MicrosoftDI
{
    public class MicrosoftServiceLocatorScoped : BaseScopedServiceLocator, IServiceLocatorScoped
    {
        private readonly IServiceScope _resolverContext;

        public MicrosoftServiceLocatorScoped(IServiceScope resolverContext, IServiceLocatorScoped parentScope)
        {
            _resolverContext = resolverContext;
            ParentScope = parentScope;
        }

        public IServiceLocatorScoped ParentScope { get; }

        public IServiceLocatorScoped CreateScope() => RegisterAndReturnScope(new MicrosoftServiceLocatorScoped(_resolverContext.ServiceProvider.CreateScope(), this));

        public void Dispose()
        {
            if (_resolverContext is null) { return; }

            _resolverContext.Dispose();
            Dispose(disposing: true);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => _resolverContext.ServiceProvider.GetServices(serviceType);//.ToList();

        public object GetInstance(Type serviceType) => _resolverContext.ServiceProvider.GetService(serviceType);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = null;

            try
            {
                instance = _resolverContext.ServiceProvider.GetService(serviceType);
            }
            catch { }

            return instance is object;
        }
    }
}