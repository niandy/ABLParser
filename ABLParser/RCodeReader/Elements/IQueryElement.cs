using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IQueryElement : IAccessibleElement
    {
        string[] GetBufferNames();
    }
}
