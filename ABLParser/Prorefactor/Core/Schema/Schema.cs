using System;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using ABLParser.Prorefactor.Treeparser;
using System.Reflection;

namespace ABLParser.Prorefactor.Core.Schema
{
    /// <summary>
    /// Schema is a singleton with methods and fields for working with database schema names, and references to those from
    /// 4gl compile units.
    /// </summary>
    public class Schema : ISchema
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(Schema));

        private static readonly IComparer<ITable> ALLTABLES_ORDER = new ComparatorAnonymousInnerClass();

        private class ComparatorAnonymousInnerClass : IComparer<ITable>
        {
            public int Compare(ITable s1, ITable s2)
            {
                int ret = string.Compare(s1.GetName(), s2.GetName(), StringComparison.CurrentCultureIgnoreCase);
                if (ret != 0)
                {
                    return ret;
                }
                return string.Compare(s1.Database.Name, s2.Database.Name, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private readonly IDictionary<string, string> aliases = new Dictionary<string, string>();
        private readonly SortedSet<IDatabase> dbSet = new SortedSet<IDatabase>(Constants.DB_NAME_ORDER);
        private readonly SortedSet<ITable> allTables = new SortedSet<ITable>(ALLTABLES_ORDER);

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public Schema(String file) throws IOException
        public Schema(string file) : this(file, false)
        {
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public Schema(String file, boolean injectMetaSchema) throws IOException
        public Schema(string file, bool injectMetaSchema)
        {
            LoadSchema(file);
            if (injectMetaSchema)
            {
                InjectMetaSchema();
            }
        }

        public Schema(params IDatabase[] dbs)
        {
            foreach (IDatabase db in dbs)
            {
                dbSet.Add(db);
                foreach (ITable tbl in db.TableSet)
                {
                    allTables.Add(tbl);
                }
            }
            InjectMetaSchema();
        }

        /// <summary>
        /// Get databases sorted by name. </summary>
        public virtual SortedSet<IDatabase> DbSet
        {
            get
            {
                return dbSet;
            }
        }

        /// <summary>
        /// Add a database alias.
        /// </summary>
        /// <param name="aliasName"> The name for the alias </param>
        /// <param name="dbName"> The database's logical name </param>
        public void CreateAlias(string aliasName, string dbName)
        {
            if (LookupDatabase2(dbName) == null)
            {
                LOGGER.Error($"Creating alias {aliasName} for unknown database {dbName}");
            }
            aliases[aliasName.ToLower(CultureInfo.GetCultureInfo("en"))] = dbName;
        }

        /// <summary>
        /// Delete a database alias.
        /// </summary>
        /// <param name="aliasName"> The name for the alias, null or empty string to delete all. </param>
        public void DeleteAlias(string aliasName)
        {
            if (string.IsNullOrEmpty(aliasName))
            {
                aliases.Clear();
            }
            else
            {
                aliases.Remove(aliasName.ToLower(CultureInfo.GetCultureInfo("en")));
            }
        }

        /// <summary>
        /// Get an iterator through all tables, sorted by db.table name. </summary>
        public virtual IEnumerator<ITable> AllTablesIterator
        {
            get
            {
                return allTables.GetEnumerator();
            }
        }

        public void InjectMetaSchema()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (IDatabase db in dbSet)
            {                
                try
                {
                    StreamReader reader = new StreamReader(assembly.GetManifestResourceStream("ABLParser.Resources.meta.txt"));
                    SchemaLineProcessor lineProcessor = new SchemaLineProcessor(db, allTables, reader);
                    lineProcessor.ProcessLines();
                }
                catch (IOException caught)
                {
                    LOGGER.Error("Unable to open file 'meta.txt'", caught);
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private final void loadSchema(File file) throws IOException
        private void LoadSchema(FileInfo file)
        {
            Database db = new Database(Path.GetFileNameWithoutExtension(file.FullName));
            dbSet.Add(db);
            StreamReader reader = new StreamReader(file.OpenRead());
            SchemaLineProcessor lineProcessor = new SchemaLineProcessor(db, allTables, reader);
            lineProcessor.ProcessLines();            
        }


        /// <summary>
        /// Load schema names from a flat file.
        /// </summary>
        /// <param name="from"> The filename to read from. </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private final void loadSchema(String from) throws IOException
        private void LoadSchema(string from)
        {
            LoadSchema(new FileInfo(from));
        }

        public IDatabase LookupDatabase(string inName)
        {
            IDatabase db = LookupDatabase2(inName);
            if (db != null)
            {
                return db;
            }
            // Check for database alias
            aliases.TryGetValue(inName.ToLower(), out string realName);
            if (realName == null)
            {
                return null;
            }
            return LookupDatabase2(realName);
        }

        public IField LookupField(string dbName, string tableName, string fieldName)
        {
            ITable table = LookupTable(dbName, tableName);
            if (table == null)
            {
                return null;
            }
            return table.LookupField(fieldName);
        }

        public ITable LookupTable(string inName)
        {
            if (inName.IndexOf('.') > -1)
            {
                ITable firstTry = LookupTable2(inName);
                if (firstTry != null)
                {
                    return firstTry;
                }
                return LookupMetaTable(inName);
            }
            return LookupTableCheckName(allTables.GetTailSet(new Table(inName)), inName);
        }

        public ITable LookupTable(string dbName, string tableName)
        {
            IDatabase db = LookupDatabase(dbName);
            if (db == null)
            {
                return null;
            }
            return LookupTableCheckName(db.TableSet.GetTailSet(new Table(tableName)), tableName);
        }

        public IField LookupUnqualifiedField(string name)
        {
            IField field;
            foreach (ITable table in allTables)
            {
                field = table.LookupField(name);
                if (field != null)
                {
                    return field;
                }
            }
            return null;
        }

        /// <summary>
        /// Lookup Database by name. Called twice by lookupDatabase().
        /// </summary>
        private IDatabase LookupDatabase2(string inName)
        {            
            SortedSet<IDatabase> dbTailSet = dbSet.GetTailSet(new Database(inName));
            if (dbTailSet.Count == 0)
            {
                return null;
            }
            IDatabase db = dbTailSet.Min;
            if (db == null || !db.Name.Equals(inName, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }
            return db;
        }

        // It turns out that we *do* have to test for uniqueness - we can't just leave
        // that job to the compiler. That's because when looking up schema names for
        // a DEF..LIKE x, if x is non-unique in schema, then we move on to temp/work/buffer names.
        private ITable LookupTableCheckName(SortedSet<ITable> set, string name)
        {
            string lname = name.ToLower();
            IEnumerator<ITable> it = set.GetEnumerator();
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            if (!it.MoveNext())
            {
                return null;
            }
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            ITable table = it.Current;
            // test that we got a match
            if (!table.GetName().ToLower().StartsWith(lname, StringComparison.Ordinal))
            {
                return null;
            }
            // test that we got a unique match
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            if (lname.Length < table.GetName().Length && it.MoveNext())
            {
                //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                ITable next = it.Current;
                if (next.GetName().ToLower().StartsWith(lname, StringComparison.Ordinal))
                {
                    return null;
                }
            }
            return table;
        }

        /// <summary>
        /// Lookup a qualified table name </summary>
        private ITable LookupTable2(string inName)
        {
            string[] parts = inName.Split('.');
            if ((parts == null) || (parts.Length == 0))
            {
                // Only in the case 'inName' equals '.'
                return null;
            }
            else if (parts.Length == 1)
            {
                return LookupTable(parts[0]);
            }
            else
            {
                return LookupTable(parts[0], parts[1]);
            }
        }

        /// <summary>
        /// This is for looking up names like "sports._file". We return the dictdb Table.
        /// </summary>
        private ITable LookupMetaTable(string inName)
        {
            string[] parts = inName.Split('.');
            IDatabase db = LookupDatabase(parts[0]);
            if ((db == null) || (parts[1] == null) || (!parts[1].StartsWith("_", StringComparison.Ordinal)))
            {
                return null;
            }
            return LookupTableCheckName(db.TableSet.GetTailSet(new Table(parts[1])), parts[1]);
        }

        internal class SchemaLineProcessor
        {
            private readonly IDatabase currDatabase;
            private ITable currTable;
            private readonly SortedSet<ITable> allTables;
            private readonly StreamReader reader;

            public SchemaLineProcessor(IDatabase currDatabase, SortedSet<ITable> allTables, StreamReader reader)
            {
                this.currDatabase = currDatabase;
                this.allTables = allTables;
                this.reader = reader;                
            }

            public void ProcessLines()
            {
                string line;
                while (((line = reader.ReadLine()) != null) && ProcessLine(line))
                {
                }
            }

            //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
            //ORIGINAL LINE: @Override public boolean processLine(String line) throws IOException
            private bool ProcessLine(string line)
            {
                char[] stringSeparators = { ':' };

                if (line.StartsWith("S", StringComparison.Ordinal))
                {
                    // No support for sequences
                }
                else if (line.StartsWith("T", StringComparison.Ordinal))
                {
                    currTable = new Table(line.Substring(1), currDatabase);
                    currDatabase.Add(currTable);
                    allTables.Add(currTable);
                }
                else if (line.StartsWith("F", StringComparison.Ordinal))
                {
                    // FieldName:DataType:Extent
                    int ch1 = line.IndexOf(':');
                    int ch2 = line.LastIndexOf(':');
                    if ((currTable == null) || (ch1 == -1) || (ch2 == -1))
                    {
                        throw new IOException("Invalid file format: " + line);
                    }
                    Field f = new Field(line.Substring(1, ch1 - 1), currTable);
                    f.SetDataType(DataType.GetDataType(line.Substring(ch1 + 1, ch2 - (ch1 + 1)).ToUpper()));
                    if (f.DataType == null)
                    {
                        throw new IOException("Unknown datatype: " + line.Substring(ch1 + 1, ch2 - (ch1 + 1)));
                    }
                    f.SetExtent(int.Parse(line.Substring(ch2 + 1)));
                    currTable.Add(f);
                }
                else if (line.StartsWith("I", StringComparison.Ordinal))
                {
                    if (currTable == null)
                    {
                        throw new IOException("No associated table for " + line);
                    }
                    // IndexName:Attributes:Field1:Field2:...
                    IList<string> lst = line.Split(stringSeparators).Select(s => s.Trim()).ToList();
                    if (lst.Count < 3)
                    {
                        throw new IOException("Invalid file format: " + line);
                    }
                    Index i = new Index(currTable, lst[0].Substring(1), lst[1].IndexOf('U') > -1, lst[1].IndexOf('P') > -1);
                    for (int zz = 2; zz < lst.Count; zz++)
                    {
                        i.AddField(currTable.LookupField(lst[zz].Substring(1)));
                    }
                    currTable.Add(i);
                }

                return true;
            }

            // WTF is this?
            public object GetResult()
            {                
                return null;
            }
        }

    }
}
