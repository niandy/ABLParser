using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v11
{
    public class MethodElementV11 : AbstractAccessibleElement, IMethodElement
    {
        protected internal const int METHOD_DESCRIPTOR_SIZE = 24;
        protected internal const int FINAL_METHOD = 1;
        protected internal const int PROTECTED_METHOD = 2;
        protected internal const int PUBLIC_METHOD = 4;
        protected internal const int PRIVATE_METHOD = 8;
        protected internal const int PROCEDURE_METHOD = 16;
        protected internal const int FUNCTION_METHOD = 32;
        protected internal const int CONSTRUCTOR_METHOD = 64;
        protected internal const int DESTRUCTOR_METHOD = 128;
        protected internal const int OVERLOADED_METHOD = 256;
        protected internal const int STATIC_METHOD = 512;

        private readonly int flags;
        private readonly int returnType;
        private readonly string returnTypeName;
        private readonly int extent;
        private readonly IParameter[] parameters;

        public MethodElementV11(string name, AccessType accessType, int flags, int returnType, string returnTypeName, int extent, IParameter[] parameters) : base(name, accessType)
        {
            this.flags = flags;
            this.returnType = returnType;
            this.returnTypeName = returnTypeName;
            this.extent = extent;
            this.parameters = parameters;
        }

        public static IMethodElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
        {
            int flags = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();
            int returnType = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();
            int paramCount = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(short)).Order(isLittleEndian).GetShort();
            int extent = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(short)).Order(isLittleEndian).GetShort();

            int nameOffset = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(int)).Order(isLittleEndian).GetInt();
            string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

            int typeNameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
            string typeName = typeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + typeNameOffset);

            uint currPos = currentPos + 24;
            IParameter[] parameters = new IParameter[paramCount];
            for (uint zz = 0; zz < paramCount; zz++)
            {
                IParameter param = MethodParameterV11.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
                currPos += (uint)param.SizeInRCode;
                parameters[zz] = param;
            }

            return new MethodElementV11(name2, accessType, flags, returnType, typeName, extent, parameters);
        }

        public virtual string ReturnTypeName => returnTypeName;

        public virtual DataType ReturnType => returnType.AsDataType();

        public virtual IParameter[] GetParameters() => this.parameters;

        public override bool Static => (flags & STATIC_METHOD) != 0;

        public virtual bool Procedure => (flags & PROCEDURE_METHOD) != 0;

        public virtual bool Final => (flags & FINAL_METHOD) != 0;

        public bool Function => (flags & FUNCTION_METHOD) != 0;

        public bool Constructor => (flags & CONSTRUCTOR_METHOD) != 0;

        public bool Destructor => (flags & DESTRUCTOR_METHOD) != 0;

        public bool Overloaded => (flags & OVERLOADED_METHOD) != 0;

        public virtual int Extent
        {
            get
            {
                if (this.extent == 32769)
                {
                    return -1;
                }
                return this.extent;
            }
        }
        public override int SizeInRCode
        {
            get
            {
                int size = 24;
                foreach (IParameter p in parameters)
                {
                    size += p.SizeInRCode;
                }
                return size;
            }
        }

        public override string ToString()
        {
            return string.Format("Method {0}({1:D} arguments) returns {2}", Name, parameters.Length, ReturnType);
        }

        public override int GetHashCode()
        {
            return (Name + "/" + ReturnType + "/" + Extent + "/" + string.Join("/", Array.ConvertAll(parameters, ConvertParameterToString))).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is IMethodElement obj2)
            {
                return Name.Equals(obj2.Name) && ReturnType.Equals(obj2.ReturnType) && (extent == obj2.Extent) && Array.Equals(parameters, obj2.GetParameters());
            }
            return false;
        }

        private string ConvertParameterToString(IParameter obj) => obj?.ToString() ?? string.Empty;
    }
}
