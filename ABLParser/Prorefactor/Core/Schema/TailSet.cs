using System.Collections.Generic;

namespace ABLParser.Prorefactor.Core.Schema
{
    internal static class TailSet
    {
        internal static SortedSet<T> GetTailSet<T>(this SortedSet<T> set, T from) 
        {            
            if (set.Count == 0 || set.Comparer.Compare(from, set.Max) > 0)
            {
                return new SortedSet<T>();
            }
            else
            {
                return set.GetViewBetween(from, set.Max);
            }
        }
    }
}
