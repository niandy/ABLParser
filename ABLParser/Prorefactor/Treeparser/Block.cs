using System.Linq;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.NodeTypes;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.Prorefactor.Treeparser.Symbols.Widgets;

namespace ABLParser.Prorefactor.Treeparser
{
    using ABLParser.Prorefactor.Core.Schema;
    using ABLParser.Prorefactor.Proparser.Antlr;
    using System.Collections.Generic;

    /// <summary>
    /// For keeping track of blocks, block attributes, and the things that are scoped within those blocks - especially buffer
    /// scopes.
    /// </summary>
    public class Block
    {
        private readonly BlockNode blockStatementNode;
        private readonly IList<Frame> frames = new List<Frame>();
        private Block parent;
        private Frame defaultFrame = null;
        private readonly ISet<BufferScope> bufferScopes = new HashSet<BufferScope>();

        /// <summary>
        /// The SymbolScope for a block is going to be the root program scope, unless the block is inside a method
        /// (function/trigger/procedure).
        /// </summary>
        private readonly TreeParserSymbolScope symbolScope;

        /// <summary>
        /// For constructing nested blocks </summary>
        public Block(Block parent, BlockNode node)
        {
            this.blockStatementNode = node;
            this.parent = parent;
            this.symbolScope = parent.symbolScope;
        }

        /// <summary>
        /// For constructing a root (method root or program root) block.
        /// </summary>
        /// <param name="symbolScope"> </param>
        /// <param name="node"> Is the Program_root if this is the program root block. </param>
        public Block(TreeParserSymbolScope symbolScope, BlockNode node)
        {
            this.blockStatementNode = node;
            this.symbolScope = symbolScope;
            if (symbolScope.ParentScope != null)
            {
                this.parent = symbolScope.ParentScope.RootBlock;
            }
            else
            {
                this.parent = null; // is program-block
            }
        }

        /// <summary>
        /// Add a reference to a BufferScope to this and all outer blocks. These references are required for duplicating
        /// Progress's scope and "raise scope" behaviours. BufferScope references are not added up past the symbol's scope.
        /// </summary>
        public virtual void AddBufferScopeReferences(BufferScope bufferScope)
        {
            // References do not get added to DO blocks.
            if (blockStatementNode.NodeType != ABLNodeType.DO)
            {
                bufferScopes.Add(bufferScope);
            }
            if (parent != null && bufferScope.Symbol.Scope.RootBlock != this)
            {
                parent.AddBufferScopeReferences(bufferScope);
            }
        }

        /// <summary>
        /// Called by Frame.setFrameScopeBlock() - not intended to be called by any client code. This should only be called by
        /// the Frame object itself. Adds a frame to this or the appropriate parent block. Returns the scoping block. Frames
        /// are scoped to FOR and REPEAT blocks, or else to a symbol scoping block. They may also be scoped with a DO WITH
        /// FRAME block, but that is handled elsewhere.
        /// </summary>
        public virtual Block AddFrame(Frame frame)
        {
            if (CanScopeFrame())
            {
                frames.Add(frame);
                return this;
            }
            else
            {
                return parent.AddFrame(frame);
            }
        }

        /// <summary>
        /// A "hidden cursor" is a BufferScope which has no side-effects on surrounding blocks like strong, weak, and reference
        /// scopes do. These are used within a CAN-FIND function. (2004.Sep:John: Maybe in triggers too? Haven't checked.)
        /// </summary>
        /// <param name="node"> The RECORD_NAME node. Must have the BufferSymbol linked to it already. </param>
        public virtual void AddHiddenCursor(RecordNameNode node)
        {
            TableBuffer symbol = node.TableBuffer;
            BufferScope buff = new BufferScope(this, symbol, BufferScope.Strength.HIDDEN_CURSOR);
            bufferScopes.Add(buff);
            // Note the difference compared to addStrong and addWeak - we don't add
            // BufferScope references to the enclosing blocks.
            node.BufferScope = buff;
        }

        /// <summary>
        /// Create a "strong" buffer scope. This is called within a DO FOR or REPEAT FOR statement. A STRONG scope prevents the
        /// scope from being raised to an enclosing block. Note that the compiler performs additional checks here that we
        /// don't.
        /// </summary>
        /// <param name="node"> The RECORD_NAME node. It must already have the BufferSymbol linked to it. </param>
        public virtual void AddStrongBufferScope(RecordNameNode node)
        {
            TableBuffer symbol = node.TableBuffer;
            BufferScope buff = new BufferScope(this, symbol, BufferScope.Strength.STRONG);
            bufferScopes.Add(buff);
            AddBufferScopeReferences(buff);
            node.BufferScope = buff;
        } // addStrongBufferScope

