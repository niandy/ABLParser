using System;

namespace ABLParser.RCodeReader.Elements.v11
{	
	public class DataSourceElementV11 : AbstractAccessibleElement, IDataSourceElement
	{
		private readonly string[] bufferNames;

		public DataSourceElementV11(string name, AccessType accessType, string queryName, string keyComponentNames, string[] bufferNames) : base(name, accessType)
		{
			this.QueryName = queryName;
			this.KeyComponents = keyComponentNames;
			this.bufferNames = bufferNames;
		}

		public static IDataSourceElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int bufferCount = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int queryNameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			string queryName = queryNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + queryNameOffset);

			int keyComponentNamesOffset = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(int)).Order(isLittleEndian).GetInt();
			string keyComponentNames = keyComponentNamesOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + keyComponentNamesOffset);

			string[] bufferNames = new string[bufferCount];
			for (uint zz = 0; zz < bufferCount; zz++)
			{
				bufferNames[zz] = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currentPos + 24 + (zz * 4), sizeof(int)).Order(isLittleEndian).GetInt());
			}

			return new DataSourceElementV11(name2, accessType, queryName, keyComponentNames, bufferNames);
		}

        public string QueryName { get; }

        public string KeyComponents { get; }

		public string[] GetBufferNames() => bufferNames;

		public override int SizeInRCode
		{
			get
			{
				int size = 24 + (this.bufferNames.Length * 4);
				return size + 7 & -8;
			}
		}

		public override string ToString()
		{
			return string.Format("Datasource {0} for {1:D} buffer(s)", QueryName, bufferNames.Length);
		}

		public override int GetHashCode()
		{
			return (QueryName + "-" + KeyComponents + "-" + string.Join(".", bufferNames)).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IDataSourceElement obj2)
			{
				return QueryName.Equals(obj2.QueryName) && KeyComponents.Equals(obj2.KeyComponents) && Array.Equals(bufferNames, obj2.GetBufferNames());
			}
			return false;
		}
	}
}
