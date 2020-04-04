using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Button : Widget, IFieldLevelWidget
    {
        public Button(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.BUTTON </returns>
        public override int ProgressType => Proparse.BUTTON;

        internal override bool IsInstanceOfType(Symbol s) => s is Button;        
    }
}
