using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    using System.Collections.Generic;

    /// <summary>
    /// Static functions for working with an existing macro tree.
    /// </summary>
    public class MacroLevel
    {

        private MacroLevel()
        {

        }

        /// <summary>
        /// Build and return an array of the MacroRef objects, which would map to the SOURCENUM attribute from JPNode. Built
        /// simply by walking the tree and adding every MacroRef to the array.
        /// </summary>
        public static MacroRef[] SourceArray(MacroRef top)
        {
            List<MacroRef> list = new List<MacroRef>();
            SourceArray2(top, list);            
            return list.ToArray();
        }

        private static void SourceArray2(MacroRef macroNode, List<MacroRef> list)
        {
            list.Add(macroNode);
            foreach (MacroEvent @event in macroNode.macroEventList)
            {
                if (@event is MacroRef)
                {
                    SourceArray2((MacroRef)@event, list);
                }
            }
        }

    }

}
