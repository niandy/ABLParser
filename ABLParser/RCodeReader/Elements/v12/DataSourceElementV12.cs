using ABLParser.RCodeReader.Elements.v11;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class DataSourceElementV12 : DataSourceElementV11
	{
		public DataSourceElementV12(string name, AccessType accessType, string queryName, string keyComponentNames, string[] bufferNames) : base(name, accessType, queryName, keyComponentNames, bufferNames)
		{			
		}

		public new static IDataSourceElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int bufferCount = ByteBuffer.Wrap(segment, currentPos + 18, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int queryNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string queryName = queryNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + queryNameOffset);

			int keyComponentNamesOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittleEndian).GetInt();
			string keyComponentNames = keyComponentNamesOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + keyComponentNamesOffset);

			string[] bufferNames = new string[bufferCount];
			for (uint zz = 0; zz < bufferCount; zz++)
			{
				bufferNames[zz] = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currentPos + 24 + (zz * 4), sizeof(int)).Order(isLittleEndian).GetInt());
			}

			return new DataSourceElementV12(name2, accessType, queryName, keyComponentNames, bufferNames);
		}
	}
}
