using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public abstract class AbstractElement : IElement
    {
        public AbstractElement() : this("<noname>")
        {
        }

        public AbstractElement(string name)
        {
            this.Name = name ?? "<noname>";
        }

        public string Name { get; }
        public abstract int SizeInRCode { get; }
    }
}
