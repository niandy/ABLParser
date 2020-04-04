using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class IncludeFileNotFoundException : FileNotFoundException
    {
        private const long serialVersionUID = -6437738654876482735L;

        private readonly string sourceFileName;
        private readonly string includeName;

        public IncludeFileNotFoundException(string fileName, string incName) : base(fileName + " - Unable to find include file '" + incName + "'")
        {
            this.sourceFileName = fileName;
            this.includeName = incName;
        }

        public new virtual string FileName => sourceFileName;
        public virtual string IncludeName => includeName;        
    }

}
