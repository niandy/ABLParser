using System.Collections.Generic;

namespace ABLParser.Prorefactor.Proparser
{
    public class IntegerIndex<T>
    {

        private readonly Dictionary<int, T> biMap = new Dictionary<int, T>();
        private readonly Dictionary<T, int> revMap = new Dictionary<T, int>();
        private int nextIndex;

        /// <summary>
        /// Add the value if it's not already there. Returns the new or existing index.
        /// </summary>
        public virtual int Add(T val)
        {
            if (revMap.TryGetValue(val, out int ret))
            {
                return ret;
            }

            ret = nextIndex++;
            biMap[ret] = val;
            revMap[val] = ret;

            return ret;
        }

        public virtual void Clear()
        {
            biMap.Clear();
            revMap.Clear();
            nextIndex = 0;
        }

        /// <summary>
        /// Returns -1 if not found
        /// </summary>
        public virtual int GetIndex(T val)
        {
            if (revMap.TryGetValue(val, out int ret))
            {
                return ret;
            }
            return -1;            
        }

        /// <summary>
        /// Returns null if not found
        /// </summary>
        public virtual T GetValue(int index)
        {
            return biMap[index];
        }

        /// <summary>
        /// Returns an array list in order from zero to number of indexes of all the values
        /// </summary>
        public virtual IList<T> Values
        {
            get
            {                
                IList<T> list = new List<T>(nextIndex);
                for (int i = 0; i < nextIndex; ++i)
                {
                    list.Add(biMap[i]);
                }

                return list;
            }
        }

        public virtual bool HasIndex(int index)
        {
            return biMap.ContainsKey(index);
        }

        public virtual bool HasValue(T value)
        {
            return biMap.ContainsValue(value);
        }

        public virtual int Size()
        {
            return nextIndex;
        }
    }

}
