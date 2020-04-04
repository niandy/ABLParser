using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;

namespace ABLParser.Prorefactor.Core.NodeTypes
{
    public class FieldRefNode : JPNode
    {
        private ContextQualifier qualifier;

        public FieldRefNode(ProToken t, JPNode parent, int num, bool hasChildren) : base(t, parent, num, hasChildren)
        {
        }

        public virtual ContextQualifier ContextQualifier
        {
            set => qualifier = value;
            get => qualifier;
        }

        /// <summary>
        /// Returns null if symbol is null or is a graphical component
        /// </summary>
        public virtual DataType DataType
        {
            get
            {
                if (Symbol == null)
                {
                    // Just in order to avoid NPE
                    return null;
                }
                if (!(Symbol is Primative))
                {
                    return null;
                }
                return ((Primative)Symbol).DataType;
            }
        }

        /// <summary>
        /// We very often need to reference the ID node for a Field_ref node. The Field_ref node is a synthetic node - it
        /// doesn't have any text. If we want the field/variable name, or the file/line/column, then we probably want to get
        /// those from the ID node.
        /// </summary>
        public override JPNode IdNode => FindDirectChild(ABLNodeType.ID.Type);

    }

}
