using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IEventElement : IAccessibleElement
    {
        DataType ReturnType { get; }
        string ReturnTypeName { get; }
        string DelegateName { get; }
        IParameter[] GetParameters();
    }
}