        /// <summary>
        /// Create a "weak" buffer scope. This is called within a FOR or PRESELECT statement.
        /// </summary>
        /// <param name="symbol"> The RECORD_NAME node. It must already have the BufferSymbol linked to it. </param>
        public virtual BufferScope AddWeakBufferScope(TableBuffer symbol)
        {
            BufferScope buff = GetBufferScope(symbol, BufferScope.Strength.WEAK);
            if (buff == null)
            {
                buff = new BufferScope(this, symbol, BufferScope.Strength.WEAK);
            }
            // Yes, add reference to outer blocks, even if we got this buffer from
            // an outer block. Might have blocks in between which need the reference
            // to be added.
            AddBufferScopeReferences(buff);
            bufferScopes.Add(buff); // necessary in case this is DO..PRESELECT block
            return buff;
        } // addWeakBufferScope

        /// <summary>
        /// Can a buffer reference be scoped to this block? </summary>
        private bool CanScopeBufferReference(TableBuffer symbol)
        {
            // REPEAT, FOR, and Program_root blocks can scope a buffer.
            switch (blockStatementNode.Type)
            {
                case Proparse.REPEAT:
                case Proparse.FOR:
                case Proparse.Program_root:
                    return true;
            }
            // If this is the root block for the buffer's symbol, then the scope
            // cannot be any higher.
            if (symbol.Scope.RootBlock == this)
            {
                return true;
            }
            return false;
        } // canScopeBufferReference

        /// <summary>
        /// Can a frame be scoped to this block? </summary>
        private bool CanScopeFrame()
        {
            if ((blockStatementNode.NodeType == ABLNodeType.REPEAT) || (blockStatementNode.NodeType == ABLNodeType.FOR))
            {
                return true;
            }
            return RootBlock;
        }

        /// <summary>
        /// Find nearest BufferScope for a BufferSymbol, if any </summary>
        private BufferScope FindBufferScope(TableBuffer symbol)
        {
            foreach (BufferScope buff in bufferScopes)
            {
                if (buff.Symbol != symbol)
                {
                    continue;
                }
                if (buff.Block == this)
                {
                    return buff;
                }
            }
            if (parent != null && symbol.Scope.RootBlock != this)
            {
                return parent.FindBufferScope(symbol);
            }
            return null;
        }

        /// <summary>
        /// Get the buffers that are scoped to this block </summary>
        public virtual TableBuffer[] GetBlockBuffers()
        {

            // We can't just return bufferScopes, because it also contains
            // references to BufferScope objects which are scoped to child blocks.
            ISet<TableBuffer> set = new HashSet<TableBuffer>();
            foreach (BufferScope buff in bufferScopes)
            {
                if (buff.Block == this)
                {
                    set.Add(buff.Symbol);
                }
            }
            return (TableBuffer[])set.ToArray();

        } // getBlockBuffers

        /// <summary>
        /// Find or create a buffer for the input BufferSymbol </summary>
        public virtual BufferScope GetBufferForReference(TableBuffer symbol)
        {
            BufferScope buffer = GetBufferScope(symbol, BufferScope.Strength.REFERENCE);
            if (buffer == null)
            {
                buffer = GetBufferForReferenceSub(symbol);
            }
            // Yes, add reference to outer blocks, even if we got this buffer from
            // an outer block. Might have blocks in between which need the reference
            // to be added.
            AddBufferScopeReferences(buffer);
            return buffer;
        } // getBufferForReference


        private BufferScope GetBufferForReferenceSub(TableBuffer symbol)
        {
            if (!CanScopeBufferReference(symbol))
            {
                return parent.GetBufferForReferenceSub(symbol);
            }
            return new BufferScope(this, symbol, BufferScope.Strength.REFERENCE);
        }

        /// <summary>
        /// Attempt to get or raise a BufferScope in this block. </summary>
        private BufferScope GetBufferScope(TableBuffer symbol, BufferScope.Strength creating)
        {
            // First try to find an existing buffer scope for this symbol.
            BufferScope buff = FindBufferScope(symbol);
            if (buff != null)
            {
                return buff;
            }
            return GetBufferScopeSub(symbol, creating);
        }

