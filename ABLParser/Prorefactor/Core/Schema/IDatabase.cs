using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    using System.Collections.Generic;

    /// <summary>
    /// Database definition
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Adds table definition
        /// </summary>
        void Add(ITable table);

        /// <returns> Database name </returns>
        string Name { get; }

        /// <returns> Sorted (by table name) list of tables </returns>
        SortedSet<ITable> TableSet { get; }
    }

}
