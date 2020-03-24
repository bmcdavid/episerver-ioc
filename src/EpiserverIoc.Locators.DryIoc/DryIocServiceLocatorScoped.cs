using EpiserverIoc.Abstractions;
using EpiserverIoc.Core;
using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EpiserverIoc.Locators.DryIoc
{
    public class DryIocServiceLocatorScoped : BaseScopedServiceLocator, IServiceLocatorScoped
    {
        private readonly IResolverContext _resolverContext;

        public DryIocServiceLocatorScoped(IResolverContext resolverContext, IServiceLocatorScoped parentScope)
        {
            _resolverContext = resolverContext;
            ParentScope = parentScope;
        }

        public IServiceLocatorScoped ParentScope { get; }

        public IServiceLocatorScoped CreateScope() => RegisterAndReturnScope(new DryIocServiceLocatorScoped(_resolverContext.OpenScope(), this));

        public void Dispose()
        {
            if (_resolverContext is null || _resolverContext.IsDisposed) { return; }

            _resolverContext.Dispose();
            Dispose(disposing: true);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => _resolverContext.ResolveMany(serviceType).ToList();

        public object GetInstance(Type serviceType) => _resolverContext.Resolve(serviceType, IfUnresolved.Throw);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _resolverContext.Resolve(serviceType, ifUnresolved: IfUnresolved.ReturnDefaultIfNotRegistered);

            return instance is object;
        }
    }
}