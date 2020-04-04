using System;
using System.Collections.Generic;
using System.Linq;

namespace ABLParser.Prorefactor.Core
{
    using System.Collections.Generic;

    internal class JPNodeQuery : ICallback<IList<JPNode>>
    {
        private readonly IList<JPNode> result = new List<JPNode>();
        private readonly ISet<ABLNodeType> findTypes;
        private readonly bool stateHeadOnly;
        private readonly bool mainFileOnly;
        private readonly JPNode currStatement;

        public JPNodeQuery(ABLNodeType type, params ABLNodeType[] types) : this(false, false, null, type, types)
        {
        }

        public JPNodeQuery(bool stateHeadOnly) : this(stateHeadOnly, false, null, null)
        {
        }

        public JPNodeQuery(bool stateHeadOnly, ABLNodeType type, params ABLNodeType[] types) : this(stateHeadOnly, false, null, type, types)
        {
        }

        public JPNodeQuery(bool stateHeadOnly, bool mainFileOnly, JPNode currentStatement, ABLNodeType type, params ABLNodeType[] types)
        {
            this.stateHeadOnly = stateHeadOnly;
            this.mainFileOnly = mainFileOnly;
            this.currStatement = currentStatement;
            if (type == null)
            {
                this.findTypes = new HashSet<ABLNodeType>(); 
            }
            else
            {
                this.findTypes = types == null ? new HashSet<ABLNodeType>() : new HashSet<ABLNodeType>(types);
                this.findTypes.Add(type);
            }
        }

        public IList<JPNode> Result => result;

        public bool VisitNode(JPNode node)
        {
            if ((currStatement != null) && (node.Statement != currStatement))
            {
                return false;
            }

            if (mainFileOnly && (node.FileIndex > 0))
            {
                return true;
            }

            if (stateHeadOnly && !node.StateHead)
            {
                return true;
            }

            if (findTypes.Count == 0 || findTypes.Contains(node.NodeType))
            {
                result.Add(node);
            }

            return true;
        }

    }

}
