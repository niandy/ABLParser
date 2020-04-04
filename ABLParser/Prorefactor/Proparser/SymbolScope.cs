using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Refactor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABLParser.Prorefactor.Proparser
{
    public class SymbolScope
    {
        private readonly RefactorSession session;
        private readonly SymbolScope superScope;

        private readonly IDictionary<string, TableRef> tableMap = new SortedDictionary<string, TableRef>();
        private readonly ISet<string> varSet = new SortedSet<string>();
        private readonly ISet<string> inlineVarSet = new HashSet<string>();

        internal SymbolScope(RefactorSession session) : this(session, null)
        {
        }

        internal SymbolScope(RefactorSession session, SymbolScope superScope)
        {
            this.session = session;
            this.superScope = superScope;
        }

        public virtual RefactorSession Session => session;

        internal virtual SymbolScope SuperScope => superScope;

        internal virtual void DefineBuffer(string bufferName, string tableName)
        {
            // Look for the tableName in tableMap /before/
            // adding the new ref. This is in case they have done:
            // DEFINE BUFFER customer FOR customer. (groan)
            // ...otherwise we find ourself, with type not defined yet...
            tableName = tableName.ToLower();
            FieldType bufferType = IsTableSchemaFirst(tableName);
            bufferName = bufferName.ToLower();
            TableRef newRef = new TableRef
            {
                bufferFor = tableName,
                tableType = bufferType
            };
            tableMap[bufferName] = newRef;
            if (newRef.tableType == FieldType.DBTABLE)
            {
                ITable table = session.Schema.LookupTable(tableName);
                if (table != null)
                {
                    newRef.dbName = table.Database.Name;
                    newRef.fullName = newRef.dbName + "." + table.GetName();
                }
                // Create a db.buffername entry.
                // If the db name was specified, then we have to use that
                // (whether it's a db alias or not) See bug #053.
                Table.Name tn = new Table.Name(tableName);
                string dbRefName = (tn.Db ?? table.Database.Name) + "." + bufferName;

                TableRef dbRef = new TableRef
                {
                    bufferFor = tableName,
                    tableType = bufferType
                };
                tableMap[dbRefName] = dbRef;
            }
        }

        internal virtual void DefineTable(string name, FieldType ttype)
        {
            TableRef newTable = new TableRef
            {
                tableType = ttype
            };
            tableMap[name.ToLower()] = newTable;
        }

        internal virtual void DefineVar(string name)
        {
            varSet.Add(name.ToLower());
        }

        internal virtual void DefineInlineVar(string name)
        {
            DefineVar(name);
            inlineVarSet.Add(name.ToLower());
        }

        /// <summary>
        /// Returns null if false, else, the table type </summary>
        internal virtual FieldType IsTable(string inName)
        {
            // isTable is not recursive, but isTableDef is.
            // First: Qualified db.table.
            ITable table = session.Schema.LookupTable(inName);
            if (table != null && inName.Contains("."))
            {
                return FieldType.DBTABLE;
            }
            // Second: temp-table/work-table/buffer name.
            FieldType ret = IsTableDef(inName);
            if (ret != null)
            {
                return ret;
            }
            // Third: unqualified db table name.
            if (table != null)
            {
                return FieldType.DBTABLE;
            }
            // Fourth: Check for built in buffer names.
            // Built in buffer for returned values from stored procedures.
            // My use of TTABLE as return type is arbitrary.
            if ("proc-text-buffer".Equals(inName))
            {
                return FieldType.TTABLE;
            }
            // It's not a valid table name.
            return null;
        }

        internal virtual FieldType IsTableDef(string inName)
        {
            // Is the name a defined table? (ttable,wtable,buffername)
            // Progress does not allow tt/wt/buffer names to be abbreviated.
            // Progress does not allow tt/wt/buffer names to be ambigous.
            // Although tt and wt names cannot be scoped by context into a
            // procedure/function/trigger block, buffer names can.
            // All of these can be inherited from a super class.
            if (tableMap.ContainsKey(inName))
            {
                return tableMap[inName].tableType;
            }
            if (superScope != null)
            {
                FieldType ft = superScope.IsTableDef(inName);
                if (ft != null)
                {
                    return ft;
                }
            }

            return null;
        }

        internal virtual FieldType IsTableSchemaFirst(string inName)
        {
            // If we find that an non-abbreviated schema table name matches,
            // we return it even before a temp/work table match.
            ITable table = session.Schema.LookupTable(inName);
            if (table != null)
            {
                Table.Name name = new Table.Name(inName);
                if (table.GetName().Length == name.Table.Length)
                {
                    return FieldType.DBTABLE;
                }
            }
            return IsTable(inName);
        }


        internal virtual bool IsVariable(string name)
        {
            // Variable names cannot be abbreviated.
            if (varSet.Contains(name.ToLower()))
            {
                return true;
            }
            if (superScope != null)
            {
                return superScope.IsVariable(name);
            }
            return false;
        }

        internal virtual bool IsInlineVariable(string name)
        {
            if (inlineVarSet.Contains(name.ToLower()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// methodOrFunc should only be called for the "unit" scope, since it is the only one that would ever contain methods
        /// or user functions.
        /// </summary>
        internal virtual int IsMethodOrFunction(string name)
        {
            return superScope.IsMethodOrFunction(name);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public void writeScope(Writer writer) throws IOException
        public virtual void WriteScope(StreamWriter writer)
        {            
            writer.Write("** SymbolScope ** \n");
            foreach(string e in varSet)
            {
                try
                {
                    writer.Write("Variable " + e + "\n");
                }
                catch (IOException)
                {
                }
            };
            foreach (KeyValuePair<string, TableRef> e in tableMap)                
            {
                try
                {
                    writer.Write("Table " + e.Key + ": " + e.Value.fullName + "\n");
                }
                catch (IOException)
                {
                }
            };
        }

        // Field and table types
        public sealed class FieldType
        {
            public static readonly FieldType VARIABLE = new FieldType("VARIABLE", InnerEnum.VARIABLE, 1);
            public static readonly FieldType DBTABLE = new FieldType("DBTABLE", InnerEnum.DBTABLE, 2);
            public static readonly FieldType TTABLE = new FieldType("TTABLE", InnerEnum.TTABLE, 3);
            public static readonly FieldType WTABLE = new FieldType("WTABLE", InnerEnum.WTABLE, 4);

            private static readonly IList<FieldType> valueList = new List<FieldType>();

            static FieldType()
            {
                valueList.Add(VARIABLE);
                valueList.Add(DBTABLE);
                valueList.Add(TTABLE);
                valueList.Add(WTABLE);
            }

            public enum InnerEnum
            {
                VARIABLE,
                DBTABLE,
                TTABLE,
                WTABLE
            }

            public readonly InnerEnum innerEnumValue;
            private readonly string nameValue;
            private readonly int ordinalValue;
            private static int nextOrdinal = 0;
            internal int intval;

            internal FieldType(string name, InnerEnum innerEnum, int intval)
            {
                this.intval = intval;

                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            }

            public static IList<FieldType> Values()
            {
                return valueList;
            }

            public int Ordinal()
            {
                return ordinalValue;
            }

            public override string ToString()
            {
                return nameValue;
            }

            public static FieldType ValueOf(string name)
            {
                foreach (FieldType enumInstance in FieldType.valueList)
                {
                    if (enumInstance.nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }

        private class TableRef
        {
            internal FieldType tableType;
            //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            //ORIGINAL LINE: @SuppressWarnings("unused") String bufferFor;
            internal string bufferFor;
            //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
            //ORIGINAL LINE: @SuppressWarnings("unused") String fullName;
            internal string fullName;
            internal string dbName;
        }

    }

}
