using System;

namespace ABLParser.Prorefactor.Proparser.Antlr
{   
    public class ProEvalException : Exception
    {
        private const long serialVersionUID = 7002021531916522201L;

        private readonly string fileName;
        private readonly int line;
        private readonly int column;

        public ProEvalException(string message) : base(message)
        {
            this.fileName = null;
            this.line = -1;
            this.column = -1;
        }

        public ProEvalException(string message, Exception caught, string fileName, int line, int column) : base(message, caught)
        {
            this.fileName = fileName;
            this.line = line;
            this.column = column;
        }

        public virtual string FileName => fileName;
        public virtual int Column => column;            
        public virtual int Line => line;            
    }

}
