using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    /// <summary>
    /// A Symbol defined with DEFINE &lt;widget-type&gt; or any of the other various syntaxes which implicitly define a widget.
    /// This includes FRAMEs, WINDOWs, MENUs, etc.
    /// </summary>
    public abstract class Widget : Symbol, IWidget
    {

        public Widget(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        public override string FullName => Name;

    }

}
