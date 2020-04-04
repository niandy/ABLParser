using ABLParser.Prorefactor.Refactor.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABLParserTests.Prorefactor.Core.Util
{
    public class UnitTestBackslashProparseSettings : ProparseSettings
    {
        public UnitTestBackslashProparseSettings() : base("Resources/data", true)
        {
        }
    }
}
