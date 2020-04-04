using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
	[Flags]
	public enum AccessType
	{
		NONE,
		PUBLIC,
		PRIVATE,
		PROTECTED,
		STATIC,
		ABSTRACT,
		FINAL,
		CONSTRUCTOR
	}

	public class AccessTypeExt
	{
		public static AccessType GetTypeFromString(int val)
		{
			AccessType set = AccessType.NONE;
			switch (val & 0x07)
			{
				case 1:
					set |= AccessType.PUBLIC;					
					break;
				case 2:
					set |= AccessType.PROTECTED;
					break;
				case 4:
					set |= AccessType.PRIVATE;
					break;
				default:
					break;
			}
			if ((val & 0x08) != 0)
			{
				set |= AccessType.CONSTRUCTOR;
			}
			if ((val & 0x10) != 0)
			{
				set |= AccessType.FINAL;
			}
			if ((val & 0x20) != 0)
			{
				set |= AccessType.STATIC;
			}
			if ((val & 0x40) != 0)
			{
				set |= AccessType.ABSTRACT;
			}

			return set;
		}

		public static AccessType ValueOf(string name)
		{
			if (Enum.TryParse(name, true, out AccessType t))
				return t;
			else
				return AccessType.NONE;
		}

	}
	/*
	public sealed class AccessType
	{
		public static readonly AccessType PUBLIC = new AccessType("PUBLIC", InnerEnum.PUBLIC);
		public static readonly AccessType PRIVATE = new AccessType("PRIVATE", InnerEnum.PRIVATE);
		public static readonly AccessType PROTECTED = new AccessType("PROTECTED", InnerEnum.PROTECTED);
		public static readonly AccessType STATIC = new AccessType("STATIC", InnerEnum.STATIC);
		public static readonly AccessType ABSTRACT = new AccessType("ABSTRACT", InnerEnum.ABSTRACT);
		public static readonly AccessType FINAL = new AccessType("FINAL", InnerEnum.FINAL);
		public static readonly AccessType CONSTRUCTOR = new AccessType("CONSTRUCTOR", InnerEnum.CONSTRUCTOR);

		private static readonly IList<AccessType> valueList = new List<AccessType>();

		static AccessType()
		{
			valueList.Add(PUBLIC);
			valueList.Add(PRIVATE);
			valueList.Add(PROTECTED);
			valueList.Add(STATIC);
			valueList.Add(ABSTRACT);
			valueList.Add(FINAL);
			valueList.Add(CONSTRUCTOR);
		}

		public enum InnerEnum
		{
			PUBLIC,
			PRIVATE,
			PROTECTED,
			STATIC,
			ABSTRACT,
			FINAL,
			CONSTRUCTOR
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private AccessType(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static ISet<AccessType> GetTypeFromString(int val)
		{
			ISet<AccessType> set = new HashSet<AccessType>();
			switch (val & 0x07)
			{
				case 1:
					set.Add(PUBLIC);
					break;
				case 2:
					set.Add(PROTECTED);
					break;
				case 4:
					set.Add(PRIVATE);
					break;
				default:
					break;
			}
			if ((val & 0x08) != 0)
			{
				set.Add(CONSTRUCTOR);
			}
			if ((val & 0x10) != 0)
			{
				set.Add(FINAL);
			}
			if ((val & 0x20) != 0)
			{
				set.Add(STATIC);
			}
			if ((val & 0x40) != 0)
			{
				set.Add(ABSTRACT);
			}

			return set;
		}

		public static IList<AccessType> Values()
		{
			return valueList;
		}

		public int Ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static AccessType ValueOf(string name)
		{
			foreach (AccessType enumInstance in AccessType.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}
	*/
}
