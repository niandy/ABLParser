using ABLParser.Prorefactor.Proparser.Antlr;
using System.Collections.Generic;

namespace ABLParser.Prorefactor.Treeparser
{
    /// <summary>
    /// One static instance of DataType is created for each data type in the 4GL. You can access each of those through this
    /// class's public static final variables. This class was created just so that we could look up, and store, an object
    /// instead of a String or int to represent data type. For example, we'll be adding datatype support into Field and
    /// schemaLoad next.
    /// </summary>
    public class DataType
    {
        private static IDictionary<string, DataType> nameMap = new Dictionary<string, DataType>();
        private static IDictionary<int, DataType> tokenTypeMap = new Dictionary<int, DataType>();

        public static readonly DataType VOID = new DataType(Proparse.VOID, "VOID");
        public static readonly DataType BIGINT = new DataType(Proparse.BIGINT, "BIGINT");
        public static readonly DataType BLOB = new DataType(Proparse.BLOB, "BLOB");
        public static readonly DataType BYTE = new DataType(Proparse.BYTE, "BYTE");
        public static readonly DataType CHARACTER = new DataType(Proparse.CHARACTER, "CHARACTER");
        public static readonly DataType CLASS = new DataType(Proparse.CLASS, "CLASS");
        public static readonly DataType CLOB = new DataType(Proparse.CLOB, "CLOB");
        public static readonly DataType COMHANDLE = new DataType(Proparse.COMHANDLE, "COM-HANDLE");
        public static readonly DataType DATE = new DataType(Proparse.DATE, "DATE");
        public static readonly DataType DATETIME = new DataType(Proparse.DATETIME, "DATETIME");
        public static readonly DataType DATETIMETZ = new DataType(Proparse.DATETIMETZ, "DATETIME-TZ");
        public static readonly DataType DECIMAL = new DataType(Proparse.DECIMAL, "DECIMAL");
        public static readonly DataType DOUBLE = new DataType(Proparse.DOUBLE, "DOUBLE");
        public static readonly DataType FIXCHAR = new DataType(Proparse.FIXCHAR, "FIXCHAR");
        public static readonly DataType FLOAT = new DataType(Proparse.FLOAT, "FLOAT");
        public static readonly DataType HANDLE = new DataType(Proparse.HANDLE, "HANDLE");
        public static readonly DataType INTEGER = new DataType(Proparse.INTEGER, "INTEGER");
        public static readonly DataType INT64 = new DataType(Proparse.INT64, "INT64");
        public static readonly DataType LONG = new DataType(Proparse.LONG, "LONG");
        public static readonly DataType LONGCHAR = new DataType(Proparse.LONGCHAR, "LONGCHAR");
        public static readonly DataType LOGICAL = new DataType(Proparse.LOGICAL, "LOGICAL");
        public static readonly DataType MEMPTR = new DataType(Proparse.MEMPTR, "MEMPTR");
        public static readonly DataType NUMERIC = new DataType(Proparse.NUMERIC, "NUMERIC");
        public static readonly DataType RAW = new DataType(Proparse.RAW, "RAW");
        public static readonly DataType RECID = new DataType(Proparse.RECID, "RECID");
        public static readonly DataType ROWID = new DataType(Proparse.ROWID, "ROWID");
        public static readonly DataType SHORT = new DataType(Proparse.SHORT, "SHORT");
        public static readonly DataType TIME = new DataType(Proparse.TIME, "TIME");
        public static readonly DataType TIMESTAMP = new DataType(Proparse.TIMESTAMP, "TIMESTAMP");
        public static readonly DataType TYPE_NAME = CLASS;
        public static readonly DataType UNSIGNEDSHORT = new DataType(Proparse.UNSIGNEDSHORT, "UNSIGNED-SHORT");
        public static readonly DataType WIDGETHANDLE = new DataType(Proparse.WIDGETHANDLE, "WIDGET-HANDLE");

        private int tokenType;
        private string progressName;

        private DataType(int tokenType, string progressName)
        {
            this.tokenType = tokenType;
            this.progressName = progressName;
            nameMap[progressName] = this;
            tokenTypeMap[this.tokenType] = this;
        }

        /// <summary>
        /// Get the DataType object for an integer token type. This can return null - when you use this function, adding a
        /// check with assert or throw might be appropriate.
        /// </summary>
        public static DataType GetDataType(int tokenType)
        {
            tokenTypeMap.TryGetValue(tokenType, out DataType dt);
            return dt;
        }

        /// <summary>
        /// Get the DataType object for a String "progress data type name", ex: "COM-HANDLE". <b>Requires all caps characters,
        /// not abbreviated.</b> This can return null - when you use this function, adding a check with assert or throw might
        /// be appropriate.
        /// </summary>
        public static DataType GetDataType(string progressCapsName)
        {
            return nameMap[progressCapsName];
        }

        /// <summary>
        /// The progress name for the data type is all caps, ex: "COM-HANDLE" </summary>
        public virtual string ProgressName
        {
            get
            {
                return progressName;
            }
        }

        /// <summary>
        /// Returns the Proparse integer token type, ex: TokenTypes.COMHANDLE </summary>
        public virtual int TokenType
        {
            get
            {
                return tokenType;
            }
        }

        /// <summary>
        /// Same as getProgressName.
        /// </summary>
        public override string ToString()
        {
            return progressName;
        }

    }

}
