using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.RCodeReader.Elements;
using System.IO;
using System.Linq;
using ABLParser.RCodeReader.Elements.v12;
using ABLParser.RCodeReader.Elements.v11;

namespace ABLParser.RCodeReader
{
    /// <summary>
    /// Import debug segment information from rcode.
    /// </summary>
    public class RCodeInfo
    {
        // Magic number, followed by same magic number written as little-endian
        private const int MAGIC1 = 0x56CED309;
        private const int MAGIC2 = 0x09D3CE56;

        // Header values
        private const int HEADER_SIZE = 68;
        private const int HEADER_OFFSET_MAGIC = 0;
        private const int HEADER_OFFSET_TIMESTAMP = 4;
        private const int HEADER_OFFSET_DIGEST = 10;
        private const int HEADER_OFFSET_DIGEST_V12 = 22;
        private const int HEADER_OFFSET_RCODE_VERSION = 14;
        private const int HEADER_OFFSET_SEGMENT_TABLE_SIZE = 0x1E;
        private const int HEADER_OFFSET_SIGNATURE_SIZE = 56;
        private const int HEADER_OFFSET_TYPEBLOCK_SIZE = 60;
        private const int HEADER_OFFSET_RCODE_SIZE = 64;

        // Segment table values
        private const int SEGMENT_TABLE_OFFSET_INITIAL_VALUE_SEGMENT_OFFSET = 0;
        private const int SEGMENT_TABLE_OFFSET_ACTION_SEGMENT_OFFSET = 4;
        private const int SEGMENT_TABLE_OFFSET_ECODE_SEGMENT_OFFSET = 8;
        private const int SEGMENT_TABLE_OFFSET_DEBUG_SEGMENT_OFFSET = 12;
        private const int SEGMENT_TABLE_OFFSET_INITIAL_VALUE_SEGMENT_SIZE = 16;
        private const int SEGMENT_TABLE_OFFSET_ACTION_SEGMENT_SIZE = 20;
        private const int SEGMENT_TABLE_OFFSET_ECODE_SEGMENT_SIZE = 24;
        private const int SEGMENT_TABLE_OFFSET_DEBUG_SEGMENT_SIZE = 28;
        private const int SEGMENT_TABLE_OFFSET_IPACS_TABLE_SIZE = 32;
        private const int SEGMENT_TABLE_OFFSET_FRAME_SEGMENT_TABLE_SIZE = 34;
        private const int SEGMENT_TABLE_OFFSET_TEXT_SEGMENT_TABLE_SIZE = 36;

        protected internal bool isLittleEndian;
        protected internal int version;
        protected internal bool sixtyFourBits;
        protected internal long timeStamp;
        protected internal int digestOffset;

        protected internal int segmentTableSize;
        protected internal int signatureSize;
        protected internal int typeBlockSize;
        protected internal int rcodeSize;
        protected internal int initialValueSegmentOffset;
        protected internal int initialValueSegmentSize;
        protected internal int debugSegmentOffset;
        protected internal int debugSegmentSize;
        protected internal int actionSegmentOffset;
        protected internal int actionSegmentSize;
        protected internal int ecodeSegmentOffset;
        protected internal int ecodeSegmentSize;
        protected internal int ipacsTableSize;
        protected internal int frameSegmentTableSize;
        protected internal int textSegmentTableSize;

        // From type block
        private readonly bool isClass = false;

        private ITypeInfo typeInfo;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public RCodeInfo(InputStream input) throws InvalidRCodeException, IOException        
        public RCodeInfo(BinaryReader input) : this(input, null)
        {
        }

