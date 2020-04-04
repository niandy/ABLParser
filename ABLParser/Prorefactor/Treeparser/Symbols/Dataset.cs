using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    using ABLParser.Prorefactor.Proparser.Antlr;
    using System.Collections.Generic;

    public class Dataset : Symbol
    {
        // Keep the buffers, in order, as part of the DATASET signature
        private readonly IList<TableBuffer> buffers = new List<TableBuffer>();

        public Dataset(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <summary>
        /// The treeparser calls this at RECORD_NAME in <code>RECORD_NAME in FOR RECORD_NAME (COMMA RECORD_NAME)*</code>.
        /// </summary>
        public virtual void addBuffer(TableBuffer buff)
        {
            buffers.Add(buff);
        }

        /// <summary>
        /// For this subclass of Symbol, fullName() returns the same value as getName(). </summary>
        public override string FullName => Name;

        internal override bool IsInstanceOfType(Symbol s)
        {
            return s is Dataset;
        }

        /// <summary>
        /// Get the list of buffers (in order) which make up this dataset's signature. </summary>
        public virtual IList<TableBuffer> Buffers
        {
            get
            {
                return buffers;
            }
        }

        /// <summary>
        /// Returns NodeTypes.DATASET.
        /// </summary>
        public override int ProgressType
        {
            get
            {
                return Proparse.DATASET;
            }
        }

    }

}
