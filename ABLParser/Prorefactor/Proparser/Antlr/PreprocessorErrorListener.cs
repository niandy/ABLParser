using ABLParser.Prorefactor.Core;
using Antlr4.Runtime;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class PreprocessorErrorListener : BaseErrorListener
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(PreprocessorErrorListener));

        private readonly ProgressLexer lexer;
        private readonly IList<ProToken> tokens;

        public PreprocessorErrorListener(ProgressLexer lexer, IList<ProToken> tokens)
        {
            this.lexer = lexer;
            this.tokens = tokens;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            LOGGER.Error(String.Format("Unexpected symbol '{0}' in preprocessor expression '{1}' at position {2}", ((IToken)offendingSymbol).Text, ExpressionAsString, charPositionInLine));
            if (tokens.Count == 0)
            {
                LOGGER.Error(String.Format("Exception found while analyzing '{0}'", lexer.GetFilename(0)));
            }
            else if (tokens[0].FileIndex == 0)
            {
                LOGGER.Error(String.Format("Exception found while analyzing '{0}' at line {1}", lexer.GetFilename(0), tokens[0].Line));
            }
            else
            {
                LOGGER.Error(String.Format("Exception found in file '{0}' at line {1} while analyzing '{2}'", lexer.GetFilename(tokens[0].FileIndex), tokens[0].Line, lexer.GetFilename(0)));
            }
        }

        private string ExpressionAsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (ProToken tok in tokens)
                {
                    sb.Append(tok.Text).Append(' ');
                }

                return sb.ToString();
            }
        }
    }

}
