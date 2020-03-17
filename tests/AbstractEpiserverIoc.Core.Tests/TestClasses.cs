using System.Collections.Generic;

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

    public abstract class BaseTest { protected BaseTest() { } }

    public class ConcreteTest : BaseTest { }

    [EPiServer.ServiceLocation.Options]
    public class Options
    {
        public bool IsSet { get; set; }

        public List<string> Strings { get; set; } = new List<string>();
    }

}