using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the definition of a Routine. Is a Symbol - used as an entry in the symbol table. A Routine is a
    /// Program_root, PROCEDURE, FUNCTION, or METHOD.
    /// </summary>
    public class Routine : Symbol
    {
        private readonly TreeParserSymbolScope routineScope;
        private readonly IList<Parameter> parameters = new List<Parameter>();
        private DataType returnDatatypeNode = null;
        private ABLNodeType progressType;

        public Routine(string name, TreeParserSymbolScope definingScope, TreeParserSymbolScope routineScope) : base(name, definingScope)
        {
            this.routineScope = routineScope;
            this.routineScope.Routine = this;
        }

        /// <summary>
        /// Called by the tree parser. </summary>
        public virtual void AddParameter(Parameter p)
        {
            parameters.Add(p);
        }

        /// <seealso cref= org.prorefactor.treeparser.symbols.Symbol#fullName() </seealso>
        public override string FullName => Name;

        public virtual IList<Parameter> Parameters => parameters;

        /// <summary>
        /// Return TokenTypes: Program_root, PROCEDURE, FUNCTION, or METHOD. </summary>
        public override int ProgressType => progressType.Type;

        /// <summary>
        /// Null for PROCEDURE, node of the datatype for FUNCTION or METHOD. For a Class return value, won't be the CLASS node,
        /// but the TYPE_NAME node.
        /// </summary>
        public virtual DataType ReturnDatatypeNode
        {
            get => returnDatatypeNode;
            set => returnDatatypeNode = value;
        }

        public virtual TreeParserSymbolScope RoutineScope => routineScope;

        public virtual Routine SetProgressType(ABLNodeType t)
        {
            progressType = t;
            return this;
        }

        internal override bool IsInstanceOfType(Symbol s) => s is Routine;        
    }
}
