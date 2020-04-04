using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    public interface IIndex
    {
        string Name { get; }
        ITable Table { get; }
        bool Unique { get; }
        bool Primary { get; }
        IList<IField> Fields { get; }
    }
}
