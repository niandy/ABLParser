using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using System.Globalization;

namespace ABLParser.Prorefactor.Macrolevel
{
    using ABLParser.Prorefactor.Core;
    using System.Collections.Generic;

    /// <summary>
    /// Catch preprocessor events in order to generate a macro tree.
    /// </summary>
    public class PreprocessorEventListener : IPreprocessorEventListener
    {
        private readonly IncludeRef root;

        // AppBuilder managed is read-only by default - Keep track of editable code sections
        private bool appBuilderCode = false;
        private readonly IList<EditableCodeSection> sections = new List<EditableCodeSection>();

        /* Temp stack of scopes, just used during tree creation */
        private readonly LinkedList<Scope> scopeStack = new LinkedList<Scope>();
        private IncludeRef currInclude;
        /* Temp stack of global defines, just used during tree creation */
        private readonly IDictionary<string, MacroDef> globalDefMap = new Dictionary<string, MacroDef>();
        private MacroRef currRef;
        /* Temp object for editable section */
        private EditableCodeSection currSection;

        public PreprocessorEventListener()
        {
            root = new IncludeRef(null, 0, 0);

            currRef = root;
            currInclude = root;
            scopeStack.AddFirst(new Scope(root));
        }

        public virtual IncludeRef MacroGraph
        {
            get
            {
                return root;
            }
        }

        public void Define(int line, int column, string name, string value, MacroDefinitionType type)
        {
            MacroDef newDef = new MacroDef(currRef, type, line, column, name, value);
            if (type == MacroDefinitionType.GLOBAL)
            {
                globalDefMap[name] = newDef;
            }
            else if (type == MacroDefinitionType.SCOPED)
            {
                Scope currScope = scopeStack.First.Value;
                currScope.defMap[name] = newDef;
            }
            currRef.macroEventList.Add(newDef);
        }

        public void PreproElse(int line, int column)
        {
            // Nothing for now
        }

        public void PreproElseIf(int line, int column)
        {
            // Nothing for now
        }

        public void PreproEndIf(int line, int column)
        {
            // Nothing for now
        }

        public void PreproIf(int line, int column, bool value)
        {
            // Nothing for now
        }

        public void Include(int line, int column, int currentFile, string incFile)
        {
            IncludeRef newRef = new IncludeRef(currRef, line, column, currentFile);
            scopeStack.AddFirst(new Scope(newRef));
            currRef.macroEventList.Add(newRef);
            currInclude = newRef;
            currRef = newRef;
            newRef.FileRefName = incFile;
        }

        public void IncludeArgument(string argName, string value, bool undefined)
        {
            int argNum = 0;
            try
            {
                argNum = int.Parse(argName);
            }
            catch (System.FormatException)
            {
            }
            MacroDef newArg;
            if ((argNum == 0) || (argNum != currInclude.NumArgs() + 1))
            {
                newArg = new MacroDef(currInclude.Parent, MacroDefinitionType.NAMEDARG, argName);
                currInclude.AddNamedArg(newArg);
            }
            else
            {
                newArg = new MacroDef(currInclude.Parent, MacroDefinitionType.NUMBEREDARG);
                currInclude.AddNumberedArg(newArg);
            }
            newArg.Value = value;
            newArg.Undefined = undefined;
            newArg.IncludeRef = currInclude;
        }


        public void IncludeEnd()
        {
            scopeStack.RemoveFirst();
            currInclude = scopeStack.First.Value.includeRef;
            currRef = currRef.Parent;
        }

        public void MacroRef(int line, int column, string macroName)
        {
            NamedMacroRef newRef = new NamedMacroRef(FindMacroDef(macroName), currRef, line, column);
            currRef.macroEventList.Add(newRef);
            currRef = newRef;
        }

        public void MacroRefEnd()
        {
            currRef = currRef.Parent;
        }

