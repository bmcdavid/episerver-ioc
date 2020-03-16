using EPiServer.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

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

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ServiceLocatorTests
    {
        [TestMethod]
        public void ShouldDecorateServiceFoo()
        {
            var collection = new ExtendedServiceCollection();
            var factory = new ServiceLocatorFactory(TestServiceLocatorFactory.CreateServiceLocator, collection);
            var serviceProviderRegistration = factory.CreateProvider();

            collection.AddSingleton<IFoo, Foo>();
            collection.Decorate<IFoo>(existing => new Foo2(existing));
            collection.Decorate<IFoo>(existing => new Foo3(existing));

            var locator = factory.CreateLocator();
            var sut = locator.GetInstance<IFoo>();

            Assert.IsTrue(sut is Foo3 fo && fo.Foo is Foo2 f2 && f2.Foo is Foo);
            Assert.AreSame(sut, locator.GetInstance<IFoo>());
            Assert.IsTrue(collection.Count == 2);
        }
    }
}