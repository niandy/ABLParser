using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core.Schema
{
    using System.Collections.Generic;

    public class Constants
    {
        public static readonly IDatabase nullDatabase = new Database("");
        public static readonly ITable nullTable = new Table("");

        /// <summary>
        /// Comparator for sorting by name. </summary>
        public static readonly IComparer<IDatabase> DB_NAME_ORDER = new ComparatorAnonymousInnerClass();

        private class ComparatorAnonymousInnerClass : IComparer<IDatabase>
        {
            public int Compare(IDatabase d1, IDatabase d2)
            {
                return string.Compare(d1.Name, d2.Name, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Comparator for sorting by name. </summary>
        public static readonly IComparer<ITable> TABLE_NAME_ORDER = new ComparatorAnonymousInnerClass2();

        private class ComparatorAnonymousInnerClass2 : IComparer<ITable>
        {
            public int Compare(ITable t1, ITable t2)
            {
                return string.Compare(t1.GetName(), t2.GetName(), StringComparison.CurrentCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Comparator for sorting by name. </summary>
        public static readonly IComparer<IField> FIELD_NAME_ORDER = new ComparatorAnonymousInnerClass3();

        private class ComparatorAnonymousInnerClass3 : IComparer<IField>
        {
            public int Compare(IField f1, IField f2)
            {
                return string.Compare(f1.GetName(), f2.GetName(), StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private Constants()
        {
            // No-op
        }
    }

}
