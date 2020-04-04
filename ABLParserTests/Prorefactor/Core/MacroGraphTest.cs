using System.Collections.Generic;
using System.IO;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Macrolevel;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class MacroGraphTest
    {
        private const string SRC_DIR = "Resources/data/preprocessor";

        private RefactorSession session;
        private ParseUnit unit;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
            unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor02.p")), session);
            unit.Parse();
        }

        private void TestVariable(JPNode topNode, string variable)
        {
            foreach (JPNode node in topNode.Query(ABLNodeType.ID))
            {
                if (node.Text.Equals(variable))
                {
                    return;
                }
            }
            Assert.Fail("Variable " + variable + " not found");
        }

        private void TestNoVariable(JPNode topNode, string variable)
        {
            foreach (JPNode node in topNode.Query(ABLNodeType.ID))
            {
                if (node.Text.Equals(variable))
                {
                    Assert.Fail("Variable " + variable + " not found");
                }
            }
        }

        [TestMethod]
        public virtual void TestGlobalDefine()
        {
            TestVariable(unit.TopNode, "var1");
            TestVariable(unit.TopNode, "var2");
            TestNoVariable(unit.TopNode, "var3");
        }

        [TestMethod]
        public virtual void TestScopedDefine()
        {
            TestNoVariable(unit.TopNode, "var4");
        }

        [TestMethod]
        public virtual void TestMacroGraph()
        {
            Assert.AreEqual(3, unit.MacroGraph.FindExternalMacroReferences().Count);
            Assert.AreEqual(1, unit.MacroGraph.FindExternalMacroReferences(new int[] { 1, 1 }, new int[] { 5, 1 }).Count);
            Assert.AreEqual(0, unit.MacroGraph.FindExternalMacroReferences(new int[] { 1, 1 }, new int[] { 2, 1 }).Count);
            Assert.AreEqual(1, unit.MacroGraph.FindIncludeReferences(0).Count);
            Assert.AreEqual(1, unit.MacroGraph.FindIncludeReferences(1).Count);
            Assert.AreEqual(1, unit.MacroGraph.FindIncludeReferences(2).Count);
            Assert.AreEqual(1, unit.MacroGraph.FindIncludeReferences(3).Count);
            Assert.AreEqual(0, unit.MacroGraph.FindIncludeReferences(4).Count);
            Assert.AreEqual(null, unit.MacroGraph.FindIncludeReferences(2)[0].GetArgNumber(0));
            Assert.AreEqual("123", unit.MacroGraph.FindIncludeReferences(2)[0].GetArgNumber(1).Value);
            Assert.AreEqual("456", unit.MacroGraph.FindIncludeReferences(2)[0].GetArgNumber(2).Value);
            Assert.AreEqual(null, unit.MacroGraph.FindIncludeReferences(2)[0].GetArgNumber(3));
        }


        [TestMethod]
        public virtual void TestMacroGraphPosition()
        {
            IList<MacroEvent> list = unit.MacroGraph.FindExternalMacroReferences();
            Assert.AreEqual(3, list[0].Position.Line);
            Assert.AreEqual(5, list[0].Position.Column);
            Assert.AreEqual(0, list[0].Position.FileNum);
        }

    }
}
