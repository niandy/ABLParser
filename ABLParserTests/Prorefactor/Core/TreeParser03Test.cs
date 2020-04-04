using System;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParserTests.Prorefactor.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class TreeParser03Test
    {
        private RefactorSession session;

        [TestInitialize()]
        public void Initialize()
        {
            IKernel kernel = new StandardKernel(new UnitTestModule());
            session = kernel.Get<RefactorSession>();
        }

        [TestMethod]
        public virtual void Test01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test01.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
        }

        [TestMethod]
        public virtual void Test02()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test02.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
        }

        [TestMethod]
        public virtual void TestTreeParser01()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test03.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);

            bool found1 = false;
            bool found2 = false;
            foreach (JPNode node in unit.TopNode.Query(ABLNodeType.DEFINE))
            {
                if ((node.State2 == ABLNodeType.TEMPTABLE.Type) && "myTT2".Equals(node.NextNode.NextNode.Text))
                {
                    Assert.AreEqual(IConstants.TRUE, node.Query(ABLNodeType.USEINDEX)[0].NextNode.AttrGet(IConstants.INVALID_USEINDEX));
                    found1 = true;
                }
                if ((node.State2 == ABLNodeType.TEMPTABLE.Type) && "myTT3".Equals(node.NextNode.NextNode.Text))
                {
                    Assert.AreEqual(0, node.Query(ABLNodeType.USEINDEX)[0].NextNode.AttrGet(IConstants.INVALID_USEINDEX));
                    found2 = true;
                }
            }
            Assert.IsTrue(found1);
            Assert.IsTrue(found2);
        }

        [TestMethod]
        public virtual void Test04()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test04.cls"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Variable xx = unit.RootScope.GetVariable("xx");
            Assert.IsNotNull(xx);
            Variable yy = unit.RootScope.GetVariable("yy");
            Assert.IsNotNull(yy);
            Variable zz = unit.RootScope.GetVariable("zz");
            Assert.IsNotNull(zz);
        }

        [TestMethod]
        public virtual void Test05()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test05.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);

            Routine f1 = unit.RootScope.LookupRoutine("f1");
            Assert.IsNotNull(f1);
            Assert.AreEqual(1, f1.Parameters.Count);
            Assert.AreEqual("zz", f1.Parameters[0].Symbol.Name);
            Assert.AreEqual(1, f1.Parameters[0].Symbol.NumReads);
            Assert.AreEqual(0, f1.Parameters[0].Symbol.NumWrites);

            Routine f2 = unit.RootScope.LookupRoutine("f2");
            Assert.IsNotNull(f2);
            Assert.AreEqual(2, f2.Parameters.Count);
            Assert.AreEqual("a", f2.Parameters[0].Symbol.Name);
            Assert.AreEqual(0, f2.Parameters[0].Symbol.NumReads);
            Assert.AreEqual(0, f2.Parameters[0].Symbol.NumWrites);
            Assert.AreEqual("zz", f2.Parameters[1].Symbol.Name);
            Assert.AreEqual(1, f2.Parameters[1].Symbol.NumReads);
            Assert.AreEqual(0, f2.Parameters[1].Symbol.NumWrites);

            Routine f3 = unit.RootScope.LookupRoutine("f3");
            Assert.IsNotNull(f3);
            Assert.AreEqual(1, f3.Parameters.Count);
            Assert.AreEqual("a", f3.Parameters[0].Symbol.Name);
            Assert.AreEqual(1, f3.Parameters[0].Symbol.NumReads);
            Assert.AreEqual(0, f3.Parameters[0].Symbol.NumWrites);

            Routine f4 = unit.RootScope.LookupRoutine("f4");
            Assert.IsNotNull(f4);
            Assert.AreEqual(0, f4.Parameters.Count);

            Routine f5 = unit.RootScope.LookupRoutine("f5");
            Assert.IsNotNull(f5);
            Assert.AreEqual(0, f5.Parameters.Count);
        }

        [TestMethod]
        public virtual void Test06()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test06.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
        }

        [TestMethod]
        public virtual void Test07()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test07.cls"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Variable prop = unit.RootScope.GetVariable("cNextSalesRepName");
            Assert.IsNotNull(prop);
            Assert.AreEqual(1, prop.NumReads);
            Assert.AreEqual(0, prop.NumWrites);
        }



        [TestMethod]
        public virtual void Test08()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test08.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Variable xx = unit.RootScope.ChildScopes[0].GetVariable("xx");
            Assert.IsNotNull(xx);
            Assert.AreEqual(1, xx.NumReads);
            Assert.AreEqual(0, xx.NumWrites);
        }

        [TestMethod]
        public virtual void Test09()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test09.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Variable xxx = unit.RootScope.GetVariable("xxx");
            Assert.IsNotNull(xxx);
        }

        [TestMethod]
        public virtual void Test11()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test11.cls"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Routine r1 = unit.RootScope.RoutineMap["foo1"];
            Assert.IsNotNull(r1);
            Assert.AreEqual(1, r1.Parameters.Count);
            Parameter p1 = r1.Parameters[0];
            Symbol s1 = p1.Symbol;
            Assert.IsNotNull(s1);
            Assert.AreEqual("ipPrm", s1.Name);
            Assert.IsTrue(s1 is Variable);
            Assert.AreEqual(DataType.INTEGER, ((Variable)s1).DataType);
            Assert.IsNotNull(s1.DefineNode);
        }

        [TestMethod]
        public virtual void Test10()
        {
            ParseUnit unit = new ParseUnit(new MemoryStream(Encoding.Default.GetBytes("define input parameter ipPrm no-undo like customer.custnum.")), "<unnamed>", session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Variable ipPrm = unit.RootScope.GetVariable("ipPrm");
            Assert.IsNotNull(ipPrm);
            Assert.AreEqual(DataType.INTEGER, ipPrm.DataType);
        }

        [TestMethod]
        public virtual void Test12()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test12.cls"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Routine r1 = unit.RootScope.RoutineMap["foo1"];
            Assert.AreEqual(DataType.CLASS, r1.ReturnDatatypeNode);
            Routine r2 = unit.RootScope.RoutineMap["foo2"];
            Assert.AreEqual(DataType.CLASS, r2.ReturnDatatypeNode);
            Routine r3 = unit.RootScope.RoutineMap["foo3"];
            Assert.AreEqual(DataType.INTEGER, r3.ReturnDatatypeNode);
            Routine r4 = unit.RootScope.RoutineMap["foo4"];
            Assert.AreEqual(DataType.CHARACTER, r4.ReturnDatatypeNode);
        }

        [TestMethod]
        public virtual void Test13()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test13.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);
            Variable xxx = unit.RootScope.GetVariable("xxx");
            Assert.IsNotNull(xxx);
            Assert.AreEqual(1, xxx.NumReads);
            Assert.AreEqual(0, xxx.NumWrites);
            Variable yyy = unit.RootScope.GetVariable("yyy");
            Assert.IsNotNull(yyy);
            Assert.AreEqual(0, yyy.NumReads);
            Assert.AreEqual(1, yyy.NumWrites);
        }

        [TestMethod]
        public virtual void Test14()
        {
            ParseUnit unit = new ParseUnit(new FileInfo("Resources/treeparser03/test14.p"), session);
            Assert.IsNull(unit.TopNode);
            unit.TreeParser01();
            Assert.IsNotNull(unit.TopNode);
            Assert.IsNotNull(unit.RootScope);

            Variable xxx = unit.RootScope.GetVariable("xxx");
            Assert.IsNotNull(xxx);
            Assert.AreEqual(1, xxx.NumReads);
            Assert.AreEqual(3, xxx.NumWrites);
        }

    }
}
