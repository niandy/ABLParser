using System;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABLParserTests.Prorefactor.Core
{
    [TestClass]
    public class NodeTypesTest
    {

        [TestMethod]
        public virtual void TestRange()
        {
            foreach (ABLNodeType type in ABLNodeType.Values)
            {
                Assert.IsTrue(type.Type >= -1000);
                // assertTrue(type.getType() != 0);
                Assert.IsTrue(type.Type < Proparse.Last_Token_Number);
            }
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: [TestMethod](enabled = false) public void generateKeywordList()
        [TestMethod]
        [Ignore()]
        public void GenerateKeywordList()
        {
            // Only for proparse.g
            foreach (ABLNodeType nodeType in ABLNodeType.Values)
            {
                if (nodeType.IsUnreservedKeywordType())
                {
                    Console.WriteLine(" | " + nodeType);
                }
            }
        }

    }
}
