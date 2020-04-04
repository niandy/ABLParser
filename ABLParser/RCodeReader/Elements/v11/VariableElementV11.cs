namespace ABLParser.RCodeReader.Elements.v11
{
	public class VariableElementV11 : AbstractAccessibleElement, IVariableElement
	{
		private const int READ_ONLY = 1;
		private const int WRITE_ONLY = 2;
		private const int BASE_IS_DOTNET = 4;
		private const int NO_UNDO = 8;

		private readonly int dataType;
		private readonly int flags;
		private readonly string typeName;

		public VariableElementV11(string name, AccessType accessType, int dataType, int extent, int flags, string typeName) : base(name, accessType)
		{
			this.dataType = dataType;
			this.Extent = extent;
			this.flags = flags;
			this.typeName = typeName;
		}

		public static IVariableElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int dataType = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetShort();
			int extent = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 6, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int typeNameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			string typeName = typeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + typeNameOffset);

			return new VariableElementV11(name2, accessType, dataType, extent, flags, typeName);
		}

        public int Extent { get; }

		public DataType DataType => dataType.AsDataType();

		public virtual string TypeName => typeName;

		public virtual bool ReadOnly => (flags & READ_ONLY) != 0;

		public virtual bool WriteOnly => (flags & WRITE_ONLY) != 0;

		public virtual bool NoUndo => (flags & NO_UNDO) != 0;

		public virtual bool BaseIsDotNet => (flags & BASE_IS_DOTNET) != 0;

		public override int SizeInRCode => 24;

		public override string ToString() => string.Format("Variable {0} [{1:D}] - {2}", Name, Extent, DataType.ToString());

		public override int GetHashCode() => (Name + "/" + DataType + "/" + Extent).GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is IVariableElement obj2)
			{
				return Name.Equals(obj2.Name) && DataType.Equals(obj2.DataType) && (Extent == obj2.Extent);
			}
			return false;
		}
	}
}
