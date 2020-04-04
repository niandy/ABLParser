
namespace ABLParser.RCodeReader.Elements.v11
{
    public class IndexComponentElementV11 : AbstractElement, IIndexComponentElement
    {
        private readonly int flags;
        private readonly int position;
        private readonly bool ascending;

        public IndexComponentElementV11(int position, int flags, bool ascending)
        {
            this.position = position;
            this.flags = flags;
            this.ascending = ascending;
        }

        protected internal static IIndexComponentElement FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
        {
            int ascending = segment[currentPos];
            int flags = segment[currentPos + 1];
            int position = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();

            return new IndexComponentElementV11(position, flags, ascending == 106);
        }

        public virtual int Flags => flags;

        public virtual int FieldPosition => this.position;

        public virtual bool Ascending => this.ascending;

        public override int SizeInRCode => 8;

        public override string ToString()
        {
            return string.Format("Field #{0:D}", position);
        }

        public override int GetHashCode()
        {
            return position * 7 + (ascending ? 3 : 1);
        }

        public override bool Equals(object obj)
        {
            if (obj is IIndexComponentElement obj2)
            {
                return (position == obj2.FieldPosition) && (ascending == obj2.Ascending);
            }
            return false;
        }
    }
}
