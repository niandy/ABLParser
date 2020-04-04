using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    using System.Collections.Generic;

    public interface ITable
    {
        /// <returns> Parent IDatabase object, or <seealso cref="Constants.nullDatabase"/> for temp-tables or work-tables </returns>
        IDatabase Database { get; }

        /// <returns> Table name </returns>
        string GetName();

        /// <returns> <seealso cref="IConstants.ST_DBTABLE"/> for DB table, <seealso cref="IConstants.ST_TTABLE"/> for temp-tables or
        ///         <seealso cref="IConstants.ST_WTABLE"/> for work-tables </returns>
        int Storetype { get; }

        /// <summary>
        /// Lookup a field by name. We do not test for uniqueness. We leave that job to the compiler. This function expects an
        /// unqualified field name (no name dots).
        /// </summary>
        IField LookupField(string name);

        /// <summary>
        /// Add a Field to this table. "Package" visibility only. </summary>
        void Add(IField field);

        /// <summary>
        /// Add a new index to this table. "Package" visibility only. </summary>
        void Add(IIndex index);

        /// <returns> Sorted (by field name) list of fields </returns>
        ISet<IField> FieldSet { get; }

        /// <summary>
        /// Get the ArrayList of fields in field position order (rather than sorted alpha)
        /// </summary>
        IList<IField> FieldPosOrder { get; }

        IList<IIndex> Indexes { get; }

        IIndex LookupIndex(string name);
    }

}
