using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IDataSourceElement : IAccessibleElement
    {
        string QueryName { get; }
        string KeyComponents { get; }
        string[] GetBufferNames();
    }
}
