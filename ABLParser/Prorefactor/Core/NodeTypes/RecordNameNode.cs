using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using System;
using static ABLParser.Prorefactor.Proparser.SymbolScope;

namespace ABLParser.Prorefactor.Core.NodeTypes
{
    public class RecordNameNode : JPNode
    {
        private string sortAccess = "";
        private bool wholeIndex;
        private string searchIndexName = "";
        private ContextQualifier qualifier;

        public RecordNameNode(ProToken t, JPNode parent, int num, bool hasChildren) : base(t, parent, num, hasChildren)
        {
        }

        public virtual ContextQualifier ContextQualifier
        {
            set => qualifier = value;
            get => qualifier;
        }
        

        public virtual string SortAccess
        {
            get => sortAccess;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    return;
                }
                sortAccess = sortAccess + (sortAccess.Length == 0 ? "" : ",") + value;
            }
        }

        public virtual bool WholeIndex
        {
            get => wholeIndex;
            set => wholeIndex = value;
        }

        public virtual string SearchIndexName
        {
            get => searchIndexName;
            set => searchIndexName = value;
        }

        public override TableBuffer TableBuffer
        {
            set
            {
                Symbol = value;
            }
            get
            {
                Symbol symbol = Symbol;
                return symbol is TableBuffer ? (TableBuffer)symbol : null;
            }
        }


        public override bool HasTableBuffer()
        {
            return Symbol is TableBuffer;
        }

        /// <summary>
        /// Set the 'store type' attribute on a RECORD_NAME node. </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public void setStoreType(@Nonnull FieldType tabletype)
        public virtual FieldType StoreType
        {
            set
            {
                switch (value.innerEnumValue)
                {
                    case FieldType.InnerEnum.DBTABLE:
                        AttrSet(IConstants.STORETYPE, IConstants.ST_DBTABLE);
                        break;
                    case FieldType.InnerEnum.TTABLE:
                        AttrSet(IConstants.STORETYPE, IConstants.ST_TTABLE);
                        break;
                    case FieldType.InnerEnum.WTABLE:
                        AttrSet(IConstants.STORETYPE, IConstants.ST_WTABLE);
                        break;
                    case FieldType.InnerEnum.VARIABLE:
                        // Never happens
                        break;
                }
            }
        }

    }

}
