using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Refactor.Settings
{
    public class ProparseSettings : IProparseSettings
    {
        private readonly bool multiParse;
        private readonly bool proparseDirectives;
        private readonly bool backslashEscape;

        private readonly OperatingSystem os;
        private readonly int processArchitecture;
        private readonly bool batchMode;
        private readonly bool skipXCode;
        private readonly string propath;
        private readonly string proversion;
        private readonly IList<string> path = new List<string>();

        private string customWindowSystem;
        private OperatingSystem customOpsys;
        private int? customProcessArchitecture;
        private bool? customBatchMode;
        private string customProversion;
        private bool? customSkipXCode;
        private bool antlrTokenInsertion = true;
        private bool antlrTokenDeletion = true;
        private bool antlrRecover = true;

        public ProparseSettings(string propath) : this(propath, false)
        {
        }

        public ProparseSettings(string propath, bool backslashAsEscape) : this(true, true, backslashAsEscape, true, OperatingSystem.OS, propath, "11.7", 64, true)
        {
        }

        public ProparseSettings(bool proparseDirectives, bool multiParse, bool backslashEscape, bool batchMode, OperatingSystem os, string propath, string proversion, int processArchitecture, bool skipXCode)
        {
            this.multiParse = multiParse;
            this.proparseDirectives = proparseDirectives;
            this.backslashEscape = backslashEscape;
            this.batchMode = batchMode;
            this.os = os;
            this.propath = propath;
            this.proversion = proversion;
            this.processArchitecture = processArchitecture;
            this.skipXCode = skipXCode;
            ((List<string>)path).AddRange(new List<string>(propath.Split(',')));
        }

        public bool MultiParse
        {
            get
            {
                return multiParse;
            }
        }

        public bool ProparseDirectives
        {
            get
            {
                return proparseDirectives;
            }
        }

        public bool UseBackslashAsEscape()
        {
            return backslashEscape;
        }

        public bool BatchMode
        {
            get
            {
                return customBatchMode == null ? batchMode : customBatchMode.Value;
            }
        }

        public OperatingSystem OpSys
        {
            get
            {
                return customOpsys == null ? os : customOpsys;
            }
        }

        public string WindowSystem
        {
            get
            {
                return string.ReferenceEquals(customWindowSystem, null) ? os.WindowSystem : customWindowSystem;
            }
        }

        public string Propath
        {
            get
            {
                return propath;
            }
        }

        public IList<string> PropathAsList
        {
            get
            {
                return path;
            }
        }

        public string Proversion
        {
            get
            {
                return customProversion == null ? proversion : customProversion;
            }
        }

        public int? ProcessArchitecture
        {
            get
            {
                return customProcessArchitecture == null ? processArchitecture : customProcessArchitecture;
            }
        }

        public bool SkipXCode
        {
            get
            {
                return customSkipXCode == null ? skipXCode : (bool)customSkipXCode;
            }
        }

        public bool AllowAntlrTokenInsertion()
        {
            return antlrTokenInsertion;
        }

        public virtual bool AntlrTokenInsertion
        {
            set
            {
                this.antlrTokenInsertion = value;
            }
        }

        public bool AllowAntlrTokenDeletion()
        {
            return antlrTokenDeletion;
        }

        public virtual bool AntlrTokenDeletion
        {
            set
            {
                this.antlrTokenDeletion = value;
            }
        }

        public bool AllowAntlrRecover()
        {
            return antlrRecover;
        }

        public virtual bool AntlrRecover
        {
            set
            {
                this.antlrRecover = value;
            }
        }

        public virtual bool CustomBatchMode
        {
            set
            {
                this.customBatchMode = value;
            }
        }

        public virtual string CustomOpsys
        {
            set
            {
                if (OperatingSystem.UNIX.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.customOpsys = OperatingSystem.UNIX;
                }
                else if (OperatingSystem.WINDOWS.Name.Equals(value))
                {
                    this.customOpsys = OperatingSystem.WINDOWS;
                }
            }
        }

        public virtual int CustomProcessArchitecture
        {
            set
            {
                this.customProcessArchitecture = value;
            }
        }

        public virtual string CustomWindowSystem
        {
            set
            {
                this.customWindowSystem = value;
            }
        }

        public virtual string CustomProversion
        {
            set
            {
                this.customProversion = value;
            }
        }

        public virtual bool CustomSkipXCode
        {
            set
            {
                this.customSkipXCode = value;
            }
        }
  

        public sealed class OperatingSystem
        {
            public static readonly OperatingSystem UNIX = new OperatingSystem("UNIX", InnerEnum.UNIX);
            public static readonly OperatingSystem WINDOWS = new OperatingSystem("WINDOWS", InnerEnum.WINDOWS);

            private static readonly IList<OperatingSystem> valueList = new List<OperatingSystem>();

            static OperatingSystem()
            {
                valueList.Add(UNIX);
                valueList.Add(WINDOWS);
            }

            public enum InnerEnum
            {
                UNIX,
                WINDOWS
            }

            public readonly InnerEnum innerEnumValue;
            private readonly string nameValue;
            private readonly int ordinalValue;
            private static int nextOrdinal = 0;

            private OperatingSystem(string name, InnerEnum innerEnum)
            {
                nameValue = name;
                ordinalValue = nextOrdinal++;
                innerEnumValue = innerEnum;
            }

            public string Name
            {
                get
                {
                    return this == OperatingSystem.WINDOWS ? "WIN32" : "UNIX";
                }
            }

            public string WindowSystem
            {
                get
                {
                    return this == OperatingSystem.WINDOWS ? "MS-WIN95" : "TTY";
                }
            }

            public int Number
            {
                get
                {
                    return this == OperatingSystem.WINDOWS ? 1 : 2;
                }
            }

            public static OperatingSystem OS
            {
                get
                {
                    return Environment.OSVersion.Platform.HasFlag(PlatformID.Win32NT) ? WINDOWS : UNIX;                    
                }
            }

            public static IList<OperatingSystem> Values()
            {
                return valueList;
            }

            public int Ordinal
            {
                get
                {
                    return ordinalValue;
                }
            }

            public override string ToString()
            {
                return nameValue;
            }

            public static OperatingSystem ValueOf(string name)
            {
                foreach (OperatingSystem enumInstance in OperatingSystem.valueList)
                {
                    if (enumInstance.nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }
    }
}
