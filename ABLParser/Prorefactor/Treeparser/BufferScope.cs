using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Treeparser.Symbols;

namespace ABLParser.Prorefactor.Treeparser
{
    using System.Collections.Generic;

    /// <summary>
    /// A record of a BufferSymbol scope to a Block. Tells us if the scope is "strong" or not.
    /// </summary>
    public class BufferScope
    {

        private Strength strength;
        private Block block;
        private TableBuffer symbol;

        public sealed class Strength
        {
            public static readonly Strength STRONG = new Strength("STRONG", InnerEnum.STRONG, 1);
            public static readonly Strength WEAK = new Strength("WEAK", InnerEnum.WEAK, 2);
            public static readonly Strength REFERENCE = new Strength("REFERENCE", InnerEnum.REFERENCE, 3);
            /// <summary>
            /// A "hidden cursor" is a BufferScope which has no side-effects on surrounding blocks like strong, weak, and
            /// reference scopes do. These are used within a CAN-FIND function.
            /// </summary>
            public static readonly Strength HIDDEN_CURSOR = new Strength("HIDDEN_CURSOR", InnerEnum.HIDDEN_CURSOR, 4);

            private static readonly IList<Strength> valueList = new List<Strength>();

            static Strength()
            {
                valueList.Add(STRONG);
                valueList.Add(WEAK);
                valueList.Add(REFERENCE);
                valueList.Add(HIDDEN_CURSOR);
            }

            public enum InnerEnum
            {
                STRONG,
                WEAK,
                REFERENCE,
                HIDDEN_CURSOR
            }

            public readonly InnerEnum innerEnumValue;
            private readonly string nameValue;
            private readonly int ordinalValue;
            private static int nextOrdinal = 0;
            internal int value;

            internal Strength(string name, InnerEnum innerEnum, int value)
            {
                this.value = value;

                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            }

            public int Value
            {
                get
                {
                    return value;
                }
            }

            public static IList<Strength> Values()
            {
                return valueList;
            }

            public int Ordinal()
            {
                return ordinalValue;
            }

            public override string ToString()
            {
                return nameValue;
            }

            public static Strength ValueOf(string name)
            {
                foreach (Strength enumInstance in Strength.valueList)
                {
                    if (enumInstance.nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }

        public BufferScope(Block block, TableBuffer symbol, Strength strength)
        {
            this.block = block;
            this.symbol = symbol;
            this.strength = strength;
        }

        public virtual Block Block
        {
            get
            {
                return block;
            }
            set
            {
                this.block = value;
            }
        }

        internal virtual Strength GetStrength()
        {
            return strength;
        }

        public virtual TableBuffer Symbol
        {
            get
            {
                return symbol;
            }
        }

        public virtual bool Strong
        {
            get
            {
                return strength == Strength.STRONG;
            }
        }

        public virtual bool Weak
        {
            get
            {
                return strength == Strength.WEAK;
            }
        }


        public virtual void SetStrength(Strength strength)
        {
            this.strength = strength;
        }

    }

}
