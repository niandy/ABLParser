using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface IPropertyElement : IAccessibleElement
    {
        IVariableElement Variable { get; }
        IMethodElement Getter { get; }
        IMethodElement Setter { get; }
    }
}
