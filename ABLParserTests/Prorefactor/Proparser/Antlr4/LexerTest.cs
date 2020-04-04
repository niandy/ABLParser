using System;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Proparser.Antlr4
{
    [TestClass]
    public class LexerTest
    {
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

		[TestMethod]
		public virtual void TestTypeName()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("CLASS Riverside.20190101.Object")), "file.txt");

			ProToken tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.CLASS, tok.NodeType);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Riverside", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
			Assert.AreEqual(".20190101", tok.Text);
		}

		[TestMethod]
		public virtual void TestAnnotation01()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("@Riverside.Lang.Object. MESSAGE 'foo'.")), "file.txt");

			ProToken tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ANNOTATION, tok.NodeType);
			Assert.AreEqual("@Riverside", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.NAMEDOT, tok.NodeType);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Lang", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.NAMEDOT, tok.NodeType);
			Assert.AreEqual(".", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.OBJECT, tok.NodeType);
			Assert.AreEqual("Object", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestAnnotation02()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("@Riverside.20190101.Object. MESSAGE 'foo'.")), "file.txt");

			ProToken tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ANNOTATION, tok.NodeType);
			Assert.AreEqual("@Riverside", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
			Assert.AreEqual(".20190101", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.NAMEDOT, tok.NodeType);
			Assert.AreEqual(".", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.OBJECT, tok.NodeType);
			Assert.AreEqual("Object", tok.Text);
			tok = (ProToken)lexer.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
		}

	

	}
}
