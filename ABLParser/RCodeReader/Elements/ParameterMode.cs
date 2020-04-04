using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.RCodeReader.Elements
{
    public enum ParameterMode
    {
        INPUT = 6028,
        OUTPUT = 6049,
        INPUT_OUTPUT = 6110,
        BUFFER = 1070,
        RETURN = -1
    }
    public static class ParameterModeExt
    {
        public static int GetRCodeConstant(this ParameterMode p)
        {
            return (int)p;
        }
        public static string GetName(this ParameterMode p)
        {
            return p.ToString().Replace('_', '-');
        }

        public static ParameterMode GetParameterMode(int type)
        {
            return Enum.IsDefined(typeof(ParameterMode), type) ? (ParameterMode)type : ParameterMode.INPUT;
        }
    }
}
