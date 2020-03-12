using AbstractEpiserverIoc.Core.Exceptions;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.AutoDiscovery;
using System;
using System.Linq;
using System.Reflection;

[assembly: ServiceLocatorFactory(typeof(AbstractEpiserverIoc.Core.ServiceLocatorFactory))]

namespace AbstractEpiserverIoc.Core
{
    // https://github.com/moattarwork/foil/blob/master/src/Foil/ServiceCollectionExtensions.cs
    public class ServiceLocatorFactory : IServiceLocatorFactory
    {
        private readonly ExtendedServiceCollection _serviceCollection;
        private readonly BuildServiceLocator _buildServiceLocator;

        public ServiceLocatorFactory() : this(null) { }

        public ServiceLocatorFactory(BuildServiceLocator buildServiceLocator)
        {
            _serviceCollection = new ExtendedServiceCollection();
            _buildServiceLocator = buildServiceLocator;
        }

        public IServiceLocator CreateLocator()
        {
            _serviceCollection.IsConfigured = true;
            var locator = _buildServiceLocator?.Invoke(_serviceCollection) ?? GetFromAssemblyAttribute();

            if (locator is null) { throw new UnableToCreateAbstractLocatorException(); }

            // todo: swap locator with new one....

            return locator;
        }

        public IServiceConfigurationProvider CreateProvider() => new ServiceConfigurationProvider(_serviceCollection);

        private IServiceLocator GetFromAssemblyAttribute()
        {
            var type = typeof(AbstractLocatorFactoryCreatorAttribute);
            var assemblyWithAttr = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass => ass.GetCustomAttribute(type) is object);
            if (assemblyWithAttr is null) { throw new NoAbstractFactoryAttributeException(); }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            var attrValue = assemblyWithAttr.GetCustomAttribute(type) as AbstractLocatorFactoryCreatorAttribute;
            var info = attrValue.CreatorType.GetMethod(attrValue.MethodName, flags);

            return info.Invoke(null, new object[] { _serviceCollection }) as IServiceLocator;
        }
    }
}