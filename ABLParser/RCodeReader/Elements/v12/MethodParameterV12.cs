using ABLParser.RCodeReader.Elements.v11;
using System;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class MethodParameterV12 : MethodParameterV11
	{
		public MethodParameterV12(int num, string name, int type, int mode, int flags, int dataType, string dataTypeName, int extent) : base(num, name, type, mode, flags, dataType, dataTypeName, extent)
		{
		}

		protected new internal static IParameter FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int parameterType = ByteBuffer.Wrap(segment, currentPos + 10, sizeof(short)).Order(isLittleEndian).GetShort();
			int paramMode = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(short)).Order(isLittleEndian).GetShort();
			int extent = ByteBuffer.Wrap(segment, currentPos + 14, sizeof(short)).Order(isLittleEndian).GetShort();
			int dataType = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 18, sizeof(short)).Order(isLittleEndian).GetShort();
			int argumentNameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();

			string dataTypeName = argumentNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + argumentNameOffset);
			string name = nameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			return new MethodParameterV12(0, name, parameterType, paramMode, flags, dataType, dataTypeName, extent);
		}
	}	
}
