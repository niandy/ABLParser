using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.NodeTypes
{
    /// <summary>
    /// Specialized type of JPNode for TYPE_NAME nodes
    /// </summary>
    public class TypeNameNode : JPNode
    {
        private readonly string qualName;

        public TypeNameNode(ProToken t, JPNode parent, int num, bool hasChildren, string qualName) : base(t, parent, num, hasChildren)
        {
            this.qualName = qualName;
        }

        public virtual string QualName => qualName;
    }

}
