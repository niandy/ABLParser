using ABLParser.Prorefactor.Core.Schema;
using ABLParser.RCodeReader.Elements;
using System.Collections.Generic;
using System.Collections.Immutable;
using System;

namespace ABLParser.Sonar.Api.Objects
{


	public class RCodeTTIndexWrapper : IIndex
	{
		private readonly IIndexElement index;
		private readonly IList<IField> fields = new List<IField>();

		public RCodeTTIndexWrapper(ITable table, IIndexElement index)
		{
			this.Table = table ?? throw new ArgumentNullException(nameof(table));
			this.index = index ?? throw new ArgumentNullException(nameof(index));
			foreach (IIndexComponentElement fld in index.GetIndexComponents())
			{
				fields.Add(table.FieldPosOrder[fld.FieldPosition]);
			}
		}

		public virtual IIndexElement BackingObject => index;

		public string Name => index.Name;

		public ITable Table { get; }

        public bool Unique => index.Unique;

		public bool Primary => index.Primary;

		public IList<IField> Fields => fields.ToImmutableList();
	}
}
