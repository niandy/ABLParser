using System;


namespace ABLParser.RCodeReader.Elements.v11
{
	public class EventElementV11 : AbstractAccessibleElement, IEventElement
	{
		private readonly int flags;
		private readonly int returnType;
		private readonly IParameter[] parameters;

		public EventElementV11(string name, AccessType accessType, int flags, int returnType, string returnTypeName, string delegateName, IParameter[] parameters) : base(name, accessType)
		{
			this.flags = flags;
			this.returnType = returnType;
			this.ReturnTypeName = returnTypeName;
			this.DelegateName = delegateName;
			this.parameters = parameters;
		}

		public static IEventElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int flags = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();
			int returnType = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();
			int parameterCount = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int typeNameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			string returnTypeName = typeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + typeNameOffset);

			int delegateNameOffset = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(int)).Order(isLittleEndian).GetInt();
			string delegateName = delegateNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + delegateNameOffset);

			uint currPos = currentPos + 24;
			IParameter[] parameters = new IParameter[parameterCount];
			for (int zz = 0; zz < parameterCount; zz++)
			{
				IParameter param = MethodParameterV11.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)param.SizeInRCode;
				parameters[zz] = param;
			}

			return new EventElementV11(name2, accessType, flags, returnType, returnTypeName, delegateName, parameters);
		}

		public DataType ReturnType => returnType.AsDataType();

        public string ReturnTypeName { get; }

        public string DelegateName { get; }

		public IParameter[] GetParameters() => this.parameters;

		public virtual int Flags => flags;

		public override int SizeInRCode
		{
			get
			{
				int size = 24;
				foreach (IParameter p in parameters)
				{
					size += ((MethodParameterV11)p).SizeInRCode;
				}

				return size;
			}
		}

		public override string ToString()
		{
			return string.Format("Event {0}", Name);
		}

		public override int GetHashCode() => (Name + "/" + DelegateName + "/" + returnType + "/" + string.Join("-", Array.ConvertAll(parameters, ConvertParameterToString))).GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is IEventElement obj2)
			{
				return Name.Equals(obj2.Name) && DelegateName.Equals(obj2.DelegateName) && (returnType.AsDataType() == obj2.ReturnType) && Array.Equals(parameters, obj2.GetParameters());
			}
			return false;
		}

		private string ConvertParameterToString(IParameter obj) => obj?.ToString() ?? string.Empty;
	}
}
