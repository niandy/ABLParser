using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface ITableElement : IAccessibleElement
    {
        string BeforeTableName { get; }
        IVariableElement[] GetFields();
        IIndexElement[] GetIndexes();
    }
}
