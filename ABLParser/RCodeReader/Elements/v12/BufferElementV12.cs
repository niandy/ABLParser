using ABLParser.RCodeReader.Elements.v11;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class BufferElementV12 : BufferElementV11
	{
		public BufferElementV12(string name, AccessType accessType, string tableName, string dbName, int flags) : base(name, accessType, tableName, dbName, flags) { }		

		public new static IBufferElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			if (isLittleEndian != BitConverter.IsLittleEndian)
				Array.Reverse(segment);
			int nameOffset = BitConverter.ToInt32(segment, (int)currentPos);
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int tableNameOffset = BitConverter.ToInt32(segment, (int)(currentPos + 4));
			string tableName = tableNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + tableNameOffset);

			int databaseNameOffset = BitConverter.ToInt32(segment, (int)(currentPos + 8));
			string databaseName = databaseNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + databaseNameOffset);

			int flags = BitConverter.ToInt16(segment, (int)(currentPos + 18)) & 0xffff;

			return new BufferElementV12(name2, accessType, tableName, databaseName, flags);
		}	

	}
}
