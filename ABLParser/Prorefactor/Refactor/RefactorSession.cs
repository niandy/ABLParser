using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using ABLParser.Prorefactor.Refactor.Settings;
using log4net;
using log4net.Config;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.RCodeReader.Elements;

namespace ABLParser.Prorefactor.Refactor
{
    public class RefactorSession
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(RefactorSession));

        private readonly IProparseSettings proparseSettings;
        private readonly ISchema schema;
        private readonly Encoding charset;

        // Structure from rcode
        private readonly IDictionary<string, ITypeInfo> typeInfoMap = new ConcurrentDictionary<string, ITypeInfo>();
        // Cached entries from propath
        private readonly IDictionary<string, FileInfo> propathCache = new Dictionary<string, FileInfo>();
        // Cached entries from propath again
        private readonly IDictionary<string, string> propathCache2 = new Dictionary<string, string>();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Inject public RefactorSession(IProparseSettings proparseSettings, ISchema schema)
        public RefactorSession(IProparseSettings proparseSettings, ISchema schema) : this(proparseSettings, schema, Encoding.Default)
        {
        }

        public RefactorSession(IProparseSettings proparseSettings, ISchema schema, Encoding charset)
        {
            this.proparseSettings = proparseSettings;
            this.schema = schema;
            this.charset = charset;
        }

        public virtual Encoding Charset => charset;

        public virtual ISchema Schema => schema;

        /// <summary>
        /// Returns the Settings for the currently loaded project
        /// </summary>
        public virtual IProparseSettings ProparseSettings => proparseSettings;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable public ITypeInfo getTypeInfo(String clz)
        public virtual ITypeInfo GetTypeInfo(string clz)
        {
            if (string.ReferenceEquals(clz, null))
            {
                return null;
            }            
            if (!typeInfoMap.TryGetValue(clz, out ITypeInfo info))
            {
                LOG.Debug($"No TypeInfo found for {clz}");
            }

            return info;
        }

        public virtual void InjectTypeInfoCollection(ICollection<ITypeInfo> units)
        {
            foreach (ITypeInfo info in units)
            {
                InjectTypeInfo(info);
            }
        }

        public virtual void InjectTypeInfo(ITypeInfo unit)
        {
            if ((unit == null) || String.IsNullOrEmpty(unit.TypeName))
            {
                return;
            }
            typeInfoMap[unit.TypeName] = unit;
        }


        public virtual FileInfo FindFile3(string fileName)
        {            
            if (propathCache.TryGetValue(fileName, out FileInfo f))
            {
                return f;
            }

            if (IsRelativePath(fileName))
            {
                FileInfo f2 = new FileInfo(fileName);
                if (f2.Exists)
                {
                    propathCache.Add(fileName, f2);
                    return f2;
                }
            }

            foreach (string p in proparseSettings.PropathAsList)
            {
                string tryPath = p + Path.DirectorySeparatorChar + fileName;
                if (Directory.Exists(tryPath) || File.Exists(tryPath))
                {
                    propathCache.Add(fileName, new FileInfo(tryPath));
                    return new FileInfo(tryPath);
                }
            }

            return null;
        }

        public virtual string FindFile(string fileName)
        {
            if (propathCache2.ContainsKey(fileName))
            {
                return propathCache2[fileName];
            }

            if (IsRelativePath(fileName) && Directory.Exists(fileName) || File.Exists(fileName))
            {
                propathCache2.Add(fileName, fileName);
                return fileName;
            }

            foreach (string p in proparseSettings.PropathAsList)
            {
                string tryPath = p + Path.DirectorySeparatorChar + fileName;
                if (Directory.Exists(tryPath) || File.Exists(tryPath))
                {
                    propathCache2.Add(fileName, tryPath);
                    return tryPath;
                }
            }

            propathCache2.Add(fileName, "");
            return "";
        }

        private bool IsRelativePath(string fileName)
        {
            // Windows drive letter, ex: "C:"
            // Relative path, "./" or "../"
            int len = fileName.Length;
            return ((len > 0) && (fileName[0] == '/' || fileName[0] == '\\')) || ((len > 1) && (fileName[1] == ':' || fileName[0] == '.'));
        }
    }
}
