using System;
using System.IO;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class PreprocessorVariablesTest
    {
        private const string SRC_DIR = "Resources/data/preprocessor";
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

		[TestMethod]
		public virtual void Test03()
		{
			ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor03.p")), session);
			unit.Parse();
			TestVariable(unit.TopNode, "var01");
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
		public virtual void Test04()
		{
			ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor04.p")), session);
			unit.Parse();
			TestVariable(unit.TopNode, "var01");
			TestNoVariable(unit.TopNode, "var02");
			TestVariable(unit.TopNode, "var03");
			TestVariable(unit.TopNode, "var04");
			TestNoVariable(unit.TopNode, "var05");
			TestVariable(unit.TopNode, "var06");
			TestNoVariable(unit.TopNode, "var07");
			TestVariable(unit.TopNode, "var08");
			TestVariable(unit.TopNode, "var09");
			TestVariable(unit.TopNode, "var10");
		}

		[TestMethod]
		public virtual void Test06()
		{
			ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor06.p")), session);
			unit.Parse();
			Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE).Count);
		}

		[TestMethod]
		public virtual void Test08()
		{
			ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor08.p")), session);
			unit.Parse();
			Assert.AreEqual(0, unit.TopNode.QueryStateHead(ABLNodeType.DEFINE).Count);
		}

	}
}
