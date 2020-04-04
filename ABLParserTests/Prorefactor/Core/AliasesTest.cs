using Ninject;
using System.IO;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ABLParserTests.Prorefactor.Core.Util;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class AliasesTest
    {

        private RefactorSession session;
        private ISchema schema;

        [TestInitialize()]
        public void Initialize() {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
            schema = session.Schema;
            schema.CreateAlias("dictdb", "sports2000");
            schema.CreateAlias("foo", "sports2000");
        }
        
        
        [TestMethod]
        public void TestMethod1()
        {            
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/data/aliases.p"), session);
            Assert.IsNull(unit.TopNode);
            Assert.IsNull(unit.RootScope);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);            
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.IsNotNull(schema.LookupDatabase("dictdb"));
            Assert.IsNotNull(schema.LookupDatabase("foo"));
            Assert.IsNull(schema.LookupDatabase("dictdb2"));
            Assert.IsNotNull(schema.LookupTable("_file"));
            Assert.IsNotNull(schema.LookupTable("dictdb", "_file"));
            Assert.IsNull(schema.LookupTable("dictdb", "_file2"));            
        }

        [TestMethod]
        public void TestMethod3()
        {
            Assert.IsNull(schema.LookupDatabase("test"));
            schema.CreateAlias("test", "sports2000");
            Assert.IsNotNull(schema.LookupDatabase("test"));
            Assert.IsNotNull(schema.LookupTable("test", "customer"));
            schema.DeleteAlias("test");
            Assert.IsNull(schema.LookupDatabase("test"));            
        }

        [TestMethod]
        public void TestMethod4()
        {
            Assert.IsNotNull(schema.LookupField("sports2000", "customer", "custnum"));
            Assert.IsNotNull(schema.LookupField("dictdb", "customer", "custnum"));
            Assert.IsNotNull(schema.LookupField("foo", "customer", "custnum"));           
        }

        [TestMethod]
        public void TestMethod5()
        {
            // Issue #27
            Assert.IsNotNull(schema.LookupTable("salesrep"), "Salesrep table exists");
            Assert.IsNull(schema.LookupTable("salesrepp"), "Typo, table doesn't exist");
            Assert.IsNull(schema.LookupTable("salesrep.salesrep"), "Table salesrep.salesrep doesn't exist (is a field)");

            Assert.IsNotNull(schema.LookupTable("_file"), "Table _file exists");
            Assert.IsNotNull(schema.LookupTable("foo._file"), "Table foo._file exists");
            Assert.IsNotNull(schema.LookupTable("sports2000._file"), "Table sports2000._file exists");
        }
    }
}
