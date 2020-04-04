using System;
using Antlr4.Runtime;
using System.Collections.Generic;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// Review the token list at an OBJCOLON token.
    /// 
    /// This is the reason this class was created in the first place. If we have an OBJCOLON, what comes before it has to
    /// be one of a few things:
    /// <ul>
    /// <li>a system handle,
    /// <li>a handle expression,
    /// <li>an Object reference expression, or
    /// <li>a type name (class name) for a static member reference
    /// </ul>
    /// <para>
    /// A type name can be pretty much anything, even a reserved keyword. It can also be a qualified class name, such as
    /// com.joanju.Foo.
    /// </para>
    /// <para>
    /// This method attempts to resolve the following problem: Because of static class member references, a class name can
    /// be the first token in an expression. Class names can be composed of reserved keywords. This means that a reserved
    /// keyword could be the first piece of an expression, and this completely breaks the lookahead in the ANTLR generated
    /// parser. So, here, we look for OBJCOLON, and make sure that what comes before it is a system handle, an ID, or a
    /// non-reserved keyword.
    /// </para>
    /// <para>
    /// A NAMEDOT token is a '.' followed by anything other than whitespace. If the OBJCOLON is proceeded by a NAMEDOT pair
    /// (NAMEDOT followed by anything), then we convert all of the NAMEDOT pairs to NAMEDOT-ID pairs. Otherwise, if the
    /// OBJCOLON is proceeded by any reserved keyword other than a systemhandlename, then we change that token's type to
    /// ID.
    /// 
    /// Comment extracted from proparse.g
    /// 
    /// Comparing identifiers in Progress code
    /// --------------------------------------
    /// Progress only allows certain ASCII characters in identifiers (field names, etc). Because of this, it is safe
    /// to store/compare lower-cased versions of identifiers, without concern for alternative code pages (I hope).
    /// 
    /// 
    /// "OBJCOLON"
    /// --------
    /// "OBJCOLON" describes a colon that is followed by non-whitespace.
    /// Note that the following compiles: c[1] :move-to-top ().  So, not only
    /// do we not want to try to figure out (from lexical) if it's an attribute
    /// or method, but we want to make sure that either field or METHOD will
    /// work in a particular spot, that METHOD is tried for first.
    /// 
    /// </para>
    /// </summary>
    public class TokenList : ITokenSource
    {
        private readonly ITokenSource source;
        private readonly LinkedList<ProToken> queue = new LinkedList<ProToken>();

        private int currentPosition;
        private ProToken currentToken;

        public TokenList(ITokenSource input)
        {
            this.source = input;
        }

        private void FillHeap()
        {
            ProToken nxt = (ProToken)source.NextToken();
            while (true)
            {
                queue.AddLast(nxt);
                if (nxt.NodeType == ABLNodeType.OBJCOLON)
                {
                    ReviewObjcolon();
                }
                if ((nxt.NodeType == ABLNodeType.OBJCOLON) || (nxt.NodeType == ABLNodeType.EOF_ANTLR4))
                {
                    break;
                }
                nxt = (ProToken)source.NextToken();
            }
        }

        private void ReviewObjcolon()
        {
            ProToken objColonToken = queue.Last.Value;
            LinkedList<ProToken> comments = new LinkedList<ProToken>();
            LinkedList<ProToken> clsName = new LinkedList<ProToken>();

            bool foundNamedot = false;
            ProToken tok = null;
            queue.RemoveLast();
            try
            {
                // Store comments and whitespaces before the colon
                tok = queue.Last?.Value;
                queue.RemoveLast();
                while ((tok.NodeType == ABLNodeType.WS) || (tok.NodeType == ABLNodeType.COMMENT))
                {
                    comments.AddFirst(tok);
                    tok = queue.Last?.Value;
                    queue.RemoveLast();
                }

                foundNamedot = false;
                while (true)
                {
                    // There can be space in front of a NAMEDOT in a table or field name. We don't want to fiddle with those here.
                    if ((tok.NodeType == ABLNodeType.WS) || (tok.NodeType == ABLNodeType.COMMENT))
                    {
                        break;
                    }

                    // If previous is NAMEDOT, then we add both tokens
                    if ((queue.Last?.Value != null) && (queue.Last.Value.NodeType == ABLNodeType.NAMEDOT))
                    {
                        clsName.AddFirst(tok);
                        clsName.AddFirst(queue.Last.Value);
                        queue.RemoveLast();
                        tok = queue.Last?.Value;
                        queue.RemoveLast();
                    }
                    else if (tok.Text.StartsWith("."))
                    {
                        clsName.AddFirst(tok);
                        tok = queue.Last?.Value;
                        queue.RemoveLast();
                    }
                    else
                    {
                        break;
                    }
                    foundNamedot = true;
                }
            }
            catch (InvalidOperationException)
            {
                QueueAddAll(clsName);
                QueueAddAll(comments);
                queue.AddLast(objColonToken);
                return;
            }

            if (foundNamedot)
            {
                // Now merge all the parts into one ID token.
                ProToken.Builder newTok = (new ProToken.Builder(tok)).SetType(ABLNodeType.ID);
                foreach (ProToken zz in clsName)
                {
                    newTok.MergeWith(zz);
                }
                queue.AddLast(newTok.Build());
                QueueAddAll(comments);
            }
            else
            {
                // Not namedotted, so if it's reserved and not a system handle, convert to ID.
                if (tok.NodeType.IsReservedKeyword() && !tok.NodeType.IsSystemHandleName())
                {
                    queue.AddLast((new ProToken.Builder(tok)).SetType(ABLNodeType.ID).Build());
                }
                else
                {
                    queue.AddLast(tok);
                }
                QueueAddAll(comments);
            }
            queue.AddLast(objColonToken);
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

            ProToken tok = queue.First.Value;
            queue.RemoveFirst();
            if (tok != null)
            {
                currentToken = tok;
                currentToken.TokenIndex = currentPosition++;
            }

            return currentToken;
        }

        public int Line => currentToken.Line;
        public int Column => currentToken.Column;
        public ICharStream InputStream => currentToken.InputStream;
        public string SourceName => IntStreamConstants.UnknownSourceName;

        public ITokenFactory TokenFactory
        {
            set => throw new System.NotSupportedException("Unable to change TokenFactory object");
            get => source.TokenFactory;
        }

        private void QueueAddAll(IEnumerable<ProToken> items)
        {
            foreach (ProToken item in items)
            {
                queue.AddLast(item);
            }
        }
    }
}
