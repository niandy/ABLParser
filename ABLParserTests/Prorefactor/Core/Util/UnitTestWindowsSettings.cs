using ABLParser.Prorefactor.Refactor.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABLParserTests.Prorefactor.Core.Util
{
    public class UnitTestWindowsSettings : ProparseSettings
    {
        public UnitTestWindowsSettings() : base(true, true, false, true, OperatingSystem.WINDOWS, "Resources,Resources/data", "11.7", 64, true)
        {
        }
    }

}
