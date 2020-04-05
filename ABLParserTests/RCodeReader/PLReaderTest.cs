using System;
using System.IO;
using ABLParser.RCodeReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABLParserTests.RCodeReader
{
    [TestClass]
    public class PLReaderTest
    {
		//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		//ORIGINAL LINE: [TestMethod] public void testRCodeInPL() throws IOException, InvalidRCodeException
		[TestMethod]
		public virtual void TestRCodeInPL()
		{
			PLReader pl = new PLReader(new FileInfo("Resources/ablunit.pl"));
			Assert.IsNotNull(pl.GetEntry("OpenEdge/ABLUnit/Reflection/ClassAnnotationInfo.r"));
			RCodeInfo rci = new RCodeInfo(new BinaryReader(pl.GetInputStream(pl.GetEntry("OpenEdge/ABLUnit/Reflection/ClassAnnotationInfo.r"))));
			Assert.IsTrue(rci.Class);
			Assert.IsTrue(rci.TypeInfo.Methods.Count > 0);
			Assert.IsTrue(rci.TypeInfo.Properties.Count > 0);
			Assert.IsTrue(rci.TypeInfo.Tables.Count == 0);
		}

	}
}
