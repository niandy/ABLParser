using ABLParser.Prorefactor.Core;
using Antlr4.Runtime;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// Just pass tokens along...
    /// </summary>
    public class NoOpPostLexer : ITokenSource
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(NoOpPostLexer));

        private readonly Lexer lexer;
        private ProToken currToken;

        public NoOpPostLexer(Lexer lexer)
        {
            this.lexer = lexer;
        }

        public IToken NextToken()
        {
            LOGGER.Debug("Entering nextToken()");
            currToken = lexer.NextToken();
            return currToken;
        }

        public int Line => currToken.Line;
        public int Column => currToken.Column;
        public ICharStream InputStream => currToken.InputStream;
        public string SourceName => IntStreamConstants.UnknownSourceName;

        public ITokenFactory TokenFactory
        {
            set => throw new System.NotSupportedException("Unable to override ProTokenFactory");            
            get => null;            
        }


    }

}
