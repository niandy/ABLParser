using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Menu : Widget
    {
        public Menu(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.MENU </returns>
        public override int ProgressType => Proparse.MENU;

        internal override bool IsInstanceOfType(Symbol s) => s is Menu;        
    }
}
