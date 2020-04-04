using System;
using System.IO;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class ClassesTest
    {
        private RefactorSession session;        

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

        [TestMethod]
        public void TestMethod1()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/rssw/pct/LoadLogger.cls"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Assert.IsTrue(unit.TopNode.Query(ABLNodeType.ANNOTATION).Count == 1);
            Assert.AreEqual("Progress.Lang.Deprecated", unit.TopNode.Query(ABLNodeType.ANNOTATION)[0].AnnotationName);
        }

        [TestMethod]
        public void TestMethod2()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/rssw/pct/ScopeTest.cls"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);

            // Only zz and zz2 properties should be there
            var zz = unit.RootScope.GetVariable("zz");
            var zz2 = unit.RootScope.GetVariable("zz2");
            Assert.AreEqual(unit.RootScope.Variables.Count, 2);
            Assert.IsNotNull(zz, "Property zz not in root scope");
            Assert.IsNotNull(zz2, "Property zz2 not in root scope");

            foreach (TreeParserSymbolScope sc in unit.RootScope.ChildScopesDeep)
            {
                if (sc.RootBlock.Node.Type == Proparse.METHOD)
                {
                    continue;
                }
                if (sc.RootBlock.Node.Type == Proparse.CATCH)
                {
                    continue;
                }
                var arg = sc.GetVariable("arg");
                var i = sc.GetVariable("i");
                Assert.AreEqual(sc.Variables.Count, 2);
                Assert.IsNotNull(arg, "Property var not in GET/SET scope");
                Assert.IsNotNull(i, "Property i not in GET/SET scope");
            }
        }

        [TestMethod]
        public void TestThisObject()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/rssw/pct/TestThisObject.cls"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);

            var prop1 = unit.RootScope.GetVariable("prop1");
            var prop2 = unit.RootScope.GetVariable("prop2");
            var var1 = unit.RootScope.GetVariable("var1");
            Assert.IsNotNull(prop1);
            Assert.IsNotNull(prop2);
            Assert.IsNotNull(var1);
            Assert.AreEqual(prop1.NumReads, 1);
            Assert.AreEqual(prop1.NumWrites, 1);
            Assert.AreEqual(prop2.NumReads, 1);
            Assert.AreEqual(prop2.NumWrites, 1);
            Assert.AreEqual(var1.NumReads, 0);
            Assert.AreEqual(var1.NumWrites, 1);
        }
    }
}
