using AbstractEpiserverIoc.Abstractions;
using AbstractEpiserverIoc.Core;
using Stashbox;
using System;
using System.Collections.Generic;

namespace AbstractEpiserverIoc.StashboxEpi
{
    public class StashboxServiceLocatorScoped : IServiceLocatorScoped
    {
        private readonly IDependencyResolver _resolverContext;

        public StashboxServiceLocatorScoped(IDependencyResolver resolverContext, IServiceLocatorScoped parentScope)
        {
            _resolverContext = resolverContext;
            ParentScope = parentScope;
        }

        public IServiceLocatorScoped ParentScope { get; }

        public IServiceLocatorScoped CreateScope() => new StashboxServiceLocatorScoped(_resolverContext.BeginScope(), this);

        public void Dispose()
        {
            if (_resolverContext is null) { return; }

            _resolverContext.Dispose();
            AmbientServiceLocator.ClearScope();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => _resolverContext.ResolveAll(serviceType);//.ToList();

        public object GetInstance(Type serviceType) => _resolverContext.Resolve(serviceType, nullResultAllowed: false);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = null;

            try
            {
                instance = _resolverContext.Resolve(serviceType, nullResultAllowed: true);
            }
            catch { }

            return instance is object;
        }
    }
}