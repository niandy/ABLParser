using ABLParser.RCodeReader.Elements.v11;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class VariableElementV12 : VariableElementV11
	{
		public VariableElementV12(string name, AccessType accessType, int dataType, int extent, int flags, string typeName) : base(name, accessType,dataType, extent, flags, typeName)
		{			
		}

		public new static IVariableElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int dataType = ByteBuffer.Wrap(segment, currentPos + 14, sizeof(short)).Order(isLittleEndian).GetShort();
			int extent = ByteBuffer.Wrap(segment, currentPos + 18, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int typeNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string typeName = typeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + typeNameOffset);

			return new VariableElementV12(name2, accessType, dataType, extent, flags, typeName);
		}
	}
}
