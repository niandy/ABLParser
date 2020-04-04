using System.Linq;
using ABLParser.Prorefactor.Core;
using Antlr4.Runtime;
using log4net;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    

    /// <summary>
    /// This class deals with &amp;IF conditions by acting as a filter between the lexer and the parser
    /// </summary>
    public class PostLexer : ITokenSource
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(PostLexer));

        private readonly Lexer lexer;
        private readonly ProgressLexer prepro;
        private readonly PreproEval eval;

        private readonly LinkedList<PreproIfState> preproIfVec = new LinkedList<PreproIfState>();
        private ProToken currToken;

        public PostLexer(Lexer lexer)
        {
            this.lexer = lexer;
            this.prepro = lexer.Preprocessor;
            this.eval = new PreproEval(prepro.ProparseSettings);
        }

        public IToken NextToken()
        {
            LOGGER.Debug("Entering nextToken()");
            for (; ; )
            {
                GetNextToken();
                LOGGER.Debug($"Token is {currToken.Text}");
                switch (currToken.Type)
                {

                    case PreprocessorParser.AMPIF:
                        PreproIf();
                        break; // loop again
                    case PreprocessorParser.AMPTHEN:
                        // &then are consumed by preproIf()
                        ThrowMessage("Unexpected &THEN");
                        break;
                    case PreprocessorParser.AMPELSEIF:
                        PreproElseif();
                        break; // loop again
                    case PreprocessorParser.AMPELSE:
                        PreproElse();
                        break; // loop again
                    case PreprocessorParser.AMPENDIF:
                        PreproEndif();
                        break; // loop again                        
                    default:
                        return currToken;
                }
            }
        }

        private ProToken Defined()
        {
            LOGGER.Debug("Entering defined()");
            // Progress DEFINED() returns a single digit: 0,1,2, or 3.
            // The text between the parens can be pretty arbitrary, and can
            // have embedded comments, so this calls a specific lexer function for it.
            GetNextToken();
            if (currToken.Type == PreprocessorParser.WS)
            {
                GetNextToken();
            }
            if (currToken.Type != PreprocessorParser.LEFTPAREN)
            {
                ThrowMessage("Bad DEFINED function in &IF preprocessor condition");
            }
            ProToken argToken = lexer.GetAmpIfDefArg();
            GetNextToken();
            if (currToken.Type != PreprocessorParser.RIGHTPAREN)
            {
                ThrowMessage("Bad DEFINED function in &IF preprocessor condition");
            }
            return (new ProToken.Builder(ABLNodeType.NUMBER, prepro.Defined(argToken.Text.Trim().ToLower()))).Build();
        }

        private void GetNextToken()
        {
            currToken = lexer.NextToken();
        }

        // For consuming tokens that has been preprocessed out (&IF FALSE...)
        private void Preproconsume()
        {
            LOGGER.Debug("Entering preproconsume()");

            int thisIfLevel = preproIfVec.Count;
            prepro.IncrementConsuming();
            while (thisIfLevel <= preproIfVec.Count && preproIfVec.ElementAt(thisIfLevel - 1).Consuming)
            {
                GetNextToken();
                switch (currToken.Type)
                {
                    case PreprocessorParser.AMPIF:
                        PreproIf();
                        break;
                    case PreprocessorParser.AMPELSEIF:
                        PreproElseif();
                        break;
                    case PreprocessorParser.AMPELSE:
                        PreproElse();
                        break;
                    case PreprocessorParser.AMPENDIF:
                        PreproEndif();
                        break;
                    case TokenConstants.EOF:
                        ThrowMessage("Unexpected end of input when consuming discarded &IF/&ELSEIF/&ELSE text");
                        break;
                    default:
                        break;
                }
            }
            prepro.DecrementConsuming();
        }

        private void PreproIf()
        {
            LOGGER.Debug("Entering preproIf()");

            // Preserve the currToken current position for listing, before evaluating the expression.
            // We can't just write to listing here, because the expression evaluation may
            // find macro references to list.
            int currLine = currToken.Line;
            int currCol = currToken.Column;
            PreproIfState preproIfState = new PreproIfState();
            preproIfVec.AddLast(preproIfState);
            // Only evaluate if we aren't consuming from an outer &IF.
            bool isTrue = PreproIfCond(!prepro.IsConsuming);
            if (isTrue)
            {
                prepro.LstListener.PreproIf(currLine, currCol, true);
                preproIfState.Done = true;
            }
            else
            {
                prepro.LstListener.PreproIf(currLine, currCol, false);
                preproIfState.Consuming = true;
                Preproconsume();
            }
        }

        private void PreproElse()
        {
            LOGGER.Debug("Entering preproElse()");

            PreproIfState preproIfState = preproIfVec.Last.Value;
            if (!preproIfState.Done)
            {
                preproIfState.Consuming = false;
                prepro.LstListener.PreproElse(currToken.Line, currToken.Column);
            }
            else
            {
                if (!preproIfState.Consuming)
                {
                    prepro.LstListener.PreproElse(currToken.Line, currToken.Column);
                    preproIfState.Consuming = true;
                    Preproconsume();
                }
                // else: already consuming. no change.
                prepro.LstListener.PreproElse(currToken.Line, currToken.Column);
            }
        }

        private void PreproElseif()
        {
            LOGGER.Debug("Entering preproElseif()");
            // Preserve the current position for listing, before evaluating the expression.
            // We can't just write to listing here, because the expression evaluation may
            // find macro references to list.
            int currLine = currToken.Line;
            int currCol = currToken.Column;
            bool evaluate = true;
            // Don't evaluate if we're consuming from an outer &IF
            if (prepro.Consuming - 1 > 0)
            {
                evaluate = false;
            }
            // Don't evaluate if we're already done with this &IF
            if (preproIfVec.Last.Value.Done)
            {
                evaluate = false;
            }
            bool isTrue = PreproIfCond(evaluate);
            prepro.LstListener.PreproElseIf(currLine, currCol);
            PreproIfState preproIfState = preproIfVec.Last.Value;
            if (isTrue && (!preproIfState.Done))
            {
                preproIfState.Done = true;
                preproIfState.Consuming = false;
            }
            else
            {
                if (!preproIfState.Consuming)
                {
                    preproIfState.Consuming = true;
                    Preproconsume();
                }
                // else: already consuming. no change.
            }
        }


        private void PreproEndif()
        {
            LOGGER.Debug("Entering preproEndif()");
            prepro.LstListener.PreproEndIf(currToken.Line, currToken.Column);
            // XXX Got a case where removeLast() fails with NoSuchElementException
            if (preproIfVec.Count > 0)
            {
                preproIfVec.RemoveLast();
            }
        }

        private bool PreproIfCond(bool evaluate)
        {
            LOGGER.Debug("Entering preproIfCond()");


            // Notes
            // An &IF here in this &IF condition is not legal. Progress would barf on it.
            // That allows us to simply use a global flag to watch for &THEN.

            IList<ProToken> tokenVector = new List<ProToken>();
            bool done = false;
            while (!done)
            {
                GetNextToken();
                switch (currToken.Type)
                {
                    case TokenConstants.EOF:
                        ThrowMessage("Unexpected end of input after &IF or &ELSEIF");
                        break;
                    case PreprocessorParser.AMPTHEN:
                        done = true;
                        break;
                    case PreprocessorParser.DEFINED:
                        if (evaluate)
                        {
                            // If not evaluating, just discard
                            tokenVector.Add(Defined());
                        }
                        break;
                    case PreprocessorParser.COMMENT:
                    case PreprocessorParser.WS:
                    case PreprocessorParser.PREPROCESSTOKEN:
                        break;
                    default:
                        if (evaluate)
                        {
                            // If not evaluating, just discard
                            tokenVector.Add(currToken);
                        }
                        break;
                }
            }

            // If it's blank or the the evaluate argument is false, we don't evaluate
            if (tokenVector.Count == 0 || !evaluate)
            {
                return false;
            }
            else
            {
                CommonTokenStream cts = new CommonTokenStream(new ListTokenSource(tokenVector.Cast<IToken>().ToList()));
                PreprocessorParser parser = new PreprocessorParser(cts)
                {
                    ErrorHandler = new BailErrorStrategy()
                };
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new PreprocessorErrorListener(prepro, tokenVector));
                try
                {
                    return (bool)eval.VisitPreproIfEval(parser.preproIfEval());
                }
                catch (ParseCanceledException)
                {
                    return false;
                }
            }
        }

        private void ThrowMessage(string msg)
        {
            throw new ProparseRuntimeException("File '" + prepro.GetFilename(0) + "' - Current position '" + prepro.GetFilename(currToken.FileIndex) + "':" + currToken.Line + " - " + msg);
        }

        private class PreproIfState
        {
            public bool Consuming { get; set; }
            public bool Done { get; set; }
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
