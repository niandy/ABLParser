using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser
{
    public interface Primative
    {

        /// <summary>
        /// Assign datatype, class, extent from another primative (for the LIKE keyword)
        /// </summary>
        void AssignAttributesLike(Primative likePrim);

        /// <summary>
        /// The name of the CLASS that this variable was defined for. This is more interesting than getDataType, which returns
        /// CLASS. Returns null if this variable was not defined for a CLASS.
        /// 
        /// TODO For 10.1B support, this should return the fully qualified class name, even if the reference wasn't fully
        /// qualified. If that's not to be the case, then John needs to look at method signatures implementation in Callgraph.
        /// </summary>
        string ClassName { get; }

        DataType DataType { get;  }

        int Extent { get; }

        Primative SetClassName(string className);

        Primative SetDataType(DataType dataType);

        Primative SetExtent(int extent);

    }

}
