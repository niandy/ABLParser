using System;
using System.IO;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class TreeParser04Test
    {

        internal string expectName = "Resources/treeparser04-expect/frames.p";
        internal string inName = "Resources/target/test-temp/treeparser04/frames.p";
        internal FileInfo outFile = new FileInfo("Resources/treeparser04/frames.p");

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void test01() throws IOException
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        [TestMethod]
        [Ignore()]
        public void Test01()
        {
            // TODO Re-enable test when FrameStack implementation is 100% identical to previous one
            IKernel kernel = new StandardKernel(new UnitTestModule());
            RefactorSession session = kernel.Get<RefactorSession>();

            outFile.Directory.Create();

            ParseUnit pu = new ParseUnit(new FileInfo(inName), session);
            pu.TreeParser01();

            StreamWriter writer = new StreamWriter(outFile.OpenWrite());
            JPNodeLister nodeLister = new TP01FramesTreeLister(pu.TopNode, writer);
            nodeLister.Print(' ');
            writer.Close();

            Assert.IsTrue(Resources.FilesContentEquals(new FileInfo(expectName), outFile));
        }

    }
}
