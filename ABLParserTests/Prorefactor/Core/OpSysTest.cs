using System;
using System.IO;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class OpSysTest
    {
        private static readonly bool IS_WINDOWS = (Environment.GetEnvironmentVariable("windir") != null);
        private const string SRC_DIR = "Resources/data/bugsfixed";

        [TestMethod]
        public virtual void TestBackslashNoEscape()
        {
            // Backslash not considered an escape character on Windows, so it has to fail on Windows
            // UNIX test not executed
            if (!IS_WINDOWS)
            {
                return;
            }
            IKernel kernel = new StandardKernel(new UnitTestModule());
            RefactorSession session = kernel.Get<RefactorSession>();            
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "escape_char.p")), session);
            try
            {
                pu.Parse();
                Assert.Fail("Should have failed");
            }
            catch (ProparseRuntimeException)
            {

            }
        }

        [TestMethod]
        public virtual void TestBackslashEscape()
        {
            // Backslash considered an escape character on Windows, so it shouldn't fail on both Windows and Unix
            IKernel kernel = new StandardKernel(new UnitTestBackslashModule());
            RefactorSession session = kernel.Get<RefactorSession>();      
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "escape_char.p")), session);
            pu.Parse();
        }

        [TestMethod]
        public virtual void TestBackslashInIncludeWindows()
        {
            // Backslash considered an escape character on Windows, so include file will fail
            if (!IS_WINDOWS)
            {
                return;
            }

            IKernel kernel = new StandardKernel(new UnitTestBackslashModule());
            RefactorSession session = kernel.Get<RefactorSession>();
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "escape_char2.p")), session);
            try
            {
                pu.Parse();
                Assert.Fail("Should have failed");
            }
            catch (FileNotFoundException)
            {

            }
        }

        [TestMethod]
        public virtual void Test2BackslashInIncludeWindows()
        {
            // Backslash not considered an escape character on Windows, so include file is OK (standard behavior)
            if (!IS_WINDOWS)
            {
                return;
            }

            IKernel kernel = new StandardKernel(new UnitTestModule());
            RefactorSession session = kernel.Get<RefactorSession>();
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "escape_char2.p")), session);
            pu.Parse();
        }

        [TestMethod]
        public virtual void TestBackslashInIncludeLinux()
        {
            // Always fail on Linux
            if (IS_WINDOWS)
            {
                return;
            }

            IKernel kernel = new StandardKernel(new UnitTestModule());
            RefactorSession session = kernel.Get<RefactorSession>();
            ParseUnit pu = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "escape_char2.p")), session);
            try
            {
                pu.Parse();
                Assert.Fail("Should have failed");
            }
            catch (FileLoadException)
            {

            }
        }

    }
}
