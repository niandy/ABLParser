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
    public class LegacyTest
    {        
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();            
        }

		[TestMethod]
		public virtual void TestAppendProgram()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/appendprogram/t01/test/t01.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions

			ParseUnit pu2 = new ParseUnit(new FileInfo("Resources/legacy/appendprogram/t01/test/t01b.p"), session);
			pu2.TreeParser01();
			Assert.IsNotNull(pu2.TopNode);
			Assert.IsNotNull(pu2.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestBubbleProgram01()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/bubble/test/bubbledecs.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions

			ParseUnit pu2 = new ParseUnit(new FileInfo("Resources/legacy/bubble/test/test2.p"), session);
			pu2.TreeParser01();
			Assert.IsNotNull(pu2.TopNode);
			Assert.IsNotNull(pu2.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestBubbleProgram02()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/bubble/test2/bubb2.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestBubbleProgram03()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/bubble/x03_test/x03.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestBubbleProgram04()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/bubble/x04/test/x04.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestBubbleProgram05()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/bubble/x05/test/x05.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestExtractMethod()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/extractmethod/t01/test/t01a.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestNames()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/names/billto.p"), session);
			pu1.TreeParser01();
			Assert.IsNotNull(pu1.TopNode);
			Assert.IsNotNull(pu1.RootScope);
			// TODO Add assertions
			ParseUnit pu2 = new ParseUnit(new FileInfo("Resources/legacy/names/customer.p"), session);
			pu2.TreeParser01();
			Assert.IsNotNull(pu2.TopNode);
			Assert.IsNotNull(pu2.RootScope);
			// TODO Add assertions
			ParseUnit pu3 = new ParseUnit(new FileInfo("Resources/legacy/names/shipto.p"), session);
			pu3.TreeParser01();
			Assert.IsNotNull(pu3.TopNode);
			Assert.IsNotNull(pu3.RootScope);
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestQualifyFields()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01a.p"), session);
			ParseUnit pu2 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01b.p"), session);
			ParseUnit pu3 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01c.p"), session);
			ParseUnit pu4 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01d.p"), session);
			ParseUnit pu5 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01e.p"), session);
			ParseUnit pu6 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01f.p"), session);
			ParseUnit pu7 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01g.p"), session);
			ParseUnit pu8 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01h.p"), session);
			ParseUnit pu9 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01i.p"), session);
			ParseUnit pu10 = new ParseUnit(new FileInfo("Resources/legacy/qualifyfields/t01/test/t01j.p"), session);
			pu1.TreeParser01();
			pu2.TreeParser01();
			pu3.TreeParser01();
			pu4.TreeParser01();
			pu5.TreeParser01();
			pu6.TreeParser01();
			pu7.TreeParser01();
			pu8.TreeParser01();
			pu9.TreeParser01();
			pu10.TreeParser01();
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestAmbiguous()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/Sports2000/Customer/Name.cls"), session);
			pu1.TreeParser01();
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestWrapProcedure()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/wrapprocedure/t01/test/t01.p"), session);
			pu1.TreeParser01();
			// TODO Add assertions
		}

		[TestMethod]
		public virtual void TestBaseDir()
		{
			ParseUnit pu1 = new ParseUnit(new FileInfo("Resources/legacy/c-win.w"), session);
			ParseUnit pu3 = new ParseUnit(new FileInfo("Resources/legacy/empty.p"), session);
			ParseUnit pu4 = new ParseUnit(new FileInfo("Resources/legacy/hello2.p"), session);
			ParseUnit pu5 = new ParseUnit(new FileInfo("Resources/legacy/jpplus1match.p"), session);
			ParseUnit pu6 = new ParseUnit(new FileInfo("Resources/legacy/match.p"), session);
			ParseUnit pu7 = new ParseUnit(new FileInfo("Resources/legacy/names.p"), session);
			ParseUnit pu8 = new ParseUnit(new FileInfo("Resources/legacy/substitute.p"), session);
			ParseUnit pu9 = new ParseUnit(new FileInfo("Resources/legacy/tw2sample.p"), session);
			pu1.TreeParser01();
			pu3.TreeParser01();
			pu4.TreeParser01();
			pu5.TreeParser01();
			pu6.TreeParser01();
			pu7.TreeParser01();
			pu8.TreeParser01();
			pu9.TreeParser01();

			Assert.AreEqual(1, pu1.TopNode.Query(ABLNodeType.BGCOLOR).Count);
			Assert.IsNotNull(pu1.TopNode.Query(ABLNodeType.BGCOLOR)[0]);
			Assert.AreEqual("_CREATE-WINDOW", pu1.TopNode.Query(ABLNodeType.BGCOLOR)[0].AnalyzeSuspend);

			Assert.AreEqual(1, pu1.TopNode.Query(ABLNodeType.WAITFOR).Count);
			Assert.IsNotNull(pu1.TopNode.Query(ABLNodeType.WAITFOR)[0]);
			Assert.AreEqual("_UIB-CODE-BLOCK,_CUSTOM,_MAIN-BLOCK,C-Win", pu1.TopNode.Query(ABLNodeType.WAITFOR)[0].AnalyzeSuspend);
		}
	}
}
