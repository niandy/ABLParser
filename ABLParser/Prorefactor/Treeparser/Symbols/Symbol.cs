using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    public abstract class Symbol : ISymbol
    {
        private int allRefsCount = 0;
        private int numReads = 0;
        private int numWrites = 0;
        private int numRefd = 0;
        private bool parameter = false;

        private ISymbol like;

        // We store the DEFINE node if available and sensible. If defined in a syntax where there is no DEFINE node briefly
        // preceeding the ID node, then we store the ID node. If this is a schema symbol, then this member is null.
        private JPNode defNode;

        // Stores the full name, original (mixed) case as in definition
        private readonly string name;

        public Symbol(string name, TreeParserSymbolScope scope) : this(name, scope, false)
        {
        }

        public Symbol(string name, TreeParserSymbolScope scope, bool parameter)
        {
            this.name = name;
            this.Scope = scope;
            this.parameter = parameter;
            scope.AddSymbol(this);
        }

        public JPNode DefinitionNode
        {
            set
            {
                defNode = value;
            }
        }

        public int AllRefsCount
        {
            get
            {
                return allRefsCount;
            }
        }

        public int NumReads
        {
            get
            {
                return numReads;
            }
        }

        public int NumWrites
        {
            get
            {
                return numWrites;
            }
        }

        public int NumReferenced
        {
            get
            {
                return numRefd;
            }
        }

        public JPNode DefineNode
        {
            get
            {
                return defNode;
            }
        }


        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public TreeParserSymbolScope Scope { get; }

        public virtual void NoteReference(ContextQualifier contextQualifier)
        {
            if (contextQualifier == null)
            {
                return;
            }
            allRefsCount++;
            if (ContextQualifier.IsRead(contextQualifier))
            {
                numReads++;
            }
            if (ContextQualifier.IsWrite(contextQualifier))
            {
                numWrites++;
            }
            if (ContextQualifier.IsReference(contextQualifier))
            {
                numRefd++;
            }
        }

        public override string ToString()
        {
            return FullName;
        }

        public abstract string FullName { get; }

        public virtual bool Parameter
        {
            set
            {
                this.parameter = value;
            }
            get
            {
                return parameter;
            }
        }


        public ISymbol LikeSymbol
        {
            set
            {
                this.like = value;
            }
            get
            {
                return like;
            }
        }

        public abstract int ProgressType { get; }

        internal abstract bool IsInstanceOfType(Symbol s);
    }

}
