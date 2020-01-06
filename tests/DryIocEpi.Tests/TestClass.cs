using EPiServer.ServiceLocation;

namespace DryIocEpi.Tests
{
    public class TestClass
    {
        private static Injected<IFoo> _staticField;
        public static Injected<IFoo> StaticField;

        private Injected<IFoo> _privateField;
        private Injected<IFoo> ProtectedField;

        protected Injected<IFoo> ProtectedProperty { get; set; }

        public Injected<IFoo> PublicPropertyFoo { get; set; }
    }
}
