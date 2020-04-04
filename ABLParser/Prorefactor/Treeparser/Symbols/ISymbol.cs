using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    public interface ISymbol
    {

        string Name { get; }

        /// <summary>
        /// Get the "full" name for this symbol. For example, we might expect "database.buffer.field" to be the return value
        /// for a field buffer.
        /// </summary>
        string FullName { get; }
        int AllRefsCount { get; }

        int NumReads { get; }

        int NumWrites { get; }

        int NumReferenced { get; }

        JPNode DefinitionNode { set; }

        /// <summary>
        /// Returns tree object where symbol was defined. Can be a DEFINE keyword or directly the ID token.
        /// </summary>
        JPNode DefineNode { get; }

        /// <summary>
        /// From TokenTypes: VARIABLE, FRAME, MENU, MENUITEM, etc. A TableBuffer object always returns BUFFER, regardless of
        /// whether the object is a named buffer or a default buffer. A FieldBuffer object always returns FIELD.
        /// </summary>
        int ProgressType { get; }

        TreeParserSymbolScope Scope { get; }

        /// <summary>
        /// Take note of a symbol reference (read, write, reference by name)
        /// </summary>
        void NoteReference(ContextQualifier contextQualifier);

        ISymbol LikeSymbol { set; get; }

    }

}
