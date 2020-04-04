using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public interface ITypeInfo
    {
        string TypeName { get; }
        string ParentTypeName { get; }
        string AssemblyName { get; }
        IList<string> Interfaces { get; }

        bool Final { get; }
        bool Interface { get; }
        bool HasStatics();
        bool BuiltIn { get; }
        bool Hybrid { get; }
        bool HasDotNetBase();
        bool Abstract { get; }
        bool Serializable { get; }
        bool UseWidgetPool { get; }

        ICollection<IMethodElement> Methods { get; }
        ICollection<IPropertyElement> Properties { get; }
        ICollection<IEventElement> Events { get; }
        ICollection<IVariableElement> Variables { get; }
        ICollection<ITableElement> Tables { get; }
        ICollection<IBufferElement> Buffers { get; }

        IBufferElement GetBuffer(string inName);
        IBufferElement GetBufferFor(string name);
        IPropertyElement GetProperty(string name);
        ITableElement GetTempTable(string inName);

        bool HasTempTable(string inName);
        bool HasMethod(string name);
        bool HasProperty(string name);
        bool HasBuffer(string inName);

    }
}
