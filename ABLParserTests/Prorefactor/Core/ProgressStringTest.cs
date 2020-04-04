using System;
using ABLParser.Prorefactor.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class ProgressStringTest
    {
        [TestMethod]
        public virtual void Test1()
        {
            ProgressString pstring = new ProgressString("\"No more 'Hello world'!\":T");
            Assert.AreEqual("No more 'Hello world'!", pstring.Text);
            Assert.AreEqual(":T", pstring.Attributes);
            Assert.AreEqual('\"', pstring.Quote);
        }

        [TestMethod]
        public virtual void Test2()
        {
            ProgressString pstring = new ProgressString("'No more \"Hello world\"!'");
            Assert.AreEqual("No more \"Hello world\"!", pstring.Text);
            Assert.AreEqual("", pstring.Attributes);
            Assert.AreEqual('\'', pstring.Quote);
        }

        [TestMethod]
        public virtual void Test3()
        {
            Assert.AreEqual("No more \"Hello world\"!", ProgressString.Dequote("No more \"Hello world\"!"));
            Assert.AreEqual("No more \"Hello world\"!", ProgressString.Dequote("'No more \"Hello world\"!'"));
            Assert.AreEqual("No more \"Hello world\"!", ProgressString.Dequote("\"No more \"Hello world\"!\""));
        }

    }
}
