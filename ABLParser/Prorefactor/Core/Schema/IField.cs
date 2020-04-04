using ABLParser.Prorefactor.Treeparser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    public interface IField : Primative
    {
        string GetName();
        IField CopyBare(ITable toTable);
        ITable Table { get; set; }
    }

}
