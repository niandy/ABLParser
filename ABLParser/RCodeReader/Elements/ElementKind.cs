using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public enum ElementKind
    {
        UNKNOWN = 0,
        METHOD = 1,
        VARIABLE = 2,
        TABLE = 3,
        BUFFER = 4,
        QUERY = 5,
        DATASET = 6,
        DATASOURCE = 7,
        PROPERTY = 8,
        EVENT = 9
    }

    public static class ElementKindExt
    {
        public static int GetNum(this ElementKind e)
        {
            return (int)e;
        }

        public static ElementKind GetKind(int type)
        {
            return Enum.IsDefined(typeof(ElementKind), type) ? (ElementKind)type : ElementKind.UNKNOWN;
        }
    }
}
