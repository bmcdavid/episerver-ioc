using DryIoc;
using EPiServer.ServiceLocation;
using EPiServer.ServiceLocation.AutoDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//[assembly: ServiceLocatorFactory(typeof(DryIocEpi.DryIocLocatorFactory))]

namespace DryIocEpi
{
    public class DryIocLocatorFactory : IServiceLocatorFactory
    {
        private static readonly Dictionary<Type, List<PropertyOrFieldServiceInfo>> _props = new Dictionary<Type, List<PropertyOrFieldServiceInfo>>();
        private readonly IContainer _container;

        public DryIocLocatorFactory()
        {
            var rules = Rules.Default 
                .WithAutoConcreteTypeResolution()
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .WithTrackingDisposableTransients()
                .WithCaptureContainerDisposeStackTrace()
                //.With(propertiesAndFields: InjectedProperties)
                .WithFuncAndLazyWithoutRegistration()

                ; //used in transient delegate cases

            _container = new Container(rules);
        }

        public DryIocLocatorFactory(IContainer container) => _container = container;

        public IServiceLocator CreateLocator()
        {
            var dsl = new DryIocServiceLocator(_container);
            _container.
                UseInstance(typeof(IServiceLocator), dsl, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            return dsl;
        }

        public IServiceConfigurationProvider CreateProvider() => new DryIocServiceConfigurationProvider(_container);

        private static bool GetInjected(Type fieldInfo)
        {
            if (!fieldInfo.IsGenericType) { return false; }

            var genericInfo = fieldInfo.GetGenericTypeDefinition();
            if (genericInfo == typeof(Injected<>)) { return true; }

            return genericInfo == typeof(InjectedCollection<>);
        }

        private static IEnumerable<PropertyOrFieldServiceInfo> InjectedProperties(Type type)
        {
            if (_props.TryGetValue(type, out var list)) { return list; }

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

            _props[type] = all;// todo: dupe key

            return all;
        }
        private static IEnumerable<PropertyOrFieldServiceInfo> InjectedProperties(Request request) =>
            InjectedProperties(request.ImplementationType ?? request.ServiceType);
    }
}