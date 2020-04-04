using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Refactor.Settings;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABLParserTests.Prorefactor.Core.Util
{    
    public class UnitTestBackslashModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISchema>().To<SportsSchema>();
            this.Bind<IProparseSettings>().To<UnitTestBackslashProparseSettings>();
        }
    }
}
