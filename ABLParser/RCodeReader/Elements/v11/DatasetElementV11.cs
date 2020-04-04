using System;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class DatasetElementV11 : AbstractAccessibleElement, IDatasetElement
	{
		private readonly string[] bufferNames;
		private readonly IDataRelationElement[] relations;

		public DatasetElementV11(string name, AccessType accessType, string[] bufferNames, IDataRelationElement[] relations) : base(name, accessType)
		{
			this.bufferNames = bufferNames;
			this.relations = relations;
		}

		public static IDatasetElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int bufferCount = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetShort();
			int relationshipCount = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			string[] bufferNames = new string[bufferCount];
			for (uint zz = 0; zz < bufferCount; zz++)
			{
				bufferNames[zz] = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currentPos + 24 + (zz * 4), sizeof(int)).Order(isLittleEndian).GetInt());
			}

			// Round to next byte
			uint currPos = currentPos + 24 + (uint)((bufferCount * 4 + 7) & -8);
			IDataRelationElement[] relations = new DataRelationElementV11[relationshipCount];
			for (int zz = 0; zz < relationshipCount; zz++)
			{
				IDataRelationElement param = DataRelationElementV11.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)param.SizeInRCode;
				relations[zz] = param;
			}

			return new DatasetElementV11(name2, accessType, bufferNames, relations);
		}

		public virtual IDataRelationElement[] GetDataRelations() => this.relations;

		public virtual string[] GetBufferNames() => bufferNames;

		public override int SizeInRCode
		{
			get
			{
				int size = 24 + (bufferNames.Length * 4 + 7 & -8);
				foreach (IDataRelationElement elem in relations)
				{
					size += elem.SizeInRCode;
				}
				return size;
			}
		}

		public override string ToString()
		{
			return string.Format("Dataset {0} for {1:D} buffer(s) and {2:D} relations", Name, bufferNames.Length, relations.Length);
		}

		public override int GetHashCode()
		{
			string str1 = string.Join("/", bufferNames);
			string str2 = string.Join("/", Array.ConvertAll<IDataRelationElement, string>(relations, ConvertDataRelationToString));
			return (str1 + "-" + str2).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IDatasetElement obj2)
			{
				return (Array.Equals(bufferNames, obj2.GetBufferNames()) && Array.Equals(relations, obj2.GetDataRelations()));
			}
			return false;
		}
		private string ConvertDataRelationToString(IDataRelationElement obj)
		{
			return obj?.ToString() ?? string.Empty;
		}
	}
}
