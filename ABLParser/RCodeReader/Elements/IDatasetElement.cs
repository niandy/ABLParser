using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IDatasetElement : IAccessibleElement
    {
        IDataRelationElement[] GetDataRelations();
        string[] GetBufferNames();
    }
}
