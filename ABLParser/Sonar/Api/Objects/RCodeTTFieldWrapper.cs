using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Treeparser;
using System;
using IVariableElement = ABLParser.RCodeReader.Elements.IVariableElement;


namespace ABLParser.Sonar.Api.Objects
{
    public class RCodeTTFieldWrapper : IField
    {
        private readonly ITable table;
        private readonly IVariableElement field;

        public RCodeTTFieldWrapper(ITable table, IVariableElement field)
        {
            this.table = table ?? throw new ArgumentNullException(nameof(table));
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public virtual IVariableElement BackingObject => field;

        public string GetName() => field.Name;

        public DataType DataType =>
                // TODO Fix conversion between datatypes
                DataType.GetDataType(field.DataType.ToString().Replace('_', '-'));

        public string ClassName =>
                // Fields can't be instances of class
                null;

        public int Extent => field.Extent;

        public ITable Table
        {
            get => table;
            set => throw new System.NotSupportedException();
        }

        public IField CopyBare(ITable toTable)
        {
            return new RCodeTTFieldWrapper(toTable, field);
        }

        public void AssignAttributesLike(Primative likePrim)
        {
            throw new System.NotSupportedException();
        }

        public Primative SetClassName(string className)
        {
            throw new System.NotSupportedException();
        }

        public Primative SetDataType(Prorefactor.Treeparser.DataType dataType)
        {
            throw new System.NotSupportedException();
        }

        public Primative SetExtent(int extent)
        {
            throw new System.NotSupportedException();
        }
    }
}
