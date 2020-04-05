using System;
using ABLParser.RCodeReader.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABLParserTests.RCodeReader
{
    [TestClass]
    public class DataTypeTest
    {
		[TestMethod]
		public virtual void Test1()
		{
			Assert.AreEqual(DataType.UNKNOWN, DataTypeExt.AsDataType(-1));
			Assert.AreEqual(DataType.VOID, DataTypeExt.AsDataType(0));
			Assert.AreEqual(DataType.RUNTYPE, DataTypeExt.AsDataType(48));
			Assert.AreEqual(DataType.UNKNOWN, DataTypeExt.AsDataType(49));
		}

		[TestMethod]
		public virtual void Test2()
		{
			Assert.AreEqual(DataType.UNKNOWN, DataTypeExt.AsDataType("-1"));
			Assert.AreEqual(DataType.VOID, DataTypeExt.AsDataType("0"));
			Assert.AreEqual(DataType.RUNTYPE, DataTypeExt.AsDataType("48"));
			Assert.AreEqual(DataType.UNKNOWN, DataTypeExt.AsDataType("49"));
			// Really ?
			Assert.AreEqual(DataType.CLASS, DataTypeExt.AsDataType(""));
			Assert.AreEqual(DataType.CLASS, DataTypeExt.AsDataType("Progress.Lang.Object"));
		}

	}
}
