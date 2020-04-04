using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public enum ParameterType
    {
        VARIABLE = 2,
        TABLE = 3,
        BUFFER = 4,
        QUERY = 5,
        DATASET = 6,
        DATA_SOURCE = 7,
        FORM = 8,
        BROWSE = 9,
        BUFFER_TEMP_TABLE = 103,
        UNKNOWN = -1
    }
    public static class ParameterTypeExt
    {
        public static int GetNum(this ParameterType p)
        {
            return (int)p;
        }
        public static string GetName(this ParameterType p)
        {
            return p.ToString().Replace('_', '-');
        }

        public static ParameterType GetParameterType(int type)
        {
            return Enum.IsDefined(typeof(ParameterType), type) ? (ParameterType)type : ParameterType.UNKNOWN;
        }
    }
}
