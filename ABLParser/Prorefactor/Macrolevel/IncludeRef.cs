using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    using System.Collections.Generic;
    using System.IO;

    public class IncludeRef : MacroRef
    {
        private readonly IList<MacroDef> includeArgs = new List<MacroDef>();
        private readonly IDictionary<string, MacroDef> argMap = new Dictionary<string, MacroDef>();
        private string fileRefName = "";
        private readonly int fileIndex;
        private bool usesNamedArgs;

        public IncludeRef(MacroRef parent, int line, int column) : base(parent, line, column)
        {
        }

        public IncludeRef(MacroRef parent, int line, int column, int fileIndex) : base(parent, line, column)
        {
            this.fileIndex = fileIndex;
        }

        public virtual void AddNamedArg(MacroDef arg)
        {
            usesNamedArgs = true;
            includeArgs.Add(arg);
            argMap[arg.Name.ToLower()] = arg;
        }

        public virtual void AddNumberedArg(MacroDef arg)
        {
            includeArgs.Add(arg);
        }

        /// <summary>
        /// Count from 1, the way that the arguments are referenced in ABL.
        /// </summary>
        public virtual MacroDef GetArgNumber(int num)
        {
            if (num > 0 && num <= includeArgs.Count)
            {
                return includeArgs[num - 1];
            }
            return null;
        }

        public override int FileIndex => fileIndex;

        /// <summary>
        /// Get the string that was used for referencing the include file name. For example, if the code was {includeMe.i},
        /// then the string "includeMe.i" is returned.
        /// </summary>
        public virtual string FileRefName
        {
            get => fileRefName;
            set => fileRefName = value;
        }

        public virtual MacroDef LookupNamedArg(string name)
        {
            if (!usesNamedArgs)
            {
                return null;
            }
            argMap.TryGetValue(name.ToLower(), out MacroDef namedArg);
            return namedArg;
        }

        public virtual int NumArgs()
        {
            return includeArgs.Count;
        }


        public virtual MacroDef Undefine(string name)
        {            
            if (argMap.TryGetValue(name, out MacroDef theArg))
            {
                argMap.Remove(name);
                argMap[""] = theArg;
                return theArg;
            }
            return null;
        }

        public override string ToString()
        {
            return "Include file at line " + Line;
        }

        public virtual void PrintMacroEvents(StreamWriter stream)
        {
            stream.WriteLine("Include #" + fileIndex + " - " + fileRefName);
            foreach (MacroEvent @event in macroEventList)
            {
                stream.WriteLine("  " + @event.ToString());
                if (@event is IncludeRef)
                {
                    ((IncludeRef)@event).PrintMacroEvents(stream);
                }
            }
        }
    }

}
