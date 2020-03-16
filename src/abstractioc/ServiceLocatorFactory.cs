﻿using AbstractEpiserverIoc.Core.Exceptions;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.AutoDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

[assembly: ServiceLocatorFactory(typeof(AbstractEpiserverIoc.Core.ServiceLocatorFactory))]

namespace AbstractEpiserverIoc.Core
{
    // https://github.com/moattarwork/foil/blob/master/src/Foil/ServiceCollectionExtensions.cs
    public class ServiceLocatorFactory : IServiceLocatorFactory
    {
        private readonly IServiceCollectionExtended _serviceCollection;
        private readonly Func<IServiceLocator> _serviceLocatorFactory;

        public ServiceLocatorFactory() : this(null, null) { }

        public ServiceLocatorFactory(Func<IServiceLocator> serviceLocatorFactory, IServiceCollectionExtended serviceDescriptors)
        {
            _serviceCollection = serviceDescriptors ?? new ExtendedServiceCollection();
            _serviceLocatorFactory = serviceLocatorFactory;
        }

        public static Func<IEnumerable<Assembly>> AssembliesProvider { get; set; } = () => AppDomain.CurrentDomain.GetAssemblies();

        public static IServiceLocator GetFromAssemblyAttribute(IEnumerable<Assembly> assemblies)
        {
            if (assemblies is null) { throw new ArgumentNullException(nameof(assemblies)); }
            var type = typeof(AbstractLocatorFactoryCreatorAttribute);
            var assemblyWithAttr = assemblies.FirstOrDefault(assembly => assembly.GetCustomAttribute(type) is object);
            if (assemblyWithAttr is null) { throw new NoAbstractFactoryAttributeException(); }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            var attrValue = assemblyWithAttr.GetCustomAttribute(type) as AbstractLocatorFactoryCreatorAttribute;
            var info = attrValue.CreatorType.GetMethod(attrValue.MethodName, flags);

            return info.Invoke(null, null) as IServiceLocator;
        }

        public IServiceLocator CreateLocator()
        {
            if (_serviceCollection is ExtendedServiceCollection es) { es.IsConfigured = true; }
            var locator = _serviceLocatorFactory?.Invoke() ?? GetFromAssemblyAttribute(AssembliesProvider?.Invoke());
            if (locator is null) { throw new UnableToCreateAbstractLocatorException(); }
            if (!(locator is IServiceLocatorWireupCollection serviceLocatorWireup)) { throw new UnableToWirewupServiceCollectionException(); }

            var ambientLocator = new AmbientServiceLocator(locator);
            (_serviceCollection as IServiceCollectionExtended).Add(new MicrosoftServiceDescriptor(typeof(IServiceLocator), ambientLocator));
            serviceLocatorWireup.WireupServices(_serviceCollection);

            return ambientLocator;
        }

        public IServiceConfigurationProvider CreateProvider() => new ServiceConfigurationProvider(_serviceCollection);
    }
}