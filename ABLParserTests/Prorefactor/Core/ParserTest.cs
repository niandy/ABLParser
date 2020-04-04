using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.NodeTypes;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class ParserTest
    {
        private const string SRC_DIR = "Resources/data/parser";
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }


        [TestMethod]
        public virtual void TestAscending01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "ascending01.p")), session);
            unit.Parse();

            IList<JPNode> stmts = unit.TopNode.QueryStateHead(ABLNodeType.DEFINE);
            foreach (JPNode stmt in stmts)
            {
                Assert.AreEqual(0, stmt.Query(ABLNodeType.ASC).Count);
                Assert.AreEqual(1, stmt.Query(ABLNodeType.ASCENDING).Count);
            }
            Assert.IsTrue(stmts[0].Query(ABLNodeType.ASCENDING)[0].Abbreviated);
            Assert.IsTrue(stmts[1].Query(ABLNodeType.ASCENDING)[0].Abbreviated);
            Assert.IsFalse(stmts[2].Query(ABLNodeType.ASCENDING)[0].Abbreviated);
        }

        // SQL not recognized anymore
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](enabled=false) public void testAscending02()
        [TestMethod]
        [Ignore()]
        public void TestAscending02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "ascending02.p")), session);
            unit.Parse();

            IList<JPNode> stmts = unit.TopNode.QueryStateHead(ABLNodeType.SELECT);
            foreach (JPNode stmt in stmts)
            {
                Assert.AreEqual(0, stmt.Query(ABLNodeType.ASC).Count);
                Assert.AreEqual(1, stmt.Query(ABLNodeType.ASCENDING).Count);
            }
            Assert.IsTrue(stmts[0].Query(ABLNodeType.ASCENDING)[0].Abbreviated);
            Assert.IsTrue(stmts[1].Query(ABLNodeType.ASCENDING)[0].Abbreviated);
            Assert.IsFalse(stmts[2].Query(ABLNodeType.ASCENDING)[0].Abbreviated);
        }

        [TestMethod]
        public virtual void TestAscending03()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "ascending03.p")), session);
            unit.Parse();

            foreach (JPNode stmt in unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE))
            {
                Assert.AreEqual(2, stmt.Query(ABLNodeType.ASC).Count);
                Assert.AreEqual(0, stmt.Query(ABLNodeType.ASCENDING).Count);
                foreach (JPNode ascNode in stmt.Query(ABLNodeType.ASC))
                {
                    Assert.IsFalse(ascNode.Abbreviated);
                }
            }
        }

        [TestMethod]
        public virtual void TestLogical01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "logical01.p")), session);
            unit.Parse();

            IList<JPNode> stmts = unit.TopNode.QueryStateHead(ABLNodeType.DEFINE);
            foreach (JPNode stmt in stmts)
            {
                Assert.AreEqual(0, stmt.Query(ABLNodeType.LOG).Count);
                Assert.AreEqual(1, stmt.Query(ABLNodeType.LOGICAL).Count);
            }
            Assert.IsTrue(stmts[0].Query(ABLNodeType.LOGICAL)[0].Abbreviated);
            Assert.IsTrue(stmts[1].Query(ABLNodeType.LOGICAL)[0].Abbreviated);
            Assert.IsFalse(stmts[2].Query(ABLNodeType.LOGICAL)[0].Abbreviated);
            Assert.IsTrue(stmts[3].Query(ABLNodeType.LOGICAL)[0].Abbreviated);
            Assert.IsTrue(stmts[4].Query(ABLNodeType.LOGICAL)[0].Abbreviated);
            Assert.IsFalse(stmts[5].Query(ABLNodeType.LOGICAL)[0].Abbreviated);
        }

        [TestMethod]
        public virtual void TestLogical02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "logical02.p")), session);
            unit.Parse();

            IList<JPNode> stmts = unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE);
            Assert.AreEqual(1, stmts[0].Query(ABLNodeType.LOG).Count);
            Assert.AreEqual(0, stmts[0].Query(ABLNodeType.LOGICAL).Count);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void testObjectInDynamicFunction()
        [TestMethod]
        [Ignore()]
        public void TestObjectInDynamicFunction()
        {
            // Issue https://github.com/Riverside-Software/sonar-openedge/issues/673
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "objindynfunc.cls")), session);
            unit.Parse();

            Assert.AreEqual(3, unit.TopNode.Query(ABLNodeType.DYNAMICFUNCTION).Count);
        }


        [TestMethod]
        public virtual void TestGetCodepage()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "getcodepage.p")), session);
            unit.Parse();

            IList<JPNode> stmts = unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE);
            Assert.AreEqual(1, stmts[0].Query(ABLNodeType.GETCODEPAGE).Count);
            Assert.AreEqual(0, stmts[0].Query(ABLNodeType.GETCODEPAGES).Count);
            Assert.AreEqual(1, stmts[1].Query(ABLNodeType.GETCODEPAGE).Count);
            Assert.AreEqual(0, stmts[1].Query(ABLNodeType.GETCODEPAGES).Count);
            Assert.AreEqual(0, stmts[2].Query(ABLNodeType.GETCODEPAGE).Count);
            Assert.AreEqual(1, stmts[2].Query(ABLNodeType.GETCODEPAGES).Count);
            Assert.AreEqual(0, stmts[3].Query(ABLNodeType.GETCODEPAGE).Count);
            Assert.AreEqual(1, stmts[3].Query(ABLNodeType.GETCODEPAGES).Count);
        }

        [TestMethod]
        public virtual void TestConnectDatabase()
        {
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("connect database dialog box")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.CONNECT).Count);
        }

        [TestMethod]
        public virtual void TestReservedKeyword()
        {
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("define temp-table xxx field to-rowid as character.")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.DEFINE).Count);
        }

        [TestMethod]
        public virtual void TestInputFunction()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "inputfunc.p")), session);
            unit.Parse();
            Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.ON).Count);
            Assert.AreEqual(2, unit.TopNode.QueryStateHead(ABLNodeType.IF)[0].QueryStateHead().Count);
        }


        [TestMethod]
        public virtual void TestParameterLike()
        {
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("define input parameter ipPrm no-undo like customer.custnum.")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(1, unit.TopNode.QueryStateHead(ABLNodeType.DEFINE).Count);
            JPNode node = unit.TopNode.QueryStateHead(ABLNodeType.DEFINE)[0];
            Assert.AreEqual(1, node.Query(ABLNodeType.NOUNDO).Count);
        }

        [TestMethod]
        public virtual void TestAnnotation01()
        {
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("@Progress.Lang.Annotation. MESSAGE 'Hello1'. MESSAGE 'Hello2'.")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(2, unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE).Count);
        }

        [TestMethod]
        public virtual void TestAnnotation02()
        {
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("@Progress.Lang.Annotation(foo='bar'). MESSAGE 'Hello1'. MESSAGE 'Hello2'.")), "<unnamed>", session);
            unit.Parse();
            Assert.AreEqual(2, unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE).Count);
        }

        /// <summary>
        /// TODO Yes, should probably move to TreeParserTest.  
        /// </summary>
        [TestMethod]
        public virtual void TesTTIndex01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "ttindex01.p")), session);
            unit.TreeParser01();

            TableBuffer tt01 = unit.RootScope.LookupTempTable("tt01");
            Assert.IsNotNull(tt01);
            Assert.IsNotNull(tt01.Table);
            Assert.AreEqual(3, tt01.Table.Indexes.Count);

            TableBuffer tt02 = unit.RootScope.LookupTempTable("tt02");
            Assert.IsNotNull(tt02);
            Assert.IsNotNull(tt02.Table);
            Assert.AreEqual(5, tt02.Table.Indexes.Count);

            TableBuffer tt03 = unit.RootScope.LookupTempTable("tt03");
            Assert.IsNotNull(tt03);
            Assert.IsNotNull(tt03.Table);
            Assert.AreEqual(2, tt03.Table.Indexes.Count);

            TableBuffer tt04 = unit.RootScope.LookupTempTable("tt04");
            Assert.IsNotNull(tt04);
            Assert.IsNotNull(tt04.Table);
            Assert.AreEqual(2, tt04.Table.Indexes.Count);

            TableBuffer tt05 = unit.RootScope.LookupTempTable("tt05");
            Assert.IsNotNull(tt05);
            Assert.IsNotNull(tt05.Table);
            Assert.AreEqual(1, tt05.Table.Indexes.Count);

            TableBuffer tt06 = unit.RootScope.LookupTempTable("tt06");
            Assert.IsNotNull(tt06);
            Assert.IsNotNull(tt06.Table);
            Assert.AreEqual(3, tt06.Table.Indexes.Count);

            TableBuffer tt07 = unit.RootScope.LookupTempTable("tt07");
            Assert.IsNotNull(tt07);
            Assert.IsNotNull(tt07.Table);
            Assert.AreEqual(2, tt07.Table.Indexes.Count);

            TableBuffer tt08 = unit.RootScope.LookupTempTable("tt08");
            Assert.IsNotNull(tt08);
            Assert.IsNotNull(tt08.Table);
            Assert.AreEqual(2, tt08.Table.Indexes.Count);

            TableBuffer tt09 = unit.RootScope.LookupTempTable("tt09");
            Assert.IsNotNull(tt09);
            Assert.IsNotNull(tt09.Table);
            Assert.AreEqual(1, tt09.Table.Indexes.Count);
        }

        /// <summary>
        /// TODO Yes, should probably move to TreeParserTest.  
        /// </summary>
        [TestMethod]
        public virtual void TestRecordNameNode()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "recordName.p")), session);
            unit.TreeParser01();

            foreach (JPNode node in unit.TopNode.Query(ABLNodeType.RECORD_NAME))
            {
                RecordNameNode recNode = (RecordNameNode)node;
                string tbl = recNode.TableBuffer.TargetFullName;
                if (recNode.Line == 5)
                {
                    Assert.AreEqual("tt01", tbl);
                }
                if (recNode.Line == 6)
                {
                    Assert.AreEqual("sports2000.Customer", tbl);
                }
                if (recNode.Line == 8)
                {
                    Assert.AreEqual("sports2000.Customer", tbl);
                }
                if (recNode.Line == 9)
                {
                    Assert.AreEqual("tt01", tbl);
                }
                if (recNode.Line == 10)
                {
                    Assert.AreEqual("tt01", tbl);
                }
                if (recNode.Line == 11)
                {
                    Assert.AreEqual("sports2000.Customer", tbl);
                }
            }
        }
    }
}
