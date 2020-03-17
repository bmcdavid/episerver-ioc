using EPiServer.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;

namespace AbstractEpiserverIoc.Core.Tests
{
    public interface IFoo { }

    public class Foo : IFoo { }

    public class Foo2 : IFoo
    {
        public Foo2(IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }

    public class Foo3 : IFoo
    {
        public Foo3(IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }

    public class Options
    {
        public bool IsSet { get; set; }

        public List<string> Strings { get; set; } = new List<string>();
    }

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ServiceLocatorTests
    {
        [DataRow("dryioc")]
        [DataRow("grace")]
        [TestMethod]
        public void ShouldDecorateServiceFoo(string key)
        {
            var collection = new ExtendedServiceCollection();
            var factory = new ServiceLocatorFactory(() => TestServiceLocatorFactory.CreateServiceLocator(key), collection);
            var serviceProviderRegistration = factory.CreateProvider();

            collection.AddSingleton<IFoo, Foo>();
            collection.Decorate<IFoo>(existing => new Foo2(existing));
            collection.Decorate<IFoo>(existing => new Foo3(existing));
            var locator = factory.CreateLocator();
            Assert.IsFalse(collection.Count != 2, key);

            var sut = locator.GetInstance<IFoo>();
            Assert.IsTrue(sut is Foo3 fo && fo.Foo is Foo2 f2 && f2.Foo is Foo, key);
            Assert.AreSame(sut, locator.GetInstance<IFoo>(), key);
        }

        [DataRow("dryioc")]
        [DataRow("grace")]
        [TestMethod]
        public void ShouldConfigureOptions(string key)
        {
            var collection = new ExtendedServiceCollection();
            var factory = new ServiceLocatorFactory(() => TestServiceLocatorFactory.CreateServiceLocator(key), collection);
            var serviceProviderRegistration = factory.CreateProvider();

            serviceProviderRegistration.AddSingleton<Options>();
            Assert.IsTrue(collection.Count == 1);

            serviceProviderRegistration.Configure2<Options>(opt =>
            {
                opt.IsSet = true;
                opt.Strings.Add("Hello");
            });

            var locator = factory.CreateLocator();

            Assert.IsTrue(locator.GetInstance<Options>().IsSet);
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