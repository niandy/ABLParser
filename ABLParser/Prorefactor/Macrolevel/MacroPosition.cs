using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    /// <summary>
    /// Position of macro in a file 
    /// </summary>
    public class MacroPosition
    {
        private readonly int fileNum;
        private readonly int line;
        private readonly int column;

        public MacroPosition(int fileNum, int line, int column)
        {
            this.fileNum = fileNum;
            this.line = line;
            this.column = column;
        }

        public virtual int FileNum => fileNum;

        public virtual int Line => line;

        public virtual int Column => column;
    }

}
