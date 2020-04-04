using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ABLParser.Prorefactor.Core;
using log4net;
using ABLParser.Prorefactor.Macrolevel;
using System.Globalization;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class Lexer
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(Lexer));

        private const int EOF_CHAR = -1;

        /// <summary>
        /// Lowercase value of current character </summary>
        private int currChar;

        /// <summary>
        /// Current character, before being lowercased </summary>
        private int currInt;
        private int currFile, currLine, currCol;
        private bool currMacro, prevMacro;
        private int prevFile, prevLine, prevCol;

        private int currStringType;
        private readonly StringBuilder currText = new StringBuilder();

        private readonly ProgressLexer prepro;

        private bool gettingAmpIfDefArg = false;
        private bool preserve = false;
        private int preserveFile;
        private int preserveLine;
        private int preserveCol;
        private int preserveSource;
        private int preserveChar;

        private int textStartFile;
        private int textStartLine;
        private int textStartCol;
        private int textStartSource;

        private readonly ISet<int> comments = new HashSet<int>();
        private readonly ISet<int> loc = new HashSet<int>();

        internal Lexer(ProgressLexer prepro)
        {
            this.prepro = prepro;
            GetChar(); // We always assume "currChar" is available.
        }
        //////////////// Lexical productions listed first, support functions follow.
        public virtual ProToken NextToken()
        {
            LOGGER.Debug("Entering nextToken()");
            for (; ; )
            {

                if (preserve)
                {
                    // The preserved character is the character prior to currChar.
                    textStartFile = preserveFile;
                    textStartLine = preserveLine;
                    textStartCol = preserveCol;
                    textStartSource = preserveSource;
                    currText.Length = 1;
                    currText[0] = (char)preserveChar;
                    PreserveDrop(); // we are done with the preservation
                    if (preserveChar == '.')
                    {
                        return PeriodStart();
                    }
                    else if (preserveChar == ':')
                    {
                        return Colon();
                    }
                }

                // Proparse Directive
                // Check this before setting currText...
                // we don't want BEGIN_PROPARSE_DIRECTIVE in the text
                if (currInt == ProgressLexer.PROPARSE_DIRECTIVE)
                {
                    textStartFile = prepro.TextStart.File;
                    textStartLine = prepro.TextStart.Line;
                    textStartCol = prepro.TextStart.Col;
                    textStartSource = prepro.TextStart.SourceNum;
                    GetChar();
                    return MakeToken(ABLNodeType.PROPARSEDIRECTIVE, prepro.ProparseDirectiveText);
                }
                else if (currInt == ProgressLexer.INCLUDE_DIRECTIVE)
                {
                    textStartFile = prepro.TextStart.File;
                    textStartLine = prepro.TextStart.Line;
                    textStartCol = prepro.TextStart.Col;
                    textStartSource = prepro.TextStart.SourceNum;
                    GetChar();
                    return MakeToken(ABLNodeType.INCLUDEDIRECTIVE, prepro.IncludeDirectiveText);
                }

                textStartFile = prepro.FileIndex;
                textStartLine = prepro.Line2;
                textStartCol = prepro.CurrentColumn;
                textStartSource = prepro.SourceNum;
                currText.Length = 1;
                currText[0] = (char)currInt;

                if (gettingAmpIfDefArg)
                {
                    GetChar();
                    gettingAmpIfDefArg = false;
                    return AmpIfDefArg();
                }

                switch (currChar)
                {

                    case '\t':
                    case '\n':
                    case '\f':
                    case '\r':
                    case ' ':
                        GetChar();
                        return Whitespace();

                    case '"':
                    case '\'':
                        if (prepro.EscapeCurrent)
                        {
                            GetChar();
                            // Escaped quote does not start a string
                            return Id(ABLNodeType.FILENAME);
                        }
                        else
                        {
                            currStringType = currInt;
                            GetChar();
                            return QuotedString();
                        }

                    case '/':
                        GetChar();
                        if (currChar == '*')
                        {
                            return Comment();
                        }
                        else if (currChar == '/')
                        {
                            return SingleLineComment();
                        }
                        else if (currChar == '(' || CurrIsSpace())
                        {
                            // slash (division) can only be followed by whitespace or '('
                            // ...that's what I found empirically, anyway. (jag 2003/05/09)
                            return MakeToken(ABLNodeType.SLASH);
                        }
                        else
                        {
                            Append();
                            GetChar();
                            return Id(ABLNodeType.FILENAME);
                        }
                    case ':':
                        GetChar();
                        return Colon();

                    case '&':
                        GetChar();
                        return AmpText();
                    case '@':
                        GetChar();
                        if (CurrIsSpace())
                        {
                            return MakeToken(ABLNodeType.LEXAT);
                        }
                        else
                        {
                            Append();
                        }
                        GetChar();
                        return Id(ABLNodeType.ANNOTATION);
                    case '[':
                        GetChar();
                        return MakeToken(ABLNodeType.LEFTBRACE);
                    case ']':
                        GetChar();
                        return MakeToken(ABLNodeType.RIGHTBRACE);
                    case '^':
                        GetChar();
                        return MakeToken(ABLNodeType.CARET);
                    case ',':
                        GetChar();
                        return MakeToken(ABLNodeType.COMMA);
                    case '!':
                        GetChar();
                        return MakeToken(ABLNodeType.EXCLAMATION);
                    case '=':
                        GetChar();
                        return MakeToken(ABLNodeType.EQUAL);
                    case '(':
                        GetChar();
                        return MakeToken(ABLNodeType.LEFTPAREN);
                    case ')':
                        GetChar();
                        return MakeToken(ABLNodeType.RIGHTPAREN);
                    case ';':
                        GetChar();
                        return MakeToken(ABLNodeType.SEMI);
                    case '*':
                        GetChar();
                        return MakeToken(ABLNodeType.STAR);
                    case '?':
                        GetChar();
                        return MakeToken(ABLNodeType.UNKNOWNVALUE);
                    case '`':
                        GetChar();
                        return MakeToken(ABLNodeType.BACKTICK);

                    case '0':
                        GetChar();
                        if ((currChar == 'x') || (currChar == 'X'))
                        {
                            Append();
                            GetChar();
                            return DigitStart(true);
                        }
                        else
                        {
                            return DigitStart(false);
                        }

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        GetChar();
                        return DigitStart(false);

                    case '.':
                        GetChar();
                        return PeriodStart();

                    case '>':
                        GetChar();
                        if (currChar == '=')
                        {
                            Append();
                            GetChar();
                            return MakeToken(ABLNodeType.GTOREQUAL);
                        }
                        else
                        {
                            return MakeToken(ABLNodeType.RIGHTANGLE);
                        }
                    case '<':
                        GetChar();
                        if (currChar == '>')
                        {
                            Append();
                            GetChar();
                            return MakeToken(ABLNodeType.GTORLT);
                        }
                        else if (currChar == '=')
                        {
                            Append();
                            GetChar();
                            return MakeToken(ABLNodeType.LTOREQUAL);
                        }
                        else
                        {
                            return MakeToken(ABLNodeType.LEFTANGLE);
                        }

                    case '+':
                        GetChar();
                        return PlusMinusStart(ABLNodeType.PLUS);
                    case '-':
                        GetChar();
                        return PlusMinusStart(ABLNodeType.MINUS);

                    case '#':
                    case '|':
                    case '%':
                        GetChar();
                        return Id(ABLNodeType.FILENAME);

                    default:
                        if (currInt == EOF_CHAR)
                        {
                            GetChar(); // preprocessor will catch any infinite loop on this.
                            return MakeToken(ABLNodeType.EOF_ANTLR4, "");
                        }
                        else
                        {
                            GetChar();
                            return Id(ABLNodeType.ID);
                        }

                }
            }
        }
        /// <summary>
        /// Get argument for &IF DEFINED(...). The nextToken function is necessarily the main entry point. This is just a
        /// wrapper around that.
        /// </summary>
        internal virtual ProToken GetAmpIfDefArg()
        {
            LOGGER.Debug("Entering getAmpIfDefArg()");

            gettingAmpIfDefArg = true;
            return NextToken();
        }

        /// <summary>
        /// Get the text between the parens for &IF DEFINED(...). The compiler seems to allow any number of tokens between the
        /// parens, and like with an &Name reference, it allows embedded comments. Here, I'm allowing for the embedded comments
        /// and just gathering all the text up to the closing paren. Hopefully that will do it.
        /// 
        /// The compiler doesn't seem to ignore extra tokens. For example, &if defined(ab cd) does not match a macro named
        /// "ab". It doesn't match "abcd" either, so all I can guess is that they are combining the text of all the tokens
        /// between the parens. I haven't found any macro name that matches &if defined(ab"cd").
        /// 
        /// The compiler works different here than it does for a typical ID token. An ID token (like a procedure name) may
        /// contain arbitrary quotation marks. Within an &if defined() function, the quotation marks must match. I don't know
        /// if that really makes a difference, because the quoted string can't contain a paren ')' anyway, so as far as I can
        /// tell we can ignore quotation marks and just watch for the closing paren. A macro name can't contain any quotation
        /// marks anyway, so for all I know the compiler's handling of quotes within defined() may just be an artifact of its
        /// lexer. I don't think there's any way to get whitespace into a macro name either.
        /// </summary>
        private ProToken AmpIfDefArg()
        {
            LOGGER.Debug("Entering ampIfDefArg()");

            for (; ; )
            {
                if (currChar == ')')
                {
                    goto loopBreak;
                }
                // Watch for comments.
                if (currChar == '/')
                {
                    GetChar();
                    if (currChar != '*')
                    {
                        Append('/');
                        goto loopContinue;
                    }
                    else
                    {
                        string s = currText.ToString();
                        Comment();
                        int l = currText.Length;
                        currText.Remove(0, l);
                        currText.Insert(0, s, l);
                        goto loopContinue;
                    }
                }
                Append();
                GetChar();
            loopContinue:;
            }
        loopBreak:
            return MakeToken(ABLNodeType.ID);
        }

        internal virtual ProToken Colon()
        {
            LOGGER.Debug("Entering colon()");

            if (currChar == ':')
            {
                Append();
                GetChar();
                return MakeToken(ABLNodeType.DOUBLECOLON);
            }
            if (CurrIsSpace())
            {
                return MakeToken(ABLNodeType.LEXCOLON);
            }
            return MakeToken(ABLNodeType.OBJCOLON);
        }

        internal virtual ProToken Whitespace()
        {
            LOGGER.Debug("Entering whitespace()");

            for (; ; )
            {
                switch (currChar)
                {
                    case ' ':
                    case '\t':
                    case '\f':
                    case '\n':
                    case '\r':
                        Append();
                        GetChar();
                        break;
                    default:
                        goto loopBreak;
                }
            }
        loopBreak:
            return MakeToken(ABLNodeType.WS);
        }
        internal virtual ProToken Comment()
        {
            LOGGER.Debug("Entering comment()");

            // Escapes in comments are processed because you can end a comment
            // with something dumb like: ~*~/
            // We preserve that text.
            // Note that macros are *not* expanded inside comments.
            // (See the preprocessor source)
            prepro.DoingComment = true;
            Append(); // currChar=='*'
            int commentLevel = 1;
            while (commentLevel > 0)
            {
                GetChar();
                UnEscapedAppend();
                _ = currText;
                if (currChar == '/')
                {
                    GetChar();
                    UnEscapedAppend();
                    if (currChar == '*')
                    {
                        commentLevel++;
                    }
                }
                else if (currChar == '*')
                {
                    while (currChar == '*')
                    {
                        GetChar();
                        UnEscapedAppend();
                        if (currChar == '/')
                        {
                            commentLevel--;
                        }
                    }
                }
                else if (currInt == EOF_CHAR)
                {
                    prepro.LexicalThrow("Missing end of comment");
                }
            }
            prepro.DoingComment = false;
            GetChar();
            return MakeToken(ABLNodeType.COMMENT);
        }

        internal virtual ProToken SingleLineComment()
        {
            LOGGER.Debug("Entering singleLineComment()");

            // Single line comments are treated just like regular comments,
            // everything till end of line is considered comment - no escape
            // character to look after
            prepro.DoingComment = true;
            Append(); // currChar=='/'

            while (true)
            {
                GetChar();
                if ((currInt == EOF_CHAR) || (!prepro.EscapeCurrent && (currChar == '\r' || currChar == '\n')))
                {
                    prepro.DoingComment = false;
                    return MakeToken(ABLNodeType.COMMENT);
                }
                else
                {
                    UnEscapedAppend();
                }
            }
        }
        internal virtual ProToken QuotedString()
        {
            LOGGER.Debug("Entering quotedString()");

            // Inside quoted strings (string constants) we preserve
            // the source code's original text - we don't discard
            // escape characters.
            // The preprocessor *does* expand macros inside strings.
            for (; ; )
            {
                if (currInt == EOF_CHAR)
                {
                    prepro.LexicalThrow("Unmatched quote");
                }
                UnEscapedAppend();
                if (currInt == currStringType && !prepro.EscapeCurrent)
                {
                    GetChar();
                    if (currInt == currStringType)
                    { // quoted quote
                        UnEscapedAppend();
                    }
                    else
                    {
                        break; // close quote
                    }
                }
                GetChar();
            }

            if (currChar == ':')
            {
                bool isStringAttributes = false;
                // Preserve the colon before calling getChar,
                // in case it belongs in the next token.
                PreserveCurrent();
                string theText = ":";
                for (; ; )
                {
                    GetChar();
                    switch (currChar)
                    {
                        case 'r':
                        case 'l':
                        case 'c':
                        case 't':
                        case 'u':
                        case 'x':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            theText += (char)currInt;
                            isStringAttributes = true;
                            break;
                        default:
                            goto for_loopBreak;
                    }
                }
            for_loopBreak:
                // either string attributes, or the preserved colon
                // goes into the next token.
                if (isStringAttributes)
                {
                    Append(theText);
                    PreserveDrop();
                }
                else
                {
                    // Fix current end position
                    prevCol--;
                    ProToken tok = MakeToken(ABLNodeType.QSTRING);
                    prevCol++;
                    return tok;
                }
            } // currChar==':'

            return MakeToken(ABLNodeType.QSTRING);
        }
        internal virtual ProToken DigitStart(bool hex)
        {
            LOGGER.Debug("Entering digitStart()");

            ABLNodeType ttype = ABLNodeType.NUMBER;
            for (; ; )
            {
                switch (currChar)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        Append();
                        GetChar();
                        break;
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        if (hex)
                        {
                            Append();
                            GetChar();
                            break;
                        }
                        else
                        {
                            Append();
                            GetChar();
                            if (ttype != ABLNodeType.FILENAME)
                            {
                                ttype = ABLNodeType.ID;
                            }
                            break;
                        }
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case '#':
                    case '$':
                    case '%':
                    case '&':
                    case '_':
                        Append();
                        GetChar();
                        if (ttype != ABLNodeType.FILENAME)
                        {
                            ttype = ABLNodeType.ID;
                        }
                        break;
                    // We don't know here if the plus or minus is in the middle or at the end.
                    // Don't change ttype.
                    case '+':
                    case '-':
                        Append();
                        GetChar();
                        break;
                    case '/':
                        Append();
                        GetChar();
                        if (ttype == ABLNodeType.NUMBER)
                        {
                            ttype = ABLNodeType.LEXDATE;
                        }
                        break;
                    case '\\':
                        Append();
                        GetChar();
                        ttype = ABLNodeType.FILENAME;
                        break;
                    case '.':
                        if (prepro.NameDot)
                        {
                            Append();
                            GetChar();
                            break;
                        }
                        else
                        {
                            goto for_loopBreak;
                        }
                    default:
                        goto for_loopBreak;
                }
            }
        for_loopBreak:
            return MakeToken(ttype);
        }
        internal virtual ProToken PlusMinusStart(ABLNodeType inputType)
        {
            LOGGER.Debug("Entering plusMinusStart()");
            ABLNodeType ttype = ABLNodeType.NUMBER;
            for (; ; )
            {
                switch (currChar)
                {
                    case '0':
                        Append();
                        GetChar();
                        if ((currChar == 'x') || (currChar == 'X'))
                        {
                            Append();
                            GetChar();
                            return DigitStart(true);
                        }
                        else
                        {
                            return DigitStart(false);
                        }

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        Append();
                        GetChar();
                        return DigitStart(false);

                    // Leave comma out of this. -1, might be part of an expression list.
                    case '#':
                    case '$':
                    case '%':
                    case '&':
                    case '/':
                    case '\\':
                    case '_':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        Append();
                        GetChar();
                        ttype = ABLNodeType.FILENAME;
                        break;
                    case '.':
                        if (prepro.NameDot)
                        {
                            Append();
                            GetChar();
                            break;
                        }
                        else
                        {
                            goto for_loopBreak;
                        }
                    default:
                        goto for_loopBreak;
                }            
            }
        for_loopBreak:
            if (currText.Length == 1)
            {
                return MakeToken(inputType);
            }
            else
            {
                return MakeToken(ttype);
            }
        }
        internal virtual ProToken PeriodStart()
        {
            LOGGER.Debug("Entering periodStart()");

            if (!char.IsDigit((char)currChar))
            {
                if (prepro.NameDot)
                {
                    return MakeToken(ABLNodeType.NAMEDOT);
                }
                else
                {
                    return MakeToken(ABLNodeType.PERIOD);
                }
            }
            ABLNodeType ttype = ABLNodeType.NUMBER;
            void Loop()
            {
                for (; ; )
                {
                    switch (currChar)
                    {
                        // We don't know here if the plus or minus is in the middle or at the end.
                        // Don't change _ttype.
                        case '+':
                        case '-':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            Append();
                            GetChar();
                            break;
                        case '#':
                        case '$':
                        case '%':
                        case '&':
                        case '/':
                        case '\\':
                        case '_':
                        case 'a':
                        case 'b':
                        case 'c':
                        case 'd':
                        case 'e':
                        case 'f':
                        case 'g':
                        case 'h':
                        case 'i':
                        case 'j':
                        case 'k':
                        case 'l':
                        case 'm':
                        case 'n':
                        case 'o':
                        case 'p':
                        case 'q':
                        case 'r':
                        case 's':
                        case 't':
                        case 'u':
                        case 'v':
                        case 'w':
                        case 'x':
                        case 'y':
                        case 'z':
                            Append();
                            GetChar();
                            ttype = ABLNodeType.FILENAME;
                            break;
                        default:
                            return;
                    }
                }
            }
            Loop();
            return MakeToken(ttype);
        }
        internal virtual ProToken Id(ABLNodeType inputTokenType)
        {
            LOGGER.Debug("Entering id()");

            // Tokens that start with a-z or underscore
            // - ID
            // - FILENAME
            // - keyword (testLiterals = true)
            // Also inputTokenType can be ANNOTATION for a token that starts with '@'.
            // Based on the PROGRESS online help, the following are the valid name characters.
            // Undocumented: you can use a slash in an index name! Arg!
            // Undocumented: the compiler allows you to start a block label with $
            // If we find a back slash, we know we're into a filename.
            // Extended characters (octal 200-377) can be used in identifiers, even at the beginning.
            ABLNodeType ttype = inputTokenType;
            for (; ; )
            {
                switch (currChar)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    case '_':
                    case '-':
                    case '$':
                    case '#':
                    case '%':
                    case '&':
                    case '/':
                    // For tokens like ALT-* and CTRL-` :
                    // Emperically, I found that the following are the only other
                    // characters that can be put into a key label. Things
                    // like ALT-, must be put into quotes "ALT-,", because
                    // the comma has special meaning in 4gl code.
                    // Note also that extended characters can come after CTRL- or ALT-.
                    // ('!'|'@'|'^'|'*'|'+'|';'|'"'|'`')
                    case '!':
                    case '"':
                    case '*':
                    case '+':
                    case ';':
                    case '@':
                    case '^':
                    case '`':
                        Append();
                        GetChar();
                        break;
                    case '\\':
                    case '\'':
                        Append();
                        GetChar();
                        if (ttype == ABLNodeType.ID)
                        {
                            ttype = ABLNodeType.FILENAME;
                        }
                        break;
                    case '.':
                        goto for_loopBreak;
                    default:
                        if (currInt >= 128 && currInt <= 255)
                        {
                            Append();
                            GetChar();
                            break;
                        }
                        else
                        {
                            goto for_loopBreak;
                        }
                }            
            }
        for_loopBreak:
            // See if it's a keyword
            if (ttype == ABLNodeType.ID)
            {
                ttype = ABLNodeType.GetLiteral(currText.ToString(), ttype);
            }
            return MakeToken(ttype);
        }
        internal virtual ProToken AmpText()
        {
            LOGGER.Debug("Entering ampText()");
            for (; ; )
            {
                if (char.IsLetterOrDigit((char)currInt) || (currInt >= 128 && currInt <= 255))
                {
                    Append();
                    GetChar();
                    continue;
                }
                switch (currChar)
                {
                    case '#':
                    case '$':
                    case '%':
                    case '&':
                    case '-':
                    case '_':
                        Append();
                        GetChar();
                        continue;
                }
                if (currChar == '/')
                {
                    // You can embed comments in (or at the end of) an &token.
                    // I've no idea why. See the regression test for bug#083.
                    PreserveCurrent();
                    GetChar();
                    if (currChar == '*')
                    {
                        string s = currText.ToString();
                        Comment();
                        int l = currText.Length;
                        currText.Remove(0, l);
                        currText.Insert(0, s, l);
                        PreserveDrop();
                        continue;
                    }
                }
                break;
            }
            ProToken t = Directive();
            if (t != null)
            {
                return t;
            }
            return MakeToken(ABLNodeType.FILENAME);
        }

        internal virtual ProToken Directive()
        {
            char[] separators = { ' ' };
            LOGGER.Debug("Entering directive()");

            // Called by ampText, which has already gather the text for
            // the *potential* directive.

            string macroType = currText.ToString().ToLower();

            if ("&global-define".StartsWith(macroType, StringComparison.Ordinal) && macroType.Length >= 4)
            {
                AppendToEOL();
                // We have to do the define *before* getting next char.
                MacroDefine(Proparse.AMPGLOBALDEFINE);
                GetChar();
                return MakeToken(ABLNodeType.AMPGLOBALDEFINE);
            }
            if ("&scoped-define".StartsWith(macroType, StringComparison.Ordinal) && macroType.Length >= 4)
            {
                AppendToEOL();
                // We have to do the define *before* getting next char.
                MacroDefine(Proparse.AMPSCOPEDDEFINE);
                GetChar();
                return MakeToken(ABLNodeType.AMPSCOPEDDEFINE);
            }

            if ("&undefine".StartsWith(macroType, StringComparison.Ordinal) && macroType.Length >= 5)
            {
                // Append whitespace between &UNDEFINE and the target token
                while (char.IsWhiteSpace((char)currChar))
                {
                    Append();
                    GetChar();
                }
                // Append the target token
                while ((!char.IsWhiteSpace((char)currChar)) && currInt != EOF_CHAR)
                {
                    Append();
                    GetChar();
                }
                // &UNDEFINE consumes up to *and including* the first whitespace
                // after the token it undefines.
                // At least that seems to be what Progress is doing.
                if (currChar == '\r')
                {
                    Append();
                    GetChar();
                    if (currChar == '\n')
                    {
                        Append();
                        GetChar();
                    }
                }
                else if (currInt != EOF_CHAR)
                {
                    Append();
                    GetChar();
                }
                MacroUndefine();
                return MakeToken(ABLNodeType.AMPUNDEFINE);
            }

            if ("&analyze-suspend".Equals(macroType))
            {
                AppendToEOL();
                string analyzeSuspend = "";
                if (currText.ToString().IndexOf(' ') != -1)
                {                    
                    // Documentation says &analyze-suspend is always followed by an option, but better to never trust documentation...
                    // Generates a clean comma-separated list of all entries
                    analyzeSuspend = string.Join(",", currText.ToString().Substring(currText.ToString().IndexOf(' ') + 1).Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList());
                }
                GetChar();
                prepro.AnalyzeSuspend(analyzeSuspend);
                prepro.LstListener.AnalyzeSuspend(analyzeSuspend, textStartLine);
                return MakeToken(ABLNodeType.AMPANALYZESUSPEND);
            }
            if ("&analyze-resume".Equals(macroType))
            {
                AppendToEOL();
                GetChar();
                prepro.AnalyzeResume();
                prepro.LstListener.AnalyzeResume(textStartLine);
                return MakeToken(ABLNodeType.AMPANALYZERESUME);
            }
            if ("&message".Equals(macroType))
            {
                AppendToEOL();
                GetChar();
                return MakeToken(ABLNodeType.AMPMESSAGE);
            }

            if ("&if".Equals(macroType))
            {
                return MakeToken(ABLNodeType.AMPIF);
            }
            if ("&then".Equals(macroType))
            {
                return MakeToken(ABLNodeType.AMPTHEN);
            }
            if ("&elseif".Equals(macroType))
            {
                return MakeToken(ABLNodeType.AMPELSEIF);
            }
            if ("&else".Equals(macroType))
            {
                return MakeToken(ABLNodeType.AMPELSE);
            }
            if ("&endif".Equals(macroType))
            {
                return MakeToken(ABLNodeType.AMPENDIF);
            }

            // If we got here, it wasn't a preprocessor directive,
            // and the caller is responsible for building the token.
            return null;

        }


        //////////////// End lexical productions, begin support functions

        internal virtual void Append()
        {
            currText.Append((char)currInt);
        }

        internal virtual void Append(char c)
        {
            currText.Append(c);
        }

        internal virtual void Append(string theText)
        {
            currText.Append(theText);
        }

        private void AppendToEOL()
        {
            // As with the other "append" functions, the caller is responsible for calling getChar() after this.
            for (; ; )
            {
                if (currChar == '/')
                {
                    Append();
                    GetChar();
                    if (currChar == '*')
                    {
                        // comment() expects to start at '*',
                        // finishes on char after closing slash
                        Comment();
                        continue;
                    }
                    continue;
                }
                if (currInt == EOF_CHAR)
                {
                    break;
                }
                Append();
                // Unescaped newline character or escaped newline where previous char is not tilde
                if ((currChar == '\n') && (!prepro.WasEscape || (prepro.WasEscape && !currText.ToString().EndsWith("~\n", StringComparison.Ordinal))))
                {
                    // We do not call getChar() here. That is because we cannot
                    // get the next character until after any &glob, &scoped, or &undefine
                    // have been dealt with. The next character might be a '{' which in
                    // turn leads to a reference to what is just now being defined or
                    // undefined.
                    break;
                }
                GetChar();
            }
        }

        internal virtual bool CurrIsSpace()
        {
            return (currInt == EOF_CHAR || char.IsWhiteSpace((char)currChar));
        }

        internal virtual void GetChar()
        {
            currInt = prepro.Char;
            currChar = char.ToLower((char)currInt);
            prevFile = currFile;
            prevLine = currLine;
            prevCol = currCol;
            prevMacro = currMacro;
            currFile = prepro.FileIndex;
            currLine = prepro.Line2;
            currCol = prepro.CurrentColumn;
            currMacro = prepro.MacroExpansion;
        }

        internal virtual void MacroDefine(int defType)
        {
            LOGGER.Debug($"Entering macroDefine({defType})");

            if (prepro.IsConsuming || prepro.LexOnly)
            {
                return;
            }
            int it = 0;
            int end = currText.Length;
            while (!char.IsWhiteSpace(currText[it]))
            {
                ++it; // "&glob..." or "&scoped..."
            }
            while (char.IsWhiteSpace(currText[it]))
            {
                ++it; // whitespace
            }
            int start = it;
            while (!char.IsWhiteSpace(currText[it]))
            {
                ++it; // macro name
            }
            string macroName = currText.ToString(start, it - start);
            while (it != end && char.IsWhiteSpace(currText[it]))
            {
                ++it; // whitespace
            }
            string defText = StringFuncs.StripComments(currText.ToString(it, currText.Length - it));
            defText = defText.Trim();
            // Do listing before lowercasing the name
            prepro.LstListener.Define(textStartLine, textStartCol, macroName.ToLower(CultureInfo.GetCultureInfo("en")), defText, defType == Proparse.AMPGLOBALDEFINE ? MacroDefinitionType.GLOBAL : MacroDefinitionType.SCOPED);
            if (defType == Proparse.AMPGLOBALDEFINE)
            {
                prepro.DefGlobal(macroName.ToLower(), defText);
            }
            else
            {
                prepro.DefScoped(macroName.ToLower(), defText);
            }
        }
        internal virtual void MacroUndefine()
        {
            LOGGER.Debug("Entering macroUndefine()");

            if (prepro.IsConsuming)
            {
                return;
            }
            int it = 0;
            int end = currText.Length;
            while (!char.IsWhiteSpace(currText[it]))
            {
                ++it; // "&undef..."
            }
            while (char.IsWhiteSpace(currText[it]))
            {
                ++it; // whitespace
            }
            int start = it;
            while (it != end && (!char.IsWhiteSpace(currText[it])))
            {
                ++it; // macro name
            }
            string macroName = currText.ToString(start, it - start);
            // List the name as in the code - not lowercased
            prepro.LstListener.Undefine(textStartLine, textStartCol, macroName);
            prepro.Undef(macroName.ToLower());
        }

        internal virtual ProToken MakeToken(ABLNodeType type)
        {
            return MakeToken(type, currText.ToString());
        }

        internal virtual ProToken MakeToken(ABLNodeType type, string text)
        {
            // Counting lines of code and commented lines only in the main file (textStartFile set to 0)
            if ((textStartFile == 0) && (type == ABLNodeType.COMMENT))
            {
                int numLines = currText.ToString().Length - currText.ToString().Replace("\n", "").Length;
                for (int zz = textStartLine; zz <= textStartLine + numLines; zz++)
                {
                    comments.Add(zz);
                }
            }
            else if ((textStartFile == 0) && (type != ABLNodeType.WS) && (type != ABLNodeType.EOF_ANTLR4) && (textStartLine > 0))
            {
                loc.Add(textStartLine);
            }
            return (new ProToken.Builder(type, text))
                .SetFileIndex(textStartFile)
                .SetFileName(prepro.GetFilename(textStartFile))
                .SetLine(textStartLine)
                .SetCharPositionInLine(textStartCol)
                .SetEndFileIndex(prevFile)
                .SetEndLine(prevLine)
                .SetEndCharPositionInLine(prevCol)
                .SetMacroExpansion(prevMacro)
                .SetMacroSourceNum(textStartSource)
                .SetAnalyzeSuspend(prepro.CurrentAnalyzeSuspend)
                .Build();
        }

        /// <summary>
        /// Returns number of lines of code in the main file (i.e. including any line where there's a non-comment and non-whitespace token
        /// </summary>
        public virtual int Loc
        {
            get
            {
                return loc.Count;
            }
        }

        public virtual int CommentedLines
        {
            get
            {
                return comments.Count;
            }
        }

        internal virtual void PreserveCurrent()
        {
            // Preserve the current character/file/line/col before looking
            // ahead to the next character. Need this because current char
            // might be appended to current token, or it might be the start
            // of the next token, depending on what character follows... but
            // as soon as we look ahead to the following character, we lose
            // our file/line/col, and that's why we need to preserve.
            preserve = true;
            preserveFile = prepro.FileIndex;
            preserveLine = prepro.Line2;
            preserveCol = prepro.CurrentColumn;
            preserveSource = prepro.SourceNum;
            preserveChar = currChar;
        }

        internal virtual void PreserveDrop()
        {
            preserve = false;
        }

        internal virtual void UnEscapedAppend()
        {
            if (prepro.WasEscape)
            {
                Append(prepro.EscapeText);
                if (prepro.EscapeAppend)
                {
                    Append();
                }
            }
            else
            {
                Append();
            }
        }

        public virtual ProgressLexer Preprocessor
        {
            get
            {
                return prepro;
            }
        }

    }

}
