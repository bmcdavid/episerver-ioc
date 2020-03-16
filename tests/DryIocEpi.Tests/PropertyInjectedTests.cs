using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DryIocEpi.Tests
{
    [TestClass]
    public class PropertyInjectedTests
    {
        [DataRow(typeof(TestClass))]
        [TestMethod]
        public void ShouldMergeInjectedFields(Type checkType)
        {
            //var sut = DryIocLocatorFactory.InjectedProperties(checkType);

            //Assert.IsTrue(sut.Count() == 6);
        }
    }
}