using ABLParser.Prorefactor.Core;
using System;
using System.IO;
using System.Linq;

namespace ABLParser.Prorefactor.Proparser.Antlr
{ 
    /// <summary>
    /// Prints out the structure of a JPNode AST. Prints nodes one per line, using indentation to show the tree structure.
    /// </summary>
    public class JPNodeLister
    {
        private const int INDENT_BY = 4;

        private readonly JPNode topNode;
        private readonly StreamWriter ofile;

        public JPNodeLister(JPNode topNode, StreamWriter writer)
        {
            this.topNode = topNode;
            this.ofile = writer;
        }

        /// <summary>
        /// Print node content to PrintWriter with default settings
        /// </summary>
        public virtual void Print()
        {
            Print(' ');
        }

        /// <summary>
        /// Print node content to PrintWriter with specified spacer char
        /// </summary>
        public virtual void Print(char spacer)
        {
            Print(spacer, false, false, false, false);
        }

        /// <summary>
        /// Print node content to PrintWriter
        /// </summary>
        public virtual void Print(char spacer, bool showLine, bool showCol, bool showFileName, bool showStore)
        {
            Print_sub(topNode, 0, spacer, showLine, showCol, showFileName, showStore);
            ofile.Flush();
        }

        protected internal virtual string GetExtraInfo(JPNode node, char spacer)
        {
            return "";
        }

        private void Print_sub(JPNode node, int level, char spacer, bool showLine, bool showCol, bool showFileName, bool showStore)
        {
            Printline(node, level, spacer, showLine, showCol, showFileName, showStore);
            foreach (JPNode child in node.DirectChildren)
            {
                Print_sub(child, level + 1, spacer, showLine, showCol, showFileName, showStore);
            }
        }

        private void Printline(JPNode node, int level, char spacer, bool showLine, bool showCol, bool showFileName, bool showStore)
        {
            // Indent node
            char[] indentArray = Enumerable.Repeat<char>(' ', level * INDENT_BY).ToArray<char>();                
            ofile.Write(indentArray);

            // Node type
            ofile.Write(node.NodeType.ToString());
            ofile.Write(spacer);
            // Node text
            ofile.Write(node.Text);
            ofile.Write(spacer);
            if (showLine)
            {
                ofile.Write(Convert.ToString(node.Line));
                ofile.Write(spacer);
            }
            if (showCol)
            {
                ofile.Write(Convert.ToString(node.Column));
                ofile.Write(spacer);
            }
            if (showFileName)
            {
                ofile.Write(Convert.ToString(node.FileIndex));
                ofile.Write(spacer);
            }
            if (showStore)
            {
                int storetype = node.AttrGet(IConstants.STORETYPE);
                if (storetype != 0)
                {
                    ofile.Write(Convert.ToString(storetype));
                    ofile.Write(spacer);
                }
            }
            ofile.Write(GetExtraInfo(node, spacer));
            ofile.WriteLine();
        }
    }   

}
