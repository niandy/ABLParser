using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ABLParser.Progress.Xref;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.NodeTypes;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.RCodeReader;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class JPNodeTest
    {
        private const string SRC_DIR = "Resources/jpnode";
        private const string TEMP_DIR = "Target/jpnode";

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
            pu.Parse();
            Assert.IsNotNull(pu.TopNode);

            StringWriter writer = new StringWriter();
            JsonNodeLister nodeLister = new JsonNodeLister(pu.TopNode, writer, ABLNodeType.LEFTPAREN, ABLNodeType.RIGHTPAREN, ABLNodeType.COMMA, ABLNodeType.PERIOD, ABLNodeType.LEXCOLON, ABLNodeType.OBJCOLON, ABLNodeType.THEN, ABLNodeType.END);
            nodeLister.Print();

            jsonNames.Add(file);
            jsonOut.Add(writer.ToString());

            return pu;
        }


        [TestMethod]
        public void TestStatements()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "query01.p")), session);
            unit.Parse();

            IList<JPNode> doStmts = unit.TopNode.QueryStateHead(ABLNodeType.DO);
            IList<JPNode> msgStmts = unit.TopNode.QueryStateHead(ABLNodeType.MESSAGE);
            Assert.AreEqual(2, doStmts.Count);
            Assert.AreEqual(3, msgStmts.Count);

            Assert.AreEqual(3, doStmts[0].Query(ABLNodeType.VIEWAS).Count);
            Assert.AreEqual(0, doStmts[0].QueryCurrentStatement(ABLNodeType.VIEWAS).Count);
            Assert.AreEqual(1, doStmts[1].Query(ABLNodeType.VIEWAS).Count);
            Assert.AreEqual(0, doStmts[1].QueryCurrentStatement(ABLNodeType.VIEWAS).Count);

            Assert.AreEqual(1, msgStmts[0].Query(ABLNodeType.VIEWAS).Count);
            Assert.AreEqual(1, msgStmts[1].Query(ABLNodeType.VIEWAS).Count);
            Assert.AreEqual(1, msgStmts[2].Query(ABLNodeType.VIEWAS).Count);
        }

        [TestMethod]
        public void TestDotComment01()
        {
            ParseUnit unit = GenericTest("dotcomment01.p");
            JPNode node = unit.TopNode.FirstNaturalChild;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.DOT_COMMENT, node.NodeType);
            // TODO Whitespaces should be kept...
            Assert.IsTrue(node.Text.StartsWith(".message"));
            Assert.AreEqual(1, node.Line);
            Assert.AreEqual(3, node.EndLine);
            Assert.AreEqual(1, node.Column);
            Assert.AreEqual(27, node.EndColumn);

            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.PERIOD, node.FirstChild.NodeType);
            Assert.AreEqual(3, node.FirstChild.Line);
            Assert.AreEqual(3, node.FirstChild.EndLine);
            Assert.AreEqual(29, node.FirstChild.Column);
            Assert.AreEqual(29, node.FirstChild.EndColumn);
        }

        [TestMethod]
        public virtual void TestDotComment02()
        {
            ParseUnit unit = GenericTest("dotcomment02.p");
            JPNode node = unit.TopNode.FirstNaturalChild;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.DOT_COMMENT, node.NodeType);
            Assert.AreEqual(".message", node.Text);

            Assert.AreEqual(1, node.Line);
            Assert.AreEqual(1, node.EndLine);
            Assert.AreEqual(1, node.Column);
            Assert.AreEqual(8, node.EndColumn);

            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.PERIOD, node.FirstChild.NodeType);
            Assert.AreEqual(1, node.FirstChild.Line);
            Assert.AreEqual(1, node.FirstChild.EndLine);
            Assert.AreEqual(9, node.FirstChild.Column);
            Assert.AreEqual(9, node.FirstChild.EndColumn);
        }

        [TestMethod]
        public virtual void TestComparison01()
        {
            ParseUnit unit = GenericTest("comparison01.p");
            JPNode node = unit.TopNode.FirstChild;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.EXPR_STATEMENT, node.NodeType);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.EQ, node.FirstChild.NodeType);
            Assert.IsNotNull(node.FirstChild.FirstChild);
            Assert.AreEqual(ABLNodeType.NUMBER, node.FirstChild.FirstChild.NodeType);
            Assert.IsNotNull(node.FirstChild.FirstChild.NextSibling);
            Assert.AreEqual(ABLNodeType.NUMBER, node.FirstChild.FirstChild.NextSibling.NodeType);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.EXPR_STATEMENT, node.NodeType);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.GTHAN, node.FirstChild.NodeType);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.EXPR_STATEMENT, node.NodeType);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.LTHAN, node.FirstChild.NodeType);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.EXPR_STATEMENT, node.NodeType);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.GE, node.FirstChild.NodeType);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.EXPR_STATEMENT, node.NodeType);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.LE, node.FirstChild.NodeType);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.EXPR_STATEMENT, node.NodeType);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.NE, node.FirstChild.NodeType);
        }

        [TestMethod]
        public virtual void TestFileName01()
        {
            ParseUnit unit = GenericTest("filename01.p");
            JPNode node = unit.TopNode.FirstChild;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.COMPILE, node.NodeType);

            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.FILENAME, node.FirstChild.NodeType);
            Assert.AreEqual("c:/foo/bar/something.p", node.FirstChild.Text);
            Assert.AreEqual(1, node.FirstChild.Line);
            Assert.AreEqual(1, node.FirstChild.EndLine);
            Assert.AreEqual(9, node.FirstChild.Column);
            Assert.AreEqual(30, node.FirstChild.EndColumn);

            Assert.IsNotNull(node.FirstChild.NextSibling);
            Assert.AreEqual(ABLNodeType.PERIOD, node.FirstChild.NextSibling.NodeType);

            JPNode node2 = node.NextSibling;
            Assert.AreEqual(ABLNodeType.INPUT, node2.NodeType);
            Assert.AreEqual(ABLNodeType.THROUGH, node2.FirstChild.NodeType);
            Assert.AreEqual(ABLNodeType.FILENAME, node2.FirstChild.NextSibling.NodeType);
            Assert.AreEqual("echo $$ $PATH c:/foobar/something.p", node2.FirstChild.NextSibling.Text);
            Assert.AreEqual(ABLNodeType.NOECHO, node2.FirstChild.NextSibling.NextSibling.NodeType);
            Assert.AreEqual(ABLNodeType.APPEND, node2.FirstChild.NextSibling.NextSibling.NextSibling.NodeType);
            Assert.AreEqual(ABLNodeType.KEEPMESSAGES, node2.FirstChild.NextSibling.NextSibling.NextSibling.NextSibling.NodeType);
        }

        [TestMethod]
        public virtual void TestAnnotation01()
        {
            ParseUnit unit = GenericTest("annotation01.p");
            JPNode node = unit.TopNode.FirstChild;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.ANNOTATION, node.NodeType);
            Assert.AreEqual("@MyAnnotation", node.Text);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.ANNOTATION, node.NodeType);
            Assert.AreEqual("@My.Super.Annotation", node.Text);

            node = node.NextSibling;
            Assert.IsNotNull(node);
            Assert.AreEqual(ABLNodeType.ANNOTATION, node.NodeType);
            Assert.AreEqual("@MyAnnotation", node.Text);
            Assert.IsNotNull(node.FirstChild);
            Assert.AreEqual(ABLNodeType.UNQUOTEDSTRING, node.FirstChild.NodeType);
            Assert.AreEqual("( xxx = \"yyy\", zz = \"abc\" )", node.FirstChild.Text);
        }

        [TestMethod]
        public virtual void TestDataType()
        {
            ParseUnit unit = GenericTest("datatype01.p");
            IList<JPNode> nodes = unit.TopNode.Query(ABLNodeType.RETURNS);
            Assert.AreEqual(12, nodes.Count);
            Assert.AreEqual(ABLNodeType.INTEGER, nodes[0].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.LOGICAL, nodes[1].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.ROWID, nodes[2].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.WIDGETHANDLE, nodes[3].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.CHARACTER, nodes[4].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.DATE, nodes[5].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.DECIMAL, nodes[6].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.INTEGER, nodes[7].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.INTEGER, nodes[8].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.RECID, nodes[9].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.ROWID, nodes[10].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.WIDGETHANDLE, nodes[11].NextNode.NodeType);


            IList<JPNode> nodes2 = unit.TopNode.Query(ABLNodeType.TO);
            Assert.AreEqual(3, nodes2.Count);
            Assert.AreEqual(ABLNodeType.CHARACTER, nodes2[0].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.INT64, nodes2[1].NextNode.NodeType);
            Assert.AreEqual(ABLNodeType.DOUBLE, nodes2[2].NextNode.NodeType);
        }


        [TestMethod]
        public virtual void TestEditing()
        {
            ParseUnit unit = GenericTest("editing01.p");
            IList<JPNode> nodes = unit.TopNode.Query(ABLNodeType.EDITING_PHRASE);
            Assert.AreEqual(2, nodes.Count);
            Assert.IsNotNull(nodes[0].FirstChild);
            Assert.AreEqual(ABLNodeType.EDITING, nodes[0].FirstChild.NodeType);
            Assert.IsNotNull(nodes[1].FirstChild);
            Assert.AreEqual(ABLNodeType.ID, nodes[1].FirstChild.NodeType);
            Assert.AreEqual("foobar", nodes[1].FirstChild.Text);
        }

        [TestMethod]
        public virtual void TestChoose()
        {
            ParseUnit unit = GenericTest("choose01.p");
            IList<JPNode> nodes = unit.TopNode.Query(ABLNodeType.CHOOSE);
            Assert.AreEqual(1, nodes.Count);
            Assert.IsNotNull(nodes[0].FirstChild);
            Assert.AreEqual(ABLNodeType.FIELD, nodes[0].FirstChild.NodeType);
        }

        [TestMethod]
        public virtual void TestFormatPhrase()
        {
            ParseUnit unit = GenericTest("formatphrase01.p");
            IList<JPNode> nodes = unit.TopNode.Query(ABLNodeType.MESSAGE);
            Assert.AreEqual(1, nodes.Count);
            Assert.IsNotNull(nodes[0].DirectChildren);
            Assert.AreEqual(4, nodes[0].DirectChildren.Count);
            Assert.AreEqual(ABLNodeType.FORM_ITEM, nodes[0].DirectChildren[0].NodeType);
            Assert.AreEqual(ABLNodeType.UPDATE, nodes[0].DirectChildren[1].NodeType);
            Assert.AreEqual(ABLNodeType.VIEWAS, nodes[0].DirectChildren[2].NodeType);
            Assert.AreEqual(ABLNodeType.PERIOD, nodes[0].DirectChildren[3].NodeType);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void testXref01() throws JAXBException, IOException, SAXException, ParserConfigurationException
        [TestMethod]
        public virtual void TestXref01()
        {
            ParseUnit unit = GenericTest("xref.p");
            unit.TreeParser01();

            XmlSerializer serializer = new XmlSerializer(typeof(Crossreference));

            Crossreference doc = (Crossreference)serializer.Deserialize(new FileStream(Path.Combine(SRC_DIR, "xref.p.xref"), FileMode.Open));
            unit.AttachXref(doc);

            IList<JPNode> nodes = unit.TopNode.Query(ABLNodeType.RECORD_NAME);
            Assert.AreEqual(5, nodes.Count);
            RecordNameNode warehouse = (RecordNameNode)nodes[0];
            RecordNameNode customer = (RecordNameNode)nodes[1];
            RecordNameNode item = (RecordNameNode)nodes[2];

            Assert.IsTrue(warehouse.WholeIndex);
            Assert.AreEqual("Warehouse.warehousenum", warehouse.SearchIndexName);

            Assert.IsFalse(customer.WholeIndex);
            Assert.AreEqual("Customer.CountryPost", customer.SearchIndexName);
            Assert.AreEqual("Address", customer.SortAccess);

            Assert.IsTrue(item.WholeIndex);
            Assert.AreEqual("Item.ItemNum", item.SearchIndexName);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void testXref02() throws JAXBException, IOException, SAXException, ParserConfigurationException
        [TestMethod]
        public virtual void TestXref02()
        {
            ParseUnit unit = GenericTest("xref2.cls");
            unit.TreeParser01();

            XmlSerializer serializer = new XmlSerializer(typeof(Crossreference));

            Crossreference doc = (Crossreference)serializer.Deserialize(new FileStream(Path.Combine(SRC_DIR, "xref2.cls.xref"), FileMode.Open));
            
            unit.AttachXref(doc);

            Assert.AreEqual(3, unit.TopNode.Query(ABLNodeType.RECORD_NAME).Count);
            foreach (JPNode node in unit.TopNode.Query(ABLNodeType.RECORD_NAME))
            {
                RecordNameNode rec = (RecordNameNode)node;
                Assert.AreEqual("ttFoo", rec.TableBuffer.Table.GetName());
                Assert.IsTrue(rec.WholeIndex);
            }
        }


    }
}
