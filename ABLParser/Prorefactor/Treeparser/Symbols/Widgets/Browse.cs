using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Browse : FieldContainer, IFieldLevelWidget
    {
        public Browse(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <returns> NodeTypes.BROWSE </returns>
        public override int ProgressType => Proparse.BROWSE;

        internal override bool IsInstanceOfType(Symbol s) => s is Browse;        
    }
}
