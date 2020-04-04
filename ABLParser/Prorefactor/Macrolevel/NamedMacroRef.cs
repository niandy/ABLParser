using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    /// <summary>
    /// A reference to a macro argument, i.e. {1} or {&amp;name}. Origin might be an include argument or an &amp;DEFINE.
    /// </summary>
    public class NamedMacroRef : MacroRef
    {
        private readonly MacroDef macroDef;

        public NamedMacroRef(MacroDef macro, MacroRef parent, int line, int column) : base(parent, line, column)
        {
            this.macroDef = macro;
        }

        public virtual MacroDef MacroDef => macroDef;

        public override int FileIndex => Parent.FileIndex;

        public override string ToString()
        {
            if (macroDef == null)
            {
                return "Reference to unknown macro";
            }
            else
            {
                return "Reference to " + macroDef.Name + " defined in file #" + macroDef.Parent.FileIndex;
            }
        }
    }

}
