using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IMethodElement : IAccessibleElement
    {
        DataType ReturnType { get; }
        string ReturnTypeName { get; }
        int Extent { get; }
        IParameter[] GetParameters();

        bool Procedure { get; }
        bool Function { get; }
        bool Constructor { get; }
        bool Destructor { get; }
        bool Overloaded { get; }
    }
}
