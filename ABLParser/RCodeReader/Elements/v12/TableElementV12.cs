using ABLParser.RCodeReader.Elements.v11;
using System;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class TableElementV12 : TableElementV11
	{		
		public TableElementV12(string name, AccessType accessType, int flags, IVariableElement[] fields, IIndexElement[] indexes, string beforeTableName) : base(name, accessType, flags, fields, indexes, beforeTableName)
		{			
		}

		public new static ITableElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int fieldCount = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(short)).Order(isLittleEndian).GetShort();
			int indexCount = ByteBuffer.Wrap(segment, currentPos + 14, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);
			int beforeNameOffset = ByteBuffer.Wrap(segment, currentPos + 4, sizeof(int)).Order(isLittleEndian).GetInt();
			string beforeTableName = beforeNameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + beforeNameOffset);

			IVariableElement[] fields = new VariableElementV12[fieldCount];
			uint currPos = currentPos + 24;
			for (int zz = 0; zz < fieldCount; zz++)
			{
				IVariableElement var = VariableElementV12.FromDebugSegment("", AccessType.NONE, segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)var.SizeInRCode;
				fields[zz] = var;
			}

			IIndexElement[] indexes = new IndexElementV12[indexCount];
			for (int zz = 0; zz < indexCount; zz++)
			{
				IIndexElement idx = IndexElementV12.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)idx.SizeInRCode;
				indexes[zz] = idx;
			}

			return new TableElementV12(name2, accessType, flags, fields, indexes, beforeTableName);
		}
	}
}
