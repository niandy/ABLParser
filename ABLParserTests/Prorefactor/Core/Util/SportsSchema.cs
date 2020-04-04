using ABLParser.Prorefactor.Core.Schema;
using System.Reflection;

namespace ABLParserTests.Prorefactor.Core.Util
{    
    class SportsSchema : Schema
    {
        public SportsSchema() : base("Resources/projects/sports2000/sports2000.cache", true)
        {
        }
    }
}
