using System;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Macrolevel;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class PreprocessorDirectiveTest
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
        public virtual void Test01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor05.p")), session);
            unit.Parse();
            Assert.AreEqual(0, unit.TopNode.Query(ABLNodeType.PROPARSEDIRECTIVE).Count);
            JPNode node1 = unit.TopNode.Query(ABLNodeType.MESSAGE)[0];
            JPNode node2 = unit.TopNode.Query(ABLNodeType.MESSAGE)[1];

            ProToken h1 = node1.HiddenBefore;
            int numDirectives = 0;
            while (h1 != null)
            {
                if (h1.Type == Proparse.PROPARSEDIRECTIVE)
                {
                    numDirectives += 1;
                }
                h1 = (ProToken)h1.HiddenBefore;
            }
            Assert.AreEqual(1, numDirectives);
            Assert.IsTrue(node1.HasProparseDirective("xyz"));
            Assert.IsFalse(node1.HasProparseDirective("abc"));

            numDirectives = 0;
            ProToken h2 = node2.HiddenBefore;
            while (h2 != null)
            {
                if (h2.Type == Proparse.PROPARSEDIRECTIVE)
                {
                    numDirectives += 1;
                }
                h2 = (ProToken)h2.HiddenBefore;
            }
            Assert.AreEqual(2, numDirectives);
            Assert.IsTrue(node2.HasProparseDirective("abc"));
            Assert.IsTrue(node2.HasProparseDirective("def"));
            Assert.IsTrue(node2.HasProparseDirective("hij"));
            Assert.IsFalse(node2.HasProparseDirective("klm"));
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void test02()
        [TestMethod]
        [Ignore()]
        public void Test02()
        {
            // See issue #341 - Won't fix
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor07.p")), session);
            unit.Parse();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void test03() throws IOException
        [TestMethod]
        public virtual void Test03()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor09.p")), session);
            ITokenSource stream = unit.Preprocess();

            Assert.AreEqual(Proparse.DEFINE, LexerTest.NextVisibleToken(stream).Type);
            Assert.AreEqual(Proparse.VARIABLE, LexerTest.NextVisibleToken(stream).Type);
            IToken tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("aaa", tok.Text);
            Assert.AreEqual(Proparse.AS, LexerTest.NextVisibleToken(stream).Type);
            Assert.AreEqual(Proparse.CHARACTER, LexerTest.NextVisibleToken(stream).Type);
            Assert.AreEqual(Proparse.PERIOD, LexerTest.NextVisibleToken(stream).Type);

            Assert.AreEqual(Proparse.MESSAGE, LexerTest.NextVisibleToken(stream).Type);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("\"text1 text2\"", tok.Text);
            Assert.AreEqual(Proparse.PERIOD, LexerTest.NextVisibleToken(stream).Type);

            Assert.AreEqual(Proparse.MESSAGE, LexerTest.NextVisibleToken(stream).Type);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("aaa", tok.Text);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("\"text3\"", tok.Text);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("aaa", tok.Text);
            Assert.AreEqual(Proparse.PERIOD, LexerTest.NextVisibleToken(stream).Type);

            Assert.AreEqual(Proparse.MESSAGE, LexerTest.NextVisibleToken(stream).Type);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("bbb", tok.Text);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("'text4'", tok.Text);
            tok = LexerTest.NextVisibleToken(stream);
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("bbb", tok.Text);
            Assert.AreEqual(Proparse.PERIOD, LexerTest.NextVisibleToken(stream).Type);
        }



        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void test04() throws IOException
        [TestMethod]
        public virtual void Test04()
        {
            ParseUnit unit01 = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("{ preprocessor/preprocessor10.i &myParam=1 }")), "<unnamed>", session);
            ITokenSource stream01 = unit01.Preprocess();
            Assert.AreEqual(Proparse.TRUE_KW, LexerTest.NextVisibleToken(stream01).Type);

            ParseUnit unit02 = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("{ preprocessor/preprocessor10.i &abc=1 &myParam }")), "<unnamed>", session);
            ITokenSource stream02 = unit02.Preprocess();
            Assert.AreEqual(Proparse.TRUE_KW, LexerTest.NextVisibleToken(stream02).Type);
            IncludeRef events02 = (IncludeRef)unit02.GetMacroSourceArray()[1];
            Assert.AreEqual(2, events02.NumArgs());
            Assert.AreEqual("abc", events02.GetArgNumber(1).Name);
            Assert.AreEqual("1", events02.GetArgNumber(1).Value);
            Assert.IsFalse(events02.GetArgNumber(1).Undefined);
            Assert.AreEqual("myParam", events02.GetArgNumber(2).Name);
            Assert.IsTrue(events02.GetArgNumber(2).Undefined);

            ParseUnit unit03 = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("{ preprocessor/preprocessor10.i &abc &myParam }")), "<unnamed>", session);
            ITokenSource stream03 = unit03.Preprocess();
            Assert.AreEqual(Proparse.TRUE_KW, LexerTest.NextVisibleToken(stream03).Type);

            ParseUnit unit04 = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("{ preprocessor/preprocessor10.i &myParam &abc }")), "<unnamed>", session);
            ITokenSource stream04 = unit04.Preprocess();
            // Different behavior in ABL
            Assert.AreEqual(Proparse.TRUE_KW, LexerTest.NextVisibleToken(stream04).Type);
            IncludeRef events04 = (IncludeRef)unit04.GetMacroSourceArray()[1];
            Assert.AreEqual(2, events04.NumArgs());
            Assert.AreEqual("myParam", events04.GetArgNumber(1).Name);
            Assert.IsTrue(events04.GetArgNumber(1).Undefined);
            Assert.AreEqual("abc", events04.GetArgNumber(2).Name);
            Assert.IsTrue(events04.GetArgNumber(2).Undefined);

            ParseUnit unit05 = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("{ preprocessor/preprocessor10.i &abc &myParam=1 }")), "<unnamed>", session);
            ITokenSource stream05 = unit05.Preprocess();
            Assert.AreEqual(Proparse.TRUE_KW, LexerTest.NextVisibleToken(stream05).Type);
            IncludeRef events05 = (IncludeRef)unit05.GetMacroSourceArray()[1];
            Assert.AreEqual(2, events05.NumArgs());
            Assert.AreEqual("abc", events05.GetArgNumber(1).Name);
            Assert.IsTrue(events05.GetArgNumber(1).Undefined);
            Assert.AreEqual("myParam", events05.GetArgNumber(2).Name);
            Assert.AreEqual("1", events05.GetArgNumber(2).Value);
            Assert.IsFalse(events05.GetArgNumber(2).Undefined);
        }



    }
}
