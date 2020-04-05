using System;
using System.Collections.Generic;
using System.Text;
using DataTypeEnum = ABLParser.RCodeReader.Elements.DataType;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class MethodParameterV11 : AbstractElement, IParameter
	{
		private const int PARAMETER_APPEND = 1;
		private const int PARAMETER_HANDLE = 2;
		private const int PARAMETER_BIND = 4;

		public const int PARAMETER_INPUT = 6028;
		public const int PARAMETER_INOUT = 6110;
		public const int PARAMETER_OUTPUT = 6049;
		public const int PARAMETER_BUFFER = 1070;

		private readonly int paramNum;
		private readonly int flags;
		private readonly int parameterType;
		private readonly int paramMode;
		private readonly int dataType;
		private readonly string dataTypeName;

		public MethodParameterV11(int num, string name, int type, int mode, int flags, int dataType, string dataTypeName, int extent) : base(name)
		{
			this.paramNum = num;
			this.parameterType = type;
			this.paramMode = mode;
			this.dataType = dataType;
			this.dataTypeName = dataTypeName;
			this.flags = flags;
			this.Extent = extent;
		}

		protected internal static IParameter FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int parameterType = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetShort();
			int paramMode = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();
			int extent = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(short)).Order(isLittleEndian).GetShort();
			int dataType = ByteBuffer.Wrap(segment, currentPos + 6, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 10, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();
			int argumentNameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(int)).Order(isLittleEndian).GetInt();

			string dataTypeName = argumentNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + argumentNameOffset);
			string name = nameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			return new MethodParameterV11(0, name, parameterType, paramMode, flags, dataType, dataTypeName, extent);
		}

        public int Extent { get; }

		public virtual DataTypeEnum ABLDataType => dataType.AsDataType();

		public string DataType
		{
			get
			{
				if (dataType == DataTypeEnum.CLASS.GetNum())
				{
					return dataTypeName;
				}
				return ABLDataType.ToString();
			}
		}

		public virtual string ArgumentName => dataTypeName;

		public ParameterType ParameterType => ParameterTypeExt.GetParameterType(this.parameterType);

		public ParameterMode Mode => ParameterModeExt.GetParameterMode(paramMode);

		public bool ClassDataType => dataType == DataTypeEnum.CLASS.GetNum();

		public virtual bool Bind => (flags & PARAMETER_BIND) != 0;

		public virtual bool Append => (flags & PARAMETER_APPEND) != 0;

		public virtual bool Handle => (flags & PARAMETER_HANDLE) != 0;

		public override int SizeInRCode => 24;

		public override string ToString()
		{
			return Mode + " " + ParameterType + " " + Name + " AS " + DataType;
		}

		public override int GetHashCode()
		{
			return (Mode + "/" + ParameterType + "/" + Name + "/" + DataType).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IParameter obj2)
			{
				return Name.Equals(obj2.Name) && Mode.Equals(obj2.Mode) && ParameterType.Equals(obj2.ParameterType) && DataType.Equals(obj2.DataType);
			}
			return false;
		}
	}
}
