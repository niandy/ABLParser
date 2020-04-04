using ABLParser.Prorefactor.Core.Schema;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    

    /// <summary>
    /// Frame and Browse widgets are FieldContainers. This class provides the services for looking up fields/variables in a
    /// Frame or Browse.
    /// </summary>
    public abstract class FieldContainer : Widget
    {

        private IList<IParseTree> statementList = new List<IParseTree>();
        private ISet<FieldBuffer> fieldSet = new HashSet<FieldBuffer>();
        private ISet<Symbol> enabledFields = new HashSet<Symbol>();
        private ISet<Symbol> otherSymbols = new HashSet<Symbol>();
        private ISet<Variable> variableSet = new HashSet<Variable>();

        public FieldContainer(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <summary>
        /// Add a statement node to the list of statements which operate on this FieldContainer. Intended to be used by the
        /// tree parser only.
        /// </summary>
        public virtual void addStatement(IParseTree node)
        {
            statementList.Add(node);
        }

        /// <summary>
        /// Add a FieldBuffer or Variable to this Frame or Browse object. Intended to be used by the tree parser only. The tree
        /// parser passes 'true' for ENABLE|UPDATE|PROMPT-FOR.
        /// </summary>
        public virtual void addSymbol(Symbol symbol, bool statementIsEnabler)
        {
            if (symbol is FieldBuffer)
            {
                fieldSet.Add((FieldBuffer)symbol);
            }
            else if (symbol is Variable)
            {
                ((Variable)symbol).ReferencedInFrame();
                variableSet.Add((Variable)symbol);
            }
            else
            {
                otherSymbols.Add(symbol);
            }
            if (statementIsEnabler)
            {
                enabledFields.Add(symbol);
            }
        }

        /// <summary>
        /// Get the fields and variables in the frame. The entries in the return list are of type Variable and/or FieldBuffer.
        /// </summary>
        public virtual IList<Symbol> AllFields
        {
            get
            {
                IList<Symbol> ret = new List<Symbol>();
                ((List<Symbol>)ret).AddRange(variableSet);
                ((List<Symbol>)ret).AddRange(fieldSet);
                return ret;
            }
        }

        /// <summary>
        /// Combines getAllFields() with all other widgets in the FieldContainer
        /// </summary>
        public virtual IList<Symbol> AllFieldsAndWidgets
        {
            get
            {
                IList<Symbol> ret = AllFields;
                ((List<Symbol>)ret).AddRange(otherSymbols);
                return ret;
            }
        }

        /// <summary>
        /// Get the enabled fields and variables in the frame. The entries in the return list are of type Variable and/or
        /// FieldBuffer.
        /// </summary>
        public virtual IList<Symbol> EnabledFields
        {
            get
            {
                IList<Symbol> ret = new List<Symbol>();
                ((List<Symbol>)ret).AddRange(enabledFields);
                return ret;
            }
        }

        /// <summary>
        /// Get the list of nodes for the statements which operate on this FieldContainer
        /// </summary>
        public virtual IList<IParseTree> StatementList
        {
            get
            {
                return statementList;
            }
        }

        /// <summary>
        /// Check to see if a name matches a Variable or a FieldBuffer in this FieldContainer. Used by the tree parser at the
        /// INPUT function for resolving the name reference.
        /// </summary>
        public virtual Symbol lookupFieldOrVar(Field.Name name)
        {
            if (name.Table == null)
            {
                foreach (Variable var in variableSet)
                {
                    if (var.Name.Equals(name.Field, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        return var;
                    }
                }
            }
            foreach (FieldBuffer fieldBuffer in fieldSet)
            {
                if (fieldBuffer.CanMatch(name))
                {
                    return fieldBuffer;
                }
            }

            // Lookup in sub-containers (e.g. browse in a frame)
            foreach (Symbol symbol in otherSymbols)
            {
                if (symbol is FieldContainer)
                {
                    Symbol s = ((FieldContainer)symbol).lookupFieldOrVar(name);
                    if (s != null)
                    {
                        return s;
                    }
                }
            }

            return null;
        }

    }

}
