using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Core
{
    [Flags]
    public enum NodeTypesOption
    {
        NONE                = 0b0,
        PLACEHOLDER         = 0b1,
        NONPRINTABLE        = 0b10,
        PREPROCESSOR        = 0b100,
        STRUCTURE           = 0b1000,
        KEYWORD             = 0b10000,
        SYMBOL              = 0b100000,
        RESERVED            = 0b1000000,
        MAY_BE_REGULAR_FUNC = 0b10000000,
        MAY_BE_NO_ARG_FUNC  = 0b100000000,
        SYSHDL              = 0b1000000000
    }
}