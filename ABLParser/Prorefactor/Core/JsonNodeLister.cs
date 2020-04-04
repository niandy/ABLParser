using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.IO;

namespace ABLParser.Prorefactor.Core
{
    

    /// <summary>
    /// Prints out the structure of a JPNode AST as JSON.
    /// </summary>
    public class JsonNodeLister
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(JsonNodeLister));

        private readonly JPNode topNode;
        private readonly TextWriter ofile;
        private readonly ISet<ABLNodeType> ignored;

        public JsonNodeLister(JPNode topNode, TextWriter writer, ABLNodeType ignoredKw, params ABLNodeType[] ignoredKws)
        {
            this.topNode = topNode;
            this.ofile = writer;
            this.ignored = ignoredKws == null ? new HashSet<ABLNodeType>() : new HashSet<ABLNodeType>(ignoredKws);
            this.ignored.Add(ignoredKw);
        }

        /// <summary>
        /// Print node content to PrintWriter
        /// </summary>
        public virtual void Print()
        {
            try
            {
                PrintSub(topNode, true);
                ofile.Close();
            }
            catch (IOException)
            {
                LOG.Error("Unable to write output");
            }
        }

        /// <summary>
        /// Print node and children
        /// </summary>
        /// <param name="node"> Node to be printed </param>
        /// <param name="firstElem"> First child of parent element ? </param>
        /// <returns> False if node is skipped </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private boolean printSub(JPNode node, boolean firstElem) throws IOException
        private bool PrintSub(JPNode node, bool firstElem)
        {
            if (ignored.Contains(node.NodeType))
            {
                return false;
            }
            if (!firstElem)
            {
                ofile.Write(',');
            }
            ofile.Write('{');
            PrintAttributes(node);
            if (node.DirectChildren.Count != 0)
            {
                bool firstChild = true;
                ofile.Write(", \"children\": [");
                foreach (JPNode child in node.DirectChildren)
                {
                    // Next element won't be first child anymore if this element is printed
                    firstChild &= !PrintSub(child, firstChild);
                }
                ofile.Write(']');
            }
            ofile.Write('}');

            return true;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private void printAttributes(JPNode node) throws IOException
        private void PrintAttributes(JPNode node)
        {
            ofile.Write("\"name\": \"" + node.NodeType.ToString());
            if (node.NodeType == ABLNodeType.ID)
            {
                ofile.Write(" [");
                ofile.Write(node.Text.Replace('\'', ' ').Replace('"', ' '));
                ofile.Write("]");
            }
            ofile.Write("\", \"head\": " + (node.StateHead ? "true" : "false") + ", \"line\": " + node.Line + ", \"column\": " + node.Column + ", \"file\": " + node.FileIndex);
        }

    }


}
