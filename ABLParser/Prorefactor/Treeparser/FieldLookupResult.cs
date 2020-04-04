using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Treeparser.Symbols;

namespace ABLParser.Prorefactor.Treeparser
{
    /// <summary>
    /// For field lookups, we need to be able to pass back the BufferScope object as well as the Field object.
    /// </summary>
    public class FieldLookupResult
    {
        private bool abbreviated;
        private bool unqualified;
        private BufferScope bufferScope;
        private ISymbol symbol;

        private FieldLookupResult()
        {
            // Use Builder object
        }

        public virtual bool Abbreviated => abbreviated;

        public virtual bool Unqualified => unqualified;

        public virtual BufferScope BufferScope => bufferScope;

        public virtual ISymbol Symbol => symbol;

        public class Builder
        {
            internal bool isAbbreviated;
            internal bool isUnqualified;
            internal BufferScope bufferScope;
            internal ISymbol symbol;

            public virtual Builder SetAbbreviated()
            {
                this.isAbbreviated = true;
                return this;
            }

            public virtual Builder SetUnqualified()
            {
                this.isUnqualified = true;
                return this;
            }

            public virtual Builder SetBufferScope(BufferScope bufferScope)
            {
                this.bufferScope = bufferScope;
                return this;
            }

            public virtual Builder SetSymbol(ISymbol symbol)
            {
                this.symbol = symbol;
                return this;
            }

            public virtual FieldBuffer Field => (FieldBuffer)symbol;

            public virtual FieldLookupResult Build()
            {
                if (symbol == null)
                {
                    throw new System.NullReferenceException("Symbol can't be null in FieldLookupResult");
                }
                FieldLookupResult result = new FieldLookupResult
                {
                    abbreviated = isAbbreviated,
                    unqualified = isUnqualified,
                    bufferScope = bufferScope,
                    symbol = symbol
                };

                return result;
            }
        }
    }
}
