using System.IO;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class XCodedFileException : IOException
    {
        private const long serialVersionUID = -6437738654876482735L;

        private readonly string fileName;

        public XCodedFileException(string fileName) : base("Unable to read xcode'd file " + fileName)
        {
            this.fileName = fileName;
        }

        public virtual string FileName => fileName;
    }

}
