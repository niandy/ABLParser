using System;
using System.IO;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class MetricsTest
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
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/include.p"), session);
            unit.Parse();

            Assert.AreEqual(2, unit.Metrics.Loc);
            Assert.AreEqual(6, unit.Metrics.Comments);
        }

        [TestMethod]
        public virtual void Test02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/inc3.i"), session);
            unit.LexAndGenerateMetrics();

            Assert.AreEqual(1, unit.Metrics.Loc);
            Assert.AreEqual(2, unit.Metrics.Comments);
        }
    }
}
