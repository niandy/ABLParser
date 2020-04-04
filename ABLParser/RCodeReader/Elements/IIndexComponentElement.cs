using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IIndexComponentElement : IElement
    {
        int FieldPosition { get; }
        bool Ascending { get; }
    }
}
