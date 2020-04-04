using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    using ABLParser.Prorefactor.Core.Schema;
    using ABLParser.Prorefactor.Proparser.Antlr;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// FieldBuffer is the Symbol object linked to from the AST for schema, temp, and work table fields, and FieldBuffer
    /// provides the link to the Field object.
    /// </summary>
    public class FieldBuffer : Symbol, Primative
    {
        private readonly TableBuffer buffer;
        private readonly IField field;

        /// <summary>
        /// When you create a FieldBuffer object, you do not set the name, because that comes from the Field object.
        /// </summary>
        public FieldBuffer(TreeParserSymbolScope scope, TableBuffer buffer, IField field) : base("", scope)
        {
            this.buffer = buffer;
            this.field = field;
            buffer.AddFieldBuffer(this);
        }

        public void AssignAttributesLike(Primative likePrim)
        {
            field.AssignAttributesLike(likePrim);
        }

        /// <summary>
        /// Could this FieldBuffer be referenced by the input name? Input Field.Name must already be all lowercase. Deals with
        /// abbreviations, unqualified table/database, and db aliases.
        /// </summary>
        public virtual bool CanMatch(Field.Name input)
        {
            // Assert that the input name is already lowercase.
            Debug.Assert(input.GenerateName().Equals(input.GenerateName(), StringComparison.CurrentCultureIgnoreCase));
            Field.Name self = new Field.Name(this.FullName.ToLower());
            if (input.Db != null)
            {
                ISchema schema = Scope.RootScope.RefactorSession.Schema;
                if (this.buffer.Table.Database != schema.LookupDatabase(input.Db))
                {
                    return false;
                }
            }
            if (input.Table != null)
            {
                if (buffer.DefaultSchema)
                {
                    if (!self.Table.StartsWith(input.Table))
                    {
                        return false;
                    }
                }
                else
                {
                    // Temp/work/buffer names can't be abbreviated.
                    if (!self.Table.Equals(input.Table))
                    {
                        return false;
                    }
                }
            }
            if (!self.Field.StartsWith(input.Field))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get "database.buffer.field" for schema fields, or "buffer.field" for temp/work table fields.
        /// </summary>
        public override string FullName
        {
            get
            {
                StringBuilder buff = new StringBuilder(buffer.FullName);
                buff.Append(".");
                buff.Append(field.GetName());
                return buff.ToString();
            }
        }

        public virtual TableBuffer Buffer => buffer;

        /// <summary>
        /// Gets the underlying Field's className (or null if not a class).
        /// </summary>
        /// <seealso cref= Primative#getClassName() </seealso>
        public string ClassName => field.ClassName;

        /// <summary>
        /// Gets the underlying Field's dataType. </summary>
        public DataType DataType => field.DataType;

        /// <summary>
        /// The extent comes from the underlying Field. </summary>
        public int Extent => field.Extent;

        public virtual IField Field => field;

        /// <summary>
        /// Returns the Field name. There is no "field buffer name". </summary>
        public override string Name => field.GetName();

        /// <summary>
        /// Always returns FIELD.
        /// </summary>
        /// <seealso cref= org.prorefactor.treeparser.symbols.Symbol#getProgressType() To see if this field buffer is for a schema table,
        ///      temp-table, or work-table, see Table.getStoreType(). </seealso>
        /// <seealso cref= org.prorefactor.core.schema.ITable#getStoretype() </seealso>
        public override int ProgressType => Proparse.FIELD;

        /// <summary>
        /// Sets the underlying Field's className. </summary>
        public Primative SetClassName(string className)
        {
            field.SetClassName(className);
            return this;
        }

        /// <summary>
        /// Sets the underlying Field's dataType. </summary>
        public Primative SetDataType(DataType dataType)
        {
            field.SetDataType(dataType);
            return this;
        }

        /// <summary>
        /// Sets the extent of the underlying Field. </summary>
        public Primative SetExtent(int extent)
        {
            field.SetExtent(extent);
            return this;
        }

        internal override bool IsInstanceOfType(Symbol s)
        {
            return s is FieldBuffer;
        }
    }

}
