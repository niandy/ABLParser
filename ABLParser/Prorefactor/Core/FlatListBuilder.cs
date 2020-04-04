using System.Collections.Generic;

namespace ABLParser.Prorefactor.Core
{

    internal class FlatListBuilder : ICallback<IList<JPNode>>
    {
        public IList<JPNode> Result { get; } = new List<JPNode>();

        public bool VisitNode(JPNode node)
        {
            Result.Add(node);
            return true;
        }

    }
}

