using System;

namespace ABLParser.RCodeReader.Elements.v11
{
	public class IndexElementV11 : AbstractElement, IIndexElement
	{
		private const int UNIQUE_INDEX = 2;
		private const int WORD_INDEX = 8;
		private const int DEFAULT_INDEX = 16;

		private readonly IIndexComponentElement[] indexComponents;
		private readonly int primary;
		private readonly int flags;

		public IndexElementV11(string name, int primary, int flags, IIndexComponentElement[] indexComponents) : base(name)
		{
			this.primary = primary;
			this.flags = flags;
			this.indexComponents = indexComponents;
		}

		protected internal static IIndexElement FromDebugSegment(byte[] segment, uint currentPos, int textAreaOffset, bool isLittleEndian)
		{
			int primary = segment[currentPos];
			int flags = segment[currentPos + 1];

			int componentCount = ByteBuffer.Wrap(segment, currentPos + 2, sizeof(short)).Order(isLittleEndian).GetShort();
			int nameOffset = ByteBuffer.Wrap(segment, currentPos + 8, sizeof(int)).Order(isLittleEndian).GetInt();
			string name = nameOffset == 0 ? "" : RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);

			uint currPos = currentPos + 16;
			IIndexComponentElement[] indexComponents = new IndexComponentElementV11[componentCount];
			for (uint zz = 0; zz < componentCount; zz++)
			{
				IIndexComponentElement component = IndexComponentElementV11.FromDebugSegment(segment, currPos, textAreaOffset, isLittleEndian);
				currPos += (uint)component.SizeInRCode;
				indexComponents[zz] = component;
			}

			return new IndexElementV11(name, primary, flags, indexComponents);
		}

		public IIndexComponentElement[] GetIndexComponents() => this.indexComponents;

		public bool Primary => primary == 1;

		public bool Unique => (flags & UNIQUE_INDEX) != 0;

		public bool WordIndex => (flags & WORD_INDEX) != 0;

		public bool DefaultIndex => (flags & DEFAULT_INDEX) != 0;

		public override int SizeInRCode
		{
			get
			{
				int size = 16;
				foreach (IIndexComponentElement elem in indexComponents)
				{
					size += elem.SizeInRCode;
				}
				return size;
			}
		}

		public override string ToString()
		{
			return string.Format("Index {0} {1} {2} - {3:D} field(s)", Name, Primary ? "PRIMARY" : "", Unique ? "UNIQUE" : "", indexComponents.Length);
		}

		public override int GetHashCode()
		{
			return (Name + "/" + flags + "/" + string.Join("/", Array.ConvertAll(indexComponents, ConvertComponentToString))).GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is IIndexElement obj2)
			{
				return Name.Equals(obj2.Name) && Array.Equals(indexComponents, obj2.GetIndexComponents());
			}
			return false;

		}

		private string ConvertComponentToString(IIndexComponentElement obj) => obj?.ToString() ?? string.Empty;
	}
}
