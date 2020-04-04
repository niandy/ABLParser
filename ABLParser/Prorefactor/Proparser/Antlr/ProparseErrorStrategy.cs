using ABLParser.Prorefactor.Core;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// As tokens are manually generated in Proparse, method <seealso cref="DefaultErrorStrategy.getMissingSymbol"/> fails with
    /// NullPointerException because <seealso cref="Token.getTokenSource"/> returns null.
    /// 
    /// We just use the same implementation with a different token creation type
    /// </summary>
    public class ProparseErrorStrategy : DefaultErrorStrategy
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(ProparseErrorStrategy));

        private readonly bool allowDeletion;
        private readonly bool allowInsertion;
        private readonly bool allowRecover;

        public ProparseErrorStrategy(bool allowDeletion, bool allowInsertion, bool allowRecover) : base()
        {
            this.allowDeletion = allowDeletion;
            this.allowInsertion = allowInsertion;
            this.allowRecover = allowRecover;
        }

        public override void Recover(Parser recognizer, RecognitionException e)
        {
            if (allowRecover)
            {
                base.Recover(recognizer, e);
            }
            else
            {
                throw new ParseCanceledException(e);
            }
        }

        [return: Nullable]
        protected override IToken SingleTokenDeletion(Parser recognizer)
        {
            if (allowDeletion)
            {
                return base.SingleTokenDeletion(recognizer);
            }
            else
            {
                return null;
            }
        }

        protected override bool SingleTokenInsertion(Parser recognizer)
        {
            if (allowInsertion)
            {
                return base.SingleTokenInsertion(recognizer);
            }
            else
            {
                return false;
            }
        }

        protected override IToken GetMissingSymbol(Parser recognizer)
        {
            // Just convert into ProToken type
            IToken tok = base.GetMissingSymbol(recognizer);
            LOGGER.Debug($"Injecting missing token {ABLNodeType.GetNodeType(tok.Type)} at line {tok.Line} - column {tok.Column}");
            return (new ProToken.Builder(ABLNodeType.GetNodeType(tok.Type), tok.Text)).SetLine(tok.Line).SetCharPositionInLine(tok.Column).Build();
        }
    }

}
