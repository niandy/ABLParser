
namespace ABLParser.RCodeReader.Elements
{
	public abstract class AbstractAccessibleElement : AbstractElement, IAccessibleElement
	{
		private readonly AccessType accessType;

		public AbstractAccessibleElement(string name, AccessType? accessType) : base(name)
		{
			this.accessType = accessType ?? AccessType.NONE;
		}

		public virtual bool Protected => accessType.HasFlag(AccessType.PROTECTED);

		public virtual bool Public => accessType.HasFlag(AccessType.PUBLIC);

		public virtual bool Private => accessType.HasFlag(AccessType.PRIVATE);

		public virtual bool Abstract => accessType.HasFlag(AccessType.ABSTRACT);

		public virtual bool Static => accessType.HasFlag(AccessType.STATIC);
	}
}
