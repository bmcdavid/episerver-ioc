using AbstractEpiserverIoc.Abstractions;
using AbstractEpiserverIoc.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AbstractEpiserverIoc.MicrosoftEpi
{
    public class MicrosoftServiceLocatorScoped : IServiceLocatorScoped
    {
        private readonly IServiceScope _resolverContext;

        public MicrosoftServiceLocatorScoped(IServiceScope resolverContext, IServiceLocatorScoped parentScope)
        {
            _resolverContext = resolverContext;
            ParentScope = parentScope;
        }

        public IServiceLocatorScoped ParentScope { get; }

        public IServiceLocatorScoped CreateScope()
        {
            var scoped = new MicrosoftServiceLocatorScoped(_resolverContext.ServiceProvider.CreateScope(), this);
            AmbientServiceLocator.AddScope(scoped);

            return scoped;
        }

        public void Dispose()
        {
            if (_resolverContext is null) { return; }

            _resolverContext.Dispose();
            AmbientServiceLocator.ClearScope();
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