        private BufferScope GetBufferScopeSub(TableBuffer symbol, BufferScope.Strength creating)
        {
            // First try to get a buffer from outermost blocks.
            if (parent != null && symbol.Scope.RootBlock != this)
            {
                BufferScope buff = parent.GetBufferScopeSub(symbol, creating);
                if (buff != null)
                {
                    return buff;
                }
            }
            BufferScope raiseBuff = null;
            foreach (BufferScope buff in bufferScopes)
            {
                if (buff.Symbol != symbol)
                {
                    continue;
                }
                // Note that if it was scoped to this (or an outer block), then
                // we would have already found it with findBufferScope.
                // If it's strong scoped (to a child block, or we would have found it already),
                // then we can't raise the scope to here.
                if (buff.Strong)
                {
                    return null;
                }
                if (creating == BufferScope.Strength.REFERENCE || buff.GetStrength() == BufferScope.Strength.REFERENCE)
                {
                    raiseBuff = buff;
                }
            }
            if (raiseBuff == null)
            {
                return null;
            }
            // Can this block scope a reference to this buffer symbol?
            if (!CanScopeBufferReference(symbol))
            {
                return null;
            }
            // We are creating, or there exists, more than one sub-BufferScope, and at least
            // one is a REFERENCE. We raise the BufferScope to this block.
            foreach (BufferScope buff in bufferScopes)
            {
                if (buff.Symbol != symbol)
                {
                    continue;
                }
                buff.Block = this;
                buff.SetStrength(BufferScope.Strength.REFERENCE);
            }
            return raiseBuff;
        } // getBufferScopeSub

        /// <summary>
        /// From the nearest frame scoping block, get the default (possibly unnamed) frame if it exists. Returns null if no
        /// default frame has been established yet.
        /// </summary>
        public virtual Frame DefaultFrame
        {
            get
            {
                if (defaultFrame != null)
                {
                    return defaultFrame;
                }
                if (!CanScopeFrame())
                {
                    return parent.DefaultFrame;
                }
                return null;
            }
        }

        /// <summary>
        /// Get a copy of the list of frames scoped to this block. </summary>
        public virtual IList<Frame> Frames
        {
            get
            {
                return new List<Frame>(frames);
            }
        }

        /// <summary>
        /// Get the node for this block. Returns a node of one of these types:
        /// Program_root/DO/FOR/REPEAT/EDITING/PROCEDURE/FUNCTION/ON/TRIGGERS.
        /// </summary>
        public virtual BlockNode Node
        {
            get
            {
                return blockStatementNode;
            }
        }

