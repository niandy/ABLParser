using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    /// <summary>
    /// A Symbol defined with DEFINE QUERY. </summary>
    public class Query : Symbol
    {
        public Query(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <summary>
        /// For this subclass of Symbol, fullName() returns the same value as getName()
        /// </summary>
        public override string FullName => Name;

        internal override bool IsInstanceOfType(Symbol s) => s is Query;

        /// <returns> NodeTypes.QUERY </returns>
        public override int ProgressType => Proparse.QUERY;
    }
}
