using DryIoc;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.AutoDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryIocEpi
{
    public class DryIocLocatorFactory : IServiceLocatorFactory
    {
        private IContainer _container;

        public DryIocLocatorFactory()
        {
            var rules = Rules.Default
                .With(propertiesAndFields: InjectedProperties)
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                //.WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .WithTrackingDisposableTransients()
                ; //used in transient delegate cases

            _container = new Container(rules);
        }

        public DryIocLocatorFactory(IContainer container)
        {
            _container = container
                .With(rules => rules
                    .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                    .With(propertiesAndFields: InjectedProperties));
        }

        public static IEnumerable<PropertyOrFieldServiceInfo> InjectedProperties(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static;

            var props = type
                .GetProperties(flags)
                .Where(p => p.CanWrite && GetInjected(p.PropertyType));

            var all = type
                .GetFields(flags)
                .Where(field => GetInjected(field.FieldType) &&
                    !field.Name.EndsWith("__BackingField"))
                .Cast<MemberInfo>()
                .Union(props)
                .Select(PropertyOrFieldServiceInfo.Of)
                .ToList();

            return all;
        }

        public IServiceLocator CreateLocator() => new DryIocServiceLocator(_container);

        public IServiceConfigurationProvider CreateProvider() =>
            new DryIocServiceConfigurationProvider(_container);

        private static bool GetInjected(Type fieldInfo)
        {
            if (!fieldInfo.IsGenericType) { return false; }

            var genericInfo = fieldInfo.GetGenericTypeDefinition();
            if (genericInfo == typeof(Injected<>)) { return true; }

            return genericInfo == typeof(InjectedCollection<>);
        }

        private static IEnumerable<PropertyOrFieldServiceInfo> InjectedProperties(Request request) =>
            InjectedProperties(request.ImplementationType ?? request.ServiceType);
    }
}
