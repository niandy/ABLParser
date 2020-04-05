using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class DataRelationElementV11 : AbstractElement, IDataRelationElement
	{
		private readonly int flags;

		public DataRelationElementV11(string name, string parentBuffer, string childBuffer, string fieldPairs, int flags) : base(name)
		{
			this.ParentBufferName = parentBuffer;
			this.ChildBufferName = childBuffer;
			this.FieldPairs = fieldPairs;
			this.flags = flags;
		}

		public static DataRelationElementV11 FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittelEndian)
		{
			int flags = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittelEndian).GetUnsignedShort();

			int parentBufferNameOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittelEndian).GetInt();
			string parentBufferName = parentBufferNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + parentBufferNameOffset);

			int childBufferNameOffset = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(int)).Order(isLittelEndian).GetInt();
			string childBufferName = childBufferNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + childBufferNameOffset);

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittelEndian).GetInt();
			string name = nameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			int fieldPairsOffset = ByteBuffer.Wrap(segment, currentPos + 20, sizeof(int)).Order(isLittelEndian).GetInt();
			string fieldPairs = fieldPairsOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + fieldPairsOffset);

			return new DataRelationElementV11(name, parentBufferName, childBufferName, fieldPairs, flags);
		}

        public string ParentBufferName { get; }

        public string ChildBufferName { get; }

        public string FieldPairs { get; }

        public virtual int Flags => flags;

		public override int SizeInRCode => 24;

		public override string ToString()
		{
			return string.Format("Data relation from {0} to {1}", ParentBufferName, ChildBufferName);
		}

		public override int GetHashCode()
		{
			return (ParentBufferName + "/" + ChildBufferName + "/" + FieldPairs).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IDataRelationElement obj2)
			{
				return (ParentBufferName.Equals(obj2.ParentBufferName) && ChildBufferName.Equals(obj2.ChildBufferName) && FieldPairs.Equals(obj2.FieldPairs));
			}
			return false;
		}
	}
}
