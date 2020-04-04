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
    public class PreprocessorParserTest
    {
        private const string SRC_DIR = "Resources/data/preprocessor";
        private RefactorSession session;
        private ParseUnit unit;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();

            //try
            {
                unit = new ParseUnit(new FileInfo(Path.Combine(SRC_DIR, "preprocessor01.p")), session);
                unit.Parse();
            }
            //catch (Exception e)
            //{
                // Just so that tests will throw NPE and fail (and not just be skipped)
            //    unit = null;
            //}
        }

        private void TestVariable(JPNode topNode, string variable)
        {
            foreach (JPNode node in topNode.Query(ABLNodeType.ID))
            {
                if (node.Text.Equals(variable))
                {
                    return;
                }
            }
            Assert.Fail("Variable " + variable + " not found");
        }

        private void TestNoVariable(JPNode topNode, string variable)
        {
            foreach (JPNode node in topNode.Query(ABLNodeType.ID))
            {
                if (node.Text.Equals(variable))
                {
                    Assert.Fail("Variable " + variable + " not found");
                }
            }
        }

        [TestMethod]
        public virtual void TestTrue()
        {
            TestVariable(unit.TopNode, "var1");
        }

        [TestMethod]
        public virtual void TestFalse()
        {
            TestVariable(unit.TopNode, "var2");
        }

        [TestMethod]
        public virtual void TestGT()
        {
            TestVariable(unit.TopNode, "var3");
        }

        [TestMethod]
        public virtual void TestLT()
        {
            TestVariable(unit.TopNode, "var4");
        }

        [TestMethod]
        public virtual void TestAnd()
        {
            TestNoVariable(unit.TopNode, "var5");
        }

        [TestMethod]
        public virtual void TestOr1()
        {
            TestVariable(unit.TopNode, "var6");
            TestVariable(unit.TopNode, "var46");
        }

        [TestMethod]
        public virtual void TestExpr1()
        {
            TestVariable(unit.TopNode, "var7");
        }

        [TestMethod]
        public virtual void TestDefined()
        {
            TestVariable(unit.TopNode, "var8");
        }

        [TestMethod]
        public virtual void TestDefined2()
        {
            TestVariable(unit.TopNode, "var9");
        }

        [TestMethod]
        public virtual void TestDefined3()
        {
            TestNoVariable(unit.TopNode, "var10");
        }

        [TestMethod]
        public virtual void TestExpression1()
        {
            TestVariable(unit.TopNode, "var11");
        }

        [TestMethod]
        public virtual void TestExpression2()
        {
            TestVariable(unit.TopNode, "var12");
        }

        [TestMethod]
        public virtual void TestGreaterEquals1()
        {
            TestVariable(unit.TopNode, "var13");
        }

        [TestMethod]
        public virtual void TestGreaterEquals2()
        {
            TestNoVariable(unit.TopNode, "var14");
        }

        [TestMethod]
        public virtual void TestGreaterEquals3()
        {
            TestVariable(unit.TopNode, "var44");
        }

        [TestMethod]
        public virtual void TestGreaterEquals4()
        {
            TestNoVariable(unit.TopNode, "var45");
        }

        [TestMethod]
        public virtual void TestLesserEquals1()
        {
            TestVariable(unit.TopNode, "var15");
        }

        [TestMethod]
        public virtual void TestLesserEquals2()
        {
            TestNoVariable(unit.TopNode, "var16");
        }

        [TestMethod]
        public virtual void TestLesserEquals3()
        {
            TestVariable(unit.TopNode, "var42");
        }

        [TestMethod]
        public virtual void TestLesserEquals4()
        {
            TestNoVariable(unit.TopNode, "var43");
        }

        [TestMethod]
        public virtual void TestSubstring()
        {
            TestVariable(unit.TopNode, "var17");
        }

        [TestMethod]
        public virtual void TestSubstring2()
        {
            TestNoVariable(unit.TopNode, "var18");
        }
        [TestMethod]
        public virtual void TestExpression3()
        {
            TestVariable(unit.TopNode, "var19");
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void TestAbsolute1()
        [TestMethod]
        [Ignore()]
        public void TestAbsolute1()
        {
            TestVariable(unit.TopNode, "var20");
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod][Ignore()] public void TestAbsolute2()
        [TestMethod]
        [Ignore()]
        public void TestAbsolute2()
        {
            TestVariable(unit.TopNode, "var21");
        }

        [TestMethod]
        public virtual void TestDecimal1()
        {
            TestVariable(unit.TopNode, "var22");
        }

        [TestMethod]
        public virtual void TestEntry1()
        {
            TestVariable(unit.TopNode, "var23");
        }

        [TestMethod]
        public virtual void TestEntry2()
        {
            TestVariable(unit.TopNode, "var24");
        }

        [TestMethod]
        public virtual void TestEntry3()
        {
            TestNoVariable(unit.TopNode, "var25");
        }

        [TestMethod]
        public virtual void TestEntry4()
        {
            TestVariable(unit.TopNode, "var66");
            TestNoVariable(unit.TopNode, "var67");
        }

        [TestMethod]
        public virtual void TestEntry5()
        {
            TestVariable(unit.TopNode, "var68");
            TestNoVariable(unit.TopNode, "var69");
        }

        [TestMethod]
        public virtual void TestIndex1()
        {
            TestVariable(unit.TopNode, "var26");
        }

        [TestMethod]
        public virtual void TestInteger1()
        {
            TestVariable(unit.TopNode, "var27");
        }

        [TestMethod]
        public virtual void TestInt641()
        {
            TestVariable(unit.TopNode, "var28");
        }

        // TODO KEYWORD and KEYWORD-ALL

        [TestMethod]
        public virtual void TestLeftTrim1()
        {
            TestVariable(unit.TopNode, "var29");
        }

        [TestMethod]
        public virtual void TestLength1()
        {
            TestVariable(unit.TopNode, "var30");
        }

        [TestMethod]
        public virtual void TestLookup1()
        {
            TestVariable(unit.TopNode, "var31");
        }

        [TestMethod]
        public virtual void TestMaximum1()
        {
            TestVariable(unit.TopNode, "var32");
        }

        [TestMethod]
        public virtual void TestMinimum1()
        {
            TestVariable(unit.TopNode, "var33");
        }

        [TestMethod]
        public virtual void TestNumEntries1()
        {
            TestVariable(unit.TopNode, "var34");
        }

        [TestMethod]
        public virtual void TestRIndex1()
        {
            TestVariable(unit.TopNode, "var35");
        }

        [TestMethod]
        public virtual void TestReplace1()
        {
            TestVariable(unit.TopNode, "var36");
        }

        [TestMethod]
        public virtual void TestRightTrim1()
        {
            TestVariable(unit.TopNode, "var37");
        }

        [TestMethod]
        public virtual void TestSubstring1()
        {
            TestVariable(unit.TopNode, "var38");
        }

        [TestMethod]
        public virtual void TestTrim1()
        {
            TestVariable(unit.TopNode, "var39");
        }

        [TestMethod]
        public virtual void TestSubstring3()
        {
            TestVariable(unit.TopNode, "var40");
        }

        [TestMethod]
        public virtual void TestSubstring4()
        {
            TestVariable(unit.TopNode, "var41");
        }

        [TestMethod]
        public virtual void TestMatches()
        {
            TestVariable(unit.TopNode, "var47");
            TestNoVariable(unit.TopNode, "var48");
        }

        [TestMethod]
        public virtual void TestBegins()
        {
            TestVariable(unit.TopNode, "var49");
            TestNoVariable(unit.TopNode, "var50");
        }

        [TestMethod]
        public virtual void TestNotEquals()
        {
            TestVariable(unit.TopNode, "var51");
            TestNoVariable(unit.TopNode, "var52");
        }

        [TestMethod]
        public virtual void TestNot()
        {
            TestVariable(unit.TopNode, "var53");
        }

        [TestMethod]
        public virtual void TestUnaryMinus()
        {
            TestVariable(unit.TopNode, "var54");
            TestVariable(unit.TopNode, "var55");
            TestNoVariable(unit.TopNode, "var56");
        }

        [TestMethod]
        public virtual void TestUnknown()
        {
            TestNoVariable(unit.TopNode, "var57");
            TestVariable(unit.TopNode, "var58");
        }

        [TestMethod]
        public virtual void TestDbType()
        {
            TestVariable(unit.TopNode, "var59");
            TestNoVariable(unit.TopNode, "var60");
        }

        [TestMethod]
        public virtual void TestAndOrNotImplemented()
        {
            TestNoVariable(unit.TopNode, "var61");
            TestNoVariable(unit.TopNode, "var62");
        }

        [TestMethod]
        public virtual void TestIfElseIf()
        {
            TestNoVariable(unit.TopNode, "var63");
            TestVariable(unit.TopNode, "var64");
        }

        [TestMethod]
        public virtual void TestBugIf()
        {
            TestVariable(unit.TopNode, "var65");
        }

        [TestMethod]
        public virtual void TestPriority()
        {
            TestVariable(unit.TopNode, "var70");
            TestNoVariable(unit.TopNode, "var71");
            TestVariable(unit.TopNode, "var72");
            TestNoVariable(unit.TopNode, "var73");
            TestVariable(unit.TopNode, "var74");
        }

        [TestMethod]
        public virtual void TestSubstring5()
        {
            TestVariable(unit.TopNode, "var75");
        }

        // TODO OPSYS, PROVERSION functions
        [TestMethod]
        public virtual void TestPropath()
        {
            TestVariable(unit.TopNode, "var76");
        }

        // TODO OPSYS, PROVERSION functions
        [TestMethod]
        public virtual void TestPropath2()
        {
            TestNoVariable(unit.TopNode, "var77");
        }

        [TestMethod]
        public virtual void TestProcessArchitecture1()
        {
            TestNoVariable(unit.TopNode, "var78");
        }

        [TestMethod]
        public virtual void TestProcessArchitecture2()
        {
            TestVariable(unit.TopNode, "var79");
        }

    }
}
