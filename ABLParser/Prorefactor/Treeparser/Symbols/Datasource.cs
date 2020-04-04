using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    public class Datasource : Symbol
    {

        public Datasource(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <summary>
        /// For this subclass of Symbol, fullName() returns the same value as getName()
        /// </summary>
        public override string FullName => Name;

        internal override bool IsInstanceOfType(Symbol s) => s is Datasource;

        /// <summary>
        /// Returns NodeTypes.DATASOURCE
        /// </summary>
        public override int ProgressType => Proparse.DATASOURCE;

    }

}
