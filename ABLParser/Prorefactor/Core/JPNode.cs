using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ABLParser.Prorefactor.Core.NodeTypes;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using Antlr4.Runtime.Tree;
using ABLParser.Prorefactor.Proparser;
using ABLParser.Prorefactor.Proparser.Antlr;
using static ABLParser.Prorefactor.Proparser.SymbolScope;

namespace ABLParser.Prorefactor.Core
{
    public class JPNode
    {
        private readonly ProToken token;
        private readonly JPNode parent;
        private readonly int childNum;
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable private final List<JPNode> children;
        private readonly IList<JPNode> children;

        // Only for statement nodes: previous and next statement
        private JPNode previousStatement;
        private JPNode nextStatement;
        // Only for statement nodes and block nodes: enclosing block
        private Block inBlock;

        // Fields are usually set in TreeParser
        private Symbol symbol;
        private FieldContainer container;
        private BufferScope bufferScope;

        private IDictionary<int, int> attrMap;

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: protected JPNode(ProToken token, JPNode parent, int num, boolean hasChildren)
        protected internal JPNode(ProToken token, JPNode parent, int num, bool hasChildren)
        {
            this.token = token;
            this.parent = parent;
            this.childNum = num;
            this.children = hasChildren ? new List<JPNode>() : null;
        }

        /// <seealso cref= ProToken#getText() </seealso>
        public virtual string Text => token.Text;

        /// <seealso cref= ProToken#getNodeType() </seealso>
        public virtual ABLNodeType NodeType => token.NodeType;

        /// <seealso cref= ProToken#getType() </seealso>
        public virtual int Type => token.NodeType.Type;

        /// <seealso cref= ProToken#getMacroSourceNum() </seealso>
        public virtual int SourceNum => token.MacroSourceNum;

        /// <seealso cref= ProToken#getLine() </seealso>
        public virtual int Line => token.Line;

        /// <seealso cref= ProToken#getEndLine() </seealso>
        public virtual int EndLine => token.EndLine;

        /// <seealso cref= ProToken#getCharPositionInLine() </seealso>
        public virtual int Column => token.Column;

        /// <seealso cref= ProToken#getEndCharPositionInLine() </seealso>
        public virtual int EndColumn => token.EndCharPositionInLine;

        /// <seealso cref= ProToken#getFileIndex() </seealso>
        public virtual int FileIndex => token.FileIndex;

        /// <seealso cref= ProToken#getFileName() </seealso>
        public virtual string FileName => token.FileName;

        /// <seealso cref= ProToken#getEndFileIndex() </seealso>
        public virtual int EndFileIndex => token.EndFileIndex;

        public virtual ProToken HiddenBefore => token.HiddenBefore;

        public virtual bool MacroExpansion => token.MacroExpansion;

        public virtual string AnalyzeSuspend => token.AnalyzeSuspend;

        // ******************
        // Navigation methods
        // ******************

        private IList<JPNode> Children => children ?? new List<JPNode>();

        public virtual int NumberOfChildren => children == null ? 0 : children.Count;

        public virtual JPNode FirstChild => children == null || children.Count == 0 ? null : children[0];

        public virtual JPNode NextSibling => (parent != null) && (parent.Children.Count > childNum + 1) ? parent.Children[childNum + 1] : null;

        public virtual JPNode Parent => parent;

        /// <returns> Previous sibling in line before this one </returns>
        public virtual JPNode PreviousSibling => (childNum > 0) && (parent != null) ? parent.Children[childNum - 1] : null;

        /// <summary>
        /// First Natural Child is found by repeating firstChild() until a natural node is found. If the start node is a
        /// natural node, then it is returned.
        /// </summary>
        public virtual JPNode FirstNaturalChild
        {
            get
            {
                if (token.Natural)
                {
                    return this;
                }
                for (JPNode n = FirstChild; n != null; n = n.FirstChild)
                {
                    if (n.token.Natural)
                    {
                        return n;
                    }
                }
                return null;
            }
        }

        /// <returns> Last child of the last child of the... </returns>
        public virtual JPNode LastDescendant
        {
            get
            {
                if (children == null || children.Count == 0)
                {
                    return this;
                }
                return children[children.Count - 1].LastDescendant;
            }
        }

