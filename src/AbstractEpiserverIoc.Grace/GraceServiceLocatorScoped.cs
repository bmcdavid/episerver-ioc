﻿using AbstractEpiserverIoc.Core;
using Grace.DependencyInjection;
using System;
using System.Collections.Generic;

namespace AbstractEpiserverIoc.GraceEpi
{
    public class GraceServiceLocatorScoped : IServiceLocatorScoped
    {
        private readonly IExportLocatorScope _resolverContext;

        public GraceServiceLocatorScoped(IExportLocatorScope resolverContext, IServiceLocatorScoped parentScope)
        {
            _resolverContext = resolverContext;
            ParentScope = parentScope;
        }

        public IServiceLocatorScoped ParentScope { get; }

        public IServiceLocatorScoped CreateScope() => new GraceServiceLocatorScoped(_resolverContext.BeginLifetimeScope(), this);

        public void Dispose()
        {
            if (_resolverContext is null) { return; }

            _resolverContext.Dispose();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) => _resolverContext.LocateAll(serviceType);//.ToList();

        public object GetInstance(Type serviceType) => _resolverContext.Locate(serviceType);

        public TService GetInstance<TService>() => (TService)GetInstance(typeof(TService));

        public object GetService(Type serviceType) => GetInstance(serviceType);

        public bool TryGetExistingInstance(Type serviceType, out object instance) => _resolverContext.TryLocate(serviceType, out instance);
    }
}