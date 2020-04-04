using ABLParser.RCodeReader.Elements.v11;

namespace ABLParser.RCodeReader.Elements.v12
{
    public class IndexComponentElementV12 : IndexComponentElementV11
    {
        public IndexComponentElementV12(int position, int flags, bool ascending) : base(position, flags, ascending)
        {
        }

        protected new internal static IIndexComponentElement FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
        {
            int ascending = segment[currentPos];
            int flags = segment[currentPos + 6];
            int position = ByteBuffer.Wrap(segment, currentPos + 7, sizeof(short)).Order(isLittleEndian).GetShort();

            return new IndexComponentElementV12(position, flags, ascending == 105);
        }
    }
}