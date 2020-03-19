using AbstractEpiserverIoc.Abstractions;
using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryIocEpi
{
    public class DryIocServiceLocatorScoped : IServiceLocatorScoped
    {
        private readonly IResolverContext _resolverContext;

        public DryIocServiceLocatorScoped(IResolverContext resolverContext, IServiceLocatorScoped parentScope)
        {
            _resolverContext = resolverContext;
            ParentScope = parentScope;
        }

        public IServiceLocatorScoped ParentScope { get; }

        public IServiceLocatorScoped CreateScope()
        {
            var scope = _resolverContext.OpenScope();
            DryIocServiceLocator.AddScope(scope);

            return new DryIocServiceLocatorScoped(scope, this);
        }

        public void Dispose()
        {
            if (_resolverContext is null || _resolverContext.IsDisposed) { return; }

            _resolverContext.Dispose();
            DryIocServiceLocator.ClearAmbientScope();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => _resolverContext.ResolveMany(serviceType).ToList();

        public object GetInstance(Type serviceType) => _resolverContext.Resolve(serviceType, IfUnresolved.Throw);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance)
        {
            instance = _resolverContext.Resolve(serviceType, ifUnresolvedReturnDefault: true);

            return instance is object;
        }
    }
}
