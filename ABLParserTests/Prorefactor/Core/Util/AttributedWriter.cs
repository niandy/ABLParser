using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABLParserTests.Prorefactor.Core.Util
{
	public class AttributedWriter
	{

		internal StreamWriter writer = null;

		private string GetAttributes(JPNode node)
		{
			StringBuilder nodeComments = new StringBuilder("");
			GetAttributesForSymbol(node, nodeComments);
			GetAttributesForBlock(node, nodeComments);
			if (nodeComments.Length > 0)
			{
				nodeComments.Insert(0, " /*");
				nodeComments.Append(" */ ");
				if (node.NodeType == ABLNodeType.PROGRAM_ROOT)
				{
					nodeComments.Append('\n');
				}
			}
			return nodeComments.ToString();
		} // getAttributes

		private void GetAttributesForBlock(JPNode node, StringBuilder nodeComments)
		{
			Block block = (Block)node.Block;
			if (block == null)
			{
				return;
			}
			TableBuffer[] buffers = block.GetBlockBuffers();
			if (buffers.Length == 0)
			{
				return;
			}
			// Collect the names in a sorted set, so we can write them in
			// a consistent (sorted) order. Important for running automated
			// unit/regression tests.			
			SortedSet<string> names = new SortedSet<string>(StringComparer.Ordinal);
			foreach (TableBuffer buffSymbol in buffers)
			{
				StringBuilder name = new StringBuilder();
				if (buffSymbol.Table.Storetype == IConstants.ST_DBTABLE)
				{
					name.Append(buffSymbol.Table.Database.Name);
					name.Append(".");
				}
				name.Append(buffSymbol.Name);
				names.Add(name.ToString());
			}
			nodeComments.Append(" buffers=");
			int i = 0;
			foreach (string name in names)
			{
				if (i++ > 0)
				{
					nodeComments.Append(",");
				}
				nodeComments.Append(name);
			}
		}

		private void GetAttributesForSymbol(JPNode node, StringBuilder nodeComments)
		{
			Symbol symbol = node.Symbol;
			if (symbol == null)
			{
				return;
			}
			nodeComments.Append(" ");
			nodeComments.Append(symbol.Scope.Depth());
			nodeComments.Append(":");
			nodeComments.Append(symbol.FullName);
			if ((node.NodeType != ABLNodeType.DEFINE) && (node.AttrGet(IConstants.ABBREVIATED) > 0))
			{
				nodeComments.Append(" abbrev");
			}
			if (node.AttrGet(IConstants.UNQUALIFIED_FIELD) > 0)
			{
				nodeComments.Append(" unqualfield");
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private void walker(JPNode node, boolean showSiblings) throws IOException
		private void Walker(JPNode node, bool showSiblings)
		{
			if (node == null)
			{
				return;
			}
			if (node.AttrGet(IConstants.OPERATOR) == IConstants.TRUE)
			{
				Walker(node.FirstChild, false);
				WriteNode(node);
				Walker(node.FirstChild.NextSibling, true);
			}
			else
			{
				WriteNode(node);
				Walker(node.FirstChild, true);
			}
			if (showSiblings)
			{
				Walker(node.NextSibling, true);
			}
		}

		/// <summary>
		/// Parse and write a source file, with comments detailing some of the node attributes added by TreeParser01.
		/// </summary>
		/// <param name="inName"> Name of the compile unit's source file. </param>
		/// <param name="outName"> Name of the file to write out to. </param>
		/// <exception cref="IOException">  </exception>
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: public void write(String inName, File outName, RefactorSession session) throws IOException
		public virtual void Write(string inName, FileInfo outName, RefactorSession session)
		{
			try
			{
				ParseUnit pu = new ParseUnit(new FileInfo(inName), session);
				pu.TreeParser01();
				writer = new StreamWriter(outName.FullName);
				Walker(pu.TopNode, true);
				writer.Write('\n');
			}
			finally
			{
				if (writer != null)
				{
					writer.Close();
				}
			}
		}

		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: private void writeNode(JPNode node) throws IOException
		private void WriteNode(JPNode node)
		{
			foreach (ProToken t in node.HiddenTokens)
			{
				writer.Write(t.Text);
			}
			writer.Write(GetAttributes(node));
			writer.Write(node.Text);
			if ((node.NodeType == ABLNodeType.RUN) || (node.NodeType == ABLNodeType.PROCEDURE))
			{
				writer.Write(' ');
			}
		}

	}

}
