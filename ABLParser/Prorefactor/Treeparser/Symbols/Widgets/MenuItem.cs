using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class MenuItem : Widget
    {
        public MenuItem(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.MENUITEM </returns>
        public override int ProgressType => Proparse.MENUITEM;

        internal override bool IsInstanceOfType(Symbol s) => s is MenuItem;        
    }
}
