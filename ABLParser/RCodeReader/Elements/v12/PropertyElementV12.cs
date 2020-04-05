using ABLParser.RCodeReader.Elements.v11;
using System;


namespace ABLParser.RCodeReader.Elements.v12
{
	public class PropertyElementV12 : PropertyElementV11
	{
		private readonly IEnumDescriptor enumDesc;

		public PropertyElementV12(string name, AccessType accessType, int flags, IVariableElement var, IMethodElement getter, IMethodElement setter, IEnumDescriptor enumDesc) : base(name, accessType, flags, var, getter, setter)
		{
			this.enumDesc = enumDesc;
		}

		public static IPropertyElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian, bool isEnum)
		{
			int flags = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			IVariableElement variable = null;
			uint currPos = currentPos + 16;
			if ((flags & PROPERTY_AS_VARIABLE) != 0)
			{
				variable = VariableElementV12.FromDebugSegment("", accessType, segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)variable.SizeInRCode;
			}

			IMethodElement getter = null;
			if ((flags & HAS_GETTER) != 0)
			{
				AccessType atp = AccessType.NONE;
				if ((flags & PUBLIC_GETTER) != 0)
				{
					atp |= AccessType.PUBLIC;
				}
				if ((flags & PROTECTED_GETTER) != 0)
				{
					atp |= AccessType.PROTECTED;
				}
				getter = MethodElementV12.FromDebugSegment("", atp, segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)getter.SizeInRCode;
			}
			IMethodElement setter = null;
			if ((flags & HAS_SETTER) != 0)
			{
				AccessType atp = AccessType.NONE;
				if ((flags & PUBLIC_SETTER) != 0)
				{
					atp |= AccessType.PUBLIC;
				}
				if ((flags & PROTECTED_SETTER) != 0)
				{
					atp |= AccessType.PROTECTED;
				}
				setter = MethodElementV12.FromDebugSegment("", atp, segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)setter.SizeInRCode;
			}
			IEnumDescriptor enumDesc = null;
			if (isEnum || ((flags & PROPERTY_IS_ENUM) != 0))
			{
				enumDesc = EnumDescriptorV12.FromDebugSegment("", segment, currPos, textAreaOffset, isLittleEndian);
			}			
			return new PropertyElementV12(name2, accessType, flags, variable, getter, setter, enumDesc);
		}
		

		public override int SizeInRCode
		{
			get
			{
				int size = 16;
				if (this.PropertyAsVariable)
				{
					size += this.Variable.SizeInRCode;
				}
				if (this.HasGetter)
				{
					size += this.Getter.SizeInRCode;
				}
				if (this.HasSetter)
				{
					size += this.Setter.SizeInRCode;
				}
				if (enumDesc != null)
				{
					size += enumDesc.SizeInRCode;
				}
				return size;
			}
		}		
	}
}
