using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    using System.Collections.Generic;

    /// <summary>
    /// Abstract class for a macro reference. There are two subclasses:<ul>
    /// <li>one for references to named macros (i.e. those named with &amp;global, &amp;scoped, or an include argument)
    /// <li>one for references to include files.
    /// </ul>
    /// </summary>
    public abstract class MacroRef : MacroEvent
    {
        private readonly MacroRef parent;
        private readonly int refColumn;
        private readonly int refLine;

        /// <summary>
        /// A list of macro references and defines that are in this macro's source </summary>
        public readonly IList<MacroEvent> macroEventList = new List<MacroEvent>();

        public MacroRef(MacroRef parent, int line, int column)
        {
            this.parent = parent;
            this.refLine = line;
            this.refColumn = column;
        }

        public override MacroRef Parent => parent;

        public virtual int Line => refLine;

        public virtual int Column => refColumn;

        /// <summary>
        /// Find <i>external macro references</i>. An external macro is an include file, a &amp;GLOBAL or a &amp;SCOPED from another
        /// file, and include args.
        /// 
        /// TODO: (Jan 26) This doesn't seem right to me anymore. An &amp;UNDEFINE only affects the local scope. If re-implemented
        /// after building a pseudoprocessor, consider dropping this. &amp;UNDEFINE of a &amp;GLOBAL or of a &amp;SCOPED from another file
        /// is considered a reference. &amp;UNDEFINE of an include argument is considered a reference.
        /// 
        /// The subroutine is recursive, because a local define may incur an external reference!
        /// </summary>
        /// <returns> An array of objects: MacroRef and MacroDef (for UNDEFINE). </returns>
        public virtual IList<MacroEvent> FindExternalMacroReferences()
        {
            IList<MacroEvent> ret = new List<MacroEvent>();
            for (IEnumerator<MacroEvent> it = macroEventList.GetEnumerator(); it.MoveNext();)
            {
                FindExternalMacroReferences(it.Current, ret);
            }
            return ret;
        }

        /// <seealso cref= #findExternalMacroReferences() </seealso>
        /// <param name="begin"> An array of two integers to indicate the beginning line/column. May be null to indicate the beginning
        ///          of the range is open ended. </param>
        /// <param name="end"> An array of two integers to indicate the ending line/column. May be null to indicate the ending of the
        ///          range is open ended. </param>
        public virtual IList<MacroEvent> FindExternalMacroReferences(int[] begin, int[] end)
        {
            IList<MacroEvent> ret = new List<MacroEvent>();
            for (IEnumerator<MacroEvent> it = macroEventList.GetEnumerator(); it.MoveNext();)
            {
                MacroEvent next = it.Current;
                MacroPosition pos = next.Position;
                if (IsInRange(pos.Line, pos.Column, begin, end))
                {
                    FindExternalMacroReferences(next, ret);
                }
            }

            return ret;
        }

        private void FindExternalMacroReferences(MacroEvent obj, IList<MacroEvent> list)
        {
            if (obj == null)
            {
                return;
            }
            if (obj is IncludeRef)
            {
                list.Add(obj);
                return;
            }
            if (obj is MacroDef def)
            {
                if (def.Type == MacroDefinitionType.UNDEFINE)
                {
                    if (def.UndefWhat.Type == MacroDefinitionType.NAMEDARG)
                    {
                        list.Add(def);
                        return;
                    }
                    if (!IsMine(def.UndefWhat.Parent))
                    {
                        list.Add(def);
                    }
                }
                return;
            }
            // Only one last type we're interested in...
            if (!(obj is NamedMacroRef))
            {
                return;
            }
            NamedMacroRef @ref = (NamedMacroRef)obj;
            if (!IsMine(@ref))
            {
                list.Add(@ref);
                return;
            }
            // It's possible for an internal macro to refer to an external macro
            for (IEnumerator<MacroEvent> it = @ref.macroEventList.GetEnumerator(); it.MoveNext();)
            {
                FindExternalMacroReferences(it.Current, list);
            }
        }

        /// <summary>
        /// Find references to an include file by the include file's file index number. Search is recursive, beginning at this
        /// MacroRef object.
        /// </summary>
        /// <param name="fileIndex"> The fileIndex for the include file we want references to. </param>
        /// <returns> An array of IncludeRef objects. </returns>
        public virtual IList<IncludeRef> FindIncludeReferences(int fileIndex)
        {
            IList<IncludeRef> ret = new List<IncludeRef>();
            FindIncludeReferences(fileIndex, this, ret);
            return ret;
        }

        private void FindIncludeReferences(int fileIndex, MacroRef @ref, IList<IncludeRef> list)
        {
            if (@ref == null)
            {
                return;
            }
            if (@ref is IncludeRef incl)
            {
                if (incl.FileIndex == fileIndex)
                {
                    list.Add(incl);
                }
            }
            for (IEnumerator<MacroEvent> it = @ref.macroEventList.GetEnumerator(); it.MoveNext();)
            {
                MacroEvent next = it.Current;
                if (next is MacroRef)
                {
                    FindIncludeReferences(fileIndex, (MacroRef)next, list);
                }
            }
        }

        public abstract int FileIndex { get; }

        public override MacroPosition Position => new MacroPosition(parent == null ? 0 : parent.FileIndex, refLine, refColumn);

        /// <summary>
        /// Assuming an x,y range, this function returns whether an input x and y are within the specified range of x,y begin
        /// and x,y end. We use this primarily for checking if a line/column are within the specified range. The "range" may be
        /// open ended, see parameter descriptions.
        /// </summary>
        /// <param name="x"> The x value to check that it is within range </param>
        /// <param name="y"> The y value to check that it is within range </param>
        /// <param name="begin"> An array of 2 integers to specify the beginning of the x,y range. May be null to indicate that the
        ///          beginning is open ended. </param>
        /// <param name="end"> An array of 2 integers to specify the ending of the x,y range. May be null to indicate that the
        ///          beginning is open ended.
        /// @return </param>
        public static bool IsInRange(int x, int y, int[] begin, int[] end)
        {
            if ((begin != null) && ((x < begin[0]) || ((x == begin[0]) && (y < begin[1]))))
            {
                return false;
            }
            if ((end != null) && ((x > end[0]) || ((x == end[0]) && (y > end[1]))))
            {
                return false;
            }
            return true;
        }
    }

}
