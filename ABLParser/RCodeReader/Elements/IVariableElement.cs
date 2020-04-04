using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IVariableElement : IAccessibleElement
    {
        int Extent { get; }
        DataType DataType { get; }
        string TypeName { get; }

        bool ReadOnly { get; }
        bool WriteOnly { get; }
        bool NoUndo { get; }
        bool BaseIsDotNet { get; }
    }
}
