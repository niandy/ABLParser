using System;
using System.Collections.Generic;
using System.Text;
using static ABLParser.RCodeReader.RCodeInfo;

namespace ABLParser.RCodeReader.Elements.v12
{
    public class TypeInfoV12 : ITypeInfo
    {
        private const int IS_FINAL = 1;
        private const int IS_INTERFACE = 2;
        private const int USE_WIDGET_POOL = 4;
        private const int IS_DOTNET = 8;
        private const int HAS_STATICS = 64;
        private const int IS_BUILTIN = 128;
        private const int IS_HYBRID = 2048;
        private const int HAS_DOTNETBASE = 4096;
        private const int IS_ABSTRACT = 32768;
        private const int IS_SERIALIZABLE = 65536;

        protected internal string typeName;
        protected internal string parentTypeName;
        protected internal string assemblyName;
        protected internal int flags;

        private TypeInfoV12()
        {
            // No-op
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public static TypeInfoV11 newTypeInfo(byte[] segment, ByteOrder order) throws InvalidRCodeException
        public static TypeInfoV12 NewTypeInfo(byte[] segment, bool isLittleEndian)
        {
            TypeInfoV12 typeInfo = new TypeInfoV12();

            int publicElementCount = ByteBuffer.Wrap(segment, 2, sizeof(short)).Order(isLittleEndian).GetShort();
            int protectedElementCount = ByteBuffer.Wrap(segment, 4, sizeof(short)).Order(isLittleEndian).GetShort();
            int privateElementCount = ByteBuffer.Wrap(segment, 6, sizeof(short)).Order(isLittleEndian).GetShort();
            int constructorCount = ByteBuffer.Wrap(segment, 8, sizeof(short)).Order(isLittleEndian).GetShort();
            int interfaceCount = ByteBuffer.Wrap(segment, 10, sizeof(short)).Order(isLittleEndian).GetShort();
            int textAreaOffset = ByteBuffer.Wrap(segment, 24, sizeof(int)).Order(isLittleEndian).GetInt();

            typeInfo.flags = ByteBuffer.Wrap(segment, 32, sizeof(int)).Order(isLittleEndian).GetInt();
            int nameOffset = ByteBuffer.Wrap(segment, 12, sizeof(int)).Order(isLittleEndian).GetInt();
            typeInfo.typeName = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + nameOffset);
            int assemblyNameOffset = ByteBuffer.Wrap(segment, 16, sizeof(int)).Order(isLittleEndian).GetInt();
            typeInfo.assemblyName = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + assemblyNameOffset);

            // ID - Access type - Kind - Name offset
            IList<int[]> entries = new List<int[]>();
            for (uint zz = 0; zz < publicElementCount + protectedElementCount + privateElementCount + constructorCount; zz++)
            {
                entries.Add(new int[] {
                    (int)ByteBuffer.Wrap(segment, 192 + 10 + (16 * zz), sizeof(short)).Order(isLittleEndian).GetShort(), 
                    (int)ByteBuffer.Wrap(segment, 192 + 12 + (16 * zz), sizeof(short)).Order(isLittleEndian).GetShort(),
                    (int)ByteBuffer.Wrap(segment, 192 + 14 + (16 * zz), sizeof(short)).Order(isLittleEndian).GetShort(), 
                    ByteBuffer.Wrap(segment, 192 + 0 + (16 * zz), sizeof(int)).Order(isLittleEndian).GetInt()
                });
            }

