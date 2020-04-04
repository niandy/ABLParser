using System;
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
    public class ApiTest
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
            FileInfo f = new FileInfo("Resources/data/hello.p");
            ParseUnit pu = new ParseUnit(f, session);
            pu.TreeParser01();
            Assert.AreEqual(pu.TopNode.Query(ABLNodeType.DISPLAY).Count, 1);
        }

        [TestMethod]
        public void TestMethod2()
        {
            FileInfo f = new FileInfo("Resources/data/no-undo.p");
            ParseUnit pu = new ParseUnit(f, session);
            pu.TreeParser01();
            JPNode node = pu.TopNode.FindDirectChild(ABLNodeType.DEFINE);
            Assert.AreEqual(ABLNodeType.VARIABLE.Type, node.AttrGet(IConstants.STATE2));
        }

        [TestMethod]
        public void TestMethod3()
        {
            FileInfo f = new FileInfo("Resources/data/include.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.TreeParser01();
            // Three include file (including main file)
            Assert.AreEqual(3, pu.GetMacroSourceArray().Length);
            // First is inc.i, at line 3
            Assert.AreEqual("inc.i", ((IncludeRef)pu.GetMacroSourceArray()[1]).FileRefName);
            Assert.AreEqual(4, ((IncludeRef)pu.GetMacroSourceArray()[1]).Position.Line);
            // Second is inc2.i, at line 2 (in inc.i)
            Assert.AreEqual("inc2.i", ((IncludeRef)pu.GetMacroSourceArray()[2]).FileRefName);
            Assert.AreEqual(2, ((IncludeRef)pu.GetMacroSourceArray()[2]).Position.Line);
        }

        [TestMethod]
        public void TestMethod4()
        {
            FileInfo f = new FileInfo("Resources/data/nowarn.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.Parse();

            // Looking for the DEFINE node
            JPNode node1 = (JPNode)pu.TopNode.FindDirectChild(ABLNodeType.DEFINE);
            Assert.IsNotNull(node1);
            Assert.IsTrue(node1.StateHead);

            // Looking for the NO-UNDO node, and trying to get the state-head node
            JPNode node2 = (JPNode)pu.TopNode.Query(ABLNodeType.NOUNDO)[0];
            JPNode parent = node2;
            while (!parent.StateHead)
            {
                parent = parent.PreviousNode;
            }
            Assert.AreEqual(node1, parent);

            // No proparse directive as nodes anymore
            JPNode left = node1.PreviousSibling;
            Assert.IsNull(left);

            // But as ProToken
            ProToken tok = node1.HiddenBefore;
            Assert.IsNotNull(tok);
            // First WS, then proparse directive
            tok = (ProToken)tok.HiddenBefore;
            Assert.IsNotNull(tok);
            Assert.AreEqual(tok.NodeType, ABLNodeType.PROPARSEDIRECTIVE);
            Assert.AreEqual(tok.Text, "prolint-nowarn(shared)");

            // First WS
            tok = (ProToken)tok.HiddenBefore;
            Assert.IsNotNull(tok);
            // Then previous directive
            tok = (ProToken)tok.HiddenBefore;
            Assert.IsNotNull(tok);
            Assert.AreEqual(tok.NodeType, ABLNodeType.PROPARSEDIRECTIVE);
            Assert.AreEqual(tok.Text, "prolint-nowarn(something)");
        }

        [TestMethod]
        public void TestMethod5()
        {
            FileInfo f = new FileInfo("Resources/data/bugsfixed/bug19.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.Parse();
            Assert.AreEqual("MESSAGE \"Hello\".", pu.TopNode.ToStringFulltext().Trim());
        }

        [TestMethod]
        public void TestMethod6()
        {
            FileInfo f = new FileInfo("Resources/data/abbrev.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.Parse();
            Assert.IsFalse(pu.TopNode.Query(ABLNodeType.LC)[0].Abbreviated);
            Assert.IsFalse(pu.TopNode.Query(ABLNodeType.LC)[0].Abbreviated);
            Assert.IsTrue(pu.TopNode.Query(ABLNodeType.FILEINFORMATION)[0].Abbreviated);
            Assert.IsFalse(pu.TopNode.Query(ABLNodeType.FILEINFORMATION)[1].Abbreviated);
            Assert.IsTrue(pu.TopNode.Query(ABLNodeType.SUBSTITUTE)[0].Abbreviated);
            Assert.IsFalse(pu.TopNode.Query(ABLNodeType.SUBSTITUTE)[1].Abbreviated);
        }

        [TestMethod]
        public void TestMethod7()
        {
            FileInfo f = new FileInfo("Resources/data/prepro.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.Parse();
            IncludeRef incRef = pu.MacroGraph;
            Assert.AreEqual(incRef.macroEventList.Count, 2);
            Assert.IsTrue(incRef.macroEventList[0] is MacroDef);
            Assert.IsTrue(incRef.macroEventList[1] is NamedMacroRef);
            NamedMacroRef nmr = (NamedMacroRef)incRef.macroEventList[1];
            Assert.AreEqual(nmr.MacroDef, incRef.macroEventList[0]);
        }

        [TestMethod]
        public void TestMethod8()
        {
            FileInfo f = new FileInfo("Resources/data/prepro2.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.Parse();
            IncludeRef incRef = pu.MacroGraph;
            Assert.AreEqual(incRef.macroEventList.Count, 3);
            Assert.IsTrue(incRef.macroEventList[0] is MacroDef);
            Assert.IsTrue(incRef.macroEventList[1] is NamedMacroRef);
            NamedMacroRef nmr = (NamedMacroRef)incRef.macroEventList[1];
            Assert.AreEqual(nmr.MacroDef, incRef.macroEventList[0]);
            IList<JPNode> nodes = pu.TopNode.Query(ABLNodeType.DEFINE);
            Assert.AreEqual(nodes.Count, 1);
            // Preprocessor magic... Keywords can start in main file, and end in include file...
            Assert.AreEqual(nodes[0].FileIndex, 0);
            Assert.AreEqual(nodes[0].EndFileIndex, 1);
            Assert.AreEqual(nodes[0].Line, 6);
            Assert.AreEqual(nodes[0].EndLine, 1);
            Assert.AreEqual(nodes[0].Column, 1);
            Assert.AreEqual(nodes[0].EndColumn, 3);
        }

        [TestMethod]
        public void TestMethod9()
        {
            FileInfo f = new FileInfo("Resources/data/prepro3.p");            
            ParseUnit pu = new ParseUnit(f, session);
            pu.Parse();
            IList<JPNode> nodes = pu.TopNode.Query(ABLNodeType.SUBSTITUTE);
            Assert.AreEqual(nodes.Count, 2);
            JPNode substNode = nodes[0];
            JPNode leftParen = substNode.NextNode;
            JPNode str = leftParen.NextNode;
            Assert.AreEqual(leftParen.Line, 2);
            Assert.AreEqual(leftParen.Column, 19);
            Assert.AreEqual(leftParen.EndLine, 2);
            Assert.AreEqual(leftParen.EndColumn, 19);
            Assert.AreEqual(str.Line, 2);
            Assert.AreEqual(str.Column, 20);
            Assert.AreEqual(str.EndLine, 2);
            Assert.AreEqual(str.EndColumn, 24);

            JPNode substNode2 = nodes[1];
            JPNode leftParen2 = substNode2.NextNode;
            JPNode str2 = leftParen2.NextNode;
            Assert.AreEqual(leftParen2.Line, 3);
            Assert.AreEqual(leftParen2.Column, 19);
            Assert.AreEqual(leftParen2.EndLine, 3);
            Assert.AreEqual(leftParen2.EndColumn, 19);
            Assert.AreEqual(str2.Line, 3);
            Assert.AreEqual(str2.Column, 20);
            Assert.AreEqual(str2.EndLine, 3);
            // FIXME Wrong value, should be 25
            Assert.AreEqual(str2.EndColumn, 20);

            IList<JPNode> dispNodes = pu.TopNode.Query(ABLNodeType.DISPLAY);
            Assert.AreEqual(dispNodes.Count, 1);
            JPNode dispNode = dispNodes[0];
            JPNode str3 = dispNode.NextNode.NextNode;
            Assert.AreEqual(str3.Line, 4);
            Assert.AreEqual(str3.EndLine, 4);
            Assert.AreEqual(str3.Column, 9);
            // FIXME Wrong value, should be 14
            Assert.AreEqual(str3.EndColumn, 9);
        }
    }
}
