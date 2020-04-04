using ABLParser.RCodeReader.Elements.v11;
using System;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class DatasetElementV12 : DatasetElementV11
	{
		public DatasetElementV12(string name, AccessType accessType, string[] bufferNames, IDataRelationElement[] relations) : base(name, accessType, bufferNames, relations)
		{			
		}

		public new static IDatasetElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int bufferCount = ByteBuffer.Wrap(segment, currentPos + 14, sizeof(short)).Order(isLittleEndian).GetShort();
			int relationshipCount = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			string[] bufferNames = new string[bufferCount];
			for (uint zz = 0; zz < bufferCount; zz++)
			{
				bufferNames[zz] = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currentPos + 24 + (zz * 4), sizeof(int)).Order(isLittleEndian).GetInt());
			}

			// Round to next byte
			uint currPos = currentPos + 24 + (uint)((bufferCount * 4 + 7) & -8);
			IDataRelationElement[] relations = new DataRelationElementV12[relationshipCount];
			for (int zz = 0; zz < relationshipCount; zz++)
			{
				IDataRelationElement param = DataRelationElementV12.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)param.SizeInRCode;
				relations[zz] = param;
			}

			return new DatasetElementV12(name2, accessType, bufferNames, relations);
		}
	}
}