            uint currOffset = (uint)(192 + 16 * (publicElementCount + protectedElementCount + privateElementCount + constructorCount));
            typeInfo.parentTypeName = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currOffset, sizeof(int)).Order(isLittleEndian).GetInt());
            currOffset += 56;
            bool isEnum = "Progress.Lang.Enum".Equals(typeInfo.ParentTypeName) || "Progress.Lang.FlagsEnum".Equals(typeInfo.ParentTypeName);


            for (int zz = 0; zz < interfaceCount; zz++)
            {
                string str = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + ByteBuffer.Wrap(segment, currOffset, sizeof(int)).Order(isLittleEndian).GetInt());
                typeInfo.Interfaces.Add(str);
                currOffset += 56;
            }

            foreach (int[] entry in entries)
            {
                string name = RCodeInfo.ReadNullTerminatedString(segment, textAreaOffset + entry[3]);
                AccessType set = AccessTypeExt.GetTypeFromString(entry[1]);
                if ((isEnum) && (ElementKindExt.GetKind(entry[2]) != ElementKind.PROPERTY))
                {
                    return typeInfo;
                }

                switch (ElementKindExt.GetKind(entry[2]))
                {
                    case ElementKind.METHOD:
                        IMethodElement mthd = MethodElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)mthd.SizeInRCode;
                        typeInfo.Methods.Add(mthd);
                        break;
                    case ElementKind.PROPERTY:
                        IPropertyElement prop = PropertyElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian, isEnum);
                        currOffset += (uint)prop.SizeInRCode;
                        typeInfo.Properties.Add(prop);
                        break;
                    case ElementKind.VARIABLE:
                        IVariableElement var = VariableElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)var.SizeInRCode;
                        typeInfo.Variables.Add(var);
                        break;
                    case ElementKind.TABLE:
                        ITableElement tbl = TableElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)tbl.SizeInRCode;
                        typeInfo.Tables.Add(tbl);
                        break;
                    case ElementKind.BUFFER:
                        IBufferElement buf = BufferElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)buf.SizeInRCode;
                        typeInfo.Buffers.Add(buf);
                        break;
                    case ElementKind.QUERY:
                        IQueryElement qry = QueryElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)qry.SizeInRCode;
                        break;
                    case ElementKind.DATASET:
                        IDatasetElement ds = DatasetElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)ds.SizeInRCode;
                        break;
                    case ElementKind.DATASOURCE:
                        IDataSourceElement dso = DataSourceElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)dso.SizeInRCode;
                        break;
                    case ElementKind.EVENT:
                        IEventElement evt = EventElementV12.FromDebugSegment(name, set, segment, currOffset, textAreaOffset, isLittleEndian);
                        currOffset += (uint)evt.SizeInRCode;
                        typeInfo.Events.Add(evt);
                        break;
                    case ElementKind.UNKNOWN:
                        throw new InvalidRCodeException("Found element kind " + entry[2]);
                }
            }

            return typeInfo;
        }

        public IBufferElement GetBufferFor(string name)
        {
            foreach (IBufferElement tbl in Buffers)
            {
                if (tbl.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return tbl;
                }
            }
            return null;
        }

        public bool HasTempTable(string inName)
        {
            foreach (ITableElement tbl in Tables)
            {
                if (tbl.Name.Equals(inName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasMethod(string name)
        {
            foreach (IMethodElement mthd in Methods)
            {
                if (mthd.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public ITableElement GetTempTable(string inName)
        {
            foreach (ITableElement tbl in Tables)
            {
                if (tbl.Name.Equals(inName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return tbl;
                }
            }
            return null;
        }

        public bool HasProperty(string name)
        {
            foreach (IPropertyElement prop in Properties)
            {
                if (prop.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) && (prop.Public || prop.Protected))
                {
                    return true;
                }
            }
            return false;
        }

        public IPropertyElement GetProperty(string name)
        {
            // Only for testing
            foreach (IPropertyElement prop in Properties)
            {
                if (prop.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return prop;
                }
            }
            return null;
        }

        public bool HasBuffer(string inName)
        {
            // TODO Can it be abbreviated ??
            foreach (IBufferElement buf in Buffers)
            {
                if (buf.Name.Equals(inName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public IBufferElement GetBuffer(string inName)
        {
            foreach (IBufferElement buf in Buffers)
            {
                if (buf.Name.Equals(inName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return buf;
                }
            }
            return null;
        }

        public ICollection<IMethodElement> Methods { get; } = new List<IMethodElement>();

        public ICollection<IPropertyElement> Properties { get; } = new List<IPropertyElement>();

        public ICollection<IEventElement> Events { get; } = new List<IEventElement>();

        public ICollection<IVariableElement> Variables { get; } = new List<IVariableElement>();

        public ICollection<ITableElement> Tables { get; } = new List<ITableElement>();

        public ICollection<IBufferElement> Buffers { get; } = new List<IBufferElement>();

        public string TypeName
        {
            get
            {
                return typeName;
            }
        }

        public string ParentTypeName
        {
            get
            {
                return parentTypeName;
            }
        }

        public string AssemblyName
        {
            get
            {
                return assemblyName;
            }
        }

        public IList<string> Interfaces { get; } = new List<string>();

        public override string ToString()
        {
            return string.Format("Type info {0} - Parent {1}", typeName, parentTypeName);
        }

        public bool Final
        {
            get
            {
                return (flags & IS_FINAL) != 0;
            }
        }

        public bool Interface
        {
            get
            {
                return (flags & IS_INTERFACE) != 0;
            }
        }

        public bool HasStatics()
        {
            return (flags & HAS_STATICS) != 0;
        }

        public bool BuiltIn
        {
            get
            {
                return (flags & IS_BUILTIN) != 0;
            }
        }

        public bool Hybrid
        {
            get
            {
                return (flags & IS_HYBRID) != 0;
            }
        }

        public bool HasDotNetBase()
        {
            return (flags & HAS_DOTNETBASE) != 0;
        }

        public bool Abstract
        {
            get
            {
                return (flags & IS_ABSTRACT) != 0;
            }
        }

        public bool Serializable
        {
            get
            {
                return (flags & IS_SERIALIZABLE) != 0;
            }
        }

        public bool UseWidgetPool
        {
            get
            {
                return (flags & USE_WIDGET_POOL) != 0;
            }
        }

        protected internal virtual bool DotNet
        {
            get
            {
                return (flags & IS_DOTNET) != 0;
            }
        }
    }
}
