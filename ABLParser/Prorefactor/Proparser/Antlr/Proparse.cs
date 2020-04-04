using Antlr4.Runtime;
using ABLParser.Prorefactor.Refactor;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public partial class Proparse : Parser
    {
        public ParserSupport ParserSupport { get; private set; }

        private ITokenStream _input => this.InputStream as ITokenStream;

        public void InitAntlr4(RefactorSession session)
        {
            ParserSupport = new ParserSupport(session);
        }
    }   
}
