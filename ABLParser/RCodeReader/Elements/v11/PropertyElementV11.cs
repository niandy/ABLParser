using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class PropertyElementV11 : AbstractAccessibleElement, IPropertyElement
	{
		public const int PUBLIC_GETTER = 1;
		public const int PROTECTED_GETTER = 2;
		public const int PRIVATE_GETTER = 4;
		public const int PUBLIC_SETTER = 8;
		public const int PROTECTED_SETTER = 16;
		public const int PRIVATE_SETTER = 32;
		public const int HAS_GETTER = 256;
		public const int HAS_SETTER = 512;
		public const int PROPERTY_AS_VARIABLE = 1024;
		public const int PROPERTY_IS_INDEXED = 8192;
		public const int PROPERTY_IS_DEFAULT = 16384;
		public const int PROPERTY_IS_ENUM = 65536;

		private readonly int flags;
		private readonly IVariableElement variable;

		public PropertyElementV11(string name, AccessType accessType, int flags, IVariableElement var, IMethodElement getter, IMethodElement setter) : base(name, accessType)
		{
			this.flags = flags;
			this.variable = var;
			this.Getter = getter;
			this.Setter = setter;
		}

		public static IPropertyElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int flags = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			IVariableElement variable = null;
			uint currPos = currentPos + 8;
			if ((flags & PROPERTY_AS_VARIABLE) != 0)
			{
				variable = VariableElementV11.FromDebugSegment("", accessType, segment, currPos, textAreaOffset, isLittleEndian);
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
				getter = MethodElementV11.FromDebugSegment("", atp, segment, currPos, textAreaOffset, isLittleEndian);
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
				setter = MethodElementV11.FromDebugSegment("", atp, segment, currPos, textAreaOffset, isLittleEndian);
			}
			return new PropertyElementV11(name2, accessType, flags, variable, getter, setter);
		}

		public virtual IVariableElement Variable => this.variable;

		public IMethodElement Getter { get; }

        public IMethodElement Setter { get; }

        public override int SizeInRCode
		{
			get
			{
				int size = 8;
				if (this.PropertyAsVariable)
				{
					size += this.variable.SizeInRCode;
				}
				if (this.HasGetter)
				{
					size += this.Getter.SizeInRCode;
				}
				if (this.HasSetter)
				{
					size += this.Setter.SizeInRCode;
				}
				return size;
			}
		}

		public virtual bool PropertyAsVariable => (flags & PROPERTY_AS_VARIABLE) != 0;

		public virtual bool HasGetter => (flags & HAS_GETTER) != 0;

		public virtual bool HasSetter => (flags & HAS_SETTER) != 0;

		public virtual bool GetterPublic => (flags & PUBLIC_GETTER) != 0;

		public virtual bool GetterProtected => (flags & PROTECTED_GETTER) != 0;

		public virtual bool GetterPrivate => (flags & PRIVATE_GETTER) != 0;

		public virtual bool SetterPublic => (flags & PUBLIC_SETTER) != 0;

		public virtual bool SetterProtected => (flags & PROTECTED_SETTER) != 0;

		public virtual bool SetterPrivate => (flags & PRIVATE_SETTER) != 0;

		public virtual bool Indexed => (flags & PROPERTY_IS_INDEXED) != 0;

		public virtual bool Default => (flags & PROPERTY_IS_DEFAULT) != 0;

		public virtual bool CanRead => (GetterPrivate || GetterProtected || GetterPublic);

		public virtual bool CanWrite => (SetterPrivate || SetterProtected || SetterPublic);

		public override string ToString()
		{
			return string.Format("Property {0} AS {1}", Name, Variable.DataType);
		}

		public override int GetHashCode()
		{
			return (Name + "/" + variable.ToString()).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IPropertyElement obj2)
			{
				return Name.Equals(obj2.Name) && variable.Equals(obj2.Variable);
			}
			return false;
		}
	}
}