        /// <summary>
        /// Parse InputStream and store debug segment information
        /// </summary>
        /// <param name="input"> Has to be closed by caller </param>
        /// <param name="out"> Output stream for debug. Can be null
        /// </param>
        /// <exception cref="InvalidRCodeException"> </exception>
        /// <exception cref="IOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public RCodeInfo(InputStream input, PrintStream out) throws InvalidRCodeException, IOException
        public RCodeInfo(BinaryReader input, StreamWriter @out)
        {
            ProcessHeader(input, @out);
            ProcessSignatureBlock(input, @out);
            ProcessSegmentTable(input, @out);

            if ((initialValueSegmentOffset >= 0) && (initialValueSegmentSize > 0))
            {
                long pos = input.BaseStream.Position;
                long bytesRead = input.BaseStream.Seek(initialValueSegmentOffset, SeekOrigin.Current);
                if (bytesRead != pos + initialValueSegmentOffset)
                {
                    throw new InvalidRCodeException("Not enough bytes to reach initial values segment");
                }
                ProcessInitialValueSegment(input, @out);
            }

            if ((debugSegmentOffset > 0) && (debugSegmentSize > 0))
            {
                long pos = input.BaseStream.Position;
                long bytesRead = input.BaseStream.Seek(debugSegmentOffset - initialValueSegmentSize, SeekOrigin.Current);
                if (bytesRead != pos + debugSegmentOffset - initialValueSegmentSize)
                {
                    throw new InvalidRCodeException("Not enough bytes to reach debug segment");
                }
                ProcessDebugSegment(input, @out);
            }

            if (typeBlockSize > 0)
            {
                int skip = debugSegmentOffset > 0 ? rcodeSize - debugSegmentOffset - debugSegmentSize : rcodeSize - initialValueSegmentSize - debugSegmentSize;
                long pos = input.BaseStream.Position;
                long bytesRead = input.BaseStream.Seek(skip, SeekOrigin.Current);
                if (bytesRead != pos + skip)
                {
                    throw new InvalidRCodeException("Not enough bytes to reach type block");
                }
                ProcessTypeBlock(input, @out);
                isClass = true;
            }

            input.Close();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private final void processHeader(InputStream input, PrintStream out) throws IOException, InvalidRCodeException
        private void ProcessHeader(BinaryReader input, StreamWriter @out)
        {
            byte[] header = input.ReadBytes(HEADER_SIZE);
            if (header.Length != HEADER_SIZE)
            {
                throw new InvalidRCodeException("Not enough bytes in header");
            }

            if (@out != null)
            {
                @out.Write("%n******%nHEADER%n******%n");
                PrintByteBuffer(@out, header);
            }

            long magic = ByteBuffer.Wrap(header, HEADER_OFFSET_MAGIC, sizeof(int)).GetInt();
            if (magic == MAGIC1)
            {
                isLittleEndian = true;
            }
            else if (magic == MAGIC2)
            {
                isLittleEndian = false;
            }
            else
            {
                throw new InvalidRCodeException("Can't find magic number");
            }

            version = ByteBuffer.Wrap(header, HEADER_OFFSET_RCODE_VERSION, sizeof(short)).Order(isLittleEndian).GetShort();
            sixtyFourBits = (version & 0x4000) != 0;            
            if ((version & 0x3FFF) >= 1200)
            {
                byte[] header2 = input.ReadBytes(16);
                if (header2.Length != 16)
                {
                    throw new InvalidRCodeException("Not enough bytes in OE12 header");
                }

                timeStamp = ByteBuffer.Wrap(header, HEADER_OFFSET_TIMESTAMP, sizeof(int)).Order(isLittleEndian).GetInt();
                digestOffset = ByteBuffer.Wrap(header, HEADER_OFFSET_DIGEST_V12, sizeof(short)).Order(isLittleEndian).GetShort();
                segmentTableSize = ByteBuffer.Wrap(header, HEADER_OFFSET_SEGMENT_TABLE_SIZE, sizeof(short)).Order(isLittleEndian).GetShort();
                signatureSize = ByteBuffer.Wrap(header, HEADER_OFFSET_SIGNATURE_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
                typeBlockSize = ByteBuffer.Wrap(header, HEADER_OFFSET_TYPEBLOCK_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
                rcodeSize = ByteBuffer.Wrap(header2, 0xc, sizeof(int)).Order(isLittleEndian).GetInt();
            }
            else if ((version & 0x3FFF) >= 1100)
            {
                timeStamp = ByteBuffer.Wrap(header, HEADER_OFFSET_TIMESTAMP, sizeof(int)).Order(isLittleEndian).GetInt();
                digestOffset = ByteBuffer.Wrap(header, HEADER_OFFSET_DIGEST, sizeof(short)).Order(isLittleEndian).GetShort();
                segmentTableSize = ByteBuffer.Wrap(header, HEADER_OFFSET_SEGMENT_TABLE_SIZE, sizeof(short)).Order(isLittleEndian).GetShort();
                signatureSize = ByteBuffer.Wrap(header, HEADER_OFFSET_SIGNATURE_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
                typeBlockSize = ByteBuffer.Wrap(header, HEADER_OFFSET_TYPEBLOCK_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
                rcodeSize = ByteBuffer.Wrap(header, HEADER_OFFSET_RCODE_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
            }
            else
            {
                throw new InvalidRCodeException("Only v11 rcode is supported");
            }
        }

        private void PrintByteBuffer(StreamWriter @out, byte[] header)
        {
            throw new NotImplementedException();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private final void processSignatureBlock(InputStream input, PrintStream out) throws IOException, InvalidRCodeException
        private void ProcessSignatureBlock(BinaryReader input, StreamWriter @out)
        {
            byte[] header = input.ReadBytes(signatureSize);
            if (header.Length != signatureSize)
            {
                throw new InvalidRCodeException("Not enough bytes in signature block");
            }
            if (@out != null)
            {
                @out.Write("%n*********%nSIGNATURE%n*********%n");
                PrintByteBuffer(@out, header);
            }

            int preambleSize = ReadAsciiEncodedNumber(header, 0, 4);
            int numElements = ReadAsciiEncodedNumber(header, 4, 4);
            // Version of signature block : offset 8, 4 bytes
            // Encoding : offset 12, null-terminated string

            int pos = preambleSize;
            for (int kk = 0; kk < numElements; kk++)
            {
                string str = ReadNullTerminatedString(header, pos);
                pos += str.Length + 1;

                // Datasets and temp-tables not read for now
                if (str.StartsWith("DSET", StringComparison.Ordinal) || str.StartsWith("TTAB", StringComparison.Ordinal))
                {
                    continue;
                }

                // Will probably be skipped
                // Function fn = parseProcSignature(str);
                // if ((unit.mainProcedure == null) && (fn.type == FunctionType.MAIN)) {
                // unit.mainProcedure = fn;
                // } else {
                // unit.funcs.add(fn);
                // }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private final void processSegmentTable(InputStream input, PrintStream out) throws IOException, InvalidRCodeException
        private void ProcessSegmentTable(BinaryReader input, StreamWriter @out)
        {
            byte[] header = input.ReadBytes(segmentTableSize);
            if (header.Length != segmentTableSize)
            {
                throw new InvalidRCodeException("Not enough bytes in segment table block");
            }
            if (@out != null)
            {
                @out.Write("%n*******%nSEGMENT%n*******%n");
                PrintByteBuffer(@out, header);
            }

            initialValueSegmentSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_INITIAL_VALUE_SEGMENT_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
            actionSegmentOffset = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_ACTION_SEGMENT_OFFSET, sizeof(int)).Order(isLittleEndian).GetInt();
            actionSegmentSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_ACTION_SEGMENT_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
            ecodeSegmentOffset = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_ECODE_SEGMENT_OFFSET, sizeof(int)).Order(isLittleEndian).GetInt();
            ecodeSegmentSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_ECODE_SEGMENT_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();
            debugSegmentOffset = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_DEBUG_SEGMENT_OFFSET, sizeof(int)).Order(isLittleEndian).GetInt();
            debugSegmentSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_DEBUG_SEGMENT_SIZE, sizeof(int)).Order(isLittleEndian).GetInt();

            ipacsTableSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_IPACS_TABLE_SIZE, sizeof(short)).Order(isLittleEndian).GetShort();
            frameSegmentTableSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_FRAME_SEGMENT_TABLE_SIZE, sizeof(short)).Order(isLittleEndian).GetShort();
            textSegmentTableSize = ByteBuffer.Wrap(header, SEGMENT_TABLE_OFFSET_TEXT_SEGMENT_TABLE_SIZE, sizeof(short)).Order(isLittleEndian).GetShort();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: void processTypeBlock(InputStream input, PrintStream out) throws IOException, InvalidRCodeException
        internal virtual void ProcessTypeBlock(BinaryReader input, StreamWriter @out)
        {
            byte[] segment = input.ReadBytes(typeBlockSize);
            if (segment.Length != typeBlockSize)
            {
                throw new InvalidRCodeException("Not enough bytes in type block");
            }
            if (@out != null)
            {
                @out.Write("%n**********%nTYPE BLOCK%n***********%n");
                PrintByteBuffer(@out, segment);
            }

            if ((version & 0x3FFF) >= 1200)
            {
                this.typeInfo = TypeInfoV12.NewTypeInfo(segment, isLittleEndian);
            }
            else
            {
                this.typeInfo = TypeInfoV11.NewTypeInfo(segment, isLittleEndian);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private final void processInitialValueSegment(InputStream input, PrintStream out) throws IOException, InvalidRCodeException
        private void ProcessInitialValueSegment(BinaryReader input, StreamWriter @out)
        {
            byte[] segment = input.ReadBytes(initialValueSegmentSize);
            if (segment.Length != initialValueSegmentSize)
            {
                throw new InvalidRCodeException("Not enough bytes in initial value segment block");
            }
            if (@out != null)
            {
                @out.Write("%n**********%nINITIAL VALUES%n***********%n");
                PrintByteBuffer(@out, segment);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: void processDebugSegment(InputStream input, PrintStream out) throws IOException, InvalidRCodeException
        internal virtual void ProcessDebugSegment(BinaryReader input, StreamWriter @out)
        {
            byte[] segment = input.ReadBytes(debugSegmentSize);
            if (segment.Length != debugSegmentSize)
            {
                throw new InvalidRCodeException("Not enough bytes in debug segment block");
            }
            if (@out != null)
            {
                @out.Write("%n*******%nDEBUG%n*******%n");
                PrintByteBuffer(@out, segment);
            }

        }

        public virtual ITypeInfo TypeInfo => typeInfo;

        public static void PrintByteBuffer(StreamWriter writer, sbyte[] block)
        {
            StringBuilder sb = new StringBuilder();
            int pos = 0;
            while (pos < block.Length)
            {
                if ((pos % 16) == 0)
                {
                    writer.Write(string.Format("{0:X10} | ", pos));
                }
                writer.Write(string.Format("{0:X2} ", block[pos]));
                if (char.IsControl((char)block[pos]))
                {
                    sb.Append('.');
                }
                else
                {
                    sb.Append((char)block[pos]);
                }
                if ((pos > 0) && (((pos + 1) % 16) == 0))
                {
                    writer.WriteLine(" | " + sb.ToString());
                    sb = new StringBuilder();
                }
                pos++;
            }
            if ((pos % 16) != 0)
            {
                writer.Write(new StringBuilder(3 * 16 - (pos % 16)).Insert(0, "   ", 16 - (pos % 16)).ToString());
                writer.WriteLine(" | " + sb.ToString());
            }
        }

        /// <summary>
        /// Returns r-code compiler version
        /// </summary>
        /// <returns> Version </returns>
        public virtual long Version => version;

        /// <summary>
        /// Returns r-code timestamp (in milliseconds)
        /// </summary>
        /// <returns> Timestamp </returns>
        public virtual long TimeStamp => timeStamp;

        public virtual bool Is64bits()
        {
            return sixtyFourBits;
        }

        public virtual bool Class => isClass;

        public static string ReadNullTerminatedString(byte[] array, int offset)
        {
            return ReadNullTerminatedString(array, offset, Encoding.Default);
        }

        public static string ReadNullTerminatedString(byte[] array, int offset, Encoding charset)
        {
            int zz = 0;
            while ((zz + offset < array.Length) && (array[zz + offset] != 0))
            {
                zz++;
            }

            return charset.GetString(array, offset, zz);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: private static int readAsciiEncodedNumber(byte[] array, int pos, int length) throws InvalidRCodeException
        private static int ReadAsciiEncodedNumber(byte[] array, int pos, int length)
        {
            try
            {
                char[] dst = new char[length];
                Array.Copy(array, pos, dst, 0, length);                
                return Convert.ToInt32(new string(dst, 0, length), 16);
            }
            catch (System.FormatException caught)
            {
                throw new InvalidRCodeException(caught);
            }
        }


        public class InvalidRCodeException : Exception
        {
            private const long serialVersionUID = 1L;

            public InvalidRCodeException(string s) : base(s)
            {
            }

            public InvalidRCodeException(Exception caught) : base("Invalid r-code", caught)
            {
            }
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @SuppressWarnings("unused") private static Function parseProcSignature(String str)
        private static Function ParseProcSignature(string str)
        {
            Function fn = new Function();

            // String is a comma-separated list of at least 3 elements
            IEnumerator<string> lst = str.Split(',').Select(s => s.Trim()).GetEnumerator();

            // First element is the description, a space-separated list of 3 sub-elements
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            lst.MoveNext();
            string desc = lst.Current;
            // Sub-element 1 is the type
            fn.Type = EnumExt.GetFunctionTypeFromString(desc.Substring(0, desc.IndexOf(' ')));
            // Sub-element 2 is the name (may contain spaces...)
            fn.Name = desc.Substring(desc.IndexOf(' ') + 1, desc.LastIndexOf(' ') - desc.IndexOf(' '));
            // Sub-element 3 is the flag
            fn.AccessTypes = EnumExt.GetSigAccessTypeFromString(desc.Substring(desc.LastIndexOf(' ') + 1));

            // Second element is return type, which may be empty (if entry doesn't return anything)
            //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
            lst.MoveNext();
            string retType = lst.Current;
            if (retType.Length == 0)
            {
                fn.ReturnType = DataType.VOID;
            }
            else
            {
                // Generics handling, which break comma-separated list...
                if (retType.Contains("[["))
                {
                    while (!retType.Contains("]]"))
                    {
                        //JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
                        lst.MoveNext();
                        retType += lst.Current;
                    }
                }
                // If not empty, then two entries separated by a space, first is data type, then extent number
                fn.ReturnExtent = int.Parse(retType.Substring(retType.LastIndexOf(' ') + 1));
                fn.ReturnType = retType.Substring(0, retType.LastIndexOf(' ')).AsDataType();
                if (fn.ReturnType == DataType.CLASS)
                {
                    fn.ClassReturnType = retType.Substring(0, retType.IndexOf(' '));
                }
            }

            // Parameters
            while (lst.MoveNext())
            {
                string str2 = lst.Current;
                if (str2.Length > 0)
                {
                    // Generics handling, which break comma-separated list...
                    if (str2.Contains("[["))
                    {
                        while (!str2.Contains("]]"))
                        {
                            str2 += lst.Current;
                        }
                    }

                    fn.Parameters.Add(ParseParameter(str2));
                }
            }

            return fn;
        }

        private static Parameter ParseParameter(string str)
        {
            Parameter param = new Parameter();
            IList<string> prm = str.Split(' ').Select(s => s.Trim()).ToList<string>();
            if (prm.Count != 4)
            {
                return param;
            }
            param.Name = prm[1];
            param.Type = EnumExt.GetParameterTypeFromString(prm[0]);
            param.Datatype = prm[2].AsDataType();
            param.Extent = int.Parse(prm[3]);
            param.ClassType = prm[2];

            return param;
        }

        public class Function
        {
            public IList<Parameter> Parameters { get; set; } = new List<Parameter>();
            public int ReturnExtent { get; set; }
            public string ClassReturnType { get; set; }
            public DataType ReturnType { get; set; }
            public SigAccessType AccessTypes { get; set; }
            public string Name { get; set; }
            public FunctionType? Type { get; set; }


            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (SigAccessType type in Enum.GetValues(AccessTypes.GetType()))
                {
                    if (AccessTypes.HasFlag(type))
                    {
                        sb.Append(type).Append(' ');
                    }
                }
                sb.Append(Type).Append(' ').Append(ReturnType);
                if (ReturnType == DataType.CLASS)
                {
                    sb.Append('{').Append(ClassReturnType).Append('}');
                }
                if (ReturnExtent > 0)
                {
                    sb.Append(" EXTENT ").Append(ReturnExtent);
                }
                sb.Append(' ').Append(Name).Append(" (");

                StringBuilder sb2 = new StringBuilder();
                foreach (Parameter p in Parameters)
                {
                    if (sb2.Length > 0)
                    {
                        sb2.Append(", ");
                    }
                    sb2.Append(p.ToString());
                }
                sb.Append(sb2.ToString()).Append(')');

                return sb.ToString();
            }
        }


        public enum FunctionType
        {
            MAIN,
            CONSTRUCTOR,
            METHOD,
            FUNCTION,
            PROCEDURE,
            EXTERNAL_PROCEDURE,
            DLL_PROCEDURE,
            DESTRUCTOR
        }

        [Flags]
        public enum SigAccessType
        {
            NONE,
            PUBLIC,
            PRIVATE,
            PROTECTED,
            STATIC,
            ABSTRACT,
            FINAL,
            OVERRIDE
        }

        public enum ParameterType
        {
            INPUT,
            OUTPUT,
            INPUT_OUTPUT,
            BUFFER
        }

        public class EnumExt
        {
            public static FunctionType? GetFunctionTypeFromString(String str)
            {
                switch (str.ToUpperInvariant())
                {
                    case "CONST":
                        return FunctionType.CONSTRUCTOR;
                    case "MAIN":
                        return FunctionType.MAIN;
                    case "METH":
                        return FunctionType.METHOD;
                    case "FUNC":
                        return FunctionType.FUNCTION;
                    case "PROC":
                        return FunctionType.PROCEDURE;
                    case "EXT":
                        return FunctionType.EXTERNAL_PROCEDURE;
                    case "DLL":
                        return FunctionType.DLL_PROCEDURE;
                    case "DEST":
                        return FunctionType.DESTRUCTOR;
                    default:
                        return null;
                }                
            }

            public static SigAccessType GetSigAccessTypeFromString(string str)
            {
                int val = int.Parse(str);
                SigAccessType set = SigAccessType.NONE;
                switch (val & 0x07)
                {
                    case 1:
                        set |= SigAccessType.PUBLIC;
                        break;
                    case 2:
                        set |= SigAccessType.PRIVATE;
                        break;
                    case 4:
                        set |= SigAccessType.PROTECTED;
                        break;
                    default:
                        break;
                }
                if ((val & 0x08) != 0)
                    set |= SigAccessType.STATIC;
                if ((val & 0x10) != 0)
                    set |= SigAccessType.ABSTRACT;
                if ((val & 0x20) != 0)
                    set |= SigAccessType.FINAL;
                if ((val & 0x40) != 0)
                    set |= SigAccessType.OVERRIDE;

                return set;
            }

            public static ParameterType? GetParameterTypeFromString(String str)
            {
                switch(str)
                {
                    case "1":
                        return ParameterType.INPUT;
                    case "2":
                        return ParameterType.OUTPUT;
                    case "3":
                        return ParameterType.INPUT_OUTPUT;
                    case "4":
                        return ParameterType.BUFFER;
                    default:
                        return null;
                }                
            }
        }

        public class Parameter
        {
            public ParameterType? Type { get; set; }
            public string Name { get; set; }
            public DataType Datatype { get; set; }
            public int Extent { get; set; }
            public string ClassType { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (Type != null)
                {
                    sb.Append(Type.ToString());
                }
                sb.Append(" PARAMETER ");
                if (Type == ParameterType.BUFFER)
                {
                    sb.Append(" FOR ").Append(Name);
                }
                else
                {
                    sb.Append(Name).Append(" AS ").Append(Datatype);
                    if (Datatype == DataType.CLASS)
                    {
                        sb.Append('{').Append(ClassType).Append('}');
                    }
                    if (Extent > 0)
                    {
                        sb.Append(" EXTENT ").Append(Extent);
                    }
                }

                return sb.ToString();
            }
        }

    }
}
