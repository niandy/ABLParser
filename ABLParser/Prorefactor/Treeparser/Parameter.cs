using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Treeparser.Symbols;

namespace ABLParser.Prorefactor.Treeparser
{
    public class Parameter
    {

        private bool bind = false;
        private int progressType = Proparse.VARIABLE;
        private ABLNodeType directionNode;
        private Symbol symbol;

        /// <summary>
        /// For a TEMP-TABLE or DATASET, was the BIND keyword used? </summary>
        public virtual bool Bind
        {
            get
            {
                return bind;
            }
            set
            {
                this.bind = value;
            }
        }

        /// <summary>
        /// The node of (BUFFER|INPUT|OUTPUT|INPUTOUTPUT|RETURN). </summary>
        public virtual ABLNodeType DirectionNode
        {
            get
            {
                return directionNode;
            }
            set
            {
                this.directionNode = value;
            }
        }

        /// <summary>
        /// Integer corresponding to TokenType for (BUFFER|VARIABLE|TEMPTABLE|DATASET|PARAMETER). The syntax
        /// <code>PARAMETER field = expression</code> is for RUN STORED PROCEDURE, and for those there is no symbol.
        /// </summary>
        public virtual int ProgressType
        {
            get
            {
                return progressType;
            }
            set
            {
                this.progressType = value;
            }
        }

        /// <summary>
        /// For call arguments that are expressions, there might be no symbol (null). For Routines, the symbol should always be
        /// non-null.
        /// </summary>
        public virtual Symbol Symbol
        {
            get
            {
                return symbol;
            }
            set
            {
                this.symbol = value;
            }
        }





    }

}
