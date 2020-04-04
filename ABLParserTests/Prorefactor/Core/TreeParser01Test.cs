using System;
using System.IO;
using ABLParser.Prorefactor.Refactor;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class TreeParser01Test
    {
        internal string expectName = "Resources/treeparser01-expect/test01.p";
        internal string inName = "Resources/treeparser01/test01.p";
        internal FileInfo outFile = new FileInfo("Resources/target/test-temp/treeparser01/test01.p");

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void test01() throws IOException
        [TestMethod]
        public virtual void Test01()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            RefactorSession session = kernel.Get<RefactorSession>();

            outFile.Directory.Create();            

            AttributedWriter writer = new AttributedWriter();
            writer.Write(inName, outFile, session);
            Assert.IsTrue(Resources.FilesContentEquals(new FileInfo(expectName), outFile));
        }

    }
}
