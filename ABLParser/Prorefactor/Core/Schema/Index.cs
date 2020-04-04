using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    public class Index : IIndex
    {
        public Index(ITable table, string name, bool unique, bool primary)
        {
            this.Table = table;
            this.Name = name;
            this.Unique = unique;
            this.Primary = primary;
        }

        public virtual void AddField(IField field)
        {
            Fields.Add(field);
        }

        public ITable Table { get; }

        public string Name { get; }

        public bool Unique { get; }

        public bool Primary { get; }

        public IList<IField> Fields { get; } = new List<IField>();
    }

}
