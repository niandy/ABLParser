using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.RCodeReader.Elements;
using ABLParser.Sonar.Api.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ABLParser.Prorefactor.Treeparser
{
    

    /// <summary>
    /// A ScopeRoot object is created for each compile unit, and it represents the program (topmost) scope. For classes, it
    /// is the class scope, but it may also have a super class scope by way of inheritance.
    /// </summary>
    public class TreeParserRootSymbolScope : TreeParserSymbolScope
    {
        private readonly RefactorSession refSession;
        private readonly IDictionary<string, ITable> tableMap = new Dictionary<string, ITable>();
        private string className = null;
        private ITypeInfo typeInfo = null;
        private bool isInterface;
        private bool abstractClass;
        private bool serializableClass;
        private bool finalClass;

        public TreeParserRootSymbolScope(RefactorSession session)
        {
            this.refSession = session;
        }

        public virtual RefactorSession RefactorSession
        {
            get
            {
                return refSession;
            }
        }

        public virtual void AddTableDefinitionIfNew(ITable table)
        {
            string lowerName = table.GetName().ToLower();
            if (tableMap[lowerName] == null)
            {
                tableMap[lowerName] = table;
            }
        }

        /// <summary>
        /// Define a temp or work table.
        /// </summary>
        /// <param name="name"> The name, with mixed case as in DEFINE node. </param>
        /// <param name="type"> IConstants.ST_TTABLE or IConstants.ST_WTABLE. </param>
        /// <returns> A newly created BufferSymbol for this temp/work table. </returns>
        public virtual TableBuffer DefineTable(string name, int type)
        {
            ITable table = new Table(name, type);
            tableMap[name.ToLower()] = table;
            // Pass empty string for name for default buffer.
            TableBuffer bufferSymbol = new TableBuffer("", this, table);
            // The default buffer for a temp/work table is not "unnamed" the way
            // that the default buffer for schema tables work. So, the buffer
            // goes into the regular bufferMap, rather than the unnamedBuffers map.
            bufferMap[name.ToLower()] = bufferSymbol;
            return bufferSymbol;
        } // defineTable()

        /// <summary>
        /// Define a temp or work table field </summary>
        public virtual FieldBuffer DefineTableField(string name, TableBuffer buffer)
        {
            ITable table = buffer.Table;
            IField field = new Field(name, table);
            return new FieldBuffer(this, buffer, field);
        }

        /// <summary>
        /// Define a temp or work table field. Does not attach the field to the table. That is expected to be done in a
        /// separate step.
        /// </summary>
        public virtual FieldBuffer DefineTableFieldDelayedAttach(string name, TableBuffer buffer)
        {
            IField field = new Field(name, null);
            return new FieldBuffer(this, buffer, field);
        }

        /// <summary>
        /// Valid only if the parse unit is a CLASS. Returns null otherwise.
        /// </summary>
        public virtual string ClassName
        {
            get => className;
            set => className = value;
        }

        /// <returns> True is parse unit is a CLASS or INTERFACE </returns>
        public virtual bool Class => className != null;

        public virtual bool Interface
        {
            set => this.isInterface = value;
            get => className != null && isInterface;
        }


        public virtual bool AbstractClass
        {
            set => abstractClass = value;
            get => abstractClass;
        }


        public virtual bool FinalClass
        {
            set => finalClass = value;
            get => finalClass;
        }


        public virtual bool SerializableClass
        {
            set => serializableClass = value;
            get => serializableClass;
        }


        public virtual TableBuffer GetLocalTableBuffer(ITable table)
        {
            Debug.Assert(table.Storetype != IConstants.ST_DBTABLE);
            return bufferMap[table.GetName().ToLower()];
        }

        public override Variable LookupVariable(string name)
        {
            Variable var = base.LookupVariable(name);
            if (var != null)
            {
                return var;
            }

            ITypeInfo info = typeInfo;
            while (info != null)
            {
                if (info.HasProperty(name))
                {
                    return new Variable(name, this);
                }
                info = refSession.GetTypeInfo(info.ParentTypeName);
            }
            return null;
        }

        public override Dataset LookupDataset(string name)
        {
            Dataset ds = base.LookupDataset(name);
            if (ds != null)
            {
                return ds;
            }

            // TODO Lookup in parent classes
            return null;
        }

        /// <summary>
        /// Lookup a temp or work table definition in this scope. Unlike most other lookup functions, this one has nothing to
        /// do with 4gl semantics, buffers, scopes, etc. This just looks up the raw Table definition for a temp or work table.
        /// </summary>
        /// <returns> null if not found </returns>
        public virtual ITable LookupTableDefinition(string name)
        {
            return tableMap[name.ToLower()];
        }

        public override TableBuffer LookupBuffer(string name)
        {
            TableBuffer buff = base.LookupBuffer(name);
            if (buff != null)
            {
                return buff;
            }

            ITypeInfo info = typeInfo;
            while (info != null)
            {
                if (info.HasBuffer(name))
                {
                    IBufferElement elem = info.GetBuffer(name);
                    ITable tbl;
                    if (!string.IsNullOrEmpty(elem.DatabaseName))
                    {
                        tbl = refSession.Schema.LookupTable(elem.DatabaseName, elem.TableName);
                    }
                    else
                    {
                        tbl = LookupTempTable(elem.TableName).Table;
                    }
                    if (tbl == null)
                    {
                        // Defaults to fake temp-table
                        tbl = new Table(name, IConstants.ST_TTABLE);
                    }
                    return new TableBuffer(name, this, tbl);
                }
                info = refSession.GetTypeInfo(info.ParentTypeName);
            }
            return null;
        }

        public override TableBuffer LookupTempTable(string name)
        {
            TableBuffer buff = base.LookupTempTable(name);
            if (buff != null)
            {
                return buff;
            }
            ITypeInfo info = typeInfo;
            while (info != null)
            {
                if (info.HasTempTable(name))
                {
                    return new TableBuffer(name, this, new RCodeTTWrapper(info.GetTempTable(name)));
                }
                info = refSession.GetTypeInfo(info.ParentTypeName);
            }
            return null;
        }

        /// <summary>
        /// Lookup an unqualified temp/work table field name. Does not test for uniqueness. That job is left to the compiler.
        /// (In fact, anywhere this is run, the compiler would check that the field name is also unique against schema tables.)
        /// Returns null if nothing found.
        /// </summary>
        protected internal virtual IField LookupUnqualifiedField(string name)
        {
            IField field;
            foreach (ITable table in tableMap.Values)
            {
                field = table.LookupField(name);
                if (field != null)
                {
                    return field;
                }
            }
            return null;
        }

        /// <returns> a Collection containing all Routine objects defined in this RootSymbolScope. </returns>
        public virtual IDictionary<string, Routine> RoutineMap => routineMap;

        public virtual ITypeInfo TypeInfo
        {
            set => typeInfo = value;
        }
    }

}
