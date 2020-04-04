using System;
using System.Collections.Generic;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class QueryElementV11 : AbstractAccessibleElement, IQueryElement
	{
		private readonly string[] bufferNames;
		private readonly int prvte;
		private readonly int flags;

		public QueryElementV11(string name, AccessType accessType, string[] buffers, int flags, int prvte) : base(name, accessType)
		{
			this.bufferNames = buffers;
			this.flags = flags;
			this.prvte = prvte;
		}

		public static IQueryElement FromDebugSegment(string name, AccessType accessType, byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int bufferCount = ByteBuffer.Wrap(segment, currentPos, sizeof(short)).Order(isLittleEndian).GetShort();
			int prvte = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();
			int flags = ByteBuffer.Wrap(segment, currentPos + 6, sizeof(short)).Order(isLittleEndian).GetShort();

			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 16, sizeof(int)).Order(isLittleEndian).GetInt();
			string name2 = nameOffset == 0 ? name : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			string[] bufferNames = new string[bufferCount];
			for (uint zz = 0; zz < bufferCount; zz++)
			{
				bufferNames[zz] = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currentPos + 24 + (zz * 4), sizeof(int)).Order(isLittleEndian).GetInt());
			}

			return new QueryElementV11(name2, accessType, bufferNames, flags, prvte);
		}

		public string[] GetBufferNames() => bufferNames;

		public virtual int Prvte => prvte;

		public virtual int Flags => flags;

		public override int SizeInRCode => (24 + 4 * bufferNames.Length) + 7 & -8;

		public override string ToString() => string.Format("Query {0} for {1:D} buffer(s)", Name, bufferNames.Length);

		public override int GetHashCode() => (Name + "/" + string.Join("-", bufferNames)).GetHashCode();

		public override bool Equals(object obj)
		{
			if (obj is IQueryElement obj2)
			{
				return Name.Equals(obj2.Name) && Array.Equals(bufferNames, obj2.GetBufferNames());
			}
			return false;
		}
	}
}
