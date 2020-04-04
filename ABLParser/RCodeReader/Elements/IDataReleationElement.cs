using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IDataRelationElement : IElement
    {
        string ParentBufferName { get; }
        string ChildBufferName { get; }
        string FieldPairs { get; }
    }
}
