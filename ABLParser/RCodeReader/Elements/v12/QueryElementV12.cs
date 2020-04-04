using ABLParser.RCodeReader.Elements.v11;
using System;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class QueryElementV12 : QueryElementV11
	{
		public QueryElementV12(string name, AccessType accessType, string[] buffers, int flags, int prvte) : base(name, accessType, buffers, flags, prvte)
		{		
		}

		public new static IQueryElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int bufferCount = ByteBuffer.Wrap(segment, currentPos + 14, sizeof(short)).Order(isLittleEndian).GetShort();
			int prvte = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			string[] bufferNames = new string[bufferCount];
			for (uint zz = 0; zz < bufferCount; zz++)
			{
				bufferNames[zz] = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currentPos + 24 + (zz * 4), sizeof(int)).Order(isLittleEndian).GetInt());
			}

			return new QueryElementV12(name2, accessType, bufferNames, flags, prvte);
		}		
	}
}