        public void Undefine(int line, int column, string name)
        {
            // Add an object for this macro event.
            MacroDef newDef = new MacroDef(currRef, MacroDefinitionType.UNDEFINE, line, column, name, "");
            currRef.macroEventList.Add(newDef);
            
            // Now process the undefine.
            Scope currScope = scopeStack.First.Value;
            // First look for local SCOPED define
            if (currScope.defMap.TryGetValue(name, out MacroDef tmp))
            {
                newDef.UndefWhat = tmp;
                currScope.defMap.Remove(name);
                return;
            }
            // Second look for a named include file argument
            tmp = currInclude.Undefine(name);
            if (tmp != null)
            {
                newDef.UndefWhat = tmp;
                return;
            }
            // Third look for a non-local SCOPED define
            IEnumerator<Scope> it = scopeStack.GetEnumerator();
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            it.MoveNext(); // skip the current scope - already checked.
            while (it.MoveNext())
            {
                currScope = it.Current;
                if (currScope.defMap.TryGetValue(name, out tmp))
                {
                    newDef.UndefWhat = tmp;
                    currScope.defMap.Remove(name);
                    return;
                }
            }
            // Fourth look for a GLOBAL define
            if (globalDefMap.TryGetValue(name.ToLower(CultureInfo.GetCultureInfo("en")), out tmp))
            {
                globalDefMap.Remove(name.ToLower(CultureInfo.GetCultureInfo("en")));
                newDef.UndefWhat = tmp;
            }
        }

        public virtual void AnalyzeSuspend(string str, int line)
        {
            appBuilderCode = true;
            if ((currInclude.FileIndex == 0) && ProToken.IsTokenEditableInAB(str))
            {
                currSection = new EditableCodeSection
                {
                    FileNum = currInclude.FileIndex,
                    StartLine = line
                };
            }
        }

        public void AnalyzeResume(int line)
        {
            if ((currSection != null) && (currInclude.FileIndex == currSection.FileNum))
            {
                currSection.EndLine = line;
                sections.Add(currSection);
            }
            currSection = null;
        }

        public virtual bool AppBuilderCode
        {
            get
            {
                return appBuilderCode;
            }
        }

        public virtual bool IsLineInEditableSection(int file, int line)
        {
            foreach (EditableCodeSection range in sections)
            {
                if ((range.FileNum == file) && (range.StartLine <= line) && (range.EndLine >= line))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Find a MacroDef by name. NOTE: {*} and other such built-in macro reference are not yet implemented.
        /// </summary>
        private MacroDef FindMacroDef(string name)
        {            
            Scope currScope = scopeStack.First.Value;
            // First look for local SCOPED define            
            if (currScope.defMap.TryGetValue(name, out MacroDef ret))
            {
                return ret;
            }
            // Second look for a named include file argument
            ret = currInclude.LookupNamedArg(name);
            if (ret != null)
            {
                return ret;
            }
            // Third look for a non-local SCOPED define
            IEnumerator<Scope> it = scopeStack.GetEnumerator();
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            it.MoveNext(); // skip the current scope - already checked.
            while (it.MoveNext())
            {
                currScope = it.Current;                
                if (currScope.defMap.TryGetValue(name, out ret))
                {
                    return ret;
                }
            }
            // Fourth look for a GLOBAL define
            globalDefMap.TryGetValue(name, out ret);
            return ret;
        }

        public virtual IList<EditableCodeSection> EditableCodeSections
        {
            get
            {
                return sections.ToImmutableList<EditableCodeSection>();
                //return Collections.unmodifiableList(sections);
            }
        }

        // These scopes are temporary, just used during tree creation
        private class Scope
        {
            internal IDictionary<string, MacroDef> defMap = new Dictionary<string, MacroDef>();
            internal IncludeRef includeRef;

            public Scope(IncludeRef @ref)
            {
                this.includeRef = @ref;
            }
        }

        public class EditableCodeSection
        {
            private int fileNum;
            private int startLine;
            private int endLine;

            /// <returns> Always 0 for now </returns>
            public virtual int FileNum
            {
                get => fileNum;
                internal set => fileNum = value;
            }

            /// <returns> Starting line number of editable code sectin </returns>
            public virtual int StartLine
            {
                get => startLine;
                internal set => startLine = value;
            }

            /// <returns> Ending line number of editable code sectin </returns>
            public virtual int EndLine
            {
                get => endLine;
                internal set => endLine = value;
            }
        }
    }

}
