using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace AbstractEpiserverIoc.Core.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ServiceLocatorTests
    {
        [TestMethod]
        public void ShouldConfigureOptions()
        {
            PrepareTest(out var collection, out var factory, out var serviceProviderRegistration);

            serviceProviderRegistration.AddSingleton<Options>();
            serviceProviderRegistration.Configure<Options>(opt =>
            {
                opt.IsSet = true;
                opt.Strings.Add("Hello");
            });

            var locator = factory.CreateLocator();

            Assert.IsTrue(locator.GetInstance<IServiceLocator>() is object);
            Assert.IsTrue(locator.GetInstance<Options>().Strings.Count == 1, TestServiceLocatorFactory.LocatorName);
            Assert.AreSame(locator.GetInstance<Options>(), locator.GetInstance<Options>(), TestServiceLocatorFactory.LocatorName);
        }

        [TestMethod]
        public void ShouldForwardService()
        {
            PrepareTest(out var collection, out var factory, out var epiProvider);

            epiProvider.AddTransient<ConcreteTest>();
            epiProvider.Forward<ConcreteTest, IForwardTest>();
            var locator = factory.CreateLocator();

            Assert.IsTrue(locator.GetInstance<IForwardTest>() is object);
        }

        [TestMethod]
        public void ShouldDecorateServiceFoo()
        {
            PrepareTest(out var collection, out var factory, out var _);

            collection.AddSingleton<IFoo, Foo>();
            collection.Decorate<IFoo>(existing => new Foo2(existing));
            collection.Decorate<IFoo>(existing => new Foo3(existing));
            var locator = factory.CreateLocator();

            var sut = locator.GetInstance<IFoo>();
            Assert.IsTrue(sut is Foo3 fo && fo.Foo is Foo2 f2 && f2.Foo is Foo, TestServiceLocatorFactory.LocatorName);
            Assert.AreSame(sut, locator.GetInstance<IFoo>(), TestServiceLocatorFactory.LocatorName);
        }

        private static void PrepareTest(out ExtendedServiceCollection collection, out ServiceLocatorFactory factory, out IServiceConfigurationProvider serviceProviderRegistration)
        {
            collection = new ExtendedServiceCollection();
            factory = new ServiceLocatorFactory(() => TestServiceLocatorFactory.CreateServiceLocator(), collection);
            serviceProviderRegistration = factory.CreateProvider();
        }
    }

    internal static class Ext
    {
        public static IServiceConfigurationProvider Configure2<TService>(this IServiceConfigurationProvider services, Action<TService> configure) where TService : class
        {
            return services.Intercept<TService>((l, s) =>
            {
                configure(s);
                return s;
            });
        }
    }
}