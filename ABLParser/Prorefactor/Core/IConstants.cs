using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core
{
    public class IConstants
    {

        //
        // Proparse.DLL Internals for attributes
        //

        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int FALSE = 0;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int TRUE = 1;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int STORETYPE = 1100;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int ST_DBTABLE = 1102;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int ST_TTABLE = 1103;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int ST_WTABLE = 1104;

        /// <summary>
        /// For attribute key "storetype", this attribute value indicates that the reference is to a local variable within the
        /// 4gl compile unit. This node attribute is set by TreeParser.
        /// </summary>
        public const int ST_VAR = 1105;

        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int OPERATOR = 1200;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int STATE2 = 1300;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int STATEHEAD = 1400;

        //
        // From version 1.2
        //

        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int ABBREVIATED = 1700;
        /// <summary>
        /// See Proparse documentation, "Node Attributes Reference". </summary>
        public const int INLINE_VAR_DEF = 2000;

        //
        // From TreeParser01
        //

        /// <summary>
        /// Node attribute key, set to 1 ("true") if the node is an unqualified table field reference. For example,
        /// "customer.name" is qualified, but "name" is unqualified. This node attribute is set by TreeParser01.
        /// </summary>
        public const int UNQUALIFIED_FIELD = 10150;

        // From JPNode, to be moved into an enum
        /// <summary>
        /// A valid value for setLink() and getLink() </summary>
        public const int SYMBOL = -210;

        /// <summary>
        /// A valid value for setLink() and getLink(). Link to a BufferScope object, set by tp01 for RECORD_NAME nodes and for
        /// Field_ref nodes for Field (not for Variable). Will not be present if this Field_ref is a reference to the symbol
        /// without referencing its value (i.e. no buffer scope).
        /// </summary>
        public const int BUFFERSCOPE = -212;
        /// <summary>
        /// A valid value for setLink() and getLink(). You should not use this directly. Only JPNodes of subtype BlockNode will
        /// have this set, so use BlockNode.getBlock instead.
        /// </summary>
        /// <seealso cref= org.prorefactor.core.nodetypes.BlockNode </seealso>
        public const int BLOCK = -214;
        /// <summary>
        /// A valid value for setLink() and getLink().
        /// </summary>
        public const int FIELD_CONTAINER = -217;
        /// <summary>
        /// A valid value for setLink() and getLink(). A link to a Call object, set by TreeParser01.
        /// </summary>
        public const int CALL = -218;
        /// <summary>
        /// A value fo setLink() and getLink(). Store index name used in SEARCH nodes
        /// </summary>
        public const int SEARCH_INDEX_NAME = -221;
        /// <summary>
        /// A value fo setLink() and getLink(). Boolean set to True if WHOLE-INDEX search
        /// </summary>
        public const int WHOLE_INDEX = -222;
        /// <summary>
        /// A value fo setLink() and getLink(). Store field name in SORT-ACCESS nodes
        /// </summary>
        public const int SORT_ACCESS = -223;

        // In statement: DEFINE TEMP-TABLE ... LIKE ... USE-INDEX xxx
        // xxx can point to an invalid index
        public const int INVALID_USEINDEX = 2800;

        private IConstants()
        {
            // Shouldn't be instantiated
        }

    } // interface IConstants

}
