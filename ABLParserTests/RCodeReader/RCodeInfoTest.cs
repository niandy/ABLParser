using System;
using System.IO;
using System.Linq;
using ABLParser.RCodeReader;
using ABLParser.RCodeReader.Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ABLParser.RCodeReader.RCodeInfo;

namespace ABLParserTests.RCodeReader
{
    [TestClass]
    public class RCodeInfoTest
    {

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestEnum() throws IOException
        [TestMethod]
        public virtual void TestEnum()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/MyEnum.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestInterface() throws IOException
        [TestMethod]
        public virtual void TestInterface()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/IMyTest.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestClass() throws IOException
        [TestMethod]
        public virtual void TestClass()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/BackupDataCallback.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestClass2() throws IOException
        [TestMethod]
        public virtual void TestClass2()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/propList.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                    ITypeInfo info = rci.TypeInfo;
                    Assert.IsNotNull(info);
                    Assert.IsNotNull(info.Properties);
                    Assert.AreEqual(6, info.Properties.Count);

                    IPropertyElement prop1 = info.GetProperty("prop1");
                    Assert.IsNotNull(prop1);
                    Assert.IsTrue(prop1.Public);

                    IPropertyElement prop2 = info.GetProperty("prop2");
                    Assert.IsNotNull(prop2);
                    Assert.IsTrue(prop2.Private);

                    IPropertyElement prop3 = info.GetProperty("prop3");
                    Assert.IsNotNull(prop3);
                    Assert.IsTrue(prop3.Public);

                    IPropertyElement prop4 = info.GetProperty("prop4");
                    Assert.IsNotNull(prop4);
                    Assert.IsTrue(prop4.Protected);

                    IPropertyElement prop5 = info.GetProperty("prop5");
                    Assert.IsNotNull(prop5);
                    Assert.IsTrue(prop5.Protected);
                    Assert.IsTrue(prop5.Abstract);

                    IPropertyElement prop6 = info.GetProperty("prop6");
                    Assert.IsNotNull(prop6);
                    Assert.IsTrue(prop6.Public);
                    Assert.IsTrue(prop6.Static);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestClass3() throws IOException
        [TestMethod]
        public virtual void TestClass3()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/ttClass.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestClassMinSize() throws IOException
        [TestMethod]
        public virtual void TestClassMinSize()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/ClassMinSize.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                    Assert.AreEqual(2, rci.TypeInfo.Properties.Count);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestProcedure() throws IOException
        [TestMethod]
        public virtual void TestProcedure()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/compile.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsFalse(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestProcedure2() throws IOException
        [TestMethod]
        public virtual void TestProcedure2()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/AbstractTTCollection.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestProcedure3() throws IOException
        [TestMethod]
        public virtual void TestProcedure3()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/FileTypeRegistry.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestProcedure4() throws IOException
        [TestMethod]
        public virtual void TestProcedure4()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/_dmpincr.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsFalse(rci.Class);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestV11() throws IOException
        [TestMethod]
        public virtual void TestV11()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/WebRequestV11.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                    Assert.AreEqual(1100, rci.Version);

                    Assert.IsNotNull(rci.TypeInfo);
                    Assert.IsNotNull(rci.TypeInfo.Methods);
                    Assert.AreEqual(24, rci.TypeInfo.Methods.Count);
                    Assert.AreEqual(0, rci.TypeInfo.Methods.Where(m => m.Protected).Count());
                    Assert.AreEqual(6, rci.TypeInfo.Methods.Where(m => m.Private).Count());
                    Assert.AreEqual(1, rci.TypeInfo.Methods.Where(m => m.Constructor).Count());
                    Assert.AreEqual(18, rci.TypeInfo.Methods.Where(m => m.Public).Count());

                    Assert.IsNotNull(rci.TypeInfo.Tables);
                    Assert.AreEqual(0, rci.TypeInfo.Tables.Count);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestV12() throws IOException
        [TestMethod]
        public virtual void TestV12()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/WebRequestV12.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                    Assert.AreEqual(-1215, rci.Version);

                    Assert.IsNotNull(rci.TypeInfo);
                    Assert.IsNotNull(rci.TypeInfo.Methods);
                    Assert.AreEqual(26, rci.TypeInfo.Methods.Count);
                    Assert.AreEqual(0, rci.TypeInfo.Methods.Where(m => m.Protected).Count());
                    Assert.AreEqual(6, rci.TypeInfo.Methods.Where(m => m.Private).Count());
                    Assert.AreEqual(1, rci.TypeInfo.Methods.Where(m => m.Constructor).Count());
                    Assert.AreEqual(20, rci.TypeInfo.Methods.Where(m => m.Public).Count());

                    Assert.IsNotNull(rci.TypeInfo.Tables);
                    Assert.AreEqual(0, rci.TypeInfo.Tables.Count);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: [TestMethod] public void TestV121() throws IOException
        [TestMethod]
        public virtual void TestV121()
        {
            try
            {
                using (FileStream input = new FileStream("Resources/rcode/NMSTrace.r", FileMode.Open, FileAccess.Read))
                {
                    RCodeInfo rci = new RCodeInfo(new BinaryReader(input));
                    Assert.IsTrue(rci.Class);
                    Assert.IsNotNull(rci.TypeInfo);
                    Assert.AreEqual(5, rci.TypeInfo.Properties.Count);
                }
            }
            catch (InvalidRCodeException caught)
            {
                throw new Exception("RCode should be valid", caught);
            }
        }

    }
}
