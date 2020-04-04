using ABLParser.Prorefactor.Macrolevel;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Refactor.Settings;
using OperatingSystem = ABLParser.Prorefactor.Refactor.Settings.ProparseSettings.OperatingSystem;
using Antlr4.Runtime;
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ABLParser.Prorefactor.Core;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ABLParserTests")]
namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// A preprocessor contains one or more IncludeFiles.
    /// 
    /// There is a special IncludeFile object created for the top-level program (ex: .p or .w).
    /// 
    /// Every time the lexer has to scan an include file, we create an IncludeFile object, for managing include file
    /// arguments and pre-processor scopes.
    /// 
    /// We keep an InputSource object, which has an input stream.
    /// 
    /// Each IncludeFile object will have one or more InputSource objects.
    /// 
    /// The bottom InputSource object for an IncludeFile is the input for the include file itself.
    /// 
    /// Each upper (potentially stacked) InputSource object is for an argument reference: - include file argument reference
    /// or reference to scoped or global preprocessor definition
    /// 
    /// We keep a reference to the input stream "A" in the InputSource object so that if "A" spawns a new input stream "B",
    /// we can return to "A" when we are done with "B".
    /// </summary>    
    public class ProgressLexer : ITokenSource, IPreprocessor
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(ProgressLexer));

        private static readonly Regex regexNumberedArg = new Regex("\\{\\d+\\}");
        private static readonly Regex regexEmptyCurlies = new Regex("\\{\\s*\\}");
        private const int EOF_CHAR = -1;
        private const int SKIP_CHAR = -100;

        public const int PROPARSE_DIRECTIVE = -101;
        public const int INCLUDE_DIRECTIVE = -102;

        private readonly IProparseSettings ppSettings;
        // Do we only read tokens ?
        private readonly bool lexOnly;
        private readonly IntegerIndex<string> filenameList = new IntegerIndex<string>();

        // How many levels of &IF FALSE are we currently into?
        private int consuming = 0;

        private int currChar;
        private int currFile;
        private int currSourceNum;
        private int currLine;
        private int currCol;
        private bool currMacroExpansion;

        // Are we in the middle of a comment?
        private bool doingComment;
        // Would you append the currently returned character to escapeText in order to see what the original code looked like
        // before escape processing? (See escape().)
        private bool escapeAppend;
        // Is the currently returned character escaped?
        private bool escapeCurrent;
        // Was there escaped text before current character?
        private bool wasEscape;
        private string escapeText;
        // Is the current '.' a name dot? (i.e. not followed by whitespace) */
        private bool nameDot;
        private string proparseDirectiveText;
        private string includeDirectiveText;
        private FilePos textStart;

        private readonly IPreprocessorEventListener lstListener;

        private IncludeFile currentInclude;
        private InputSource currentInput;
        private readonly IDictionary<string, string> globalDefdNames = new Dictionary<string, string>();
        private bool gotLookahead = false;
        private readonly LinkedList<IncludeFile> includeVector = new LinkedList<IncludeFile>();
        private int laFile;
        private int laLine;
        private int laCol;
        private int laSourceNum;
        private int laChar;
        private int safetyNet = 0;
        private int sequence = 0;
        private int sourceCounter = -1;

        // From ProgressLexer
        private readonly Lexer lexer;
        private readonly RefactorSession session;
        private readonly ITokenSource wrapper;

        // Cached include files for current lexer
        private readonly IDictionary<string, int> includeCache = new Dictionary<string, int>();
        private readonly IDictionary<int, string> includeCache2 = new Dictionary<int, string>();
        /// <summary>
        /// An existing reference to the input stream is required for construction. The caller is responsible for closing that
        /// stream once parsing is complete.
        /// </summary>
        /// <param name="session"> Current parser session </param>
        /// <param name="src"> Byte array of source file </param>
        /// <param name="fileName"> Referenced under which name </param>
        /// <param name="lexOnly"> Don't use preprocessor </param>
        /// <exception cref="UncheckedIOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: public ProgressLexer(RefactorSession session, ByteSource src, String fileName, boolean lexOnly)
        public ProgressLexer(RefactorSession session, Stream src, string fileName, bool lexOnly)
        {
            LOGGER.Debug($"New ProgressLexer instance {fileName}");
            this.ppSettings = session.ProparseSettings;
            this.session = session;
            this.lexOnly = lexOnly;

            // Create input source with flag isPrimaryInput=true
            try
            {
                currentInput = new InputSource(++sourceCounter, fileName, src, session.Charset, currFile, true, true);
            }
            catch (IOException caught)
            {
                throw new FileLoadException("Primary input read error", caught);
            }
            currFile = AddFilename(fileName);
            currentInclude = new IncludeFile(fileName, currentInput);
            includeVector.AddLast(currentInclude);
            currSourceNum = currentInput.SourceNum;
            lstListener = new PreprocessorEventListener();

            lexer = new Lexer(this);
            ITokenSource postlexer = lexOnly ? (ITokenSource)new NoOpPostLexer(lexer) : new PostLexer(lexer);
            ITokenSource filter0 = new NameDotTokenFilter(postlexer);
            ITokenSource filter1 = new TokenList(filter0);
            wrapper = new FunctionKeywordTokenFilter(filter1);
            LOGGER.Debug($"Created FunctionKeywordTokenFilter");
        }

        /// <summary>
        /// Test-only constructor, no token filters added
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: protected ProgressLexer(RefactorSession session, ByteSource src, String fileName)
        internal ProgressLexer(RefactorSession session, Stream src, string fileName)
        {
            LOGGER.Debug($"New ProgressLexer instance {fileName}");
            this.ppSettings = session.ProparseSettings;
            this.session = session;
            this.lexOnly = false;

            // Create input source with flag isPrimaryInput=true
            try
            {
                currentInput = new InputSource(++sourceCounter, fileName, src, session.Charset, currFile, true, true);
            }
            catch (IOException caught)
            {
                throw new FileLoadException("Primary input read error", caught);
            }
            currFile = AddFilename(fileName);
            currentInclude = new IncludeFile(fileName, currentInput);
            includeVector.AddLast(currentInclude);
            currSourceNum = currentInput.SourceNum;
            lstListener = new PreprocessorEventListener();

            lexer = new Lexer(this);
            wrapper = new NoOpPostLexer(lexer);
            LOGGER.Debug($"Created NoOpPostLexer");
        }

        public virtual string MainFileName
        {
            get
            {
                return filenameList.GetValue(0);
            }
        }

        public virtual IntegerIndex<string> FilenameList
        {
            get
            {
                return filenameList;
            }
        }

        public virtual string GetFilename(int fileIndex)
        {
            return filenameList.GetValue(fileIndex);
        }

        protected internal virtual int AddFilename(string filename)
        {
            if (filenameList.HasValue(filename))
            {
                return filenameList.GetIndex(filename);
            }

            return filenameList.Add(filename);
        }

        // **********************
        // TokenSource interface
        // **********************

        // Only exposed to unit test classes
        protected internal virtual ITokenSource TokenSource
        {
            get
            {
                return wrapper;
            }
        }

        public IToken NextToken()
        {
            return wrapper.NextToken();
        }

        public int Line => wrapper.Line;            
        public int Column => wrapper.Column;
        public ICharStream InputStream => wrapper.InputStream;
        public string SourceName => Filename;
        public ITokenFactory TokenFactory
        {
            set => wrapper.TokenFactory = value;            
            get=> wrapper.TokenFactory;            
        }


        // ****************************
        // End of TokenSource interface
        // ****************************

        // ****************************
        // IPreprocessor implementation
        // ****************************

        public string Defined(string argName)
        {
            // Yes, the precedence for defined() really is 3,2,3,1,0.
            // First look for local SCOPED define
            if (currentInclude.IsNameDefined(argName))
            {
                return "3";
            }
            // Second look for named include arg
            if (currentInclude.GetNamedArg(argName) != null)
            {
                return "2";
            }
            // Third look for a non-local SCOPED define
            foreach (IncludeFile incl in includeVector)
            {
                if (incl.IsNameDefined(argName))
                {
                    return "3";
                }
            }
            // Finally, check for global define
            if (globalDefdNames.ContainsKey(argName))
            {
                return "1";
            }
            // Not defined
            return "0";
        }

        public void DefGlobal(string argName, string argVal)
        {
            LOGGER.Debug($"Global define '{argName}': '{argVal}'");
            globalDefdNames[argName] = argVal;
        }

        public void DefScoped(string argName, string argVal)
        {
            LOGGER.Debug($"Scoped define '{argName}': '{argVal}'");
            currentInclude.ScopeDefine(argName, argVal);
        }

        public string GetArgText(int argNum)
        {
            return currentInclude.GetNumberedArgument(argNum);
        }

        public string GetArgText(string argName)
        {
            LOGGER.Debug($"getArgText('{argName}')");
            string ret;
            // First look for local &SCOPE define
            ret = currentInclude.GetValue(argName);
            if (ret is object)
            {
                LOGGER.Debug($"Found scope-defined variable: '{ret}'");
                return ret;
            }
            // Second look for a named include file argument
            ret = currentInclude.GetNamedArg(argName);
            if (ret is object)
            {
                LOGGER.Debug($"Found named argument: '{ret}'");
                return ret;
            }
            // Third look for a non-local SCOPED define
            for (int i = includeVector.Count - 1; i >= 0; --i)
            {
                ret = includeVector.ElementAt(i).GetValue(argName);
                if (ret is object)
                {
                    LOGGER.Debug($"Found non-local scope-defined variable: '{ret}'");
                    return ret;
                }
            }
            // Fourth look for a global define            
            if (globalDefdNames.TryGetValue(argName, out ret))
            {
                LOGGER.Debug($"Found global-defined variable: '{ret}'");
                return ret;
            }
            // * to return all include arguments, space delimited.
            if ("*".Equals(argName))
            {
                LOGGER.Debug("Return all include arugments");
                return currentInclude.AllArguments;
            }
            // &* to return all named include argument definitions
            if ("&*".Equals(argName))
            {
                LOGGER.Debug("Return all named include arugments");
                return currentInclude.AllNamedArgs;
            }
            // Built-ins
            if ("batch-mode".Equals(argName))
            {
                return Convert.ToString(ppSettings.BatchMode);
            }
            if ("opsys".Equals(argName))
            {
                return ppSettings.OpSys.Name;
            }
            if ("process-architecture".Equals(argName))
            {
                return ppSettings.ProcessArchitecture.ToString();
            }
            if ("window-system".Equals(argName))
            {
                return ppSettings.WindowSystem;
            }
            if ("file-name".Equals(argName))
            {
                // {&FILE-NAME}, unlike {0}, returns the filename as found on the PROPATH.
                ret = session.FindFile(currentInclude.GetNumberedArgument(0));
                // Progress seems to be converting the slashes for the appropriate OS.
                // I don't convert the slashes when I store the filename - instead I do it here.
                // (Saves us from converting the slashes for each and every include reference.)
                if (ppSettings.OpSys == OperatingSystem.UNIX)
                {
                    ret = ret.Replace('\\', '/');
                }
                else
                {
                    ret = ret.Replace('/', '\\');
                }
                return ret;
            }
            if ("line-number".Equals(argName))
            {
                return Convert.ToString(Line2);
            }
            if ("sequence".Equals(argName))
            {
                return Convert.ToString(sequence++);
            }

            // Not defined
            LOGGER.Debug("Nothing found...");
            return "";
        }

        public void Undef(string argName)
        {
            // First look for a local file scoped defined name to undef
            if (currentInclude.IsNameDefined(argName))
            {
                currentInclude.RemoveVariable(argName);
                return;
            }
            // Second look for a named include arg to undef
            if (currentInclude.UndefNamedArg(argName))
            {
                return;
            }
            // Third look for a non-local file scoped defined name to undef            
            LinkedListNode<IncludeFile> it = includeVector.Last;
            while (it.Previous != null)
            {
                it = it.Previous;
                IncludeFile incfile = it.Value;
                if (incfile.IsNameDefined(argName))
                {
                    incfile.RemoveVariable(argName);
                    return;
                }                
            }
            // Last, look for a global arg to undef
            UndefHelper(argName, globalDefdNames);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override public void analyzeSuspend(@Nonnull String analyzeSuspend)
        public void AnalyzeSuspend(string analyzeSuspend)
        {
            // Notify current include
            currentInput.AnalyzeSuspend = analyzeSuspend;
        }

        public void AnalyzeResume()
        {
            // Notify current include
            currentInput.AnalyzeSuspend = "";
        }

        private bool UndefHelper(string argName, IDictionary<string, string> names)
        {
            if (names.ContainsKey(argName))
            {
                names.Remove(argName);
                return true;
            }
            return false;
        }

        // ***********************************
        // End of IPreprocessor implementation
        // ***********************************

        internal virtual int Char
        {
            get
            {
                wasEscape = false;
                for (; ; )
                {
                    escapeCurrent = false;
                    if (gotLookahead)
                    {
                        LaUse();
                    }
                    else
                    {
                        GetRawChar();
                    }
                    switch (currChar)
                    {
                        case '\\':
                        case '~':
                            {
                                // Escapes are *always* processed, even inside strings and comments.
                                if ((currChar == '\\') && (ppSettings.OpSys == OperatingSystem.WINDOWS) && !ppSettings.UseBackslashAsEscape())
                                {
                                    return currChar;
                                }
                                int retChar = Escape();
                                if (retChar == '.')
                                {
                                    CheckForNameDot();
                                }
                                if (retChar != SKIP_CHAR)
                                {
                                    return retChar;
                                }
                                // else do another loop
                                break;
                            }
                        case '{':
                            // Macros are processed inside strings, but not inside comments.
                            if (doingComment)
                            {
                                return currChar;
                            }
                            else
                            {
                                MacroReference();
                                if ((currChar == PROPARSE_DIRECTIVE) || (currChar == INCLUDE_DIRECTIVE))
                                {
                                    return currChar;
                                }
                                // else do another loop
                            }
                            break;
                        case '.':
                            CheckForNameDot();
                            return currChar;
                        default:
                            return currChar;
                    }
                }
            }
        }

        // ****************************
        // IPreprocessor implementation
        // ****************************

        internal virtual bool LexOnly => lexOnly;
        internal virtual int CurrentColumn => currCol;
        internal virtual bool MacroExpansion => currMacroExpansion;
        internal virtual int FileIndex => currFile;
        internal virtual int Line2 => currLine;
        internal virtual int SourceNum => currSourceNum;

        /// <summary>
        /// We keep a record of discarded escape characters. This is in case the client wants to fetch those and use them. (Ex:
        /// Our lexer preserves original text inside string constants, including escape sequences).
        /// </summary>
        private int Escape()
        {
            // We may have multiple contiguous discarded characters
            // or a new escape sequence.
            if (wasEscape)
            {
                escapeText += (char)currChar;
            }
            else
            {
                wasEscape = true;
                escapeText = Convert.ToString((char)currChar);
                escapeAppend = true;
            }
            // Discard current character ('~' or '\\'), get next.
            GetRawChar();
            int retChar = currChar;
            escapeCurrent = true;
            switch (currChar)
            {
                case '\n':
                    // An escaped newline can be pretty much anywhere: mid-keyword, mid-identifier, between '*' and '/', etc.
                    // It is discarded.
                    escapeText += (char)currChar;
                    retChar = SKIP_CHAR;
                    break;
                case '\r':
                    // Lookahead to the next character.
                    // If it's anything other than '\n', we need to do normal processing on it. Progress does not strip '\r' the way
                    // it strips '\n'. There is one issue here - Progress treats "\r\r\n" the same as "\r\n". I'm not sure how I
                    // could implement it.
                    if (!gotLookahead)
                    {
                        LaGet();
                    }
                    if (laChar == '\n')
                    {
                        escapeText += "\r\n";
                        LaUse();
                        retChar = SKIP_CHAR;
                    }
                    else
                    {
                        retChar = '\r';
                    }
                    break;
                case 'r':
                    // An escaped 'r' or an escaped 'n' gets *converted* to a different character. We don't just drop chars.
                    escapeText += (char)currChar;
                    escapeAppend = false;
                    retChar = '\r';
                    break;
                case 'n':
                    // An escaped 'r' or an escaped 'n' gets *converted* to a different character. We don't just drop chars.
                    escapeText += (char)currChar;
                    escapeAppend = false;
                    retChar = '\n';
                    break;
                default:
                    escapeAppend = true;
                    break; // No change to retChar.                    
            }
            return retChar;
        }

        private string Filename => GetFilename(currentInput.FileIndex);

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @CheckForNull String getCurrentAnalyzeSuspend()
        internal virtual string CurrentAnalyzeSuspend => currentInput.AnalyzeSuspend;

        /// <summary>
        /// Deal with end of input stream, and switch to previous. Because Progress allows you to switch streams in the middle
        /// of a token, we have to completely override EOF handling, right at the point where we get() a new character from the
        /// input stream. If we are at an EOF other than the topmost program file, then we don't want the EOF to get into our
        /// character stream at all. If we've popped an include file off the stack (not just argument or preprocessor text),
        /// then we have to insert a space into the character stream, because that's what Progress's compiler does.
        /// </summary>
        private void GetRawChar()
        {
            currLine = currentInput.NextLine;
            currCol = currentInput.NextCol;
            currChar = currentInput.Get();
            currMacroExpansion = currentInput.MacroExpansion;
            if (currChar == 0xFFFD)
            {
                // This is the 'replacement' character in Unicode, used by Java as a
                // placeholder for a character which could not be converted.
                // We replace those characters at runtime with a space, and log an error
                LOGGER.Error($"Character conversion error in {Filename} at line {currLine} column {currCol} from encoding {session.Charset.EncodingName}");
                currChar = ' ';
            }
            while (currChar == EOF_CHAR)
            {
                switch (PopInput())
                {
                    case 0: // nothing left to pop
                        safetyNet++;
                        if (safetyNet > 100)
                        {
                            throw new ProparseRuntimeException("Proparse error. Infinite loop caught by preprocessor.");
                        }
                        return;
                    case 1: // popped an include file
                        currFile = currentInput.FileIndex;
                        currLine = currentInput.NextLine;
                        currCol = currentInput.NextCol;
                        currSourceNum = currentInput.SourceNum;
                        currMacroExpansion = currentInput.MacroExpansion;
                        currChar = ' ';
                        return;
                    case 2: // popped a macro ref or include arg ref
                        currFile = currentInput.FileIndex;
                        currLine = currentInput.NextLine;
                        currCol = currentInput.NextCol;
                        currChar = currentInput.Get(); // might be another EOF
                        currSourceNum = currentInput.SourceNum;
                        currMacroExpansion = currentInput.MacroExpansion;
                        break;
                    default:
                        throw new System.InvalidOperationException("Proparse error. popInput() returned unexpected value.");
                }
            }
        }

        /*
         * Get the next include reference arg, reposition the charpos. A doublequote will start a string - all this means is
         * that we'll collect whitespace. A singlequote does not have this effect.
         */
        private string IncludeRefArg(CharPos cp)
        {
            bool gobbleWS = false;
            StringBuilder theRet = new StringBuilder();
            // Iterate up to, but not including, closing curly.
            while (cp.pos < cp.chars.Length - 1)
            {
                char c = cp.chars[cp.pos];
                switch (c)
                {
                    case '"':
                        if (cp.chars[cp.pos + 1] == '"')
                        {
                            // quoted quote - does not open/close a string
                            theRet.Append('"');
                            ++cp.pos;
                            ++cp.pos;
                        }
                        else
                        {
                            gobbleWS = !gobbleWS;
                            ++cp.pos;
                        }
                        break;
                    case ' ':
                    case '\t':
                    case '\f':
                    case '\n':
                    case '\r':
                        if (gobbleWS)
                        {
                            theRet.Append(c);
                            ++cp.pos;
                        }
                        else
                        {
                            return theRet.ToString();
                        }
                        break;
                    default:
                        theRet.Append(c);
                        ++cp.pos;
                        break;
                }
            }
            return theRet.ToString();
        }

        /// <summary>
        /// Get the lookahead character. The caller is responsible for knowing that the lookahead isn't already there, ex: if
        /// (!gotLookahead) laGet();
        /// </summary>
        private void LaGet()
        {
            int saveFile = currFile;
            int saveLine = currLine;
            int saveCol = currCol;
            int saveSourceNum = currSourceNum;
            int saveChar = currChar;
            GetRawChar();
            gotLookahead = true;
            laFile = currFile;
            laLine = currLine;
            laCol = currCol;
            laChar = currChar;
            laSourceNum = currSourceNum;
            currFile = saveFile;
            currLine = saveLine;
            currCol = saveCol;
            currSourceNum = saveSourceNum;
            currChar = saveChar;
        }

        private void LaUse()
        {
            gotLookahead = false;
            currFile = laFile;
            currLine = laLine;
            currCol = laCol;
            currSourceNum = laSourceNum;
            currChar = laChar;
        }

        internal virtual void LexicalThrow(string theMessage)
        {
            throw new ProparseRuntimeException(Filename + ":" + Convert.ToString(Line2) + " " + theMessage);
        }

        private void MacroReference()
        {
            List<IncludeArg> incArgs = new List<IncludeArg>();

            this.textStart = new FilePos(currFile, currLine, currCol, currSourceNum);
            // Preserve the macro reference start point, because textStart get messed with if this macro reference itself contains any macro references.
            FilePos refPos = new FilePos(textStart.File, textStart.Line, textStart.Col, textStart.SourceNum);

            // Gather the macro reference text
            // Do not stop on escaped '}'
            StringBuilder refTextBldr = new StringBuilder("{");
            char macroChar = (char)Char;
            while ((macroChar != '}' || wasEscape) && macroChar != EOF_CHAR)
            {
                refTextBldr.Append(macroChar);
                macroChar = (char)Char;
            }
            if (macroChar == EOF_CHAR)
            {
                LexicalThrow("Unmatched curly brace");
            }
            refTextBldr.Append(macroChar); // should be '}'
            string refText = refTextBldr.ToString();
            CharPos cp = new CharPos(refText.ToCharArray(), 0);
            int refTextEnd = refText.Length;
            int closingCurly = refTextEnd - 1;

            // Proparse Directive
            if (refText.ToLower().StartsWith("{&_proparse_", StringComparison.Ordinal) && ppSettings.ProparseDirectives)
            {
                currChar = PROPARSE_DIRECTIVE;
                // We strip "{&_proparse_", trailing '}', and leading/trailing whitespace
                proparseDirectiveText = refText.Substring(12, closingCurly - 12).Trim();
                // This will be counted as a source whether picked up here or picked
                // up as a normal macro ref.
                ++sourceCounter;
                lstListener.MacroRef(refPos.Line, refPos.Col, "_proparse");
                return;
            }

            // {*} -- all arguments
            else if ("{*}".Equals(refText))
            {
                NewMacroRef("*", refPos);
                return;
            }

            // {&* -- all named arguments
            else if (refText.StartsWith("{&*", StringComparison.Ordinal))
            {
                NewMacroRef("&*", refPos);
                return;
            }

            // {(0..9)+} -- a numbered argument
            else if (regexNumberedArg.IsMatch(refText))
            {
                string theText = refText.Substring(1, closingCurly - 1);
                int argNum = int.Parse(theText);
                NewMacroRef(argNum, refPos);
                return;
            }

            // { } -- empty curlies - ignored
            else if (regexEmptyCurlies.IsMatch(refText))
            {
                return;
            }

            // {& -- named argument or macro expansion
            // Note that you can reference "{&}" to get an
            // undefined named include argument.
            // In that case, argName remains blank.
            // Trailing whitespace is trimmed.
            else if (refText.StartsWith("{&", StringComparison.Ordinal))
            {
                string argName = refText.Substring(2, closingCurly - 2).Trim().ToLower();
                NewMacroRef(argName, refPos);
                return;
            }

            else
            { // If we got here, it's an include file reference
                bool usingNamed = false;
                string argName;
                string argVal;

                // '{'
                cp.pos = 1; // skip '{'

                // whitespace?
                while (char.IsWhiteSpace(cp.chars[cp.pos]))
                {
                    ++cp.pos;
                }

                // filename
                string includeFilename = IncludeRefArg(cp);

                // whitespace?
                while (char.IsWhiteSpace(cp.chars[cp.pos]))
                {
                    ++cp.pos;
                }

                // no include args?
                if (cp.pos == closingCurly)
                {
                    // do nothing
                }

                else if (cp.chars[cp.pos] == '&')
                { // include '&' named args
                    usingNamed = true;
                    while (cp.pos != refTextEnd && cp.chars[cp.pos] == '&')
                    {
                        ++cp.pos; // skip '&'

                        // Arg name
                        // Consume to '=' or closing '}'
                        // discard all WS
                        argName = "";
                        while (cp.pos != refTextEnd)
                        {
                            if (cp.pos == closingCurly || cp.chars[cp.pos] == '=' || cp.chars[cp.pos] == '&')
                            {
                                break;
                            }
                            if (!(char.IsWhiteSpace(cp.chars[cp.pos])))
                            {
                                argName += cp.chars[cp.pos];
                            }
                            ++cp.pos;
                        }

                        argVal = "";
                        bool undefined = true;
                        if (cp.chars[cp.pos] == '=')
                        {
                            undefined = false;
                            // '=' with optional WS
                            ++cp.pos;
                            while (cp.pos != closingCurly && char.IsWhiteSpace(cp.chars[cp.pos]))
                            {
                                ++cp.pos;
                            }
                            // Arg val
                            if (cp.pos != closingCurly)
                            {
                                argVal = IncludeRefArg(cp);
                            }
                        }

                        // Add the argument name/val pair
                        incArgs.Add(new IncludeArg(argName, argVal, undefined));

                        // Anything not beginning with & is discarded
                        while (cp.pos != refTextEnd && cp.chars[cp.pos] != '&')
                        {
                            ++cp.pos;
                        }

                    } // while loop
                } // include '&' named args

                else
                { // include numbered args
                    usingNamed = false;
                    while (cp.pos != refTextEnd)
                    {
                        while (char.IsWhiteSpace(cp.chars[cp.pos]))
                        {
                            ++cp.pos;
                        }
                        // Are we at closing curly?
                        if (cp.pos == closingCurly)
                        {
                            break;
                        }
                        incArgs.Add(new IncludeArg("", IncludeRefArg(cp)));
                    }
                } // numbered args

                // If lex only, we generate a token
                if (lexOnly)
                {
                    currChar = INCLUDE_DIRECTIVE;
                    includeDirectiveText = refText.Trim();
                    return;
                }
                else
                {
                    // newInclude() returns false if filename is blank or currently
                    // "consuming" due to &IF FALSE.
                    // newInclude() will throw() if file not found or cannot be opened.
                    if (NewInclude(includeFilename))
                    {
                        // Unlike currline and currcol,
                        // currfile is only updated with a push/pop of the input stack.
                        currFile = currentInput.FileIndex;
                        currSourceNum = currentInput.SourceNum;
                        lstListener.Include(refPos.Line, refPos.Col, currFile, includeFilename);
                        // Add the arguments to the new include object.
                        int argNum = 1;
                        foreach (IncludeArg incarg in incArgs)
                        {
                            if (usingNamed)
                            {
                                currentInclude.AddNamedArgument(incarg.argName, incarg.argVal);
                            }
                            else
                            {
                                currentInclude.AddArgument(incarg.argVal);
                            }
                            lstListener.IncludeArgument(usingNamed ? incarg.argName : Convert.ToString(argNum), incarg.argVal, incarg.undefined);
                            argNum++;
                        }
                    }
                }

            } // include file reference

        } // macroReference()

        private bool NewInclude(string referencedWithName)
        {
            // Progress doesn't enter include files if &IF FALSE
            // It *is* possible to get here with a blank include file
            // name. See bug#034. Don't enter if the includefilename is blank.
            string fName = referencedWithName.Trim().Replace('\\', '/');
            if (IsConsuming || LexOnly || fName.Length == 0)
            {
                return false;
            }

            FileInfo incFile = null;
            // Did we ever read file with same referenced name ?            
            if (!includeCache.TryGetValue(fName, out int idx))
            {
                // No, then we have to read file
                incFile = session.FindFile3(fName);
                if (incFile == null)
                {
                    throw new FileNotFoundException("Include not found", new IncludeFileNotFoundException(Filename, referencedWithName));
                }
                try
                {
                    idx = AddFilename(Path.GetFullPath(incFile.FullName));
                }
                catch (IOException caught)
                {
                    throw new FileLoadException("Add file excpetion", caught);
                }
                includeCache[fName] = (int)idx;
            }

            if (includeCache2.TryGetValue(idx, out string cache))
            {
                try
                {
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(cache));
                    currentInput = new InputSource(++sourceCounter, fName, ms, Encoding.UTF8, (int)idx, ppSettings.SkipXCode, false);
                    ms.Dispose();
                }
                catch (IOException caught)
                {
                    throw new FileLoadException("Include file read exception", caught);
                }
            }
            else
            {
                try
                {
                    currentInput = new InputSource(++sourceCounter, incFile, session.Charset, (int)idx, ppSettings.SkipXCode, false);
                    includeCache2[(int)idx] = currentInput.Content;
                }
                catch (IOException caught)
                {
                    throw new FileLoadException("Include file read exception", caught);
                }
            }
            currentInclude = new IncludeFile(referencedWithName, currentInput);
            includeVector.AddLast(currentInclude);
            LOGGER.Debug($"Entering file: {Filename}");

            return true;
        }

        /// <summary>
        /// New macro or named/numbered argument reference. Input either macro/argument name or the argument number, as well as
        /// fileIndex, line, and column where the '{' appeared. Returns false if there's nothing to expand.
        /// </summary>
        private void NewMacroRef(string macroName, FilePos refPos)
        {
            // Using this trick: {{&undefined-argument}{&*}}
            // it is possible to get line breaks into what we
            // get here as the macroName. See test data bug15.p and bug15.i.
            lstListener.MacroRef(refPos.Line, refPos.Col, macroName);
            NewMacroRef2(GetArgText(macroName), refPos);
        }

        private void NewMacroRef(int argNum, FilePos refPos)
        {
            lstListener.MacroRef(refPos.Line, refPos.Col, Convert.ToString(argNum));
            NewMacroRef2(GetArgText(argNum), refPos);
        }

        private void NewMacroRef2(string theText, FilePos refPos)
        {
            if (theText.Length == 0)
            {
                ++sourceCounter;
                lstListener.MacroRefEnd();
                return;
            }
            // We must expand macros even if consuming,
            // because we can have &ENDIF inside a preprocesstoken
            // For a macro/argument expansion, we use the file/line/col of
            // the opening curly '{' of the ref file, for all characters/tokens.
            currentInput = new InputSource(++sourceCounter, theText, refPos.File, refPos.Line, refPos.Col);
            currentInclude.AddInputSource(currentInput);
            currentInput.NextLine = refPos.Line;
            currentInput.NextCol = refPos.Col;
        }
        /// <summary>
        /// Cleanup work, once the parse is complete.
        /// </summary>
        public virtual void ParseComplete()
        {
            while (PopInput() != 0)
            {
                // No-op
            }
            // Clean up the temporary junk
            includeCache.Clear();
            includeCache2.Clear();
            currentInclude = null;
            currentInput = null;
        }

        /// <summary>
        /// Pop the current input source off the stack. Returns true if we've popped off the end of an include file, false if
        /// we've just popped off an argument or preprocessor text. The calling program has to know this, to add the space ' '
        /// at the end of the include reference.
        /// </summary>
        private int PopInput()
        {
            // Returns 2 if we popped a macro or arg ref off the input stack.
            // Returns 1 if we popped an include file off the input stack.
            // Returns 0 if there's nothing left to pop.
            // There's no need to pop the primary input source, so we leave it
            // around. There's a good chance that something will want to refer
            // to currentInclude or currentInput anyway, even though it's done.
            InputSource tmp;
            if ((tmp = currentInclude.Pop()) != null)
            {
                currentInput = tmp;
                lstListener.MacroRefEnd();
                return 2;
            }
            else if (includeVector.Count > 1)
            {
                includeVector.RemoveLast();
                currentInclude = includeVector.Last.Value;
                currentInput = currentInclude.LastSource;
                lstListener.IncludeEnd();
                LOGGER.Debug($"Back to file: {Filename}");
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private void CheckForNameDot()
        {
            // Have to check for nameDot in the preprocessor because nameDot is true
            // even if the next character is a '{' which eventually expands
            // out to be a space character.
            if (!gotLookahead)
            {
                LaGet();
            }
            nameDot = (laChar != EOF_CHAR) && !char.IsWhiteSpace((char)laChar) && (laChar != '.');
        }

        public virtual bool DoingComment
        {
            set => this.doingComment = value;
        }

        public virtual bool EscapeAppend => escapeAppend;
        public virtual bool EscapeCurrent => escapeCurrent;
        public virtual string EscapeText => escapeText;
        public virtual bool WasEscape => wasEscape;
        public virtual bool NameDot => nameDot;
        public virtual string ProparseDirectiveText => proparseDirectiveText;
        public virtual string IncludeDirectiveText => includeDirectiveText;
        public virtual bool IsConsuming => consuming != 0;
        public virtual int Consuming => consuming;
        public virtual void IncrementConsuming()
        {
            consuming++;
        }

        public virtual void DecrementConsuming()
        {
            consuming--;
        }
        public virtual FilePos TextStart
        {
            get
            {
                return textStart;
            }
        }

        public virtual IPreprocessorEventListener LstListener
        {
            get
            {
                return lstListener;
            }
        }

        public virtual JPNodeMetrics Metrics
        {
            get
            {
                return new JPNodeMetrics(lexer.Loc, lexer.CommentedLines);
            }
        }

        public virtual IncludeRef MacroGraph
        {
            get
            {
                return ((PreprocessorEventListener)lstListener).MacroGraph;
            }
        }

        public virtual IProparseSettings ProparseSettings
        {
            get
            {
                return ppSettings;
            }
        }

        public virtual bool AppBuilderCode
        {
            get
            {
                return ((PreprocessorEventListener)lstListener).AppBuilderCode;
            }
        }

        private class CharPos
        {
            public readonly char[] chars;
            public int pos;

            internal CharPos(char[] c, int p)
            {
                chars = c;
                pos = p;
            }
        }

        public class FilePos
        {
            private readonly int file;
            private readonly int line;
            private readonly int col;
            private readonly int sourceNum;

            public FilePos(int file, int line, int col, int sourceNum)
            {
                this.file = file;
                this.line = line;
                this.col = col;
                this.sourceNum = sourceNum;
            }

            public virtual int File => file;
            public virtual int Line => line;
            public virtual int Col => col;
            public virtual int SourceNum => sourceNum;

            public override int GetHashCode()
            {
                return (13 * file) + (17 * line) + (31 * col) + (37 * sourceNum);
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                if (this.GetType() == obj.GetType())
                {
                    FilePos fp = (FilePos)obj;
                    return (fp.file == file) && (fp.line == line) && (fp.col == col) && (fp.sourceNum == sourceNum);
                }
                else
                {
                    return false;
                }
            }
        }
        private class IncludeArg
        {
            public readonly string argName;
            public readonly string argVal;
            public readonly bool undefined;

            internal IncludeArg(string argName, string argVal) : this(argName, argVal, false)
            {
            }

            internal IncludeArg(string argName, string argVal, bool undefined)
            {
                this.argName = argName;
                this.argVal = argVal;
                this.undefined = undefined;
            }            
        }

    }

    /*
     * EOF Notes
     * 
     * Note[1] Cannot track file/line/col of include ref arguments. Why? Because it gathers the {...} into a string, and
     * preprocessing takes place on that text as it is gathered into the string. (Escape sequences, especially.) Once that
     * is complete, *then* it begins to evaluate the string for include arguments. The only option is to try to synch the
     * source with the listing.
     * 
     */

}
