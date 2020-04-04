using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Submenu : Widget
    {
        public Submenu(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.SUBMENU </returns>
        public override int ProgressType => Proparse.SUBMENU;

        internal override bool IsInstanceOfType(Symbol s) => s is Submenu;        
    }
}
