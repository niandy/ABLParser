using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    /// <summary>
    /// A Symbol defined with DEFINE STREAM or any other syntax which implicitly define a stream.
    /// </summary>
    public class Stream : Symbol
    {

        public Stream(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        /// <summary>
        /// For this subclass of Symbol, fullName() returns the same value as getName()
        /// </summary>
        public override string FullName => Name;

        internal override bool IsInstanceOfType(Symbol s) => s is Stream;

        /// <summary>
        /// Returns NodeTypes.STREAM
        /// </summary>
        public override int ProgressType => Proparse.STREAM;
    }
}
