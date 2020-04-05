using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class BufferElementV11 : AbstractAccessibleElement, IBufferElement
	{
		private const int TEMP_TABLE = 4;
		private readonly int flags;

		public BufferElementV11(string name, AccessType accessType, string tableName, string dbName, int flags) : base(name, accessType)
		{
			this.TableName = tableName;
			this.DatabaseName = dbName;
			this.flags = flags;
		}

		public static IBufferElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{			
			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int tableNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string tableName = tableNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + tableNameOffset);

			int databaseNameOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittleEndian).GetInt();
			string databaseName = databaseNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + databaseNameOffset);

			int flags = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(short)).Order(isLittleEndian).GetUnsignedShort();

			return new BufferElementV11(name2, accessType, tableName, databaseName, flags);
		}

        public string TableName { get; }

        public string DatabaseName { get; }

        public bool TempTableBuffer => (flags & TEMP_TABLE) != 0;

		public override int SizeInRCode => 24;

		public override string ToString() => $"Buffer {Name} for {DatabaseName}.{TableName}";

		public override int GetHashCode() => (Name + "/" + DatabaseName + "/" + TableName).GetHashCode();		

		public override bool Equals(object obj)
		{
			if (obj is IBufferElement obj2)
			{
				return Name.Equals(obj2.Name, StringComparison.InvariantCultureIgnoreCase)
					&& DatabaseName.Equals(obj2.DatabaseName, StringComparison.InvariantCultureIgnoreCase)
					&& TableName.Equals(obj2.TableName, StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}

	}
}
