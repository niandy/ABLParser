using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Rectangle : Widget, IFieldLevelWidget
    {
        public Rectangle(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.RECTANGLE </returns>
        public override int ProgressType => Proparse.RECTANGLE;

        internal override bool IsInstanceOfType(Symbol s) => s is Rectangle;        
    }
}
