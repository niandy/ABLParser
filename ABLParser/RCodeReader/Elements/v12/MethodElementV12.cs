using ABLParser.RCodeReader.Elements.v11;
using System;

namespace ABLParser.RCodeReader.Elements.v12
{
    public class MethodElementV12 : MethodElementV11
    {
        public MethodElementV12(string name, AccessType accessType, int flags, int returnType, string returnTypeName, int extent, IParameter[] parameters) : base(name, accessType, flags, returnType, returnTypeName, extent, parameters)
        {            
        }

        public new static IMethodElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
        {
            int flags = ByteBuffer.Wrap(segment, currentPos + 14, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();
            int returnType = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(short)).Order(isLittleEndian).GetShort();
            int paramCount = ByteBuffer.Wrap(segment, currentPos + 18, sizeof(short)).Order(isLittleEndian).GetShort();
            int extent = ByteBuffer.Wrap(segment, currentPos + 22, sizeof(short)).Order(isLittleEndian).GetShort();

            int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
            string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

            int typeNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
            string typeName = typeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + typeNameOffset);

            uint currPos = currentPos + 56;
            IParameter[] parameters = new IParameter[paramCount];
            for (uint zz = 0; zz < paramCount; zz++)
            {
                IParameter param = MethodParameterV12.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
                currPos += (uint)param.SizeInRCode;
                parameters[zz] = param;
            }

            return new MethodElementV12(name2, accessType, flags, returnType, typeName, extent, parameters);
        }

        
        public override int SizeInRCode
        {
            get
            {
                int size = 56;
                foreach (IParameter p in GetParameters())
                {
                    size += p.SizeInRCode;
                }
                return size;
            }
        }
       
    }
}
