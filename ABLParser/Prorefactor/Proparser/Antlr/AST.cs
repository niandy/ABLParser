using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// ANTLR4 version of antlr.AST, where only the interesting methods are kept. 
    /// </summary>
    public interface AST
    {

        /// <returns> First child of this node; null if no children </returns>
        AST FirstChild { get; }

        /// <returns> Next sibling in line after this one </returns>
        AST NextSibling { get; }

        /// <returns> Token text for this node </returns>
        string Text { get; }

        /// <returns> Get the token type for this node </returns>
        ABLNodeType NodeType { get; }

        /// <returns> Get the token type for this node </returns>
        int Type { get; }

        /// <returns> Line number of this node </returns>
        int Line { get; }

        /// <returns> Column number of this node </returns>
        int Column { get; }

        /// <returns> Number of children of this node; if leaf, returns 0 </returns>
        int NumberOfChildren { get; }
    }
    }
