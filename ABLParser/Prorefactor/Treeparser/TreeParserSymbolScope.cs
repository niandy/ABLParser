using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.Prorefactor.Treeparser.Symbols.Widgets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ABLParser.Prorefactor.Treeparser
{
    

    /// <summary>
    /// For keeping track of PROCEDURE, FUNCTION, and trigger scopes within a 4gl compile unit. Note that scopes are nested.
    /// There is the outer program scope, and within it the other types of scopes which may themselves nest trigger scopes.
    /// (Trigger scopes may be deeply nested). These scopes are defined <b>Symbol</b> scopes. They have nothing to do with
    /// record or frame scoping!
    /// </summary>
    public class TreeParserSymbolScope
    {
        protected internal readonly TreeParserSymbolScope parentScope;

        protected internal IList<Symbol> allSymbols = new List<Symbol>();
        protected internal IList<TreeParserSymbolScope> childScopes = new List<TreeParserSymbolScope>();
        protected internal Block rootBlock;
        protected internal Routine routine;
        protected internal IDictionary<string, TableBuffer> bufferMap = new Dictionary<string, TableBuffer>();
        protected internal IDictionary<string, IFieldLevelWidget> fieldLevelWidgetMap = new Dictionary<string, IFieldLevelWidget>();
        protected internal IDictionary<string, Routine> routineMap = new Dictionary<string, Routine>();
        protected internal IDictionary<ITable, TableBuffer> unnamedBuffers = new Dictionary<ITable, TableBuffer>();
        protected internal IDictionary<int, IDictionary<string, Symbol>> typeMap = new Dictionary<int, IDictionary<string, Symbol>>();
        protected internal IDictionary<string, Variable> variableMap = new Dictionary<string, Variable>();

        protected internal TreeParserSymbolScope() : this(null)
        {
        }

        /// <summary>
        /// Only Scope and derivatives may create a Scope object.
        /// </summary>
        /// <param name="parentScope"> null if called by the SymbolScopeRoot constructor. </param>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings({"unchecked", "rawtypes"}) private TreeParserSymbolScope(TreeParserSymbolScope parentScope)
        private TreeParserSymbolScope(TreeParserSymbolScope parentScope)
        {
            this.parentScope = parentScope;
            typeMap[Proparse.VARIABLE] = variableMap.ToDictionary(k => k.Key, v => (Symbol)v.Value);
        }

        /// <summary>
        /// Add a FieldLevelWidget for names lookup. </summary>
        private void Add(IFieldLevelWidget widget)
        {
            fieldLevelWidgetMap[widget.Name.ToLower()] = widget;
        }

        public virtual Routine Routine
        {
            set
            {
                if (this.routine != null)
                {
                    throw new System.InvalidOperationException();
                }
                this.routine = value;
            }
            get => routine;            
        }


        /// <summary>
        /// Add a Routine for call handling. Note that this isn't really complete. It's possible to have an IN SUPER
        /// declaration, as well as a local definition. The local definition should be the one in this map, but as it stands,
        /// the *last added* is what will be found.
        /// </summary>
        private void Add(Routine routine)
        {
            routineMap[routine.Name.ToLower()] = routine;
        }

        /// <summary>
        /// Add a TableBuffer for names lookup. This is called when copying a SymbolScopeSuper's symbols for inheritance
        /// purposes.
        /// </summary>
        private void Add(TableBuffer tableBuffer)
        {
            ITable table = tableBuffer.Table;
            AddTableBuffer(tableBuffer.Name, table, tableBuffer);
            RootScope.AddTableDefinitionIfNew(table);
        }

        /// <summary>
        /// Add a Variable for names lookup. </summary>
        private void Add(Variable var)
        {
            variableMap[var.Name.ToLower()] = var;
        }

        /// <summary>
        /// Add a TableBuffer to the appropriate map. </summary>
        private void AddTableBuffer(string name, ITable table, TableBuffer buffer)
        {
            if (name.Length == 0)
            {
                if (table.Storetype == IConstants.ST_DBTABLE)
                {
                    unnamedBuffers[table] = buffer;
                }
                else // default buffers for temp/work tables go into the "named" buffer map
                {
                    bufferMap[table.GetName().ToLower()] = buffer;
                }
            }
            else
            {
                bufferMap[name.ToLower()] = buffer;
            }
        }

        /// <summary>
        /// Add a Symbol for names lookup. </summary>
        public virtual void Add(Symbol symbol)
        {
            if (symbol is IFieldLevelWidget)
            {
                Add((IFieldLevelWidget)symbol);
            }
            else if (symbol is Variable)
            {
                Add((Variable)symbol);
            }
            else if (symbol is Routine)
            {
                Add((Routine)symbol);
            }
            else if (symbol is TableBuffer)
            {
                Add((TableBuffer)symbol);
            }
            else
            {                
                if (!typeMap.TryGetValue(symbol.ProgressType, out IDictionary<string, Symbol> map))
                {
                    map = new Dictionary<string, Symbol>();
                    typeMap[symbol.ProgressType] = map;
                }
                map[symbol.Name.ToLower()] = symbol;
            }
        }

        /// <summary>
        /// Add a new scope to this scope. </summary>
        public virtual TreeParserSymbolScope AddScope()
        {
            TreeParserSymbolScope newScope = new TreeParserSymbolScope(this);
            childScopes.Add(newScope);
            return newScope;
        }

        /// <summary>
        /// All symbols within this scope are added to this scope's symbol list. This method has "package" visibility, since
        /// the Symbol object adds itself to its scope.
        /// </summary>
        public virtual void AddSymbol(Symbol symbol)
        {
            allSymbols.Add(symbol);
        }

        /// <summary>
        /// Define a new BufferSymbol.
        /// </summary>
        /// <param name="name"> Input "" for a default or unnamed buffer, otherwise the "named buffer" name. </param>
        public virtual TableBuffer DefineBuffer(string name, ITable table)
        {
            TableBuffer buffer = new TableBuffer(name, this, table);
            if (table != null)
            {
                AddTableBuffer(name, table, buffer);
            }
            return buffer;
        }

        /// <summary>
        /// Get the integer "depth" of the scope. Zero might be either the unit (program/class) scope, or if this is a class
        /// which inherits from super classes, then zero would be the top of the inheritance chain. Functions and procedures
        /// will always be depth: (unitDepth + 1), and trigger scopes can be nested, so they will always be one or greater. I
        /// use this function for unit testing - I want to be able to examine the scope of a symbol, and make sure that the
        /// symbol belongs to the scope that I expect.
        /// </summary>
        public virtual int Depth()
        {
            int depth = 0;
            TreeParserSymbolScope scope = this;
            while ((scope = scope.ParentScope) != null)
            {
                depth++;
            }
            return depth;
        }

        /// <summary>
        /// Get a *copy* of the list of all symbols in this scope </summary>
        public virtual IList<Symbol> GetAllSymbols()
        {
            return new List<Symbol>(allSymbols);            
        }

        /// <summary>
        /// Get a list of this scope's symbols which match a given class </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings("unchecked") public <T extends Symbol> List<T> getAllSymbols(Class<T> klass)
        public virtual IList<T> GetAllSymbols<T>(T klass) where T : Symbol
        {
            List<T> ret = new List<T>();
            foreach (Symbol s in allSymbols)
            {
                if (klass.IsInstanceOfType(s))
                {
                    ret.Add((T)s);
                }
            }
            return ret;
        }

        /// <summary>
        /// Get a list of this scope's symbols, and all symbols of all descendant scopes. </summary>
        public virtual IList<Symbol> GetAllSymbolsDeep()
        {
            
            List<Symbol> ret = new List<Symbol>(allSymbols);
            foreach (TreeParserSymbolScope child in childScopes)
            {
                ret.AddRange(child.GetAllSymbolsDeep());
            }
            return ret;
            
        }

        /// <summary>
        /// Get a list of this scope's symbols, and all symbols of all descendant scopes, which match a given class. </summary>
        public virtual IList<T> GetAllSymbolsDeep<T>(T klass) where T : Symbol
        {
            IList<T> ret = GetAllSymbols(klass);
            foreach (TreeParserSymbolScope child in childScopes)
            {
                ((List<T>)ret).AddRange(child.GetAllSymbols(klass));
            }
            return ret;
        }

        /// <summary>
        /// Get the set of named buffers </summary>
        public virtual ISet<KeyValuePair<string, TableBuffer>> BufferSet => new HashSet<KeyValuePair<string, TableBuffer>>(bufferMap.ToList());

        /// <summary>
        /// Given a name, find a BufferSymbol (or create if necessary for unnamed buffer). </summary>
        public virtual TableBuffer GetBufferSymbol(string inName)
        {
            TableBuffer symbol = LookupBuffer(inName);
            if (symbol != null)
            {
                return symbol;
            }
            // The default buffer for temp and work tables was defined at
            // the time that the table was defined. So, lookupBuffer() would have found
            // temp/work table references, and all we have to search now is schema.
            ITable table = RootScope.RefactorSession.Schema.LookupTable(inName);
            if (table == null)
            {
                return null;
            }
            return GetUnnamedBuffer(table);
        }

        /// <summary>
        /// Get a *copy* of the list of child scopes </summary>
        public virtual IList<TreeParserSymbolScope> ChildScopes => new List<TreeParserSymbolScope>(childScopes);

        /// <summary>
        /// Get a list of all child scopes, and their child scopes, etc </summary>
        public virtual IList<TreeParserSymbolScope> ChildScopesDeep
        {
            get
            {
                List<TreeParserSymbolScope> ret = new List<TreeParserSymbolScope>();
                foreach (TreeParserSymbolScope child in childScopes)
                {
                    ret.Add(child);
                    ret.AddRange(child.ChildScopesDeep);
                }
                return ret;
            }
        }

        public virtual TreeParserSymbolScope ParentScope => parentScope;

        public virtual Block RootBlock
        {
            get => rootBlock;
            set => rootBlock = value;
        }

        public virtual TreeParserRootSymbolScope RootScope => (parentScope == null) ? (TreeParserRootSymbolScope)this : parentScope.RootScope;

        /// <summary>
        /// Get or create the unnamed buffer for a schema table. </summary>
        public virtual TableBuffer GetUnnamedBuffer(ITable table)
        {
            // Check this and parents for the unnamed buffer. Table triggers
            // can scope an unnamed buffer - that's why we don't go straight to
            // the root scope.
            TreeParserSymbolScope nextScope = this;
            while (nextScope != null)
            {                
                if (nextScope.unnamedBuffers.TryGetValue(table, out TableBuffer buffer))
                {
                    return buffer;
                }
                nextScope = nextScope.parentScope;
            }
            return RootScope.DefineBuffer("", table);
        }

        /// <summary>
        /// Get the Variables. (vars, params, etc, etc.) </summary>
        public virtual ICollection<Variable> Variables
        {
            get
            {
                return variableMap.Values;
            }
        }

        public virtual Variable GetVariable(string name)
        {
            return variableMap[name.ToLower()];
        }

        /// <summary>
        /// Answer whether the scope has a Routine named by param.
        /// </summary>
        /// <param name="name"> - the name of the routine. </param>
        public virtual bool HasRoutine(string name)
        {            
            return (name is null) ? false : routineMap.ContainsKey(name.ToLower());
        }

        /// <summary>
        /// Is this scope active in the input scope? In other words, is this scope the input scope, or any of the parents of
        /// the input scope?
        /// </summary>
        public virtual bool IsActiveIn(TreeParserSymbolScope theScope)
        {
            while (theScope != null)
            {
                if (this == theScope)
                {
                    return true;
                }
                theScope = theScope.parentScope;
            }
            return false;
        }

        /// <summary>
        /// Lookup a named record/table buffer in this scope or an enclosing scope.
        /// </summary>
        /// <param name="inName"> String buffer name </param>
        /// <returns> A TableBuffer, or null if not found. </returns>
        public virtual TableBuffer LookupBuffer(string inName)
        {
            // - Buffer names cannot be abbreviated.
            // - Buffer names *can* be qualified with a database name.
            // - Buffer names *are* unique in a given scope: you cannot have two buffers with the same name in the same scope
            // even if they are for two different databases.
            string[] parts = inName.Split('.');
            string bufferPart;
            string dbPart = "";
            if (parts.Length == 1)
            {
                bufferPart = inName;
            }
            else
            {
                dbPart = parts[0];
                bufferPart = parts[1];
            }

            bufferMap.TryGetValue(bufferPart.ToLower(), out TableBuffer symbol);
            if (symbol == null || (dbPart.Length > 0 && !dbPart.Equals(symbol.Table.Database.Name, StringComparison.OrdinalIgnoreCase)) || (dbPart.Length > 0 && (symbol.Table.Storetype == IConstants.ST_TTABLE)))
            {
                if (parentScope != null)
                {
                    TableBuffer tb = parentScope.LookupBuffer(inName);
                    if (tb != null)
                    {
                        return tb;
                    }
                }
                return null;
            }
            return symbol;
        }


        public virtual Dataset LookupDataset(string name)
        {
            return (Dataset)LookupSymbolLocally(Proparse.DATASET, name);
        }

        public virtual Datasource LookupDatasource(string name)
        {
            return (Datasource)LookupSymbolLocally(Proparse.DATASOURCE, name);
        }

        /// <summary>
        /// Lookup a FieldLevelWidget in this scope or an enclosing scope. </summary>
        public virtual IFieldLevelWidget LookupFieldLevelWidget(string inName)
        {            
            if (!fieldLevelWidgetMap.TryGetValue(inName.ToLower(), out IFieldLevelWidget wid) && parentScope != null)
            {
                return parentScope.LookupFieldLevelWidget(inName);
            }
            return wid;
        }

        public virtual Query LookupQuery(string name)
        {
            return (Query)LookupSymbolLocally(Proparse.QUERY, name);
        }

        public virtual Routine LookupRoutine(string name)
        {
            routineMap.TryGetValue(name.ToLower(), out Routine routine);
            return routine;
        }

        public virtual Stream LookupStream(string name)
        {
            return (Stream)LookupSymbolLocally(Proparse.STREAM, name);
        }

        public virtual Symbol LookupSymbol(int symbolType, string name)
        {
            Symbol symbol = LookupSymbolLocally(symbolType, name);
            if (symbol != null)
            {
                return symbol;
            }
            if (parentScope != null)
            {
                return parentScope.LookupSymbol(symbolType, name);
            }
            return null;
        }

        public virtual Symbol LookupSymbolLocally(int symbolType, string name)
        {            
            if (!typeMap.TryGetValue(symbolType, out IDictionary<string, Symbol> map))
            {
                return null;
            }
            map.TryGetValue(name.ToLower(), out Symbol symbol);
            return symbol;
        }

        /// <summary>
        /// Lookup a Table or a BufferSymbol, schema table first. It seems to work like this: unabbreviated schema name, then
        /// buffer/temp/work name, then abbreviated schema names. Sheesh.
        /// </summary>
        public virtual TableBuffer LookupTableOrBufferSymbol(string inName)
        {
            string tblName = inName.IndexOf('.') == -1 ? inName : inName.Substring(inName.IndexOf('.') + 1);

            ITable table = RootScope.RefactorSession.Schema.LookupTable(tblName);
            if ((table != null) && tblName.Equals(table.GetName(), StringComparison.OrdinalIgnoreCase))
            {
                return GetUnnamedBuffer(table);
            }

            TableBuffer ret2 = LookupBuffer(tblName);
            if (ret2 != null)
            {
                return ret2;
            }
            if (table != null)
            {
                return GetUnnamedBuffer(table);
            }
            if (parentScope == null)
            {
                return null;
            }
            return parentScope.LookupTableOrBufferSymbol(inName);
        }

        public virtual TableBuffer LookupTempTable(string name)
        {            
            if (bufferMap.TryGetValue(name.ToLower(), out TableBuffer buff))
            {
                return buff;
            }
            if (parentScope == null)
            {
                return null;
            }

            return parentScope.LookupTempTable(name);
        }

        /// <summary>
        /// Lookup a Variable in this scope or an enclosing scope.
        /// </summary>
        /// <param name="inName"> The string field name to lookup. </param>
        /// <returns> A Variable, or null if not found. </returns>
        public virtual Variable LookupVariable(string inName)
        {            
            if (!variableMap.TryGetValue(inName.ToLower(), out Variable var) && parentScope != null)
            {
                return parentScope.LookupVariable(inName);
            }
            return var;
        }

        /// <summary>
        /// Lookup a Widget based on TokenType (FRAME, BUTTON, etc) and the name in this scope or enclosing scope. </summary>
        public virtual Widget LookupWidget(int widgetType, string name)
        {
            Widget ret = (Widget)LookupSymbolLocally(widgetType, name);
            if (ret == null && parentScope != null)
            {
                return parentScope.LookupWidget(widgetType, name);
            }
            return ret;
        }
       
        public override string ToString()
        {
            return (new StringBuilder("SymbolScope associated with ")).Append(rootBlock.ToString()).ToString();
        }
    }

}
