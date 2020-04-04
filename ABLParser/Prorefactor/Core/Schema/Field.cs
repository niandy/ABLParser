using ABLParser.Prorefactor.Treeparser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    /// <summary>
    /// Field objects are created both by the Schema class and they are also created for temp and work table fields defined
    /// within a 4gl compile unit.
    /// </summary>
    public class Field : IField
    {
        private readonly string name;
        private ITable table;

        /// <summary>
        /// Standard constructor.
        /// </summary>
        /// <param name="table"> Use null if you want to assign the field to a table as a separate step. </param>
        public Field(string inName, ITable table)
        {
            this.name = inName;
            this.table = table;
            if (table != null)
            {
                table.Add(this);
            }
        }

        /// <summary>
        /// Constructor for temporary "lookup" fields. "Package" visibility. </summary>
        internal Field(string inName)
        {
            this.name = inName;
            this.table = Constants.nullTable;
        }

        public void AssignAttributesLike(Primative likePrim)
        {
            DataType = likePrim.DataType;
            ClassName = likePrim.ClassName;
            Extent = likePrim.Extent;
        }

        /// <summary>
        /// Copy the bare minimum attributes to a new Field object.
        /// </summary>
        /// <param name="toTable"> The table that the field is being added to. </param>
        /// <returns> The newly created Field, though you may not need it for anything since it has already been added to the
        ///         Table. </returns>
        public IField CopyBare(ITable toTable)
        {
            Field f = new Field(name, toTable)
            {
                DataType = DataType,
                Extent = Extent,
                ClassName = ClassName
            };
            return f;
        }

        public string ClassName { get; private set; } = null;

        public DataType DataType { get; private set; }

        public int Extent { get; private set; }

        public string GetName() => name;

        public Primative SetClassName(string s)
        {
            this.ClassName = s;
            return this;
        }

        public Primative SetDataType(DataType dataType)
        {
            this.DataType = dataType;
            return this;
        }

        public Primative SetExtent(int extent)
        {
            this.Extent = extent;
            return this;
        }

        /// <summary>
        /// Use this to set the field to a table if you used null for the table in the constructor. </summary>
        public ITable Table
        {
            set
            {
                this.table = value;
                value.Add(this);
            }
            get => table;            
        }

        /// <summary>
        /// This is a convenience class for working with a string field name, where there may or may not be a database or table
        /// qualifier in the name.
        /// </summary>
        public class Name
        {
            private readonly string db;
            private readonly string table;
            private readonly string field;

            public Name(string dbPart, string tablePart, string fieldPart)
            {
                db = dbPart;
                table = tablePart;
                field = fieldPart;
            }

            public Name(string name)
            {
                string[] parts = name.Split('.');
                if (parts.Length == 1)
                {
                    db = null;
                    table = null;
                    field = parts[0];
                }
                else if (parts.Length == 2)
                {
                    db = null;
                    table = parts[0];
                    field = parts[1];
                }
                else
                {
                    db = parts[0];
                    table = parts[1];
                    field = parts[2];
                }
            }

            public virtual string Db => db;

            public virtual string Table => table;


            public virtual string Field => field;

            public virtual string GenerateName()
            {
                StringBuilder buff = new StringBuilder();
                if (table != null && table.Length > 0)
                {
                    if (db != null && db.Length > 0)
                    {
                        buff.Append(db);
                        buff.Append(".");
                    }
                    buff.Append(table);
                    buff.Append(".");
                }
                buff.Append(field);
                return buff.ToString();
            }
        }

    }
}
