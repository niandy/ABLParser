using Antlr4.Runtime;
using log4net;
using System.IO;
using ABLParser.Prorefactor.Core;
using System;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class DescriptiveErrorListener : BaseErrorListener
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(DescriptiveErrorListener));        

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            ProToken tok = (ProToken)offendingSymbol;
            if (tok.FileIndex != 0)
            {
                LOG.Error(String.Format("Syntax error -- {0} -- {1}:{2}:{3} -- {4} -- {5}", recognizer.InputStream.SourceName, tok.FileName, line, charPositionInLine, msg, e != null ? "Recover" : ""));
            }
            else
            {
                LOG.Error(String.Format("Syntax error -- {0}:{1}:{2} -- {3} -- {4}", tok.FileName, line, charPositionInLine, msg, e != null ? "Recover" : ""));
            }
        }
    }

}
