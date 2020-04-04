using System;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class TableElementV11 : AbstractAccessibleElement, ITableElement
	{
		private readonly int flags;
		private readonly IVariableElement[] fields;
		private readonly IIndexElement[] indexes;

		public TableElementV11(string name, AccessType accessType, int flags, IVariableElement[] fields, IIndexElement[] indexes, string beforeTableName) : base(name, accessType)
		{
			this.fields = fields;
			this.indexes = indexes;
			this.BeforeTableName = beforeTableName;
			this.flags = flags;
		}

		public static ITableElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int fieldCount = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetShort();
			int indexCount = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);
			int beforeNameOffset = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(int)).Order(isLittleEndian).GetInt();
			string beforeTableName = beforeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + beforeNameOffset);

			IVariableElement[] fields = new VariableElementV11[fieldCount];
			uint currPos = currentPos + 24;
			for (int zz = 0; zz < fieldCount; zz++)
			{
				IVariableElement var = VariableElementV11.FromDebugSegment("", AccessType.NONE, segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)var.SizeInRCode;
				fields[zz] = var;
			}

			IIndexElement[] indexes = new IndexElementV11[indexCount];
			for (int zz = 0; zz < indexCount; zz++)
			{
				IIndexElement idx = IndexElementV11.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)idx.SizeInRCode;
				indexes[zz] = idx;
			}

			return new TableElementV11(name2, accessType, flags, fields, indexes, beforeTableName);
		}

		public virtual int Flags => flags;

		public IVariableElement[] GetFields() => fields;

		public IIndexElement[] GetIndexes() => indexes;

        public string BeforeTableName { get; }

		public override int SizeInRCode
		{
			get
			{
				int size = 24;
				foreach (IVariableElement e in fields)
				{
					size += e.SizeInRCode;
				}
				foreach (IIndexElement e in indexes)
				{
					size += e.SizeInRCode;
				}
				return size;
			}
		}

		public override string ToString() => string.Format("Table {0} - BeforeTable {1}", Name, BeforeTableName);

		public override int GetHashCode() => (Name + "/" + string.Join("-", Array.ConvertAll(fields, ConvertFieldToString)) + "/" + string.Join("-", Array.ConvertAll(indexes, ConvertIndexToString))).GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is ITableElement)
			{
				ITableElement obj2 = (ITableElement)obj;
				return Name.Equals(obj2.Name) && BeforeTableName.Equals(obj2.BeforeTableName) && Array.Equals(fields, obj2.GetFields()) && Array.Equals(indexes, obj2.GetIndexes());
			}
			return false;
		}

		private string ConvertFieldToString(IVariableElement obj) => obj?.ToString() ?? string.Empty;

		private string ConvertIndexToString(IIndexElement obj) => obj?.ToString() ?? string.Empty;
	}
}
