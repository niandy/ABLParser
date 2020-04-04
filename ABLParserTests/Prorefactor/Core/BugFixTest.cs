using System;
using System.Collections.Generic;
using System.IO;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.RCodeReader;
using ABLParserTests.Prorefactor.Core.Util;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class BugFixTest
    {
        private const string SRC_DIR = "Resources/data/bugsfixed";
        private const string TEMP_DIR = "Target/nodes-lister/data/bugsfixed";


        private RefactorSession session;

        private readonly IList<string> jsonOut = new List<string>();
        private readonly IList<string> jsonNames = new List<string>();   

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();

            session.Schema.CreateAlias("foo", "sports2000");
            session.InjectTypeInfo(
                new RCodeInfo(new BinaryReader(new FileStream("Resources/data/rssw/pct/ParentClass.r", FileMode.Open))).TypeInfo);
            session.InjectTypeInfo(
                new RCodeInfo(new BinaryReader(new FileStream("Resources/data/rssw/pct/ChildClass.r", FileMode.Open))).TypeInfo);
            session.InjectTypeInfo(
                new RCodeInfo(new BinaryReader(new FileStream("Resources/data/ttClass.r", FileMode.Open))).TypeInfo);
            session.InjectTypeInfo(
                new RCodeInfo(new BinaryReader(new FileStream("Resources/data/ProtectedTT.r", FileMode.Open))).TypeInfo);

            Directory.CreateDirectory(TEMP_DIR);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            StreamWriter writer = new StreamWriter(Path.Combine(TEMP_DIR, "index.html"));
            writer.WriteLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><link rel=\"stylesheet\" type=\"text/css\" href=\"http://riverside-software.fr/d3-style.css\" />");
            writer.WriteLine("<script src=\"http://riverside-software.fr/jquery-1.10.2.min.js\"></script><script src=\"http://riverside-software.fr/d3.v3.min.js\"></script>");
            writer.WriteLine("<script>var data= { \"files\": [");
            int zz = 1;
            foreach (string str in jsonNames)
            {
                if (zz > 1)
                {
                    writer.Write(',');
                }
                writer.Write("{ \"file\": \"" + str + "\", \"var\": \"json" + zz++ + "\" }");
            }
            writer.WriteLine("]};");
            zz = 1;
            foreach (string str in jsonOut)
            {
                writer.WriteLine("var json" + zz++ + " = " + str + ";");
            }
            writer.WriteLine("</script></head><body><div id=\"wrapper\"><div id=\"left\"></div><div id=\"tree-container\"></div></div>");
            writer.WriteLine("<script src=\"http://riverside-software.fr/dndTreeDebug.js\"></script></body></html>");
            writer.Close();
        }

        private ParseUnit GenericTest(string file)
        {
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, file)), session);
            Assert.IsNull(pu.TopNode);
            Assert.IsNull(pu.RootScope);
            pu.Parse();
            pu.TreeParser01();
            Assert.IsNotNull(pu.TopNode);
            Assert.IsNotNull(pu.RootScope);

            StringWriter writer = new StringWriter();
            JsonNodeLister nodeLister = new JsonNodeLister(pu.TopNode, writer, ABLNodeType.LEFTPAREN, ABLNodeType.RIGHTPAREN, ABLNodeType.COMMA, ABLNodeType.PERIOD, ABLNodeType.LEXCOLON, ABLNodeType.OBJCOLON, ABLNodeType.THEN, ABLNodeType.END);
            nodeLister.Print();

            jsonNames.Add(file);
            jsonOut.Add(writer.ToString());

            return pu;
        }

        private ITokenSource GenericLex(string file)
        {
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, file)), session);
            Assert.IsNull(pu.TopNode);
            Assert.IsNull(pu.Metrics);
            Assert.IsNull(pu.RootScope);
            pu.LexAndGenerateMetrics();
            Assert.IsNotNull(pu.Metrics);
            return pu.Lex();
        }

        [TestMethod]
        public virtual void TestVarUsage()
        {
            ParseUnit unit = GenericTest("varusage.cls");
            Assert.AreEqual(2, unit.RootScope.GetVariable("x1").NumWrites);
            Assert.AreEqual(1, unit.RootScope.GetVariable("x1").NumReads);
            Assert.AreEqual(1, unit.RootScope.GetVariable("x2").NumWrites);
            Assert.AreEqual(1, unit.RootScope.GetVariable("x2").NumReads);
            Assert.AreEqual(1, unit.RootScope.GetVariable("x3").NumWrites);
            Assert.AreEqual(0, unit.RootScope.GetVariable("x3").NumReads);
            Assert.AreEqual(1, unit.RootScope.GetVariable("x4").NumReads);
            Assert.AreEqual(0, unit.RootScope.GetVariable("x4").NumWrites);

            Assert.AreEqual(1, unit.RootScope.GetVariable("lProcedure1").NumReads);
            Assert.AreEqual(0, unit.RootScope.GetVariable("lProcedure1").NumWrites);
            Assert.AreEqual(1, unit.RootScope.GetVariable("lProcedure2").NumReads);
            Assert.AreEqual(0, unit.RootScope.GetVariable("lProcedure2").NumWrites);
            Assert.AreEqual(1, unit.RootScope.GetVariable("lApsv").NumReads);
            Assert.AreEqual(0, unit.RootScope.GetVariable("lApsv").NumWrites);
            Assert.AreEqual(0, unit.RootScope.GetVariable("lRun").NumReads);
            Assert.AreEqual(1, unit.RootScope.GetVariable("lRun").NumWrites);
        }

        [TestMethod]
        public virtual void Test01()
        {
            GenericTest("bug01.p");
        }

        [TestMethod]
        public virtual void Test02()
        {
            GenericTest("bug02.p");
        }

        [TestMethod]
        public virtual void Test03()
        {
            GenericTest("bug03.p");
        }

        [TestMethod]
        public virtual void Test04()
        {
            GenericTest("bug04.p");
        }

        [TestMethod]
        public virtual void Test05()
        {
            GenericTest("bug05.p");
        }

        [TestMethod]
        public virtual void Test06()
        {
            GenericTest("bug06.p");
        }

        [TestMethod]
        public virtual void Test07()
        {
            GenericTest("interface07.cls");
        }

        [TestMethod]
        public virtual void Test08()
        {
            GenericTest("bug08.cls");
        }

        [TestMethod]
        public virtual void Test09()
        {
            GenericTest("bug09.p");
        }

        [TestMethod]
        public virtual void Test10()
        {
            GenericTest("bug10.p");
        }

        [TestMethod]
        public virtual void Test11()
        {
            GenericTest("bug11.p");
        }

        [TestMethod]
        public virtual void Test12()
        {
            GenericTest("bug12.p");
        }

        [TestMethod]
        public virtual void Test13()
        {
            GenericTest("bug13.p");
        }

        [TestMethod]
        public virtual void Test14()
        {
            GenericTest("bug14.p");
        }

        [TestMethod]
        public virtual void Test15()
        {
            GenericTest("bug15.p");
        }

        [TestMethod]
        public virtual void Test16()
        {
            GenericTest("bug16.p");
        }

        [TestMethod]
        public virtual void Test17()
        {
            GenericTest("bug17.p");
        }

        [TestMethod]
        public virtual void Test18()
        {
            GenericTest("bug18.p");
        }

        [TestMethod]
        public virtual void Test19()
        {
            GenericTest("bug19.p");
        }

        [TestMethod]
        public virtual void Test20()
        {
            GenericTest("bug20.p");
        }

        [TestMethod]
        public virtual void Test21()
        {
            GenericTest("bug21.cls");
        }

        [TestMethod]
        public virtual void Test22()
        {
            GenericTest("bug22.cls");
        }

        [TestMethod]
        public virtual void Test23()
        {
            GenericTest("bug23.cls");
        }

        [TestMethod]
        public virtual void Test24()
        {
            GenericTest("bug24.p");
        }

        [TestMethod]
        public virtual void Test25()
        {
            GenericTest("bug25.p");
        }

        [TestMethod]
        public virtual void Test26()
        {
            GenericTest("bug26.cls");
        }

        [TestMethod]
        public virtual void Test27()
        {
            GenericTest("bug27.cls");
        }

        [TestMethod]
        public virtual void Test28()
        {
            GenericTest("bug28.cls");
        }

        [TestMethod]
        public virtual void Test29()
        {
            GenericTest("bug29.p");
        }

        [TestMethod]
        public virtual void Test30()
        {
            GenericTest("bug30.p");
        }

        [TestMethod]
        public virtual void Test31()
        {
            GenericTest("bug31.cls");
        }

        [TestMethod]
        public virtual void Test32()
        {
            GenericLex("bug32.i");
        }

        [TestMethod]
        public virtual void Test33()
        {
            GenericTest("bug33.cls");
        }

        [TestMethod]
        public virtual void Test34()
        {
            GenericTest("bug34.p");
        }

        [TestMethod]
        public virtual void Test35()
        {
            GenericTest("bug35.p");
        }

        [TestMethod]
        public virtual void Test36()
        {
            GenericTest("bug36.p");
        }

        [TestMethod]
        public virtual void Test41()
        {
            GenericTest("bug41.cls");
        }

        [TestMethod]
        public virtual void Test43()
        {
            GenericTest("bug43.p");
        }

        [TestMethod]
        public virtual void Test44()
        {
            ParseUnit unit = GenericTest("bug44.cls");
            Assert.AreEqual(6, unit.TopNode.QueryStateHead().Count);
        }

        [TestMethod]
        public virtual void Test45()
        {
            ParseUnit unit = GenericTest("bug45.p");
            Assert.AreEqual(5, unit.TopNode.QueryStateHead().Count);
        }

        [TestMethod]
        public virtual void Test46()
        {
            ParseUnit unit = GenericTest("bug46.p");
            Assert.AreEqual(1, unit.TopNode.QueryStateHead().Count);
        }


        [TestMethod]
        public virtual void Test47()
        {
            ParseUnit unit = GenericTest("bug47.cls");
            Assert.AreEqual(2, unit.TopNode.QueryStateHead().Count);
        }

        // Next two tests : same exception should be thrown in both cases
        //  @Test(expectedExceptions = {ProparseRuntimeException.class})
        //  public void TestCache1() {
        //    genericTest("CacheChild.cls");
        //  }
        //
        //  @Test(expectedExceptions = {ProparseRuntimeException.class})
        //  public void TestCache2() {
        //    genericTest("CacheChild.cls");
        //  }

        [TestMethod]
        public virtual void TestSerializableKeyword()
        {
            GenericTest("serialkw.cls");
        }

        [TestMethod]
        public virtual void TestXor()
        {
            GenericTest("xor.p");
        }

        [TestMethod]
        public virtual void TestSaxWriter()
        {
            GenericTest("sax-writer.p");
        }

        [TestMethod]
        public virtual void TestNoBox()
        {
            GenericTest("nobox.p");
        }

        [TestMethod]
        public virtual void TestOnStatement()
        {
            GenericTest("on_statement.p");
        }

        [TestMethod]
        public virtual void TestIncludeInComment()
        {
            GenericTest("include_comment.p");
        }

        [TestMethod]
        public virtual void TestCreateComObject()
        {
            ParseUnit unit = GenericTest("createComObject.p");
            IList<JPNode> list = unit.TopNode.Query(ABLNodeType.CREATE);
            // COM automation
            Assert.AreEqual(3, list[0].Line);
            Assert.AreEqual(Proparse.Automationobject, list[0].State2);
            Assert.AreEqual(4, list[1].Line);
            Assert.AreEqual(Proparse.Automationobject, list[1].State2);
            // Widgets
            Assert.AreEqual(8, list[2].Line);
            Assert.AreEqual(Proparse.WIDGET, list[2].State2);
            Assert.AreEqual(12, list[3].Line);
            Assert.AreEqual(Proparse.WIDGET, list[3].State2);
            // Ambiguous
            Assert.AreEqual(15, list[4].Line);
            Assert.AreEqual(Proparse.WIDGET, list[4].State2);
        }

        [TestMethod]
        public virtual void TestCopyLob()
        {
            GenericTest("copylob.p");
        }

        [TestMethod]
        public virtual void TestOsCreate()
        {
            GenericTest("oscreate.p");
        }

        [TestMethod]
        public virtual void TestOnEvent()
        {
            GenericTest("onEvent.p");
        }

        [TestMethod]
        public virtual void TestGetDbClient()
        {
            GenericTest("getdbclient.p");
        }

        [TestMethod]
        public virtual void TestDoubleColon()
        {
            GenericTest("double-colon.p");
        }

        [TestMethod]
        public virtual void TestDynProp()
        {
            GenericTest("dynprop.p");
        }

        [TestMethod]
        public virtual void TestTildeInComment()
        {
            ITokenSource stream = GenericLex("comment-tilde.p");
            IToken tok = stream.NextToken();
            Assert.AreEqual(Proparse.COMMENT, tok.Type);
            Assert.AreEqual("// \"~n\"", tok.Text);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.DEFINE, stream.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTildeInComment2()
        {
            ITokenSource stream = GenericLex("comment-tilde2.p");
            Assert.AreEqual(Proparse.DEFINE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            IToken tok = stream.NextToken();
            Assert.AreEqual(Proparse.COMMENT, tok.Type);
            Assert.AreEqual("// \"~n\"", tok.Text);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](enabled = false, description = "Issue #309, won't fix,") public void TestAbstractKw()
        [TestMethod]
        [Ignore()]
        [Description("Issue #309, won't fix,")] 
        public void TestAbstractKw()
        {
            GenericTest("abstractkw.p");
        }

        [TestMethod]
        public virtual void TestNoArgFunc()
        {
            ParseUnit pu = GenericTest("noargfunc.p");
            IList<JPNode> nodes = pu.TopNode.Query(ABLNodeType.MESSAGE);
            Assert.AreEqual(ABLNodeType.GUID, nodes[0].FirstChild.FirstChild.NodeType);
            Assert.AreEqual(ABLNodeType.FIELD_REF, nodes[1].FirstChild.FirstChild.NodeType);
            Assert.AreEqual(ABLNodeType.TIMEZONE, nodes[2].FirstChild.FirstChild.NodeType);
            Assert.AreEqual(ABLNodeType.FIELD_REF, nodes[3].FirstChild.FirstChild.NodeType);
            Assert.AreEqual(ABLNodeType.MTIME, nodes[4].FirstChild.FirstChild.NodeType);
            Assert.AreEqual(ABLNodeType.FIELD_REF, nodes[5].FirstChild.FirstChild.NodeType);
        }

        [TestMethod]
        public virtual void TestLexer01()
        {
            //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            //ORIGINAL LINE: @SuppressWarnings("unused") ITokenSource stream = genericLex("lex.p");
            _ = GenericLex("lex.p");
        }

        [TestMethod]
        public virtual void TestDataset()
        {
            GenericTest("DatasetParentFields.p");
        }

        [TestMethod]
        public virtual void TestExtentFunction()
        {
            GenericTest("testextent1.cls");
            GenericTest("testextent2.p");
        }

        [TestMethod]
        public virtual void TestTTLikeDB01()
        {
            GenericTest("ttlikedb01.p");
        }

        [TestMethod]
        public virtual void TestStopAfter()
        {
            GenericTest("stopafter.p");
        }

        [TestMethod]
        public virtual void TestDefined()
        {
            // https://github.com/Riverside-Software/sonar-openedge/issues/515
            GenericTest("defined.p");
        }

        [TestMethod]
        public virtual void TestTTLikeDB02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/bugsfixed/ttlikedb02.p"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);

            // First FIND statement
            JPNode node = unit.TopNode.QueryStateHead(ABLNodeType.FIND)[0];
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Query(ABLNodeType.RECORD_NAME).Count);
            object obj = node.Query(ABLNodeType.RECORD_NAME)[0].Symbol;
            Assert.IsNotNull(obj);
            Assert.AreEqual(IConstants.ST_DBTABLE, ((TableBuffer)obj).Table.Storetype);

            // Second FIND statement
            node = unit.TopNode.QueryStateHead(ABLNodeType.FIND)[1];
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Query(ABLNodeType.RECORD_NAME).Count);
            obj = node.Query(ABLNodeType.RECORD_NAME)[0].Symbol;
            Assert.IsNotNull(obj);
            Assert.AreEqual(IConstants.ST_TTABLE, ((TableBuffer)obj).Table.Storetype);

            // Third FIND statement
            node = unit.TopNode.QueryStateHead(ABLNodeType.FIND)[2];
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Query(ABLNodeType.RECORD_NAME).Count);
            obj = node.Query(ABLNodeType.RECORD_NAME)[0].Symbol;
            Assert.IsNotNull(obj);
            Assert.AreEqual(IConstants.ST_DBTABLE, ((TableBuffer)obj).Table.Storetype);

            // Fourth FIND statement
            node = unit.TopNode.QueryStateHead(ABLNodeType.FIND)[3];
            Assert.IsNotNull(node);
            Assert.AreEqual(1, node.Query(ABLNodeType.RECORD_NAME).Count);
            obj = node.Query(ABLNodeType.RECORD_NAME)[0].Symbol;
            Assert.IsNotNull(obj);
            Assert.AreEqual(IConstants.ST_TTABLE, ((TableBuffer)obj).Table.Storetype);
        }

        [TestMethod]
        public virtual void TestRCodeStructure()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/rssw/pct/ChildClass.cls"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
        }

        [TestMethod]
        public virtual void TestProtectedTTAndBuffers()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/ProtectedTT.cls"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
        }
        [TestMethod]
        public virtual void TestAscendingFunction()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/bugsfixed/ascending.p"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);

            // Message statement
            JPNode node = unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE)[0];
            Assert.IsNotNull(node);
            Assert.AreEqual(0, node.Query(ABLNodeType.ASCENDING).Count);
            Assert.AreEqual(1, node.Query(ABLNodeType.ASC).Count);

            // Define TT statement
            JPNode node2 = unit.TopNode.QueryStateHead(ABLNodeType.DEFINE)[0];
            Assert.IsNotNull(node2);
            Assert.AreEqual(1, node2.Query(ABLNodeType.ASCENDING).Count);
            Assert.AreEqual(0, node2.Query(ABLNodeType.ASC).Count);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](enabled = false, description = "Issue #356, won't fix,") public void TestDefineMenu()
        [TestMethod]
        [Ignore()]
        [Description("Issue #356, won't fix,")]
        public void TestDefineMenu()
        {
            GenericTest("definemenu.p");
        }

        [TestMethod]
        public virtual void TestOptionsField()
        {
            GenericTest("options_field.p");
        }

        [TestMethod]
        public virtual void TestTooManyStatements()
        {
            // Verifies that lots of statements (5000 here) don't raise a stack overflow exception
            GenericTest("tooManyStatements.p");
        }

    }
}
