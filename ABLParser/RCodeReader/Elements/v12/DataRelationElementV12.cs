using ABLParser.RCodeReader.Elements.v11;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class DataRelationElementV12 : DataRelationElementV11
	{
		public DataRelationElementV12(string name, string parentBuffer, string childBuffer, string fieldPairs, int flags) : base(name, parentBuffer, childBuffer, fieldPairs, flags)
		{			
		}

		public new static DataRelationElementV12 FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittelEndian)
		{
			int flags = ByteBuffer.Wrap(segment, currentPos + 22, sizeof(short)).Order(isLittelEndian).GetShort() & 0xffff;

			int parentBufferNameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittelEndian).GetInt();
			string parentBufferName = parentBufferNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + parentBufferNameOffset);

			int childBufferNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittelEndian).GetInt();
			string childBufferName = childBufferNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + childBufferNameOffset);

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittelEndian).GetInt();
			string name = nameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int fieldPairsOffset = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(int)).Order(isLittelEndian).GetInt();
			string fieldPairs = fieldPairsOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + fieldPairsOffset);

			return new DataRelationElementV12(name, parentBufferName, childBufferName, fieldPairs, flags);
		}
	}
}
