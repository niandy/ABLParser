using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class BufferElementV11 : AbstractAccessibleElement, IBufferElement
	{
		private const int TEMP_TABLE = 4;

		private readonly string tableName;
		private readonly string databaseName;
		private readonly int flags;

		public BufferElementV11(string name, AccessType accessType, string tableName, string dbName, int flags) : base(name, accessType)
		{
			this.tableName = tableName;
			this.databaseName = dbName;
			this.flags = flags;
		}

		public static IBufferElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			if (isLittleEndian != BitConverter.IsLittleEndian)
				Array.Reverse(segment);
			int nameOffset = BitConverter.ToInt32(segment, (int)currentPos);
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int tableNameOffset = BitConverter.ToInt32(segment, (int)(currentPos + 4));
			string tableName = tableNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + tableNameOffset);

			int databaseNameOffset = BitConverter.ToInt32(segment, (int)(currentPos + 8));
			string databaseName = databaseNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + databaseNameOffset);

			int flags = BitConverter.ToInt16(segment, (int)(currentPos + 12)) & 0xffff;

			return new BufferElementV11(name2, accessType, tableName, databaseName, flags);
		}

		public string TableName
		{
			get
			{
				return tableName;
			}
		}

		public string DatabaseName
		{
			get
			{
				return databaseName;
			}
		}

		public bool TempTableBuffer
		{
			get
			{
				return (flags & TEMP_TABLE) != 0;
			}
		}

		public override int SizeInRCode
		{
			get
			{
				return 24;
			}
		}

		public override string ToString()
		{
			return string.Format("Buffer {0} for {1}.{2}", Name, databaseName, tableName);
		}

		public override int GetHashCode()
		{
			return (Name + "/" + databaseName + "/" + tableName).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IBufferElement obj2)
			{
				return Name.Equals(obj2.Name, StringComparison.InvariantCultureIgnoreCase) && databaseName.Equals(obj2.DatabaseName, StringComparison.InvariantCultureIgnoreCase) && tableName.Equals(obj2.TableName, StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}

	}
}
