using System;
using System.IO;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.RCodeReader;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class NewSyntaxTest
    {
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
            
            session.InjectTypeInfo(
                new RCodeInfo(new BinaryReader(new FileStream("Resources/data/newsyntax/101b/deep/FindMe.r", FileMode.Open))).TypeInfo);
            session.InjectTypeInfo(
                new RCodeInfo(new BinaryReader(new FileStream("Resources/data/newsyntax/101b/Test1.r", FileMode.Open))).TypeInfo);
        }

        private void TestNewSyntax(string file)
        {
            // Just run the TreeParser to see if file can be parsed without error
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine("Resources/data/newsyntax", file)), session);
            pu.TreeParser01();
        }

        [TestMethod]
        public virtual void Test01()
        {
            TestNewSyntax("101b/Test1.cls");
        }

        [TestMethod]
        public virtual void Test02()
        {
            TestNewSyntax("101b/Test2.cls");
        }

        [TestMethod]
        public virtual void Test03()
        {
            TestNewSyntax("101b/deep/FindMe.cls");
        }

        [TestMethod]
        public virtual void Test04()
        {
            TestNewSyntax("101c102a/test01.p");
        }

        [TestMethod]
        public virtual void Test05()
        {
            TestNewSyntax("101c102a/Test02.cls");
        }

        [TestMethod]
        public virtual void Test06()
        {
            TestNewSyntax("102b/ClassDecl.cls");
        }

        [TestMethod]
        public virtual void Test07()
        {
            TestNewSyntax("102b/CustomException.cls");
        }

        [TestMethod]
        public virtual void Test08()
        {
            TestNewSyntax("102b/Display.cls");
        }

        [TestMethod]
        public virtual void Test09()
        {
            TestNewSyntax("102b/DisplayTest.p");
        }

        [TestMethod]
        public virtual void Test10()
        {
            TestNewSyntax("102b/ExtentTest.cls");
        }

        [TestMethod]
        public virtual void Test11()
        {
            TestNewSyntax("102b/IEmpty.cls");
        }

        [TestMethod]
        public virtual void Test12()
        {
            TestNewSyntax("102b/ITest.cls");
        }

        [TestMethod]
        public virtual void Test13()
        {
            TestNewSyntax("102b/Kernel.cls");
        }

        [TestMethod]
        public virtual void Test14()
        {
            TestNewSyntax("102b/KeywordMethodName.cls");
        }

        [TestMethod]
        public virtual void Test15()
        {
            TestNewSyntax("102b/r-CustObj.cls");
        }

        [TestMethod]
        public virtual void Test16()
        {
            TestNewSyntax("102b/r-CustObjAbstract.cls");
        }

        [TestMethod]
        public virtual void Test17()
        {
            TestNewSyntax("102b/r-CustObjAbstractImpl.cls");
        }

        [TestMethod]
        public virtual void Test18()
        {
            TestNewSyntax("102b/r-CustObjAbstractProc.p");
        }

        [TestMethod]
        public virtual void Test19()
        {
            TestNewSyntax("102b/r-CustObjProc.p");
        }

        [TestMethod]
        public virtual void Test20()
        {
            TestNewSyntax("102b/r-CustObjStatic.cls");
        }

        [TestMethod]
        public virtual void Test21()
        {
            TestNewSyntax("102b/r-CustObjStaticProc.p");
        }

        [TestMethod]
        public virtual void Test22()
        {
            TestNewSyntax("102b/r-DefineProperties1.cls");
        }

        [TestMethod]
        public virtual void Test23()
        {
            TestNewSyntax("102b/r-DefineProperties2.cls");
        }

        [TestMethod]
        public virtual void Test24()
        {
            TestNewSyntax("102b/r-EventPublish.cls");
        }

        [TestMethod]
        public virtual void Test25()
        {
            TestNewSyntax("102b/r-EventPubSub.p");
        }

        [TestMethod]
        public virtual void Test26()
        {
            TestNewSyntax("102b/r-EventSubscribe.cls");
        }

        [TestMethod]
        public virtual void Test27()
        {
            TestNewSyntax("102b/r-ICustObj.cls");
        }

        [TestMethod]
        public virtual void Test28()
        {
            TestNewSyntax("102b/r-ICustObjImpl.cls");
        }

        [TestMethod]
        public virtual void Test29()
        {
            TestNewSyntax("102b/r-ICustObjImpl2.cls");
        }

        [TestMethod]
        public virtual void Test30()
        {
            TestNewSyntax("102b/r-ICustObjProc.p");
        }

        [TestMethod]
        public virtual void Test31()
        {
            TestNewSyntax("102b/r-ICustObjProc2.p");
        }

        [TestMethod]
        public virtual void Test32()
        {
            TestNewSyntax("102b/Settings.cls");
        }

        [TestMethod]
        public virtual void Test33()
        {
            TestNewSyntax("102b/stopafter.p");
        }

        [TestMethod]
        public virtual void Test34()
        {
            TestNewSyntax("102b/type_names.p");
        }

        [TestMethod]
        public virtual void Test35()
        {
            TestNewSyntax("11.4/BaseInterface.cls");
        }

        [TestMethod]
        public virtual void Test36()
        {
            TestNewSyntax("11.4/ExtendedInterface.cls");
        }

        [TestMethod]
        public virtual void Test37()
        {
            TestNewSyntax("11.4/getclass.p");
        }

        [TestMethod]
        public virtual void Test38()
        {
            TestNewSyntax("11.4/SerializableClass.cls");
        }

        [TestMethod]
        public virtual void Test39()
        {
            TestNewSyntax("11.4/StreamHandleClass.cls");
        }

        [TestMethod]
        public virtual void Test40()
        {
            TestNewSyntax("11.4/StreamHandleClass2.cls");
        }

        [TestMethod]
        public virtual void Test41()
        {
            TestNewSyntax("11.6/singlelinecomment.p");
        }

        [TestMethod]
        public virtual void Test42()
        {
            TestNewSyntax("11n/Class01.cls");
        }

        [TestMethod]
        public virtual void Test43()
        {
            TestNewSyntax("11n/Class02.cls");
        }

        [TestMethod]
        public virtual void Test44()
        {
            TestNewSyntax("prolint/regrtest-oo/test1.cls");
        }

        [TestMethod]
        public virtual void Test45()
        {
            TestNewSyntax("prolint/regrtest-oo/test2.cls");
        }

        [TestMethod]
        public virtual void Test46()
        {
            TestNewSyntax("prolint/regrtest-oo/test3.cls");
        }

        [TestMethod]
        public virtual void Test47()
        {
            TestNewSyntax("prolint/regrtest-oo/test4.cls");
        }

        [TestMethod]
        public virtual void Test48()
        {
            TestNewSyntax("prolint/regrtest-oo/test5.cls");
        }

        [TestMethod]
        public virtual void Test49()
        {
            TestNewSyntax("prolint/regrtest-oo/test6.cls");
        }

        [TestMethod]
        public virtual void Test50()
        {
            TestNewSyntax("11n/Class03.cls");
        }

        [TestMethod]
        public virtual void Test51()
        {
            TestNewSyntax("11n/ParameterHandleTo.p");
        }

        [TestMethod]
        public virtual void TestTenantKeywords()
        {
            TestNewSyntax("11n/tenant.p");
        }

    }
}
