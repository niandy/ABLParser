using System;
using log4net;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics.Contracts;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    

    public class InputSource
    {        
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(InputSource));

        // TODO Almost sure those two fields are useless
        private readonly bool primaryInput;
        private readonly int sourceNum;

        private readonly string fileContent;
        private readonly int fileIndex;
        private readonly bool macroExpansion;

        private int nextCol = 1;
        private int nextLine = 1;
        private int currPos;
        private string currAnalyzeSuspend = null;

        public InputSource(int sourceNum, string str, int fileIndex, int line, int col)
        {
            LOGGER.Debug(String.Format("New InputSource object for macro element '{0}'", str));
            this.sourceNum = sourceNum;
            this.primaryInput = false;
            this.fileContent = str;
            this.fileIndex = fileIndex;
            this.macroExpansion = true;
            this.nextLine = line;
            this.nextCol = col;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public InputSource(int sourceNum, File file, Charset charset, int fileIndex, boolean skipXCode) throws IOException
        public InputSource(int sourceNum, FileInfo file, Encoding charset, int fileIndex, bool skipXCode) : this(sourceNum, file, charset, fileIndex, false, skipXCode)
        {
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public InputSource(int sourceNum, File file, Charset charset, int fileIndex, boolean skipXCode, boolean isPrimary) throws IOException
        public InputSource(int sourceNum, FileInfo file, Encoding charset, int fileIndex, bool skipXCode, bool isPrimary)
        {
            Contract.Requires(file != null);
            LOGGER.Debug($"New InputSource object for file '{file.Name}'");
            this.sourceNum = sourceNum;
            this.primaryInput = isPrimary;
            this.fileIndex = fileIndex;
            this.macroExpansion = false;
            using (Stream input = file.Open(FileMode.Open, FileAccess.Read))
            {
                BinaryReader src = new BinaryReader(input, charset);
                if ((src.PeekChar() == 0x11) || (src.PeekChar() == 0x13))
                {
                    if (skipXCode)
                    {
                        this.fileContent = " ";
                    }
                    else
                    {
                        throw new XCodedFileException(file.Name);
                    }
                }
                else
                {
                    this.fileContent = new string(src.ReadChars((int)input.Length));
                }
                src.Dispose();
            }
            // Skip first character if it's a BOM
            if (fileContent.Length > 0 && fileContent[0] == (char)0xFEFF)
            {
                currPos++;
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
        //ORIGINAL LINE: public InputSource(int sourceNum, String fileName, ByteSource src, Charset charset, int fileIndex, boolean skipXCode, boolean isPrimary) throws IOException
        public InputSource(int sourceNum, string fileName, Stream src, Encoding charset, int fileIndex, bool skipXCode, bool isPrimary)
        {
            LOGGER.Debug($"New InputSource object for include stream '{fileName}'");
            BinaryReader br = new BinaryReader(src, charset);
            this.sourceNum = sourceNum;
            this.primaryInput = isPrimary;
            this.fileIndex = fileIndex;
            this.macroExpansion = false;
            if ((br.PeekChar() == 0x11) || (br.PeekChar() == 0x13))
            {
                fileContent = skipXCode ? " " : throw new XCodedFileException(fileName);                
            }
            else
            {                
                fileContent = new string(br.ReadChars((int)src.Length));
            }
            // Skip first character if it's a BOM
            if (fileContent.Length > 0 && fileContent[0] == (char)0xFEFF)
            {
                currPos++;
            }
            br.Dispose();
        }

        public virtual int Get()
        {
            // We use nextLine and nextCol - that way '\n' can have a column number at the end of the line it's on, rather than
            // at column 0 of the following line.
            // If this is a macro expansion, then we don't increment column or line number. Those just stay put at the file
            // position where the macro '{' was referenced.
            if (currPos >= fileContent.Length)
            {
                return -1;
            }

            int currChar = fileContent[currPos++];
            if (!macroExpansion)
            {
                if (currChar == '\n')
                {
                    nextLine++;
                    nextCol = 1;
                }
                else
                {
                    nextCol++;
                }
            }
            return currChar;
        }

        public virtual int FileIndex => fileIndex;
        public virtual bool MacroExpansion => macroExpansion;
        public virtual int SourceNum => sourceNum;
        public virtual int NextCol
        {
            get => nextCol;            
            set => nextCol = value;            
        }

        public virtual int NextLine
        {
            get => nextLine;            
            set => nextLine = value;            
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @CheckForNull public String getAnalyzeSuspend()
        public virtual string AnalyzeSuspend
        {
            get => currAnalyzeSuspend;            
            set => currAnalyzeSuspend = value;
        }

        public virtual bool PrimaryInput => primaryInput;           

        public virtual string Content => fileContent;                
    }

}
