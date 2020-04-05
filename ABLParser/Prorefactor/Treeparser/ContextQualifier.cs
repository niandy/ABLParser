using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser
{
    public sealed class ContextQualifier
    {
        /// <summary>
        /// Is the symbol reference also an initializer? i.e. An input parameter. Also used in FIND statement for record
        /// buffer.
        /// </summary>
        public static readonly ContextQualifier INIT = new ContextQualifier("INIT", InnerEnum.INIT);
        /// <summary>
        /// Referencing the symbol's <b>value</b>.
        /// </summary>
        public static readonly ContextQualifier REF = new ContextQualifier("REF", InnerEnum.REF);
        /// <summary>
        /// Reference and update the symbol's value. Usually this is in an UPDATE statement, which displays and updates.
        /// </summary>
        public static readonly ContextQualifier REFUP = new ContextQualifier("REFUP", InnerEnum.REFUP);
        /// <summary>
        /// Updating the symbol's value.
        /// </summary>
        public static readonly ContextQualifier UPDATING = new ContextQualifier("UPDATING", InnerEnum.UPDATING);
        /// <summary>
        /// Creating (thus updating) symbol's value with a GUI component
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: The following line uses invalid syntax:
        public static readonly ContextQualifier UPDATING_UI = new ContextQualifier("UPDATING_UI", InnerEnum.UPDATING_UI);
        /// <summary>
        /// We are strictly referencing the symbol - not its value. Used both for field and table symbols. For table symbols,
        /// the lookup is done by schema symbols first, buffer symbols second.
        /// </summary>
        public static readonly ContextQualifier SYMBOL = new ContextQualifier("SYMBOL", InnerEnum.SYMBOL);
        /// <summary>
        /// Referencing a buffer symbol. The lookup is done by buffer symbols first, schema symbols second.
        /// </summary>
        public static readonly ContextQualifier BUFFERSYMBOL = new ContextQualifier("BUFFERSYMBOL", InnerEnum.BUFFERSYMBOL);
        /// <summary>
        /// A temp or work table symbol.
        /// </summary>
        public static readonly ContextQualifier TEMPTABLESYMBOL = new ContextQualifier("TEMPTABLESYMBOL", InnerEnum.TEMPTABLESYMBOL);
        /// <summary>
        /// A schema table symbol.
        /// </summary>
        public static readonly ContextQualifier SCHEMATABLESYMBOL = new ContextQualifier("SCHEMATABLESYMBOL", InnerEnum.SCHEMATABLESYMBOL);
        /// <summary>
        /// INIT, but for a "weak" scoped buffer
        /// </summary>
        public static readonly ContextQualifier INITWEAK = new ContextQualifier("INITWEAK", InnerEnum.INITWEAK);
        /// <summary>
        /// Static reference to class
        /// </summary>
        public static readonly ContextQualifier STATIC = new ContextQualifier("STATIC", InnerEnum.STATIC);

        private static readonly IList<ContextQualifier> valueList = new List<ContextQualifier>();

        static ContextQualifier()
        {
            valueList.Add(INIT);
            valueList.Add(REF);
            valueList.Add(REFUP);
            valueList.Add(UPDATING);
            valueList.Add(UPDATING_UI);
            valueList.Add(SYMBOL);
            valueList.Add(BUFFERSYMBOL);
            valueList.Add(TEMPTABLESYMBOL);
            valueList.Add(SCHEMATABLESYMBOL);
            valueList.Add(INITWEAK);
            valueList.Add(STATIC);
        }

        public enum InnerEnum
        {
            INIT,
            REF,
            REFUP,
            UPDATING,
            UPDATING_UI,
            SYMBOL,
            BUFFERSYMBOL,
            TEMPTABLESYMBOL,
            SCHEMATABLESYMBOL,
            INITWEAK,
            STATIC
        }

        public readonly InnerEnum innerEnumValue;
        private readonly string nameValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        private ContextQualifier(string name, InnerEnum innerEnum)
        {
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
        }

        /// <summary>
        /// Is symbol's value "read" in this context?
        /// </summary>
        public static bool IsRead(ContextQualifier cq)
        {
            switch (cq.innerEnumValue)
            {
                case ContextQualifier.InnerEnum.INIT:
                case ContextQualifier.InnerEnum.INITWEAK:
                case ContextQualifier.InnerEnum.REF:
                case ContextQualifier.InnerEnum.REFUP:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the symbol's value "written" in this context?
        /// </summary>
        public static bool IsWrite(ContextQualifier cq)
        {
            switch (cq.innerEnumValue)
            {
                case ContextQualifier.InnerEnum.REFUP:
                case ContextQualifier.InnerEnum.UPDATING:
                case ContextQualifier.InnerEnum.UPDATING_UI:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Is the symbol's value "referenced" in this context?
        /// </summary>
        public static bool IsReference(ContextQualifier cq) => cq == SYMBOL ? true : false;

        public static IList<ContextQualifier> Values() => valueList;

        public int Ordinal() => ordinalValue;

        public override string ToString() => nameValue;        

        public static ContextQualifier ValueOf(string name)
        {
            foreach (ContextQualifier enumInstance in ContextQualifier.valueList)
            {
                if (enumInstance.nameValue == name)
                {
                    return enumInstance;
                }
            }
            throw new System.ArgumentException(name);
        }
    }
}
