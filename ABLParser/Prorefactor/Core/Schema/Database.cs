using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    /// <summary>
    /// Database objects are created by the Schema class, and they are used when looking up table names from 4gl compile
    /// units.
    /// </summary>
    public class Database : IDatabase
    {

        /// <summary>
        /// New Database object </summary>
        /// <param name="name"> Main DB name </param>
        public Database(string name)
        {
            this.Name = name;
        }

        public void Add(ITable table)
        {
            TableSet.Add(table);
        }

        public string Name { get; }

        public SortedSet<ITable> TableSet { get; } = new SortedSet<ITable>(Constants.TABLE_NAME_ORDER);

        public override string ToString()
        {
            return (new StringBuilder("DB ")).Append(Name).ToString();
        }
    }

}
