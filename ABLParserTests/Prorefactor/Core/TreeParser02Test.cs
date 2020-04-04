using System;
using System.IO;
using ABLParser.Prorefactor.Refactor;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class TreeParser02Test
    {
        private RefactorSession session;

        private const string SOURCEDIR = "Resources/treeparser02";
        private const string TARGETDIR = "Resources/target/test-temp/treeparser02";
        private const string EXPECTDIR = "Resources/treeparser02-expect";

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
            session.Schema.CreateAlias("foo", "sports2000");
            Directory.CreateDirectory(TARGETDIR);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test01() throws IOException
        [TestMethod]
        public virtual void Test01()
        {
            GenericTest("test01.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test02() throws IOException
        [TestMethod]
        public virtual void Test02()
        {
            GenericTest("test02.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test03() throws IOException
        [TestMethod]
        public virtual void Test03()
        {
            GenericTest("test03.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test04() throws IOException
        [TestMethod]
        public virtual void Test04()
        {
            GenericTest("test04.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test05() throws IOException
        [TestMethod]
        public virtual void Test05()
        {
            GenericTest("test05.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test06() throws IOException
        [TestMethod]
        public virtual void Test06()
        {
            GenericTest("test06.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test07() throws IOException
        [TestMethod]
        public virtual void Test07()
        {
            GenericTest("test07.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test08() throws IOException
        [TestMethod]
        public virtual void Test08()
        {
            GenericTest("test08.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test09() throws IOException
        [TestMethod]
        public virtual void Test09()
        {
            GenericTest("test09.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test10() throws IOException
        [TestMethod]
        public virtual void Test10()
        {
            GenericTest("test10.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test11() throws IOException
        [TestMethod]
        public virtual void Test11()
        {
            GenericTest("test11.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test12() throws IOException
        [TestMethod]
        public virtual void Test12()
        {
            GenericTest("test12.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test13() throws IOException
        [TestMethod]
        public virtual void Test13()
        {
            GenericTest("test13.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test14() throws IOException
        [TestMethod]
        public virtual void Test14()
        {
            GenericTest("test14.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test15() throws IOException
        [TestMethod]
        public virtual void Test15()
        {
            GenericTest("test15.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test16() throws IOException
        [TestMethod]
        public virtual void Test16()
        {
            GenericTest("test16.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test17() throws IOException
        [TestMethod]
        public virtual void Test17()
        {
            GenericTest("test17.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test18() throws IOException
        [TestMethod]
        public virtual void Test18()
        {
            GenericTest("test18.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test19() throws IOException
        [TestMethod]
        public virtual void Test19()
        {
            GenericTest("test19.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test20() throws IOException
        [TestMethod]
        public virtual void Test20()
        {
            GenericTest("test20.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test21() throws IOException
        [TestMethod]
        public virtual void Test21()
        {
            GenericTest("test21.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test22() throws IOException
        [TestMethod]
        public virtual void Test22()
        {
            GenericTest("test22.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test23() throws IOException
        [TestMethod]
        public virtual void Test23()
        {
            GenericTest("test23.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test24() throws IOException
        [TestMethod]
        public virtual void Test24()
        {
            GenericTest("test24.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test25() throws IOException
        [TestMethod]
        public virtual void Test25()
        {
            GenericTest("test25.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test26() throws IOException
        [TestMethod]
        public virtual void Test26()
        {
            GenericTest("test26.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test27() throws IOException
        [TestMethod]
        public virtual void Test27()
        {
            GenericTest("test27.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test28() throws IOException
        [TestMethod]
        public virtual void Test28()
        {
            GenericTest("test28.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test29() throws IOException
        [TestMethod]
        public virtual void Test29()
        {
            GenericTest("test29.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test30() throws IOException
        [TestMethod]
        public virtual void Test30()
        {
            GenericTest("test30.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test31() throws IOException
        [TestMethod]
        public virtual void Test31()
        {
            GenericTest("test31.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test32() throws IOException
        [TestMethod]
        public virtual void Test32()
        {
            GenericTest("test32.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test33() throws IOException
        [TestMethod]
        public virtual void Test33()
        {
            GenericTest("test33.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test34() throws IOException
        [TestMethod]
        public virtual void Test34()
        {
            GenericTest("test34.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void Test35() throws IOException
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        [TestMethod]
        [Ignore()]
        public void Test35()
        {
            GenericTest("test35.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void Test36() throws IOException
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        [TestMethod]
        [Ignore()]
        public void Test36()
        {
            GenericTest("test36.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test37() throws IOException
        [TestMethod]
        public virtual void Test37()
        {
            GenericTest("test37.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test38() throws IOException
        [TestMethod]
        public virtual void Test38()
        {
            GenericTest("test38.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test39() throws IOException
        [TestMethod]
        public virtual void Test39()
        {
            GenericTest("test39.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test40() throws IOException
        [TestMethod]
        public virtual void Test40()
        {
            GenericTest("test40.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test41() throws IOException
        [TestMethod]
        public virtual void Test41()
        {
            GenericTest("test41.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test42() throws IOException
        [TestMethod]
        public virtual void Test42()
        {
            GenericTest("test42.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test43() throws IOException
        [TestMethod]
        public virtual void Test43()
        {
            GenericTest("test43.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test44() throws IOException
        [TestMethod]
        public virtual void Test44()
        {
            GenericTest("test44.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test45() throws IOException
        [TestMethod]
        public virtual void Test45()
        {
            GenericTest("test45.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test46() throws IOException
        [TestMethod]
        public virtual void Test46()
        {
            GenericTest("test46.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test47() throws IOException
        [TestMethod]
        public virtual void Test47()
        {
            GenericTest("test47.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test48() throws IOException
        [TestMethod]
        public virtual void Test48()
        {
            GenericTest("test48.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void Test49() throws IOException
        [TestMethod]
        public virtual void Test49()
        {
            GenericTest("test49.p");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private void genericTest(String name) throws IOException
        private void GenericTest(string name)
        {
            AttributedWriter writer = new AttributedWriter();
            writer.Write(Path.Combine(SOURCEDIR, name), new FileInfo(Path.Combine(TARGETDIR, name)), session);
            Assert.IsTrue(Resources.FilesContentEquals(new FileInfo(Path.Combine(EXPECTDIR, name)), new FileInfo(Path.Combine(TARGETDIR, name))));
        }


    }
}