        /// <summary>
        /// This returns the <em>block of the parent scope</em>. </summary>
        public virtual Block Parent
        {
            get
            {
                return parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public virtual TreeParserSymbolScope SymbolScope
        {
            get
            {
                return symbolScope;
            }
        }

        /// <summary>
        /// Is a buffer scoped to this or any parent of this block. </summary>
        public virtual bool IsBufferLocal(BufferScope buff)
        {
            for (Block block = this; block.parent != null; block = block.parent)
            {
                if (buff.Block == block)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A method-block is a block for a function/trigger/internal-procedure. </summary>
        public virtual bool MethodBlock
        {
            get
            {
                return (symbolScope.RootBlock == this) && (symbolScope.ParentScope != null);
            }
        }

        /// <summary>
        /// The program-block is the outer program block (not internal procedure block) </summary>
        public virtual bool ProgramBlock
        {
            get
            {
                return (symbolScope.RootBlock == this) && (symbolScope.ParentScope == null);
            }
        }

        /// <summary>
        /// A root-block is the root block for any SymbolScope whether program, function, trigger, or internal procedure.
        /// </summary>
        public virtual bool RootBlock
        {
            get
            {
                return symbolScope.RootBlock == this;
            }
        }

        /// <summary>
        /// General lookup for Field or Variable. Does not guarantee uniqueness. That job is left to the compiler.
        /// </summary>
        public virtual FieldLookupResult LookupField(string name, bool getBufferScope)
        {
            FieldLookupResult.Builder result = new FieldLookupResult.Builder();
            TableBuffer tableBuff;
            int lastDot = name.LastIndexOf('.');
            // Variable or unqualified field
            if (lastDot == -1)
            {
                // Variables, FieldLevelWidgets, and Events come first.
                Variable v = symbolScope.LookupVariable(name);
                if (v != null)
                {
                    return result.SetSymbol(v).Build();
                }

                IFieldLevelWidget flw = symbolScope.LookupFieldLevelWidget(name);
                if (flw != null)
                {
                    return result.SetSymbol(flw).Build();
                }

                Symbol s = symbolScope.LookupSymbol(Proparse.EVENT, name);
                if (s != null)
                {
                    return result.SetSymbol(s).Build();
                }
                // Lookup unqualified field by buffers in nearest scopes.
                result = LookupUnqualifiedField(name);

                // Lookup unqualified field by any table.
                // The compiler expects the name to be unique
                // amongst all schema and temp/work tables. We don't check for
                // uniqueness, we just take the first we find.
                if (result == null)
                {
                    IField field;
                    result = new FieldLookupResult.Builder();
                    field = symbolScope.RootScope.LookupUnqualifiedField(name);
                    if (field != null)
                    {
                        tableBuff = symbolScope.RootScope.GetLocalTableBuffer(field.Table);
                    }
                    else
                    {
                        field = symbolScope.RootScope.RefactorSession.Schema.LookupUnqualifiedField(name);
                        if (field == null)
                        {
                            return null;
                        }
                        tableBuff = symbolScope.GetUnnamedBuffer(field.Table);
                    }
                    result.SetSymbol(tableBuff.GetFieldBuffer(field));
                }
                result.SetUnqualified();
                if (name.Length < result.Field.Name.Length)
                {
                    result.SetAbbreviated();
                }
            }
            else
            { // Qualified Field Name
                string fieldPart = name.Substring(lastDot + 1);
                string tablePart = name.Substring(0, lastDot);
                tableBuff = symbolScope.GetBufferSymbol(tablePart);
                if (tableBuff == null)
                {
                    return null;
                }
                IField field = tableBuff.Table.LookupField(fieldPart);
                if (field == null)
                {
                    return null;
                }
                result.SetSymbol(tableBuff.GetFieldBuffer(field));
                if (fieldPart.Length < result.Field.Name.Length)
                {
                    result.SetAbbreviated();
                }
                // Temp/work/buffer names can't be abbreviated, but direct refs to schema can be.
                if (tableBuff.DefaultSchema)
                {
                    string[] parts = tablePart.Split('.');
                    string tblPart = parts[parts.Length - 1];
                    if (tblPart.Length < tableBuff.Table.GetName().Length)
                    {
                        result.SetAbbreviated();
                    }
                }
            } // if ... Qualified Field Name
            if (getBufferScope)
            {
                BufferScope buffScope = GetBufferForReference(result.Field.Buffer);
                result.SetBufferScope(buffScope);
            }
            return result.Build();
        } // lookupField()


        /// <summary>
        /// Find a field based on buffers which are referenced in nearest enclosing blocks. Note that the compiler enforces
        /// uniqueness here. We don't, we just find the first possible and return it.
        /// </summary>
        protected internal virtual FieldLookupResult.Builder LookupUnqualifiedField(string name)
        {
            IDictionary<TableBuffer, BufferScope> buffs = new Dictionary<TableBuffer, BufferScope>();
            FieldLookupResult.Builder result = null;
            foreach (BufferScope buff in bufferScopes)
            {
                TableBuffer symbol = buff.Symbol;
                if (buff.Block == this)
                {
                    buffs[symbol] = buff;
                    continue;
                }                
                if (buffs.TryGetValue(symbol, out BufferScope buffSeen))
                {
                    if (buffSeen.Block == this)
                    {
                        continue;
                    }
                    if (buffSeen.Strong)
                    {
                        continue;
                    }
                }
                buffs[symbol] = buff;
            }
            foreach (BufferScope buffScope in buffs.Values)
            {
                TableBuffer tableBuff = buffScope.Symbol;
                // Check for strong scope preventing raise to this block.
                if (buffScope.Strong && !IsBufferLocal(buffScope))
                {
                    continue;
                }
                // Weak scoped named buffers don't get raised for field references.
                if (buffScope.Weak && !IsBufferLocal(buffScope) && !tableBuff.Default)
                {
                    continue;
                }
                IField field = tableBuff.Table.LookupField(name);
                if (field == null)
                {
                    continue;
                }
                // The buffers aren't sorted, but "named" buffers and temp/work
                // tables take priority. Default buffers for schema take lower priority.
                // So, if we got a named buffer or work/temp table, we return with it.
                // Otherwise, we just hang on to the result until the loop is done.
                result = (new FieldLookupResult.Builder()).SetSymbol(tableBuff.GetFieldBuffer(field));
                if (!tableBuff.DefaultSchema)
                {
                    return result;
                }
            }
            if (result != null)
            {
                return result;
            }
            // Resolving names is done by looking at inner blocks first, then outer blocks.
            if (parent != null)
            {
                return parent.LookupUnqualifiedField(name);
            }
            return null;
        } // lookupUnqualifiedField

        /// <summary>
        /// Explicitly set the default frame for this block. This should only be called by the Frame object itself. This is
        /// especially important to be called for DO WITH FRAME statements because DO blocks do not normally scope frames. This
        /// should also be called for REPEAT WITH FRAME and FOR WITH FRAME blocks.
        /// </summary>
        public virtual void SetDefaultFrameExplicit(Frame value)
        {
            this.defaultFrame = value;
            frames.Add(value);
        }

        /// <summary>
        /// In the nearest frame scoping block, set the default implicit (unnamed) frame. This should only be called by the
        /// Frame object itself. Returns the Block that scopes the frame.
        /// </summary>
        public virtual Block SetDefaultFrameImplicit(Frame frame)
        {
            if (CanScopeFrame())
            {
                this.defaultFrame = frame;
                frames.Add(frame);
                return this;
            }
            else
            {
                return parent.SetDefaultFrameImplicit(frame);
            }
        }

        public override string ToString()
        {
            return (new StringBuilder("Block ")).Append(blockStatementNode.ToString()).ToString();
        }
    }

}
