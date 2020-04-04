
using ABLParser.Prorefactor.Treeparser;

namespace ABLParser.Prorefactor.Core.NodeTypes
{
    /// <summary>
    /// Specialized type of JPNode for those token types: DO, FOR, REPEAT, FUNCTION, PROCEDURE, CONSTRUCTOR, DESTRUCTOR,
    /// METHOD, CANFIND, CATCH, ON, PROPERTY_GETTER, PROPERTY_SETTER
    /// </summary>
    public class BlockNode : JPNode
    {
        private Block block;
        private JPNode firstStatement;

        public BlockNode(ProToken t, JPNode parent, int num, bool hasChildren) : base(t, parent, num, hasChildren)
        {
        }

        public virtual JPNode FirstStatement
        {
            set => firstStatement = value;
            get => firstStatement;
        }


        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable @Override public org.prorefactor.treeparser.Block getBlock()
        public override Block Block
        {
            get => block;
            set => block = value;
        }


        public override bool HasBlock()
        {
            return block != null;
        }
    }
}

