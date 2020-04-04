using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABLParserTests.Prorefactor.Core.Util
{
    using ABLParser.Prorefactor.Core;
    using ABLParser.Prorefactor.Core.NodeTypes;
    using ABLParser.Prorefactor.Proparser.Antlr;
    using ABLParser.Prorefactor.Treeparser;
    using ABLParser.Prorefactor.Treeparser.Symbols;
    using ABLParser.Prorefactor.Treeparser.Symbols.Widgets;
    using System.IO;
    using System.Text;

	public class TP01FramesTreeLister : JPNodeLister
	{

		public TP01FramesTreeLister(JPNode topNode, StreamWriter writer) : base(topNode, writer)
		{
		}

		private void AppendName(StringBuilder buff, FieldContainer container)
		{
			if (container.Name.Length == 0)
			{
				buff.Append('"').Append(container.Name).Append('"');
			}
			else
			{
				buff.Append(container.Name);
			}
		}

		protected internal override string GetExtraInfo(JPNode node, char spacer)
		{
			StringBuilder buff = new StringBuilder();
			// buff.append(indent(1));
			if (node is BlockNode)
			{
				BlockNode(buff, (BlockNode)node, spacer);
			}
			if (node is FieldRefNode)
			{
				FieldRefNode(buff, (FieldRefNode)node, spacer);
				FieldContainer(buff, node, spacer);
			}
			if (node.StateHead)
			{
				FieldContainer(buff, node, spacer);
			}
			return buff.ToString();
		}

		private void BlockNode(StringBuilder buff, BlockNode blockNode, char spacer)
		{
			Block block = blockNode.Block;
			if (block.DefaultFrame != null)
			{
				buff.Append("defaultFrame:").Append(spacer);
				AppendName(buff, block.DefaultFrame);
			}
			buff.Append("frames:").Append(spacer);
			foreach (Frame frame in block.Frames)
			{
				buff.Append(" ");
				AppendName(buff, frame);
			}
		}

		private void FieldContainer(StringBuilder buff, JPNode node, char spacer)
		{
			FieldContainer fieldContainer = node.FieldContainer;
			if (fieldContainer == null)
			{
				return;
			}
			buff.Append(spacer).Append(ABLNodeType.GetNodeType(fieldContainer.ProgressType)).Append("=");
			AppendName(buff, fieldContainer);
		}

		private void FieldRefNode(StringBuilder buff, FieldRefNode refNode, char spacer)
		{
			Symbol symbol = refNode.Symbol;
			buff.Append(spacer).Append(symbol == null ? "" : symbol.FullName);
		}

	}

}
