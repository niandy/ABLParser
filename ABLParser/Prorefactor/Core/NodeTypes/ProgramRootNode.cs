using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.NodeTypes
{
    public class ProgramRootNode : BlockNode
    {
        public ProgramRootNode(ProToken t, JPNode parent, int num, bool hasChildren) : base(t, parent, num, hasChildren)
        {
        }
    }
}
