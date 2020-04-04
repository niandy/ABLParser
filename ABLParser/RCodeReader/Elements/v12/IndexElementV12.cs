using ABLParser.RCodeReader.Elements.v11;

namespace ABLParser.RCodeReader.Elements.v12
{
	public class IndexElementV12 : IndexElementV11
	{
		public IndexElementV12(string name, int primary, int flags, IIndexComponentElement[] indexComponents) : base(name, primary, flags, indexComponents)
		{			
		}

		protected new internal static IIndexElement FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int primary = segment[currentPos + 14];
			int flags = segment[currentPos + 15];

			int componentCount = ByteBuffer.Wrap(segment, currentPos + 12, sizeof(short)).Order(isLittleEndian).GetShort();
			int nameOffset = ByteBuffer.Wrap(segment, currentPos, sizeof(int)).Order(isLittleEndian).GetInt();
			string name = nameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			uint currPos = currentPos + 16;
			IIndexComponentElement[] indexComponents = new IndexComponentElementV12[componentCount];
			for (uint zz = 0; zz < componentCount; zz++)
			{
				IIndexComponentElement component = IndexComponentElementV12.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)component.SizeInRCode;
				indexComponents[zz] = component;
			}

			return new IndexElementV12(name, primary, flags, indexComponents);
		}
	}
}
