using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IBufferElement : IAccessibleElement
    {
        string DatabaseName { get; }
        string TableName { get; }
        bool TempTableBuffer { get; }
    }
}
