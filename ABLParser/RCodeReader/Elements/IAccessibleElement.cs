using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IAccessibleElement : IElement
    {
        bool Protected { get; }
        bool Public { get; }
        bool Private { get; }
        bool Abstract { get; }
        bool Static { get; }
    }
}
