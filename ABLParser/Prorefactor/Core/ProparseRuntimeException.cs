using System;

namespace ABLParser.Prorefactor.Core
{
    public class ProparseRuntimeException : Exception
    {
        private const long serialVersionUID = -1350324743265891607L;

        public ProparseRuntimeException(string message) : base(message)
        {
        }

        public ProparseRuntimeException(Exception cause) : base("Proparse nested exception", cause)
        {
        }
    }

}
