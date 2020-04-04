using System;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Refactor.Settings;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Antlr4.Runtime.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class ParserRecoverTest
    {
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

        [TestMethod]
        public virtual void Test01()
        {
            ((ProparseSettings)session.ProparseSettings).AntlrRecover = true;
            ((ProparseSettings)session.ProparseSettings).AntlrTokenInsertion = true;
            ((ProparseSettings)session.ProparseSettings).AntlrTokenDeletion = true;
            // Everything should be fine here
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("define variable xyz as character no-undo.")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.DEFINE).Count);
        }

        [TestMethod]
        public virtual void Test02()
        {
            ((ProparseSettings)session.ProparseSettings).AntlrRecover = true;
            ((ProparseSettings)session.ProparseSettings).AntlrTokenInsertion = true;
            ((ProparseSettings)session.ProparseSettings).AntlrTokenDeletion = true;
            // Doesn't compile but recover is on, so should be silently discarded
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("define variable xyz character no-undo.")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(0, unit.TopNode.QueryStateHead(ABLNodeType.DEFINE).Count);
            Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.PERIOD).Count);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](expectedExceptions = ParseCancellationException.class) public void test03()
        [TestMethod]
        [ExpectedException(typeof(ParseCanceledException), "Has to fail here")]
        public void Test03()
        {
            ((ProparseSettings)session.ProparseSettings).AntlrRecover = false;
            ((ProparseSettings)session.ProparseSettings).AntlrTokenInsertion = true;
            ((ProparseSettings)session.ProparseSettings).AntlrTokenDeletion = true;
            // Doesn't compile and recover is off, so should throw ParseCancellationException
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("define variable xyz character no-undo.")), "<unnamed>", session);
            unit.Parse();
        }
    }
}
