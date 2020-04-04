using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Image : Widget, IFieldLevelWidget
    {
        public Image(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.IMAGE </returns>
        public override int ProgressType => Proparse.IMAGE;

        internal override bool IsInstanceOfType(Symbol s) => s is Image;        
    }
}
