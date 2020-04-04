using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    /// <summary>
    /// A macro DEFINE (global or scoped) or UNDEFINE or an include argument (named or numbered/positional).
    /// </summary>
    public class MacroDef : MacroEvent
    {
        private readonly MacroRef parent;
        private readonly MacroDefinitionType type;
        private readonly int column;
        private readonly int line;
        private readonly string name;

        /// <summary>
        /// For an UNDEFINE - undef what? </summary>
        private MacroDef undefWhat = null;
        /// <summary>
        /// For an include argument - what include reference is it for? </summary>
        private IncludeRef includeRef = null;
        private string value;
        // If named argument doesn't have any defined value
        private bool undefined;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public MacroDef(MacroRef parent, @Nonnull MacroDefinitionType type)
        public MacroDef(MacroRef parent, MacroDefinitionType type) : this(parent, type, 0, 0, "", "")
        {
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public MacroDef(MacroRef parent, @Nonnull MacroDefinitionType type, @Nonnull String name)
        public MacroDef(MacroRef parent, MacroDefinitionType type, string name) : this(parent, type, 0, 0, name, "")
        {
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public MacroDef(MacroRef parent, @Nonnull MacroDefinitionType type, int line, int column, @Nonnull String name, String value)
        public MacroDef(MacroRef parent, MacroDefinitionType type, int line, int column, string name, string value)
        {
            this.parent = parent;
            this.type = type;
            this.line = line;
            this.column = column;
            this.name = name;
            this.value = value;
        }

        public virtual string Name => name;

        public virtual string Value
        {
            set => this.value = value;
            get => value;
        }


        public virtual bool Undefined
        {
            set => undefined = value;
            get => undefined;
        }


        public override MacroRef Parent => parent;

        public virtual MacroDefinitionType Type => type;

        public override MacroPosition Position => new MacroPosition(parent.Position.FileNum, line, column);

        public virtual MacroDef UndefWhat
        {
            set => undefWhat = value;
            get => undefWhat;
        }

        public virtual IncludeRef IncludeRef
        {
            set => includeRef = value;
            get => includeRef;
        }
        public override string ToString()
        {
            return type + " macro '" + name + "' at position " + line + ":" + column;
        }
    }

}
