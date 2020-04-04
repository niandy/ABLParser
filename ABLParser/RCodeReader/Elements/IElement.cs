using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IElement
    {
        string Name { get; }
        int SizeInRCode { get; }
    }
}
