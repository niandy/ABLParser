using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols
{
    /// <summary>
    /// A Symbol defined with DEFINE VARIABLE or any of the other various syntaxes which implicitly define a variable.
    /// </summary>
    public class Variable : Symbol, Primative, Value
    {
        private bool refInFrame = false;        

        public Variable(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
        }

        public Variable(string name, TreeParserSymbolScope scope, bool parameter) : base(name, scope, parameter)
        {
        }

        public void AssignAttributesLike(Primative likePrim)
        {
            DataType = likePrim.DataType;
            ClassName = likePrim.ClassName;
            Extent = likePrim.Extent;
        }

        /// <summary>
        /// Return the name of the variable. For this subclass of Symbol, fullName() returns the same value as getName().
        /// </summary>
        public override string FullName => Name;

        public string ClassName { get; private set; } = null;

        public DataType DataType { get; private set; }

        public int Extent { get; private set; }

        public object Value { get; set; }

        public bool GraphicalComponent { get; private set; } = false;

        /// <summary>
        /// Returns NodeTypes.VARIABLE
        /// </summary>
        public override int ProgressType => Proparse.VARIABLE;

        public Primative SetClassName(string s)
        {
            this.ClassName = s;
            return this;
        }

        public Primative SetDataType(DataType dataType)
        {
            this.DataType = dataType;
            return this;
        }

        public Primative SetExtent(int extent)
        {
            this.Extent = extent;
            return this;
        }


        public virtual void ReferencedInFrame()
        {
            this.refInFrame = true;
        }

        public virtual bool IsReferencedInFrame()
        {            
            return refInFrame;            
        }

        public override void NoteReference(ContextQualifier contextQualifier)
        {
            base.NoteReference(contextQualifier);
            if (contextQualifier == ContextQualifier.UPDATING_UI)
            {
                GraphicalComponent = true;
            }
        }

        internal override bool IsInstanceOfType(Symbol s) => s is Variable;        
    }
}
