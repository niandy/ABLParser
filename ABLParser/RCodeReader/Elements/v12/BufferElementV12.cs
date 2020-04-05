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
			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int tableNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string tableName = tableNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + tableNameOffset);

			int databaseNameOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittleEndian).GetInt();
			string databaseName = databaseNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + databaseNameOffset);

			int flags = ByteBuffer.Wrap(segment, currentPos + 18, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();

			return new BufferElementV12(name2, accessType, tableName, databaseName, flags);
		}	

	}
}
