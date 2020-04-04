using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    /// <summary>
    /// Represents the list of all available db, aliases and tables in an OpenEdge session
    /// </summary>
    public interface ISchema
    {
        /// <summary>
        /// Add a database alias.
        /// </summary>
        /// <param name="aliasname"> The name for the alias </param>
        /// <param name="dbname"> The database's logical name </param>
        void CreateAlias(string aliasname, string dbname);

        /// <summary>
        /// Delete a database alias.
        /// </summary>
        /// <param name="aliasname"> The name for the alias, null or empty string to delete all. </param>
        void DeleteAlias(string aliasname);

        /// <summary>
        /// Returns the database with the given (or alias) </summary>
        /// <param name="name"> Database name </param>
        /// <returns> Null if not found </returns>
        IDatabase LookupDatabase(string name);

        /// <summary>
        /// Lookup a Field, given the db, table, and field names </summary>
        /// <param name="dbName"> </param>
        /// <param name="tableName"> </param>
        /// <param name="fieldName">
        /// @return </param>
        IField LookupField(string dbName, string tableName, string fieldName);

        /// <summary>
        /// Lookup a table by name.
        /// </summary>
        /// <param name="inName"> The string table name to lookup. </param>
        /// <returns> A Table, or null if not found. If a name like "db.table" fails on the first lookup try, we next search
        ///         dictdb for the table, in case it's something like "sports._file". In that case, the Table from the "dictdb"
        ///         database would be returned. We don't keep meta-schema records in the rest of the databases. </returns>
        ITable LookupTable(string inName);

        /// <summary>
        /// Lookup a table, given a database name and a table name. </summary>
        ITable LookupTable(string dbName, string tableName);

        /// <summary>
        /// Lookup an unqualified schema field name. Does not test for uniqueness. That job is left to the compiler. In fact,
        /// anywhere this is run, the compiler would check that the field name is also unique against temp/work tables.
        /// </summary>
        /// <param name="name"> Unqualified schema field name </param>
        /// <returns> Null if nothing found </returns>
        IField LookupUnqualifiedField(string name);
    }

}
