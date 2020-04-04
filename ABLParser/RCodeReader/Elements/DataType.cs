using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public enum DataType
    {
        VOID = 0,
        CHARACTER = 1,
        DATE = 2,
        LOGICAL = 3,
        INTEGER = 4,
        DECIMAL = 5,
        RECID = 7,
        RAW = 8,
        HANDLE = 10,
        MEMPTR = 11,
        SQLDYN = 12,
        ROWID = 13,
        COMPONENT_HANDLE = 14,
        TABLE = 15,
        UNKNOWN = 16,
        TABLE_HANDLE = 17,
        BLOB = 18,
        CLOB = 19,
        XLOB = 20,
        BYTE = 21,
        SHORT = 22,
        LONG = 23,
        FLOAT = 24,
        DOUBLE = 25,
        UNSIGNED_SHORT = 26,
        UNSIGNED_BYTE = 27,
        CURRENCY = 28,
        ERROR_CODE = 29,
        UNKNOWN2 = 30,
        FIXCHAR = 31,
        BIGINT = 32,
        TIME = 33,
        DATETIME = 34,
        FIXRAW = 35,
        DATASET = 36,
        DATASET_HANDLE = 37,
        LONGCHAR = 39,
        DATETIME_TZ = 40,
        INT64 = 41,
        CLASS = 42,
        UNSIGNED_INTEGER = 44,
        UNSIGNED_INT64 = 43,
        SINGLE_CHARACTER = 46,
        RUNTYPE = 48
    }

    public static class DataTypeExt
    {
        private const int LAST_VALUE = 48;  

        public static int GetNum(this DataType d)
        {
            return (int)d;
        }

        public static DataType AsDataType(this int num)
        {
            return (num >= 0) && (num <= LAST_VALUE) ? (DataType)num : DataType.UNKNOWN;
        }

        public static DataType AsDataType(this string str)
        {
            try
            {
                return int.Parse(str).AsDataType();
            }
            catch (System.FormatException)
            {
                return DataType.CLASS;
            }
        }        
    }
}