        /// <returns> First child if there is one, otherwise next sibling </returns>
        public virtual JPNode NextNode => children == null || children.Count == 0 ? NextSibling : children[0];

        /// <returns> Previous sibling if there is one, otherwise parent </returns>
        public virtual JPNode PreviousNode => childNum > 0 ? PreviousSibling : Parent;

        // *************************
        // End of navigation methods
        // *************************

        // ***************
        // Various queries
        // ***************

        /// <summary>
        /// Get list of the direct children of this node.
        /// </summary>
        public virtual IList<JPNode> DirectChildren => Children;

        /// <summary>
        /// Return first direct child of a given type, or null if not found
        /// </summary>
        public virtual JPNode GetFirstDirectChild(ABLNodeType type)
        {
            JPNode n = FirstChild;
            while (n != null)
            {
                if (n.NodeType == type)
                {
                    return n;
                }
                n = n.NextSibling;
            }
            return null;
        }

        /// <summary>
        /// Get a list of the direct children of a given type
        /// </summary>
        public virtual IList<JPNode> GetDirectChildren(ABLNodeType type, params ABLNodeType[] types)
        {
            IList<JPNode> ret = new List<JPNode>();
            if (children != null)
            {
                foreach (JPNode n in children)
                {
                    if (n.NodeType == type)
                    {
                        ret.Add(n);
                    }
                    if (types != null)
                    {
                        foreach (ABLNodeType t in types)
                        {
                            if (n.NodeType == t)
                            {
                                ret.Add(n);
                            }
                        }
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// Get an array of all descendant nodes (including this node) of a given type
        /// </summary>
        public virtual IList<JPNode> Query2(Predicate<JPNode> pred)
        {
            JPNodePredicateQuery query = new JPNodePredicateQuery(pred);
            Walk(query);

            return query.Result;
        }

        /// <seealso cref= JPNode#query2(Predicate) </seealso>
        public virtual IList<JPNode> Query2(Predicate<JPNode> pred1, Predicate<JPNode> pred2)
        {
            JPNodePredicateQuery query = new JPNodePredicateQuery(pred1, pred2);
            Walk(query);

            return query.Result;
        }

        /// <seealso cref= JPNode#query2(Predicate) </seealso>
        public virtual IList<JPNode> Query2(Predicate<JPNode> pred1, Predicate<JPNode> pred2, Predicate<JPNode> pred3)
        {
            JPNodePredicateQuery query = new JPNodePredicateQuery(pred1, pred2, pred3);
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Get an array of all descendant nodes (including this node) of a given type
        /// </summary>
        public virtual IList<JPNode> Query2(ABLNodeType type, params ABLNodeType[] findTypes)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final EnumSet<ABLNodeType> tmp = EnumSet.of(type, findTypes);            
            HashSet<ABLNodeType> tmp = findTypes.Length == 0 ? new HashSet<ABLNodeType>() : new HashSet<ABLNodeType>(findTypes);
            tmp.Add(type);
            JPNodePredicateQuery query = new JPNodePredicateQuery(node => tmp.Contains(node.NodeType));
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Get an array of all descendant nodes (including this node) of a given type
        /// </summary>
        public virtual IList<JPNode> Query(ABLNodeType type, params ABLNodeType[] findTypes)
        {
            JPNodeQuery query = new JPNodeQuery(type, findTypes);
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Get an array of all descendant nodes (including this node) of a given type
        /// </summary>
        public virtual IList<JPNode> QueryMainFile(ABLNodeType type, params ABLNodeType[] findTypes)
        {
            JPNodeQuery query = new JPNodeQuery(false, true, null, type, findTypes);
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Get an array of all descendant statement nodes (including this node)
        /// </summary>
        public virtual IList<JPNode> QueryStateHead()
        {
            JPNodeQuery query = new JPNodeQuery(true);
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Get an array of all descendant nodes (including this node) of a given type
        /// </summary>
        public virtual IList<JPNode> QueryStateHead(ABLNodeType type, params ABLNodeType[] findTypes)
        {
            JPNodeQuery query = new JPNodeQuery(true, type, findTypes);
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Get an array of all descendant nodes of a given type within current statement
        /// </summary>
        public virtual IList<JPNode> QueryCurrentStatement(ABLNodeType type, params ABLNodeType[] findTypes)
        {
            JPNodeQuery query = new JPNodeQuery(false, false, this.Statement, type, findTypes);
            Walk(query);

            return query.Result;
        }

        /// <summary>
        /// Find the first hidden token after this node's last descendant. </summary>
        public virtual ProToken FindFirstHiddenAfterLastDescendant()
        {
            // There's no direct way to get a "hidden after" token,
            // so to find the hidden tokens after the current node's last
            // descendant, we find the next sibling of the current node,
            // find the first "natural" descendant of it (if it is not
            // itself natural), and then get its first hidden token.
            JPNode nextNatural = NextSibling;
            if (nextNatural == null)
            {
                return null;
            }
            if (nextNatural.NodeType != ABLNodeType.PROGRAM_TAIL)
            {
                nextNatural = nextNatural.FirstNaturalChild;
                if (nextNatural == null)
                {
                    return null;
                }
            }
            return nextNatural.HiddenFirst;
        }

        /// <summary>
        /// Find the first direct child with a given node type. </summary>
        public virtual JPNode FindDirectChild(ABLNodeType nodeType)
        {
            if (children == null)
            {
                return null;
            }
            foreach (JPNode node in children)
            {
                if (node.NodeType == nodeType)
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the first direct child with a given node type. </summary>
        public virtual JPNode FindDirectChild(int nodeType)
        {
            return FindDirectChild(ABLNodeType.GetNodeType(nodeType));
        }

        // *****************************
        // Various attributes management
        // *****************************

        public virtual int AttrGet(int key)
        {
            if ((attrMap != null) && attrMap.ContainsKey(key))
            {
                return attrMap[key];
            }
            switch (key)
            {
                case IConstants.ABBREVIATED:
                    return Abbreviated ? 1 : 0;
                default:
                    return 0;
            }            
        }

        public virtual void AttrSet(int key, int val)
        {
            if (attrMap == null)
            {
                InitAttrMap();
            }
            attrMap[key] = val;
        }

        /// <summary>
        /// Mark a node as "operator"
        /// </summary>
        public virtual void SetOperator()
        {
            AttrSet(IConstants.OPERATOR, IConstants.TRUE);
        }

        public virtual int State2
        {
            get
            {
                return AttrGet(IConstants.STATE2);
            }
        }

        /// <summary>
        /// Mark a node as a "statement head" </summary>
        public virtual void SetStatementHead()
        {
            AttrSet(IConstants.STATEHEAD, IConstants.TRUE);
        }

        /// <summary>
        /// Mark a node as a "statement head" </summary>
        public virtual int StatementHead
        {
            set
            {
                AttrSet(IConstants.STATEHEAD, IConstants.TRUE);
                if (value != 0)
                {
                    AttrSet(IConstants.STATE2, value);
                }
            }
        }

        /// <summary>
        /// Certain nodes will have a link to a Symbol, set by TreeParser. </summary>
        public virtual Symbol Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                this.symbol = value;
            }
        }

        public virtual bool HasTableBuffer()
        {
            return false;
        }

        public virtual bool HasBufferScope()
        {
            return bufferScope != null;
        }

        public virtual bool HasBlock()
        {
            return false;
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable public Block getBlock()
        public virtual Block Block
        {
            get => null;
            set => throw new System.ArgumentException("Not a Block node");
            
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable public TableBuffer getTableBuffer()
        public virtual TableBuffer TableBuffer
        {
            get => null;            
            set => throw new System.ArgumentException("Not a Block node");            
        }

        /// <summary>
        /// Get the FieldContainer (Frame or Browse) for a statement head node or a frame field reference. This value is set by
        /// TreeParser01. Head nodes for statements with the [WITH FRAME | WITH BROWSE] option have this value set. Is also
        /// available on the Field_ref node for #(Field_ref INPUT ...) and for #(USING #(Field_ref...)...).
        /// </summary>
        public virtual FieldContainer FieldContainer
        {
            get
            {
                return container;
            }
            set
            {
                this.container = value;
            }
        }

        public virtual BufferScope BufferScope
        {
            get
            {
                return bufferScope;
            }
            set
            {
                this.bufferScope = value;
            }
        }                

        public virtual bool HasProparseDirective(string directive)
        {
            ProToken tok = HiddenBefore;
            char[] stringSeparators = { ',' };

            while (tok != null)
            {
                if (tok.NodeType == ABLNodeType.PROPARSEDIRECTIVE)
                {
                    string str = tok.Text.Trim();
                    if (str.StartsWith("prolint-nowarn(", StringComparison.Ordinal) && str[str.Length - 1] == ')')
                    {                        
                        foreach (string rule in str.Substring(15, (str.Length - 1) - 15).Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
                        {
                            if (rule.Equals(directive))
                            {
                                return true;
                            }
                        }
                    }
                }
                tok = tok.HiddenBefore;
            }
            // If token has been generated by the parser (ie synthetic token), then we look for hidden token attached to the
            // first child
            if (token.Synthetic)
            {
                JPNode child = FirstChild;
                if ((child != null) && (child.HasProparseDirective(directive)))
                {
                    return true;
                }
                // And for synthetic ASSIGN statements, we have to look for the first grandchild
                // See root node of assignstate2
                if ((child != null) && (token.NodeType == ABLNodeType.ASSIGN))
                {
                    child = child.FirstChild;
                    if ((child != null) && child.HasProparseDirective(directive))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public virtual ProToken HiddenFirst
        {
            get
            {
                // Some day, I'd like to change the structure for the hidden tokens,
                // so that nodes only store a reference to "first before", and each of those
                // only store a pointer to "next".
                ProToken t = HiddenBefore;
                if (t != null)
                {
                    ProToken ttemp = t;
                    while (ttemp != null)
                    {
                        t = ttemp;
                        ttemp = t.HiddenBefore;
                    }
                }
                return t;
            }
        }

        public virtual LinkedList<ProToken> HiddenTokens
        {
            get
            {
                LinkedList<ProToken> ret = new LinkedList<ProToken>();
                ProToken tkn = HiddenBefore;
                while (tkn != null)
                {
                    ret.AddFirst(tkn);
                    tkn = tkn.HiddenBefore;
                }
                return ret;
            }
        }

        /// <summary>
        /// Return self if statehead, otherwise returns enclosing statehead. </summary>
        public virtual JPNode Statement
        {
            get
            {
                JPNode n = this;
                while (n != null && !n.StateHead)
                {
                    n = n.Parent;
                }
                return n;
            }
        }

        /// <param name="type"> ABLNodeType to search for </param>
        /// <returns> Parent node within the current statement of the given type. Null if not found </returns>
        public virtual JPNode GetParent(ABLNodeType type)
        {
            if (type == NodeType)
            {
                return this;
            }
            if (StateHead)
            {
                return null;
            }
            if (Parent != null)
            {
                return Parent.GetParent(type);
            }
            return null;
        }

        /// <returns> The full name of the annotation, or an empty string is node is not an annotation </returns>
        public virtual string AnnotationName
        {
            get
            {
                if (NodeType != ABLNodeType.ANNOTATION)
                {
                    return "";
                }
                StringBuilder annName = new StringBuilder(token.Text.Substring(1));
                JPNode tok = FirstChild;
                while ((tok != null) && (tok.NodeType != ABLNodeType.PERIOD) && (tok.NodeType != ABLNodeType.LEFTPAREN))
                {
                    annName.Append(tok.Text);
                    tok = tok.NextSibling;
                }

                return annName.ToString();
            }
        }



        private void InitAttrMap()
        {
            if (attrMap == null)
            {
                attrMap = new Dictionary<int, int>();
            }
        }

        public virtual bool Abbreviated
        {
            get
            {
                return token.NodeType.IsAbbreviated(Text);
            }
        }

        /// <returns> True if token is part of an editable section in AppBuilder managed code </returns>
        public virtual bool EditableInAB
        {
            get
            {
                return FirstNaturalChild.token.EditableInAB;
            }
        }

        /// <summary>
        /// Is this a natural node (from real source text)? If not, then it is a synthetic node, added just for tree structure.
        /// </summary>
        public virtual bool Natural
        {
            get
            {
                return token.Natural;
            }
        }

        /// <summary>
        /// Does this node have the Proparse STATEHEAD attribute? </summary>
        public virtual bool StateHead
        {
            get
            {
                return AttrGet(IConstants.STATEHEAD) == IConstants.TRUE;
            }
        }


        /// <summary>
        /// Used by TreeParser in order to assign Symbol to the right node
        /// Never returns null
        /// </summary>
        public virtual JPNode IdNode
        {
            get
            {
                // TODO Probably a better way to do that...
                if ((NodeType == ABLNodeType.DEFINE) || (NodeType == ABLNodeType.BUFFER) || (NodeType == ABLNodeType.BEFORETABLE))
                {
                    foreach (JPNode child in DirectChildren)
                    {
                        if (child.NodeType == ABLNodeType.ID)
                        {
                            return child;
                        }
                    }
                    return this;
                }
                else if ((NodeType == ABLNodeType.NEW) || (NodeType == ABLNodeType.OLD))
                {
                    JPNode nxt = NextNode;
                    if ((nxt != null) && (nxt.NodeType == ABLNodeType.ID))
                    {
                        return nxt;
                    }
                    if ((nxt != null) && (nxt.NodeType == ABLNodeType.BUFFER))
                    {
                        nxt = nxt.NextNode;
                        if ((nxt != null) && (nxt.NodeType == ABLNodeType.ID))
                        {
                            return nxt;
                        }
                        else
                        {
                            return this;
                        }
                    }
                    return this;
                }
                else if (NodeType == ABLNodeType.TABLEHANDLE)
                {
                    if ((NextNode != null) && (NextNode.NodeType == ABLNodeType.ID))
                    {
                        return NextNode;
                    }
                    else
                    {
                        return this;
                    }
                }
                else
                {
                    return this;
                }
            }
        }

        /// <returns> Number total number of JPNode objects  </returns>
        public virtual int Size
        {
            get
            {
                int sz = 1;
                foreach (JPNode node in DirectChildren)
                {
                    sz += node.Size;
                }
                return sz;
            }
        }

        /// <returns> Number total number of natural JPNode objects  </returns>
        public virtual int NaturalSize
        {
            get
            {
                int sz = Natural ? 1 : 0;
                foreach (JPNode node in DirectChildren)
                {
                    sz += node.NaturalSize;
                }
                return sz;
            }
        }

        public override string ToString()
        {
            StringBuilder buff = new StringBuilder();
            buff.Append(token.NodeType).Append(" \"").Append(Text).Append("\" F").Append(FileIndex).Append('/').Append(Line).Append(':').Append(Column);
            return buff.ToString();
        }

        /// <summary>
        /// Get the full, preprocessed text from a node. When run on top node, the result is very comparable to
        /// COMPILE..PREPROCESS. This is the same as the old C++ Proparse API writeNode(). Also see org.joanju.proparse.Iwdiff.
        /// </summary>
        public virtual string ToStringFulltext()
        {
            ICallback<IList<JPNode>> callback = new FlatListBuilder();
            Walk(callback);
            IList<JPNode> list = callback.Result;
            StringBuilder bldr = new StringBuilder();
            foreach (JPNode node in list)
            {
                StringBuilder hiddenText = new StringBuilder();
                ProToken tok = node.HiddenBefore;
                while (tok != null)
                {
                    if ((tok.NodeType == ABLNodeType.COMMENT) || (tok.NodeType == ABLNodeType.WS))
                    {
                        hiddenText.Insert(0, tok.Text);
                    }
                    tok = tok.HiddenBefore;
                }
                bldr.Append(hiddenText.ToString());
                bldr.Append(node.Text);
            }

            return bldr.ToString();
        }

        /// <summary>
        /// Walk down the tree from the input node
        /// </summary>
        public virtual void Walk<T1>(ICallback<T1> callback)
        {
            if (AttrGet(IConstants.OPERATOR) == IConstants.TRUE)
            {
                // Assuming OPERATORs only have two children (which should be the case)
                FirstChild.Walk(callback);
                callback.VisitNode(this);
                FirstChild.NextSibling.Walk(callback);
            }
            else
            {
                if (callback.VisitNode(this))
                {
                    foreach (JPNode child in DirectChildren)
                    {
                        child.Walk(callback);
                    }
                }
            }
        }

        public virtual string AllLeadingHiddenText()
        {
            StringBuilder ret = new StringBuilder();
            ProToken t = HiddenBefore;
            while (t != null)
            {
                ret.Insert(0, t.Text);
                t = t.HiddenBefore;
            }
            return ret.ToString();
        }

        public virtual JPNode PreviousStatement
        {
            set
            {
                this.previousStatement = value;
            }
            get
            {
                return previousStatement;
            }
        }


        public virtual JPNode NextStatement
        {
            set
            {
                this.nextStatement = value;
            }
            get
            {
                return nextStatement;
            }
        }


        public virtual Block InBlock
        {
            set
            {
                this.inBlock = value;
            }
        }

        public virtual Block EnclosingBlock
        {
            get
            {
                return inBlock;
            }
        }
        public class Builder
        {
            private ProToken tok;
            private IParseTree ctx;
            private Builder right;
            private Builder down;
            private bool stmt;
            private ABLNodeType stmt2;
            private bool @operator;
            private FieldType tabletype;
            private string className;
            private bool inline;

            public Builder(ProToken tok)
            {
                this.tok = tok;
            }

            public Builder(ABLNodeType type) : this((new ProToken.Builder(type, "")).SetSynthetic(true).Build())
            {
            }

            public virtual Builder UpdateToken(ProToken tok)
            {
                this.tok = tok;
                return this;
            }

            public virtual Builder SetRuleNode(IParseTree ctx)
            {
                this.ctx = ctx;
                return this;
            }

            public virtual Builder UnsetRuleNode()
            {
                this.ctx = null;
                return this;
            }

            public virtual Builder SetRight(Builder right)
            {
                this.right = right;
                return this;
            }

            public virtual Builder SetDown(Builder down)
            {
                this.down = down;
                return this;
            }

            public virtual Builder Down => down;
            public virtual Builder Right => right;
            
            public virtual Builder ChangeType(ABLNodeType type)
            {
                this.tok.NodeType = type;
                return this;
            }

            public virtual Builder Last => (right == null) ? this : right.Last;

            public virtual Builder SetStatement()
            {
                this.stmt = true;
                return this;
            }

            public virtual Builder SetStatement(ABLNodeType stmt2)
            {
                this.stmt = true;
                this.stmt2 = stmt2;
                return this;
            }

            public virtual Builder SetOperator()
            {
                this.@operator = true;
                return this;
            }

            public virtual Builder SetStoreType(FieldType tabletype)
            {
                this.tabletype = tabletype;
                return this;
            }

            public virtual Builder SetClassname(string name)
            {
                this.className = name;
                return this;
            }

            public virtual ProToken Token
            {
                get
                {
                    return tok;
                }
            }
            public virtual ABLNodeType NodeType
            {
                get
                {
                    return tok.NodeType;
                }
            }

            public virtual Builder SetInlineVar()
            {
                this.inline = true;
                return this;
            }

            /// <summary>
            /// Transforms <pre>x1 - x2 - x3 - x4</pre> into
            /// <pre>
            /// x1 - x3 - x4
            /// |
            /// x2
            /// </pre>
            /// Then to: <pre>
            /// x1 - x4
            /// |
            /// x2 - x3
            /// </pre>
            /// @return
            /// </summary>
            public virtual Builder MoveRightToDown()
            {
                if (this.right == null)
                {
                    throw new System.NullReferenceException();
                }
                if (this.down == null)
                {
                    this.down = this.right;
                    this.right = this.down.right;
                    this.down.right = null;
                }
                else
                {
                    Builder target = this.down;
                    while (target.Right != null)
                    {
                        target = target.Right;
                    }
                    target.right = this.right;
                    this.right = target.right.right;
                    target.right.right = null;
                }

                return this;
            }

            public virtual JPNode Build(ParserSupport support)
            {
                return Build(support, null, 0);
            }

            private JPNode Build(ParserSupport support, JPNode up, int num)
            {
                JPNode node;
                bool hasChildren = (down != null) && ((down.NodeType != ABLNodeType.EMPTY_NODE) || down.right != null || down.down != null);

                switch (tok.NodeType.Type)
                {
                    case ABLNodeType.EMPTY_NODE_TYPE:
                        throw new System.InvalidOperationException("Empty node can't generate JPNode");                    
                    case Proparse.RECORD_NAME:
                        node = new RecordNameNode(tok, up, num, hasChildren);
                        break;
                    case Proparse.Field_ref:
                        node = new FieldRefNode(tok, up, num, hasChildren);
                        break;
                    case Proparse.Program_root:
                        node = new ProgramRootNode(tok, up, num, hasChildren);
                        break;
                    case Proparse.FOR:
                        // FOR in 'DEFINE BUFFER x FOR y' is not a BlockNode
                        node = stmt ? new BlockNode(tok, up, num, hasChildren) : new JPNode(tok, up, num, hasChildren);
                        break;
                    case Proparse.TYPE_NAME:
                        node = new TypeNameNode(tok, up, num, hasChildren, className);
                        break;
                    case Proparse.DO:
                    case Proparse.REPEAT:
                    case Proparse.FUNCTION:
                    case Proparse.PROCEDURE:
                    case Proparse.CONSTRUCTOR:
                    case Proparse.DESTRUCTOR:
                    case Proparse.METHOD:
                    case Proparse.CANFIND:
                    case Proparse.CATCH:
                    case Proparse.ON:
                    case Proparse.Property_getter:
                    case Proparse.Property_setter:
                        node = new BlockNode(tok, up, num, hasChildren);
                        break;
                    default:
                        node = new JPNode(tok, up, num, hasChildren);
                        break;

                }                
               
                if (stmt)
                {
                    node.StatementHead = stmt2 == null ? 0 : stmt2.Type;
                }
                if (@operator)
                {
                    node.SetOperator();
                }
                if (inline)
                {
                    node.AttrSet(IConstants.INLINE_VAR_DEF, IConstants.TRUE);
                }
                if (tabletype != null)
                {
                    switch (tabletype.innerEnumValue)
                    {
                        case FieldType.InnerEnum.DBTABLE:
                            node.AttrSet(IConstants.STORETYPE, IConstants.ST_DBTABLE);
                            break;
                        case FieldType.InnerEnum.TTABLE:
                            node.AttrSet(IConstants.STORETYPE, IConstants.ST_TTABLE);
                            break;
                        case FieldType.InnerEnum.WTABLE:
                            node.AttrSet(IConstants.STORETYPE, IConstants.ST_WTABLE);
                            break;
                        case FieldType.InnerEnum.VARIABLE:
                            // Never happens
                            break;
                    }
                }

                if ((ctx != null) && (support != null))
                {
                    support.PushNode(ctx, node);
                }
                // Attach first non-empty builder node to node.down
                Builder tmp = down;
                Builder tmpRight = null;
                while (tmp != null)
                {
                    if (tmp.NodeType == ABLNodeType.EMPTY_NODE)
                    {
                        // Safety net: EMPTY_NODE can't have children
                        if (tmp.down != null)
                        {
                            throw new System.InvalidOperationException("Found EMPTY_NODE with children (first is " + tmp.down.NodeType);
                        }
                        tmp = tmp.right;
                    }
                    else
                    {
                        node.children.Add(tmp.Build(support, node, 0));
                        tmpRight = tmp.right;
                        tmp = null;
                    }
                }
                int numCh = 1;
                // Same for node.right
                while (tmpRight != null)
                {
                    if (tmpRight.NodeType == ABLNodeType.EMPTY_NODE)
                    {
                        // Safety net: EMPTY_NODE can't have children
                        if (tmpRight.down != null)
                        {
                            throw new System.InvalidOperationException("Found EMPTY_NODE with children (first is " + tmpRight.down.NodeType);
                        }
                        tmpRight = tmpRight.right;
                    }
                    else
                    {
                        node.children.Add(tmpRight.Build(support, node, numCh++));
                        tmpRight = tmpRight.right;
                    }
                }

                return node;
            }
        }

    }

}

