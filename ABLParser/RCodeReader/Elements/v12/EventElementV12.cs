using ABLParser.RCodeReader.Elements.v11;
using System;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class EventElementV12 : EventElementV11
	{
		public EventElementV12(string name, AccessType accessType, int flags, int returnType, string returnTypeName, string delegateName, IParameter[] parameters) : base(name, accessType, flags, returnType, returnTypeName, delegateName, parameters)
		{		
		}

		public new static IEventElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int flags = ByteBuffer.Wrap(segment, currentPos + 18, sizeof(short)).Order(isLittleEndian).GetShort() & 0xffff;
			int returnType = ByteBuffer.Wrap(segment, currentPos + 22, sizeof(short)).Order(isLittleEndian).GetShort();
			int parameterCount = ByteBuffer.Wrap(segment, currentPos + 22, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int typeNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string returnTypeName = typeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + typeNameOffset);

			int delegateNameOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittleEndian).GetInt();
			string delegateName = delegateNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + delegateNameOffset);

			uint currPos = currentPos + 24;
			IParameter[] parameters = new IParameter[parameterCount];
			for (int zz = 0; zz < parameterCount; zz++)
			{
				IParameter param = MethodParameterV12.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)param.SizeInRCode;
				parameters[zz] = param;
			}

			return new EventElementV12(name2, accessType, flags, returnType, returnTypeName, delegateName, parameters);
		}
	}
}
