using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    /// <summary>
    /// Table objects are created both by the Schema class and also when temp and work tables are defined within a 4gl
    /// compile unit. For temp and work tables, the database is Schema.nullDatabase.
    /// </summary>
    public class Table : ITable
    {
        private readonly string name;
        private readonly SortedSet<IField> fieldSet = new SortedSet<IField>(Constants.FIELD_NAME_ORDER);

        /// <summary>
        /// Constructor for schema </summary>
        public Table(string name, IDatabase database)
        {
            this.name = name;
            Database = database;
            Storetype = IConstants.ST_DBTABLE;
            database.Add(this);
        }

        /// <summary>
        /// Constructor for temp / work tables </summary>
        public Table(string name, int storetype)
        {
            this.name = name;
            Storetype = storetype;
            Database = Constants.nullDatabase;
        }

        /// <summary>
        /// Constructor for temporary "comparator" objects. </summary>
        public Table(string name)
        {
            this.name = name;
            Storetype = IConstants.ST_DBTABLE;
            Database = Constants.nullDatabase;
        }

        public void Add(IField field)
        {
            FieldSet.Add(field);
            FieldPosOrder.Add(field);
        }

        public void Add(IIndex index)
        {
            Indexes.Add(index);
        }

        public IDatabase Database { get; }

        public IList<IField> FieldPosOrder { get; } = new List<IField>();

        public ISet<IField> FieldSet => fieldSet;

        public IList<IIndex> Indexes { get; } = new List<IIndex>();

        public IIndex LookupIndex(string name)
        {
            foreach (IIndex idx in Indexes)
            {
                if (idx.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return idx;
                }
            }
            return null;
        }

        public string GetName()
        {
            return name;
        }

        public int Storetype { get; }

        public IField LookupField(string lookupName)
        {            
            SortedSet<IField> fieldTailSet = fieldSet.GetTailSet(new Field(lookupName));
            if (fieldTailSet.Count == 0)
            { 
                return null;
            }
            IField field = fieldTailSet.Min;
            if (field == null || !field.GetName().ToLower().StartsWith(lookupName.ToLower(), StringComparison.Ordinal))
            {
                return null;
            }
            return field;
        }


        public override string ToString()
        {
            return (new StringBuilder(Storetype == IConstants.ST_DBTABLE ? "DB Table" : Storetype == IConstants.ST_TTABLE ? "Temp-table" : "Work-table")).Append(' ').Append(name).ToString();
        }

        /// <summary>
        /// This is a convenience class for working with a string table name, where there may or may not be a database
        /// qualifier in the name.
        /// </summary>
        public class Name
        {
            private readonly string db;
            private readonly string table;

            public Name(string dbPart, string tablePart)
            {
                db = dbPart;
                table = tablePart;
            }

            public Name(string name)
            {
                string[] parts = name.Split('.');
                if (parts.Length == 1)
                {
                    db = null;
                    table = parts[0];
                }
                else
                {
                    db = parts[0];
                    table = parts[1];
                }
            }

            public virtual string Db
            {
                get
                {
                    return db;
                }
            }

            public virtual string Table
            {
                get
                {
                    return table;
                }
            }

            public virtual string GenerateName()
            {
                StringBuilder buff = new StringBuilder();
                if (db != null && db.Length > 0)
                {
                    buff.Append(db);
                    buff.Append(".");
                }
                buff.Append(table);
                return buff.ToString();
            }
        }
    }


}
