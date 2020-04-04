using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IIndexElement : IElement
    {
        IIndexComponentElement[] GetIndexComponents();
        bool Primary { get; }
        bool Unique { get; }
        bool WordIndex { get; }
        bool DefaultIndex { get; }
    }
}
