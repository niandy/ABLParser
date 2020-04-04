using System;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Refactor.Settings;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class LexerTest
    {
        private const string SRC_DIR = "Resources/data/lexer";

        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

        [TestMethod]
        public virtual void TestTokenList01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist01.p")), session);
            ITokenSource src = unit.Lex();

            // CURRENT-WINDOW:HANDLE.
            Assert.AreEqual(Proparse.CURRENTWINDOW, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.HANDLE, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // SESSION:FIRST-SERVER-SOCKET:HANDLE.
            Assert.AreEqual(Proparse.SESSION, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.HANDLE, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // TEMP-TABLE tt1::fld1.
            Assert.AreEqual(Proparse.TEMPTABLE, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.DOUBLECOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // DATASET ds1::tt1.
            Assert.AreEqual(Proparse.DATASET, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.DOUBLECOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // DATASET ds1::tt1:set-callback().
            Assert.AreEqual(Proparse.DATASET, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.DOUBLECOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.LEFTPAREN, src.NextToken().Type);
            Assert.AreEqual(Proparse.RIGHTPAREN, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist02.p")), session);
            ITokenSource src = unit.Lex();

            // Progress.Security.PAMStatus:AccessDenied.
            ProToken tok = (ProToken)src.NextToken();
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("Progress.Security.PAMStatus", tok.Text);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(27, tok.EndCharPositionInLine);
            Assert.AreEqual(0, tok.Channel);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // Progress.Security.PAMStatus :AccessDenied.
            tok = (ProToken)src.NextToken();
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("Progress.Security.PAMStatus", tok.Text);
            Assert.AreEqual(2, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(2, tok.EndLine);
            Assert.AreEqual(27, tok.EndCharPositionInLine);
            Assert.AreEqual(0, tok.Channel);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // Progress.Security.PAMStatus <bazinga> :AccessDenied.
            tok = (ProToken)src.NextToken();
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("Progress.Security.PAMStatus", tok.Text);
            Assert.AreEqual(3, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(3, tok.EndLine);
            Assert.AreEqual(27, tok.EndCharPositionInLine);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            tok = (ProToken)src.NextToken();
            Assert.AreEqual(Proparse.COMMENT, tok.Type);
            Assert.AreEqual("//Test", tok.Text);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            tok = (ProToken)src.NextToken();
            Assert.AreEqual(Proparse.COMMENT, tok.Type);
            Assert.AreEqual("//Test2", tok.Text);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // Progress.117x.clsName:StaticProperty.
            tok = (ProToken)src.NextToken();
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("Progress.117x.clsName", tok.Text);
            Assert.AreEqual(7, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(7, tok.EndLine);
            Assert.AreEqual(21, tok.EndCharPositionInLine);
            Assert.AreEqual(0, tok.Channel);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](enabled = false) public void testTokenList03()
        [TestMethod]
        [Ignore()]
        public void TestTokenList03()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist03.p")), session);
            ITokenSource src = unit.Lex();

            // MESSAGE Progress./* Holy shit */   Security.PAMStatus:AccessDenied.
            // The compiler accepts that...
            Assert.AreEqual(Proparse.MESSAGE, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            IToken tok = src.NextToken();
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("Progress.Security.PAMStatus", tok.Text);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList04()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist04.p")), session);
            ITokenSource src = unit.Lex();

            // .Security.PAMStatus:AccessDenied.
            // Nothing recognized here, so we don't change the stream 
            Assert.AreEqual(Proparse.NAMEDOT, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList05()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist05.p")), session);
            ITokenSource src = unit.Lex();

            // MESSAGE customer.custnum Progress.Security.PAMStatus:AccessDenied.
            Assert.AreEqual(Proparse.MESSAGE, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);

            // MESSAGE customer.custnum. Progress.Security.PAMStatus:AccessDenied.
            Assert.AreEqual(Proparse.MESSAGE, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList06()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist06.p")), session);
            ITokenSource src = unit.Lex();

            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList07()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist07.p")), session);
            ITokenSource src = unit.Lex();

            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.ID, src.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList08()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist08.p")), session);
            ITokenSource src = unit.Lex();
            Assert.AreEqual(Proparse.COMMENT, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.OBJCOLON, src.NextToken().Type);
            Assert.AreEqual(Proparse.FILE, src.NextToken().Type);
            Assert.AreEqual(Proparse.WS, src.NextToken().Type);
            Assert.AreEqual(Proparse.PLUS, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestTokenList09()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist09.p")), session);
            ITokenSource stream = unit.Lex();
            // First line
            IToken tok1 = stream.NextToken();
            Assert.AreEqual(Proparse.ID, tok1.Type);
            Assert.AreEqual("customer.name", tok1.Text);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            // Second line
            Assert.AreEqual(Proparse.ID, stream.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.ID, stream.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            // Third line: comment after period results in NAMEDOT
            IToken tok2 = stream.NextToken();
            Assert.AreEqual(Proparse.ID, tok2.Type);
            Assert.AreEqual("customer.name", tok2.Text);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            // Fourth line: same behaviour even if there's a space after the comment
            IToken tok3 = stream.NextToken();
            Assert.AreEqual(Proparse.ID, tok3.Type);
            Assert.AreEqual("customer.name", tok3.Text);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            // Fifth line: this line doesn't compile...
            Assert.AreEqual(Proparse.MESSAGE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.QSTRING, stream.NextToken().Type);
            Assert.AreEqual(Proparse.NAMEDOT, stream.NextToken().Type);
            Assert.AreEqual(Proparse.COMMENT, stream.NextToken().Type);
            Assert.AreEqual(Proparse.MESSAGE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.QSTRING, stream.NextToken().Type);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            // Sixth line: same behaviour even if there's a space after the comment
            IToken tok4 = stream.NextToken();
            Assert.AreEqual(Proparse.ID, tok4.Type);
            Assert.AreEqual("customer.name", tok4.Text);
            Assert.AreEqual(Proparse.PERIOD, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestPostLexer01Init()
        {
            // First time verifying the channel locations
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "postlexer01.p")), session);
            ITokenSource src = unit.Preprocess();
            // Whitespaces on hidden channel
            IToken tok = src.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            Assert.AreEqual(1, tok.Channel);
            // Then scoped-define on a different channel again
            tok = src.NextToken();
            Assert.AreEqual(Proparse.AMPSCOPEDDEFINE, tok.Type);
            Assert.AreEqual(2, tok.Channel);
            // Whitespace again
            tok = src.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            Assert.AreEqual(1, tok.Channel);
            // And again...
            tok = src.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            Assert.AreEqual(1, tok.Channel);
            // Then the string
            tok = src.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("\"zz\"", tok.Text);
        }

        [TestMethod]
        public virtual void TestPostLexer01()
        {
            // First time verifying the channel locations
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "postlexer01.p")), session);
            ITokenSource src = unit.Preprocess();
            // Whitespaces on hidden channel
            IToken tok = NextVisibleToken(src);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("\"zz\"", tok.Text);
        }

        [TestMethod]
        public virtual void TestPostLexer02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "postlexer02.p")), session);
            ITokenSource src = unit.Preprocess();
            IToken tok = NextVisibleToken(src);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("\"yy\"", tok.Text);
        }

        [TestMethod]
        public virtual void TestPostLexer03()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "postlexer03.p")), session);
            ITokenSource src = unit.Preprocess();
            IToken tok = NextVisibleToken(src);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual("\"zz\"", tok.Text);
        }

        [TestMethod]
        public virtual void TestPostLexer04()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "postlexer04.p")), session);
            ITokenSource src = unit.Preprocess();
            IToken tok = NextVisibleToken(src);
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            // The best we can do right now... This is to cover edge cases in preprocessing...
            Assert.AreEqual("\"a'aabb'bxxx~\nyyy\"", tok.Text);
        }

        [TestMethod]
        public virtual void TestEndOfFile()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "tokenlist01.p")), session);
            ITokenSource src = unit.Lex();

            while (src.NextToken().Type != TokenConstants.EOF)
            {

            }
            for (int zz = 0; zz < 1000; zz++)
            {
                // Verify safety net is not triggered
                src.NextToken();
            }
            // Make sure NextToken() always return EOF (and no null element or any exception)
            Assert.AreEqual(TokenConstants.EOF, src.NextToken().Type);
            Assert.AreEqual(TokenConstants.EOF, src.NextToken().Type);
        }

        [TestMethod]
        public virtual void TestAnalyzeSuspend()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer05.p")), session);
            ITokenSource src = unit.Lex();

            ProToken tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsFalse(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsFalse(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsFalse(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsFalse(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);
            tok = NextToken(src, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);

            ParseUnit unit2 = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer05.p")), session);
            unit2.Parse();
            Assert.IsFalse(unit2.IsInEditableSection(0, 9));
            Assert.IsFalse(unit2.IsInEditableSection(0, 18));
            Assert.IsTrue(unit2.IsInEditableSection(0, 28));
        }

        [TestMethod]
        public virtual void TestPreproErrorMessages01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer06.p")), session);
            try
            {
                ITokenSource src = unit.Preprocess();
                while (src.NextToken().Type != TokenConstants.EOF)
                {

                }
            }
            catch (ProparseRuntimeException caught)
            {
                Assert.IsTrue(caught.Message.Replace('\\', '/').StartsWith("File '" + Path.GetFullPath(SRC_DIR + "/lexer06.p").Replace('\\', '/') + "'"));
                Assert.IsTrue(caught.Message.EndsWith("Unexpected &THEN"));
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Unwanted exception...");
            }
            Assert.Fail("No exception found");
        }

        [TestMethod]
        public virtual void TestPreproErrorMessages02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer07.p")), session);
            try
            {
                ITokenSource src = unit.Preprocess();
                while (src.NextToken().Type != TokenConstants.EOF)
                {

                }
            }
            catch (ProparseRuntimeException caught)
            {
                Assert.IsTrue(caught.Message.Replace('\\', '/').StartsWith("File '" + Path.GetFullPath(SRC_DIR + "/lexer07.p").Replace('\\', '/') + "'"));
                Assert.IsTrue(caught.Message.EndsWith("Unexpected end of input after &IF or &ELSEIF"));
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Unwanted exception...");
            }
            Assert.Fail("No exception found");
        }

        [TestMethod]
        public virtual void TestPreproErrorMessages03()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer08.p")), session);
            try
            {
                ITokenSource src = unit.Preprocess();
                while (src.NextToken().Type != TokenConstants.EOF)
                {

                }
            }
            catch (ProparseRuntimeException caught)
            {
                Assert.IsTrue(caught.Message.Replace('\\', '/').StartsWith("File '" + Path.GetFullPath(SRC_DIR + "/lexer08.p").Replace('\\', '/') + "'"));
                Assert.IsTrue(caught.Message.EndsWith("Unexpected end of input when consuming discarded &IF/&ELSEIF/&ELSE text"));
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Unwanted exception...");
            }
            Assert.Fail("No exception found");
        }


        [TestMethod]
        public virtual void TestPreproErrorMessages04()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer09.p")), session);
            try
            {
                ITokenSource src = unit.Preprocess();
                while (src.NextToken().Type != TokenConstants.EOF)
                {

                }
            }
            catch (ProparseRuntimeException caught)
            {
                string msg = caught.Message.Replace('\\', '/');
                Assert.IsTrue(msg.StartsWith("File '" + Path.GetFullPath(SRC_DIR + "/lexer09.p").Replace('\\', '/') + "' - Current position '", StringComparison.Ordinal), msg);
                Assert.IsTrue(msg.EndsWith("Resources/data/lexer/lexer09.i':2 - Unexpected &THEN", StringComparison.Ordinal), msg);
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Unwanted exception...");
            }
            Assert.Fail("No exception found");
        }

        [TestMethod]
        public virtual void TestAnalyzeSuspendIncludeFile()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer10.p")), session);
            ITokenSource stream = unit.Preprocess();

            // First MESSAGE in main file
            ProToken tok = NextToken(stream, ABLNodeType.MESSAGE);
            Assert.IsNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);

            // First MESSAGE in first include file
            tok = NextToken(stream, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsFalse(string.IsNullOrEmpty(tok.AnalyzeSuspend));
            Assert.IsTrue(tok.EditableInAB);

            // Second MESSAGE in first include file
            tok = NextToken(stream, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsFalse(string.IsNullOrEmpty(tok.AnalyzeSuspend));
            Assert.IsFalse(tok.EditableInAB);

            // MESSAGE in second include file
            tok = NextToken(stream, ABLNodeType.MESSAGE);
            Assert.IsNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);

            // Back to first include file
            tok = NextToken(stream, ABLNodeType.MESSAGE);
            Assert.IsNotNull(tok.AnalyzeSuspend);
            Assert.IsTrue(string.IsNullOrEmpty(tok.AnalyzeSuspend));
            Assert.IsFalse(tok.EditableInAB);

            // Back to main file
            tok = NextToken(stream, ABLNodeType.MESSAGE);
            Assert.IsNull(tok.AnalyzeSuspend);
            Assert.IsTrue(tok.EditableInAB);
        }

        [TestMethod]
        public virtual void TestQuotedStringPosition()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer11.p")), session);
            ITokenSource stream = unit.Lex();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.DO, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(2, tok.EndCharPositionInLine);

            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WHILE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.ID, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.RIGHTANGLE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            // Quoted string
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(15, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            // The important test here, end column has to be 16 even when followed by ':'
            Assert.AreEqual(16, tok.EndCharPositionInLine);

            // Colon
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.LEXCOLON, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(17, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(17, tok.EndCharPositionInLine);
        }

        [TestMethod]
        public virtual void TestQuotedStringPosition2()
        {
            // Same as previous test, but with a space before the colon
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer11-2.p")), session);
            ITokenSource stream = unit.Lex();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.DO, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(2, tok.EndCharPositionInLine);

            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WHILE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.ID, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);
            Assert.AreEqual(Proparse.RIGHTANGLE, stream.NextToken().Type);
            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);

            // Quoted string
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(15, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(16, tok.EndCharPositionInLine);

            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);

            // Colon
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.LEXCOLON, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(18, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(18, tok.EndCharPositionInLine);
        }

        [TestMethod]
        public virtual void TestQuotedStringPosition3()
        {
            // Same as previous test, but with a space before the colon
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer11-3.p")), session);
            ITokenSource stream = unit.Lex();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(10, tok.EndCharPositionInLine);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.PERIOD, tok.Type);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(11, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(11, tok.EndCharPositionInLine);

            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual(2, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(2, tok.EndLine);
            Assert.AreEqual(6, tok.EndCharPositionInLine);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.PERIOD, tok.Type);
            Assert.AreEqual(2, tok.Line);
            Assert.AreEqual(7, tok.Column);
            Assert.AreEqual(2, tok.EndLine);
            Assert.AreEqual(7, tok.EndCharPositionInLine);

            Assert.AreEqual(Proparse.WS, stream.NextToken().Type);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.AreEqual(3, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(3, tok.EndLine);
            Assert.AreEqual(8, tok.EndCharPositionInLine);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.PERIOD, tok.Type);
            Assert.AreEqual(3, tok.Line);
            Assert.AreEqual(9, tok.Column);
            Assert.AreEqual(3, tok.EndLine);
            Assert.AreEqual(9, tok.EndCharPositionInLine);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](enabled = false) public void testMacroExpansion()
        [TestMethod]
        [Ignore()]
        public void TestMacroExpansion()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer12.p")), session);
            ITokenSource stream = unit.Preprocess();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.MESSAGE, tok.Type);
            Assert.IsTrue(tok.MacroExpansion);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.QSTRING, tok.Type);
            Assert.IsTrue(tok.MacroExpansion);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.PERIOD, tok.Type);
            // Bug lies here in the lexer
            Assert.IsTrue(tok.MacroExpansion);
        }
        [TestMethod]
        public virtual void TestUnicodeBom()
        {
            RefactorSession session2 = new RefactorSession(new ProparseSettings("c:/temp/utests/resources/data"), new Schema(), Encoding.UTF8);
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer13.p")), session2);
            ITokenSource src = unit.Preprocess();

            ProToken tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.QSTRING, tok.NodeType);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.QSTRING, tok.NodeType);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
        }

        [TestMethod]
        public virtual void TestXCode1()
        {
            // Default behavior is that it shouldn't fail
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer14.p")), session);
            ITokenSource src = unit.Preprocess();

            // lexer14.i contains 'message "xcode".'
            ProToken tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(Proparse.MESSAGE, tok.Type);
            Assert.AreEqual(2, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(2, tok.EndLine);
            Assert.AreEqual(7, tok.EndCharPositionInLine);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.QSTRING, tok.NodeType);
            Assert.AreEqual("\"hello world\"", tok.Text);
        }

        [TestMethod]
        public virtual void TestXCode2()
        {
            // Test with customSkipXCode set to true
            ProparseSettings settings = new ProparseSettings("Resources/data")
            {
                CustomSkipXCode = true
            };
            RefactorSession session2 = new RefactorSession(settings, new Schema(), Encoding.UTF8);
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer14.p")), session2);
            ITokenSource src = unit.Preprocess();

            // lexer14.i contains 'message "xcode".'
            ProToken tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(Proparse.MESSAGE, tok.Type);
            Assert.AreEqual(2, tok.Line);
            Assert.AreEqual(1, tok.Column);
            Assert.AreEqual(2, tok.EndLine);
            Assert.AreEqual(7, tok.EndCharPositionInLine);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.QSTRING, tok.NodeType);
            Assert.AreEqual("\"hello world\"", tok.Text);
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](expectedExceptions = UncheckedIOException.class) public void testXCode3()
        [TestMethod]
        [ExpectedException(typeof(FileLoadException), "Has to fail here")]

        public void TestXCode3()
        {
            // Test with customSkipXCode set to false
            ProparseSettings settings = new ProparseSettings("Resources/data")
            {
                CustomSkipXCode = false
            };
            RefactorSession session2 = new RefactorSession(settings, new Schema(), Encoding.UTF8);
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer14.p")), session2);
            // Has to fail here
            unit.Preprocess();
        }

        [TestMethod]
        public virtual void TestXCode4()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer14-2.p")), session);
            ITokenSource src = unit.Preprocess();

            // lexer14.i contains 'message "xcode".'
            ProToken tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(Proparse.MESSAGE, tok.Type);

            tok = (ProToken)src.NextToken();
            Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
            Assert.AreEqual(TokenConstants.HiddenChannel, tok.Channel);

            tok = (ProToken)src.NextToken();
            Assert.AreEqual(ABLNodeType.QSTRING, tok.NodeType);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(27, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(33, tok.EndCharPositionInLine);

            // Two xcoded include files are replaced by a two whitespaces leading to one token
            tok = (ProToken)src.NextToken();
            Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
            Assert.AreEqual(TokenConstants.HiddenChannel, tok.Channel);

            tok = (ProToken)src.NextToken();
            Assert.AreEqual(ABLNodeType.QSTRING, tok.NodeType);
            Assert.AreEqual(1, tok.Line);
            Assert.AreEqual(72, tok.Column);
            Assert.AreEqual(1, tok.EndLine);
            Assert.AreEqual(84, tok.EndCharPositionInLine);
        }

        [TestMethod]
        public virtual void TestProparseDirectiveLexPhase()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer15.p")), session);
            ITokenSource stream = unit.Lex();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.PROPARSEDIRECTIVE, tok.NodeType);
            Assert.AreEqual(ProToken.PROPARSE_CHANNEL, tok.Channel);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.WS, tok.NodeType);
            Assert.AreEqual(TokenConstants.HiddenChannel, tok.Channel);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.EQUAL, tok.NodeType);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);

            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.PROPARSEDIRECTIVE, tok.NodeType);
            Assert.AreEqual(ProToken.PROPARSE_CHANNEL, tok.Channel);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
            Assert.AreEqual("customer.custnum", tok.Text);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.EQUAL, tok.NodeType);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);

            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.PROPARSEDIRECTIVE, tok.NodeType);
            Assert.AreEqual(ProToken.PROPARSE_CHANNEL, tok.Channel);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
            Assert.AreEqual("sp2k.customer.custnum", tok.Text);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.EQUAL, tok.NodeType);
            _ = (ProToken)stream.NextToken();
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
        }

        [TestMethod]
        public virtual void TestProparseDirectivePreprocessPhase()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer15.p")), session);
            ITokenSource src = unit.Preprocess();

            ProToken tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
            Assert.AreEqual("custnum", tok.Text);
            // FIXME Hidden tokens are attached in JPNodeVisitor, so this has to be tested in a later stage
            // assertNotNull(tok.getHiddenBefore());
            // assertEquals(((ProToken) tok.getHiddenBefore()).getNodeType(), ABLNodeType.WS);
            // assertNotNull(tok.getHiddenBefore().getHiddenBefore());
            // assertEquals(((ProToken) tok.getHiddenBefore().getHiddenBefore()).getNodeType(), ABLNodeType.PROPARSEDIRECTIVE);
            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.EQUAL, tok.NodeType);
            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.ID, tok.NodeType);
            Assert.AreEqual("customer.custnum", tok.Text);
            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.EQUAL, tok.NodeType);
            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.NUMBER, tok.NodeType);
            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.PERIOD, tok.NodeType);
        }
        [TestMethod]
        public virtual void TestHexNumbers()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer16.p")), session);
            ITokenSource stream = unit.Lex();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.MESSAGE, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("125", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("0x65", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("0X66", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("0xfb", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("0xab", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("-0x01", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.PERIOD, tok.Type);
        }

        [TestMethod]
        public virtual void TestHexNumbers2()
        {
            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer17.p")), session);
            ITokenSource stream = unit.Lex();

            ProToken tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.MESSAGE, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.NUMBER, tok.Type);
            Assert.AreEqual("125", tok.Text);

            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.WS, tok.Type);
            tok = (ProToken)stream.NextToken();
            Assert.AreEqual(Proparse.ID, tok.Type);
            Assert.AreEqual("0x2g8", tok.Text);
        }

        [TestMethod]
        public virtual void TestFileNumName()
        {
            // Use Windows settings here in order to use backlash directory separator
            IKernel kernel = new StandardKernel(new UnitTestWindowsModule());
            RefactorSession session = kernel.Get<RefactorSession>();

            ParseUnit unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "lexer18.p")), session);
            ITokenSource src = unit.Preprocess();

            ProToken tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.STOP, tok.NodeType);
            Assert.AreEqual(0, tok.TokenIndex);
            Assert.AreEqual(0, tok.FileIndex);
            Assert.AreEqual(Path.GetFullPath("Resources/data/lexer/lexer18.p").Replace('\\', '/'), tok.FileName.Replace('\\', '/'));

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);
            Assert.AreEqual(2, tok.TokenIndex);
            Assert.AreEqual(1, tok.FileIndex);
            Assert.IsTrue(tok.FileName.Replace('\\', '/').EndsWith("Resources/data/lexer/lexer18.i"));

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);
            Assert.AreEqual(4, tok.TokenIndex);
            Assert.AreEqual(1, tok.FileIndex);
            Assert.IsTrue(tok.FileName.Replace('\\', '/').EndsWith("Resources/data/lexer/lexer18.i"));

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.QUIT, tok.NodeType);
            Assert.AreEqual(6, tok.TokenIndex);
            Assert.AreEqual(2, tok.FileIndex);
            Assert.IsTrue(tok.FileName.Replace('\\', '/').EndsWith("Resources/data/lexer/lexer18-2.i"));

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);
            Assert.AreEqual(8, tok.TokenIndex);
            Assert.AreEqual(1, tok.FileIndex);
            Assert.IsTrue(tok.FileName.Replace('\\', '/').EndsWith("Resources/data/lexer/lexer18.i"));

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.MESSAGE, tok.NodeType);
            Assert.AreEqual(10, tok.TokenIndex);
            Assert.AreEqual(1, tok.FileIndex);
            Assert.IsTrue(tok.FileName.Replace('\\', '/').EndsWith("Resources/data/lexer/lexer18.i"));

            tok = (ProToken)NextVisibleToken(src);
            Assert.AreEqual(ABLNodeType.STOP, tok.NodeType);
            Assert.AreEqual(12, tok.TokenIndex);
            Assert.AreEqual(0, tok.FileIndex);
            Assert.IsTrue(tok.FileName.Replace('\\', '/').EndsWith("Resources/data/lexer/lexer18.p"));
        }


        /// <summary>
        /// Utility method for tests, returns next node of given type
        /// </summary>
        private ProToken NextToken(ITokenSource stream, ABLNodeType type)
        {
            ProToken tok = (ProToken)stream.NextToken();
            while (tok.NodeType != ABLNodeType.MESSAGE)
            {
                tok = (ProToken)stream.NextToken();
            }
            return tok;
        }

        /// <summary>
        /// Utility method for preprocess(), removes all tokens from hidden channels
        /// </summary>
        protected internal static IToken NextVisibleToken(ITokenSource src)
        {
            IToken tok = src.NextToken();
            while ((tok.Type != TokenConstants.EOF) && (tok.Channel != TokenConstants.DefaultChannel))
            {
                tok = src.NextToken();
            }
            return tok;
        }


    }
}
