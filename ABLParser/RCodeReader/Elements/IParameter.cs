using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IParameter : IElement
    {        
        int Extent { get; }
        string DataType { get; }
        ParameterMode Mode { get; }
        ParameterType ParameterType { get; }
        bool ClassDataType { get; }
    }
}
