using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Refactor.Settings;
using Ninject.Modules;
using System;

namespace ABLParserTests.Prorefactor.Core.Util
{
    public class UnitTestModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISchema>().To<SportsSchema>();
            this.Bind<IProparseSettings>().To<UnitTestSettings>();
        }
    }
}
