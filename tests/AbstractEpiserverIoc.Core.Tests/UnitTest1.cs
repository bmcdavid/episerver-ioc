using EPiServer.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace AbstractEpiserverIoc.Core.Tests
{
    public interface IFoo { }

    public class Foo : IFoo { }

    public class Foo2 : IFoo
    {
        public Foo2(IFoo foo)
        {

        }
    }

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var collection = new ExtendedServiceCollection();
            var sut = new ServiceConfigurationProvider(collection);
            var t = new DryIocServiceLocator(collection);

            sut.AddSingleton<IFoo, Foo>();
            //sut.Intercept<IFoo>((locator, existing) => new Foo2(existing));
            //var t = collection.TestInterceptor<IFoo>();

            Assert.IsTrue(t.GetInstance(typeof(IFoo)) is Foo);
        }
    }
}
