using System.Collections.Immutable;

namespace ABLParser.Sonar.Api.Objects
{
    using ABLParser.Prorefactor.Core;
    using ABLParser.Prorefactor.Core.Schema;
    using ABLParser.RCodeReader.Elements;
    using System;
	using System.Collections.Generic;

	public class RCodeTTWrapper : ITable
	{
		private readonly ITableElement table;

		private readonly IList<IField> fields = new List<IField>();
		private readonly IList<IIndex> indexes = new List<IIndex>();
		private readonly SortedSet<IField> sortedFields = new SortedSet<IField>(Constants.FIELD_NAME_ORDER);

		public RCodeTTWrapper(ITableElement t)
		{
			this.table = t;

			foreach (IVariableElement fld in table.GetFields())
			{
				IField iFld = new RCodeTTFieldWrapper(this, fld);
				fields.Add(iFld);
				sortedFields.Add(iFld);
			}
			foreach (IIndexElement idx in table.GetIndexes())
			{
				IIndex iIdx = new RCodeTTIndexWrapper(this, idx);
				indexes.Add(iIdx);
			}
		}

		public virtual ITableElement BackingObject => table;

		public IDatabase Database => Constants.nullDatabase;

		public string GetName() => table.Name;

		public void Add(IField field) => throw new System.NotSupportedException();

		public void Add(IIndex index) => throw new System.NotSupportedException();

		public IField LookupField(string lookupName)
		{
			foreach (IField fld in fields)
			{
				if (fld.GetName().ToLower().StartsWith(lookupName.ToLower(), StringComparison.Ordinal))
				{
					return fld;
				}
			}
			return null;
		}

		public ISet<IField> FieldSet => sortedFields.ToImmutableSortedSet();

		public IList<IField> FieldPosOrder => fields.ToImmutableList();

		public IList<IIndex> Indexes => indexes.ToImmutableList();

		public IIndex LookupIndex(string name)
		{
			foreach (IIndex idx in indexes)
			{
				if (idx.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return idx;
				}
			}
			return null;
		}

		public int Storetype => IConstants.ST_TTABLE;

		public override string ToString() => $"TT Wrapper for {table.Name} - {FieldSet.Count} fields - {Indexes.Count} indexes";
	}
}
