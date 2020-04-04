using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core
{
    public class JPNodePredicateQuery : ICallback<IList<JPNode>>
    {
        public static readonly System.Predicate<JPNode> MAIN_FILE_ONLY = node => node.FileIndex == 0;
        public static readonly System.Predicate<JPNode> STATEMENT_ONLY = node => node.StateHead;

        private readonly IList<JPNode> result = new List<JPNode>();
        private readonly IList<System.Predicate<JPNode>> predicates = new List<System.Predicate<JPNode>>();

        public JPNodePredicateQuery(System.Predicate<JPNode> pred1)
        {
            predicates.Add(pred1);
        }

        public JPNodePredicateQuery(System.Predicate<JPNode> pred1, System.Predicate<JPNode> pred2)
        {
            predicates.Add(pred1);
            predicates.Add(pred2);
        }

        public JPNodePredicateQuery(System.Predicate<JPNode> pred1, System.Predicate<JPNode> pred2, System.Predicate<JPNode> pred3)
        {
            predicates.Add(pred1);
            predicates.Add(pred2);
            predicates.Add(pred3);
        }

        public IList<JPNode> Result
        {
            get
            {
                return result;
            }
        }

        public bool VisitNode(JPNode node)
        {
            bool isValid = true;
            int offset = 0;
            while (isValid && (offset < predicates.Count))
            {
                isValid &= predicates[offset++].Invoke(node);
            }
            if (isValid)
            {
                result.Add(node);
            }

            return true;
        }

    }

}
