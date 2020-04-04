using System;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParserTests.Prorefactor.Core.Util;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Proparser.Antlr4
{
    [TestClass]
    public class NameDotTokenFilterTest
    {
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

		[TestMethod]
		public virtual void TestEmptyList()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			IToken tok = filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(TokenConstants.EOF, tok.Type);
			// Indefinitely returns EOF
			tok = filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(TokenConstants.EOF, tok.Type);
		}

		[TestMethod]
		public virtual void TestNoNameDot()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("message 'Hello'.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			IToken tok = filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.MESSAGE.Type, tok.Type);
			tok = filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS.Type, tok.Type);
			tok = filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.QSTRING.Type, tok.Type);
			tok = filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD.Type, tok.Type);
		}

		[TestMethod]
		public virtual void TestNameDot00()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes(".Lang.Object.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.NAMEDOT, tok.NodeType);
			Assert.AreEqual(".", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Lang.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestNameDot01()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("using Riverside.Lang.Object.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.USING, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Riverside.Lang.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.EOF_ANTLR4, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestNameDot02()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("using Progress.Lang.Object.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.USING, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			// Changed from ABLNodeType.PROGRESS to ABLNodeType.ID
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Progress.Lang.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.EOF_ANTLR4, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestNameDot03()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("using Riverside.20190101.Object.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.USING, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Riverside.20190101.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.EOF_ANTLR4, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestNameDot04()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("using Riverside./* Woot */ /* Woot woot */20190101.Object.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.USING, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
			Assert.AreEqual("Riverside.20190101.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.EOF_ANTLR4, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestAnnotation01()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("@Riverside.Lang.Object. MESSAGE 'foo'.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ANNOTATION, tok.NodeType);
			Assert.AreEqual("@Riverside.Lang.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);
		}

		[TestMethod]
		public virtual void TestAnnotation02()
		{
			ProgressLexer lexer = new ProgressLexer(session, new MemoryStream(Encoding.Default.GetBytes("@Riverside.20190101.Object. MESSAGE 'foo'.")), "file.txt");
			ITokenSource filter = new NameDotTokenFilter(lexer.TokenSource);

			ProToken tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.ANNOTATION, tok.NodeType);
			Assert.AreEqual("@Riverside.20190101.Object", tok.Text);
			tok = (ProToken)filter.NextToken();
			Assert.IsNotNull(tok);
			Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
		}

	}
}
