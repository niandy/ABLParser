using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    using ABLParser.Prorefactor.Core.Schema;
    using ABLParser.Prorefactor.Proparser.Antlr;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// A TableBuffer is a Symbol which provides a link from the syntax tree to a Table object.
    /// </summary>
    public class TableBuffer : Symbol
    {
        private readonly ITable table;
        private readonly bool isDefault;
        private readonly IDictionary<IField, FieldBuffer> fieldBuffers = new Dictionary<IField, FieldBuffer>();

        /// <summary>
        /// Constructor for a named buffer.
        /// </summary>
        /// <param name="name"> Input "" for an unnamed or default buffer </param>
        public TableBuffer(string name, TreeParserSymbolScope scope, ITable table) : base(name, scope)
        {
            this.table = table;
            this.isDefault = name.Length == 0;
        }

        internal virtual void AddFieldBuffer(FieldBuffer fieldBuffer)
        {
            fieldBuffers[fieldBuffer.Field] = fieldBuffer;
        }

        /// <summary>
        /// Return fully qualified table name (with DB) of the table buffer is pointing to
        /// </summary>
        public virtual string TargetFullName
        {
            get
            {
                if (table.Storetype == IConstants.ST_DBTABLE)
                {
                    return (new StringBuilder(table.Database.Name)).Append(".").Append(table.GetName()).ToString();
                }
                else
                {
                    return table.GetName();
                }
            }
        }

        /// <summary>
        /// Get the "database.buffer" name for schema buffers, get "buffer" for temp/work table buffers.
        /// </summary>
        public override string FullName
        {
            get
            {
                if (table.Storetype != IConstants.ST_DBTABLE)
                {
                    return Name;
                }

                return (new StringBuilder(table.Database.Name)).Append(".").Append(Name).ToString();
            }
        }

        /// <summary>
        /// Get a list of FieldBuffer symbols that have been created for this TableBuffer. </summary>
        public virtual ICollection<FieldBuffer> FieldBufferList => fieldBuffers.Values;

        /// <summary>
        /// Always returns BUFFER, whether this is a named buffer or a default buffer.
        /// </summary>
        /// <seealso cref= org.prorefactor.treeparser.symbols.Symbol#getProgressType() </seealso>
        /// <seealso cref= org.prorefactor.core.schema.ITable#getStoretype() </seealso>
        public override int ProgressType => Proparse.BUFFER;

        /// <summary>
        /// Get or create a FieldBuffer for a Field. </summary>
        public virtual FieldBuffer GetFieldBuffer(IField field)
        {
            Debug.Assert(field.Table == this.table);            
            if (fieldBuffers.TryGetValue(field, out FieldBuffer ret))
            {
                return ret;
            }
            ret = new FieldBuffer(this.Scope, this, field);
            fieldBuffers[field] = ret;
            return ret;
        }

        internal override bool IsInstanceOfType(Symbol s) => s is TableBuffer;        

        /// <summary>
        /// Get the name of the buffer (overrides Symbol.getName). Returns the name of the table for default (unnamed) buffers.
        /// </summary>
        public override string Name
        {
            get
            {
                if (base.Name.Length == 0)
                {
                    return table.GetName();
                }

                return base.Name;
            }
        }
        public virtual ITable Table => table;

        /// <summary>
        /// Is this the default (unnamed) buffer? </summary>
        public virtual bool Default => isDefault;

        /// <summary>
        /// Is this a default (unnamed) buffer for a schema table? </summary>
        public virtual bool DefaultSchema => isDefault && table.Storetype == IConstants.ST_DBTABLE;

    }

}
