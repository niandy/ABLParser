using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Refactor.Settings
{
    public interface IProparseSettings
    {

        bool MultiParse { get; }
        bool ProparseDirectives { get; }
        bool UseBackslashAsEscape();
        string Propath { get; }
        IList<string> PropathAsList { get; }

        bool BatchMode { get; }
        ABLParser.Prorefactor.Refactor.Settings.ProparseSettings.OperatingSystem OpSys { get; }
        string Proversion { get; }
        string WindowSystem { get; }
        int? ProcessArchitecture { get; }
        bool SkipXCode { get; }

        bool AllowAntlrTokenInsertion();
        bool AllowAntlrTokenDeletion();
        bool AllowAntlrRecover();
    }

}
