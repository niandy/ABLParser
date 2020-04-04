using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ABLParserTests
{
    [TestClass]
    class Resources
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            StreamWriter dbg = new StreamWriter("c:\\temp\\ut.txt");
            dbg.WriteLine(string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            dbg.WriteLine(context.TestDir);
            dbg.WriteLine(context.DeploymentDirectory);
            dbg.WriteLine(Directory.GetCurrentDirectory());
            dbg.Close();
        }        

        public static bool FilesContentEquals(FileInfo file1, FileInfo file2) => file1.Length == file2.Length && File.ReadAllBytes(file1.FullName).SequenceEqual(File.ReadAllBytes(file2.FullName));
    }
}
