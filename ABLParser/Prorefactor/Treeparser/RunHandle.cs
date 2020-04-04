using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser
{
    /// <summary>
    /// Represents a procedure handle value, used in a run statement of the form: run &lt;proc&gt; in &lt;handle&gt;.
    /// 
    /// </summary>
    public class RunHandle : Value
    {

        private string fileName;

        public object Value
        {
            set
            {
                this.fileName = (string)value;
            }
            get
            {
                return fileName;
            }
        }


    }

}
