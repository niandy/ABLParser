using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using log4net;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// Convert some tokens to another type when not followed by LEFTPAREN:
    /// <ul>
    /// <li>ASC to ASCENDING</li>
    /// <li>LOG to LOGICAL</li>
    /// <li>GET-CODEPAGE to GET-CODEPAGES</li>
    /// </ul>
    /// </summary>
    public class FunctionKeywordTokenFilter : ITokenSource
    {        
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(FunctionKeywordTokenFilter));

        private readonly ITokenSource source;
        private readonly LinkedList<IToken> heap = new LinkedList<IToken>();

        public FunctionKeywordTokenFilter(ITokenSource source)
        {
            this.source = source;
        }

        public IToken NextToken()
        {
            if (heap.Count > 0)
            {
                IToken tok = heap.First.Value;
                heap.RemoveFirst();
                if (LOGGER.IsDebugEnabled)
                {
                    LogToken(tok);
                }
                return tok;
            }
            LOGGER.Debug($"Buiding heap");
            ProToken currToken = (ProToken)source.NextToken();
            
            if ((currToken.NodeType == ABLNodeType.ASC) || (currToken.NodeType == ABLNodeType.LOG) || (currToken.NodeType == ABLNodeType.GETCODEPAGE) || (currToken.NodeType == ABLNodeType.GETCODEPAGES))
            {
                ProToken nxt = (ProToken)source.NextToken();
                while ((nxt.Type != TokenConstants.EOF) && (nxt.Channel != TokenConstants.DefaultChannel))
                {
                    LOGGER.Debug($"Adding {nxt.Text} of type {ABLNodeType.GetFullText(nxt.Type)}");
                    heap.AddLast(nxt);
                    nxt = (ProToken)source.NextToken();
                }
                heap.AddLast(nxt);
                if (nxt.NodeType != ABLNodeType.LEFTPAREN)
                {
                    if (currToken.NodeType == ABLNodeType.ASC)
                    {
                        currToken.NodeType = ABLNodeType.ASCENDING;
                    }
                    else if (currToken.NodeType == ABLNodeType.LOG)
                    {
                        currToken.NodeType = ABLNodeType.LOGICAL;
                    }
                    else if (currToken.NodeType == ABLNodeType.GETCODEPAGE)
                    {
                        currToken.NodeType = ABLNodeType.GETCODEPAGES;
                    }
                }
                else if (currToken.NodeType == ABLNodeType.GETCODEPAGES)
                {
                    currToken.NodeType = ABLNodeType.GETCODEPAGE;
                }
            }
            if (LOGGER.IsDebugEnabled)
            {
                LogToken(currToken);
            }
            return currToken;
        }

        private void LogToken(IToken tok)
        {
            if (LOGGER.IsDebugEnabled)
            {
                LOGGER.Debug(String.Format("'{0}' -- {1}", tok.Text.Replace('\n', ' ').Replace('\r', ' '), ABLNodeType.GetNodeType(tok.Type)));
            }
        }

        public  int Line
        {
            get
            {
                return source.Line;
            }
        }

        public int Column => source.Column;

        public ICharStream InputStream
        {
            get
            {
                return source.InputStream;
            }
        }

        public string SourceName => source.SourceName;


        public ITokenFactory TokenFactory
        {
            set => throw new System.NotSupportedException("Unable to change TokenFactory object");            
            get => source.TokenFactory;            
        }
    }
}
