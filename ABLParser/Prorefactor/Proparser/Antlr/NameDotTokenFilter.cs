using System;
using System.Collections.Generic;
using ABLParser.Prorefactor.Core;
using Antlr4.Runtime;

namespace ABLParser.Prorefactor.Proparser.Antlr
{

    /// <summary>
    /// Merge NAMEDOT with previous and next tokens with the following rules:<ul>
    ///  <li> ( ID | keyword ) NAMEDOT ( ID | keyword ) </li>
    ///  <li> A comment can follow immediately NAMEDOT. If so, then an unlimited number of WS and COMMENT can follow NAMEDOT before the ID</li></ul>
    /// </summary>
    public class NameDotTokenFilter : ITokenSource
    {
        private readonly ITokenSource source;
        private readonly LinkedList<ProToken> queue = new LinkedList<ProToken>();

        private ProToken currentToken;

        public NameDotTokenFilter(ITokenSource source)
        {
            this.source = source;
        }

        private void FillHeap()
        {
            bool loop = true;
            while (loop)
            {
                ProToken nxt = (ProToken)source.NextToken();
                queue.AddLast(nxt);
                ABLNodeType type = nxt.NodeType;
                if (nxt.NodeType == ABLNodeType.FILENAME)
                {
                    type = ReviewFileName();
                }
                else if (nxt.NodeType == ABLNodeType.NAMEDOT)
                {
                    type = ReviewNameDot();
                }
                else if ((nxt.NodeType == ABLNodeType.NUMBER) && nxt.Text.StartsWith("."))
                {
                    type = ReviewNumber();
                }
                // NAMEDOTs can be chained, so we stay in the loop until an expected end of statement
                loop = (type != ABLNodeType.EOF_ANTLR4) && (type != ABLNodeType.PERIOD);
            }
        }

        private ABLNodeType ReviewNumber()
        {
            ProToken nameDot = queue.Last.Value;
            queue.RemoveLast();
            ProToken prev = queue.Count == 0 ? null : queue.Last.Value;
            if (queue.Count > 0)
                queue.RemoveLast();
            if (prev == null)
            {
                // In case NUMBER is the only one in queue (e.g. first token in procedure), we just exit safely by putting
                // it back in the queue
                queue.AddLast(nameDot);
            }
            else if ((prev.NodeType == ABLNodeType.ID) || prev.NodeType.IsKeyword() || (prev.NodeType == ABLNodeType.ANNOTATION))
            {
                // Merge both tokens in first one
                ProToken.Builder builder = (new ProToken.Builder(prev)).MergeWith(nameDot);
                if (prev.NodeType != ABLNodeType.ANNOTATION)
                {
                    builder.SetType(ABLNodeType.ID);
                }
                queue.AddLast(builder.Build());
            }
            else
            {
                // Anything else, we just put tokens back in the queue
                queue.AddLast(prev);
                queue.AddLast(nameDot);
            }

            return queue.Last.Value.NodeType;
        }

        private ABLNodeType ReviewNameDot()
        {
            ProToken nameDot = queue.Last.Value;
            queue.RemoveLast();
            ProToken prev = queue.Count == 0 ? null : queue.Last.Value;
            if (queue.Count > 0)
                queue.RemoveLast();
            if (prev == null)
            {
                // In case NAMEDOT is the only one in queue (e.g. first token in procedure), we just exit safely by putting
                // it back in the queue
                queue.AddLast(nameDot);
            }
            else if ((prev.NodeType == ABLNodeType.ID) || prev.NodeType.IsKeyword() || (prev.NodeType == ABLNodeType.ANNOTATION))
            {
                ProToken nxt = (ProToken)source.NextToken();
                if (nxt.NodeType == ABLNodeType.COMMENT)
                {
                    // We can consume as much WS and COMMENT
                    while ((nxt.NodeType == ABLNodeType.COMMENT) || (nxt.NodeType == ABLNodeType.WS))
                    {
                        nxt = (ProToken)source.NextToken();
                    }
                    // Then we merge everything in first token
                    ProToken.Builder builder = (new ProToken.Builder(prev)).MergeWith(nameDot).MergeWith(nxt);
                    if (prev.NodeType != ABLNodeType.ANNOTATION)
                    {
                        builder.SetType(ABLNodeType.ID);
                    }
                    queue.AddLast(builder.Build());
                }
                else
                {
                    // Merge everything in first token
                    ProToken.Builder builder = (new ProToken.Builder(prev)).MergeWith(nameDot).MergeWith(nxt);
                    if (prev.NodeType != ABLNodeType.ANNOTATION)
                    {
                        builder.SetType(ABLNodeType.ID);
                    }
                    queue.AddLast(builder.Build());
                }
            }
            else
            {
                // Anything else, we just put tokens back in the queue
                queue.AddLast(prev);
                queue.AddLast(nameDot);
            }

            return queue.Last.Value.NodeType;
        }
        private ABLNodeType ReviewFileName()
        {
            ProToken fName = queue.Last.Value;
            queue.RemoveLast();
            ProToken prev = queue.Count == 0 ? null : queue.Last.Value;
            if (queue.Count > 0)
                queue.RemoveLast();
            if (prev == null)
            {
                queue.AddLast(fName);
            }
            else if ((prev.NodeType == ABLNodeType.ID) || (prev.NodeType == ABLNodeType.ANNOTATION))
            {
                ProToken.Builder builder = (new ProToken.Builder(prev)).MergeWith(fName);
                queue.AddLast(builder.Build());
            }
            else
            {
                queue.AddLast(prev);
                queue.AddLast(fName);
            }

            return queue.Last.Value.NodeType;
        }


        public IToken NextToken()
        {
            if ((currentToken != null) && (currentToken.Type == TokenConstants.EOF))
            {
                return currentToken;
            }

            if (queue.Count == 0)
            {
                FillHeap();
            }

            ProToken tok = queue.Count == 0 ? null : queue.First.Value;
            if (queue.Count > 0)
                queue.RemoveFirst();
            if (tok != null)
            {
                currentToken = tok;
            }

            return currentToken;
        }

        public int Line => source.Line;


        public int Column => source.Column;            

        public ICharStream InputStream => source.InputStream;            

        public string SourceName => source.SourceName;            

        public ITokenFactory TokenFactory
        {
            set => throw new System.NotSupportedException("Unable to change TokenFactory object");            
            get => source.TokenFactory;            
        }
    }
}
