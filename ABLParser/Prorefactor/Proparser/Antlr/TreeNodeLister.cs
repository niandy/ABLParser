using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.NodeTypes;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// Prints out the structure of a JPNode AST as plain text.
    /// </summary>
    public class TreeNodeLister
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TreeNodeLister));

        private readonly JPNode topNode;
        private readonly StreamWriter ofile;
        private readonly ISet<ABLNodeType> ignored;
        private readonly ParserSupport support;

        public TreeNodeLister(JPNode topNode, ParserSupport support, StreamWriter writer, ABLNodeType ignoredKw, params ABLNodeType[] ignoredKws)
        {
            this.topNode = topNode;
            this.ofile = writer;
            this.ignored = ignoredKws.Length == 0 ? new HashSet<ABLNodeType>() : new HashSet<ABLNodeType>(ignoredKws);
            this.ignored.Add(ignoredKw);            
            this.support = support;
        }

        /// <summary>
        /// Print node content to PrintWriter
        /// </summary>
        public virtual void Print()
        {
            try
            {
                PrintSub(topNode, 0);
                ofile.Write("\n\n\n");
                support.UnitScope.WriteScope(ofile);
                ofile.Write("\n\n\n");
                foreach (SymbolScope scope in support.InnerScopes)
                {
                    scope.WriteScope(ofile);
                    ofile.Write("\n\n\n");
                }
            }
            catch (IOException uncaught)
            {
                LOG.Error("Unable to write output", uncaught);
            }
        }

        /// <summary>
        /// Print node and children
        /// </summary>
        /// <param name="node"> Node to be printed </param>
        /// <param name="firstElem"> First child of parent element ? </param>
        /// <returns> False if node is skipped </returns>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private boolean printSub(JPNode node, int tabs) throws IOException
        private bool PrintSub(JPNode node, int tabs)
        {
            if (ignored.Contains(node.NodeType))
            {
                return false;
            }
            PrintAttributes(node, tabs);
            if (node.DirectChildren.Count > 0)
            {
                foreach (JPNode child in node.DirectChildren)
                {
                    PrintSub(child, tabs + 1);
                }
            }
            return true;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private void printAttributes(JPNode node, int tabs) throws IOException
        private void PrintAttributes(JPNode node, int tabs)
        {
            if (node.NodeType == ABLNodeType.EOF_ANTLR4)
            {
                return;
            }
            ofile.Write(string.Format("{0,3} {1}", tabs, new string(' ', tabs)));
            ofile.Write(node.NodeType + (node.StateHead ? "^ " : " ") + (node.StateHead && (node.State2 != 0) ? node.State2.ToString() : ""));
            if (node.AttrGet(IConstants.OPERATOR) == IConstants.TRUE)
            {
                ofile.Write("*OP* ");
            }
            if (node.AttrGet(IConstants.INLINE_VAR_DEF) == IConstants.TRUE)
            {
                ofile.Write("*IN* ");
            }
            if (node.AttrGet(IConstants.STORETYPE) > 0)
            {
                ofile.Write("StoreType " + node.AttrGet(IConstants.STORETYPE) + " ");
            }
            if ((node.NodeType == ABLNodeType.ID) || (node.NodeType == ABLNodeType.TYPE_NAME))
            {
                ofile.Write("[");
                ofile.Write(node.Text.Replace('\'', ' ').Replace('"', ' '));
                ofile.Write("] ");
            }
            if (node is TypeNameNode)
            {
                string qualName = ((TypeNameNode)node).QualName;
                if (!string.ReferenceEquals(qualName, null))
                {
                    ofile.Write(" Qualified name: '" + qualName + "'");
                }
            }
            ofile.Write(string.Format("@F{0:D}:{1:D}:{2:D}", node.FileIndex, node.Line, node.Column));
            ofile.Write("\n");
        }

    }

}
