using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    /// <summary>
    /// Interface for a node in the macro event tree.
    /// </summary>
    public abstract class MacroEvent
    {

        /// <returns> Parent element </returns>
        public abstract MacroRef Parent { get; }

        /// <returns> Position of this macro reference </returns>
        public abstract MacroPosition Position { get; }

        /// <summary>
        /// Is a macro ref/def myself, or, a child of mine? </summary>
        internal virtual bool IsMine(MacroEvent obj)
        {
            if (obj == null)
                return false;
            if (obj == this)
                return true;
            return IsMine(obj.Parent);
        }

    }


}
