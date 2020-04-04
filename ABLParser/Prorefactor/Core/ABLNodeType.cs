using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ABLParser.Prorefactor.Proparser.Antlr;
using Antlr4.Runtime;

namespace ABLParser.Prorefactor.Core
{
    public class ABLNodeType
    {
        private const string ERR_INIT = "Error while initializing typeMap - Duplicate key ";
        private static readonly IDictionary<string, ABLNodeType> literalsMap = new Dictionary<string, ABLNodeType>();
        private static readonly IDictionary<int, ABLNodeType> typeMap = new Dictionary<int, ABLNodeType>();

        // ABL additional types
        public const int EMPTY_NODE_TYPE = -1000;        

        // Placeholders and unknown tokens
        public static readonly ABLNodeType EMPTY_NODE = new ABLNodeType(EMPTY_NODE_TYPE, NodeTypesOption.PLACEHOLDER);
        public static readonly ABLNodeType INVALID_NODE = new ABLNodeType(TokenConstants.InvalidType, NodeTypesOption.PLACEHOLDER);
        public static readonly ABLNodeType EOF_ANTLR4 = new ABLNodeType(TokenConstants.EOF, NodeTypesOption.PLACEHOLDER);
        public static readonly ABLNodeType INCLUDEDIRECTIVE = new ABLNodeType(Proparse.INCLUDEDIRECTIVE, NodeTypesOption.PLACEHOLDER);

        // A
        public static readonly ABLNodeType AACBIT = new ABLNodeType(Proparse.AACBIT, "_cbit", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType AACONTROL = new ABLNodeType(Proparse.AACONTROL, "_control", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType AALIST = new ABLNodeType(Proparse.AALIST, "_list", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AAMEMORY = new ABLNodeType(Proparse.AAMEMORY, "_memory", NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType AAMSG = new ABLNodeType(Proparse.AAMSG, "_msg", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType AAPCONTROL = new ABLNodeType(Proparse.AAPCONTROL, "_pcontrol", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType AASERIAL = new ABLNodeType(Proparse.AASERIAL, "_serial-num", 7, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType AATRACE = new ABLNodeType(Proparse.AATRACE, "_trace", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ABSOLUTE = new ABLNodeType(Proparse.ABSOLUTE, "absolute", 3, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ABSTRACT = new ABLNodeType(Proparse.ABSTRACT, "abstract", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ACCELERATOR = new ABLNodeType(Proparse.ACCELERATOR, "accelerator", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ACCUMULATE = new ABLNodeType(Proparse.ACCUMULATE, "accumulate", 5, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ACTIVEFORM = new ABLNodeType(Proparse.ACTIVEFORM, "active-form", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType ACTIVEWINDOW = new ABLNodeType(Proparse.ACTIVEWINDOW, "active-window", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType ADD = new ABLNodeType(Proparse.ADD, "add", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ADDINTERVAL = new ABLNodeType(Proparse.ADDINTERVAL, "add-interval", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ADVISE = new ABLNodeType(Proparse.ADVISE, "advise", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ALERTBOX = new ABLNodeType(Proparse.ALERTBOX, "alert-box", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ALIAS = new ABLNodeType(Proparse.ALIAS, "alias", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ALL = new ABLNodeType(Proparse.ALL, "all", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ALLOWREPLICATION = new ABLNodeType(Proparse.ALLOWREPLICATION, "allow-replication", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ALTER = new ABLNodeType(Proparse.ALTER, "alter", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ALTERNATEKEY = new ABLNodeType(Proparse.ALTERNATEKEY, "alternate-key", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AMBIGUOUS = new ABLNodeType(Proparse.AMBIGUOUS, "ambiguous", 5, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType AMPANALYZERESUME = new ABLNodeType(Proparse.AMPANALYZERESUME, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPANALYZESUSPEND = new ABLNodeType(Proparse.AMPANALYZESUSPEND, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPELSE = new ABLNodeType(Proparse.AMPELSE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPELSEIF = new ABLNodeType(Proparse.AMPELSEIF, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPENDIF = new ABLNodeType(Proparse.AMPENDIF, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPGLOBALDEFINE = new ABLNodeType(Proparse.AMPGLOBALDEFINE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPIF = new ABLNodeType(Proparse.AMPIF, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPMESSAGE = new ABLNodeType(Proparse.AMPMESSAGE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPSCOPEDDEFINE = new ABLNodeType(Proparse.AMPSCOPEDDEFINE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPTHEN = new ABLNodeType(Proparse.AMPTHEN, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType AMPUNDEFINE = new ABLNodeType(Proparse.AMPUNDEFINE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType ANALYZE = new ABLNodeType(Proparse.ANALYZE, "analyze", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AND = new ABLNodeType(Proparse.AND, "and", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ANNOTATION = new ABLNodeType(Proparse.ANNOTATION);
        public static readonly ABLNodeType ANSIONLY = new ABLNodeType(Proparse.ANSIONLY, "ansi-only", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ANY = new ABLNodeType(Proparse.ANY, "any", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ANYWHERE = new ABLNodeType(Proparse.ANYWHERE, "anywhere", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType APPEND = new ABLNodeType(Proparse.APPEND, "append", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType APPLICATION = new ABLNodeType(Proparse.APPLICATION, "application", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType APPLY = new ABLNodeType(Proparse.APPLY, "apply", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ARRAYMESSAGE = new ABLNodeType(Proparse.ARRAYMESSAGE, "array-message", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AS = new ABLNodeType(Proparse.AS, "as", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ASC = new ABLNodeType(Proparse.ASC, "asc", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ASCENDING = new ABLNodeType(Proparse.ASCENDING, "ascending", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ASKOVERWRITE = new ABLNodeType(Proparse.ASKOVERWRITE, "ask-overwrite", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ASSEMBLY = new ABLNodeType(Proparse.ASSEMBLY, "assembly", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ASSIGN = new ABLNodeType(Proparse.ASSIGN, "assign", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ASSIGN_DYNAMIC_NEW = new ABLNodeType(Proparse.Assign_dynamic_new, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType ASYNCHRONOUS = new ABLNodeType(Proparse.ASYNCHRONOUS, "asynchronous", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AT = new ABLNodeType(Proparse.AT, "at", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ATTACHMENT = new ABLNodeType(Proparse.ATTACHMENT, "attachment", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ATTRSPACE = new ABLNodeType(Proparse.ATTRSPACE, "attr-space", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType AUDITCONTROL = new ABLNodeType(Proparse.AUDITCONTROL, "audit-control", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType AUDITENABLED = new ABLNodeType(Proparse.AUDITENABLED, "audit-enabled", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType AUDITPOLICY = new ABLNodeType(Proparse.AUDITPOLICY, "audit-policy", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType AUTHORIZATION = new ABLNodeType(Proparse.AUTHORIZATION, "authorization", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType AUTOCOMPLETION = new ABLNodeType(Proparse.AUTOCOMPLETION, "auto-completion", 9, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AUTOENDKEY = new ABLNodeType(Proparse.AUTOENDKEY, "auto-end-key", "auto-endkey", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AUTOGO = new ABLNodeType(Proparse.AUTOGO, "auto-go", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AUTOMATIC = new ABLNodeType(Proparse.AUTOMATIC, "automatic", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AUTORETURN = new ABLNodeType(Proparse.AUTORETURN, "auto-return", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType AVAILABLE = new ABLNodeType(Proparse.AVAILABLE, "available", 5, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType AVERAGE = new ABLNodeType(Proparse.AVERAGE, "average", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType AVG = new ABLNodeType(Proparse.AVG, "avg", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType AGGREGATE_PHRASE = new ABLNodeType(Proparse.Aggregate_phrase, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType ARRAY_SUBSCRIPT = new ABLNodeType(Proparse.Array_subscript, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType ASSIGN_FROM_BUFFER = new ABLNodeType(Proparse.Assign_from_buffer, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType AUTOMATION_OBJECT = new ABLNodeType(Proparse.Automationobject, NodeTypesOption.STRUCTURE);

        // B
        public static readonly ABLNodeType BACKGROUND = new ABLNodeType(Proparse.BACKGROUND, "background", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BACKSLASH = new ABLNodeType(Proparse.BACKSLASH, "\\", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType BACKTICK = new ABLNodeType(Proparse.BACKTICK, "`", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType BACKWARDS = new ABLNodeType(Proparse.BACKWARDS, "backwards", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BASE64 = new ABLNodeType(Proparse.BASE64, "base64", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BASE64DECODE = new ABLNodeType(Proparse.BASE64DECODE, "base64-decode", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BASE64ENCODE = new ABLNodeType(Proparse.BASE64ENCODE, "base64-encode", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BASEKEY = new ABLNodeType(Proparse.BASEKEY, "base-key", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BATCHSIZE = new ABLNodeType(Proparse.BATCHSIZE, "batch-size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BEFOREHIDE = new ABLNodeType(Proparse.BEFOREHIDE, "before-hide", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BEFORETABLE = new ABLNodeType(Proparse.BEFORETABLE, "before-table", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BEGINS = new ABLNodeType(Proparse.BEGINS, "begins", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BELL = new ABLNodeType(Proparse.BELL, "bell", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BETWEEN = new ABLNodeType(Proparse.BETWEEN, "between", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BGCOLOR = new ABLNodeType(Proparse.BGCOLOR, "bgcolor", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BIGENDIAN = new ABLNodeType(Proparse.BIGENDIAN, "big-endian", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BIGINT = new ABLNodeType(Proparse.BIGINT, "bigint", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BINARY = new ABLNodeType(Proparse.BINARY, "binary", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BIND = new ABLNodeType(Proparse.BIND, "bind", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BINDWHERE = new ABLNodeType(Proparse.BINDWHERE, "bind-where", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BLANK = new ABLNodeType(Proparse.BLANK, "blank", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BLOB = new ABLNodeType(Proparse.BLOB, "blob", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BLOCK_LABEL = new ABLNodeType(Proparse.BLOCK_LABEL, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType BLOCKLEVEL = new ABLNodeType(Proparse.BLOCKLEVEL, "block-level", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BOTH = new ABLNodeType(Proparse.BOTH, "both", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BOTTOM = new ABLNodeType(Proparse.BOTTOM, "bottom", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BOX = new ABLNodeType(Proparse.BOX, "box", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BREAK = new ABLNodeType(Proparse.BREAK, "break", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BROWSE = new ABLNodeType(Proparse.BROWSE, "browse", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BTOS = new ABLNodeType(Proparse.BTOS, "btos", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BUFFER = new ABLNodeType(Proparse.BUFFER, "buffer", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BUFFERCHARS = new ABLNodeType(Proparse.BUFFERCHARS, "buffer-chars", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BUFFERCOMPARE = new ABLNodeType(Proparse.BUFFERCOMPARE, "buffer-compare", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BUFFERCOPY = new ABLNodeType(Proparse.BUFFERCOPY, "buffer-copy", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BUFFERGROUPID = new ABLNodeType(Proparse.BUFFERGROUPID, "buffer-group-id", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BUFFERGROUPNAME = new ABLNodeType(Proparse.BUFFERGROUPNAME, "buffer-group-name", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BUFFERLINES = new ABLNodeType(Proparse.BUFFERLINES, "buffer-lines", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BUFFERNAME = new ABLNodeType(Proparse.BUFFERNAME, "buffer-name", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BUFFERTENANTNAME = new ABLNodeType(Proparse.BUFFERTENANTNAME, "buffer-tenant-name", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BUFFERTENANTID = new ABLNodeType(Proparse.BUFFERTENANTID, "buffer-tenant-id", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType BUTTON = new ABLNodeType(Proparse.BUTTON, "button", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BUTTONS = new ABLNodeType(Proparse.BUTTONS, "buttons", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BY = new ABLNodeType(Proparse.BY, "by", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BYPOINTER = new ABLNodeType(Proparse.BYPOINTER, "by-pointer", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BYREFERENCE = new ABLNodeType(Proparse.BYREFERENCE, "by-reference", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BYTE = new ABLNodeType(Proparse.BYTE, "byte", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BYVALUE = new ABLNodeType(Proparse.BYVALUE, "by-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType BYVARIANTPOINTER = new ABLNodeType(Proparse.BYVARIANTPOINTER, "by-variant-pointer", 16, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType BLOCK_ITERATOR = new ABLNodeType(Proparse.Block_iterator, NodeTypesOption.STRUCTURE);

        // C
        public static readonly ABLNodeType CACHE = new ABLNodeType(Proparse.CACHE, "cache", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CACHESIZE = new ABLNodeType(Proparse.CACHESIZE, "cache-size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CALL = new ABLNodeType(Proparse.CALL, "call", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CANCELBUTTON = new ABLNodeType(Proparse.CANCELBUTTON, "cancel-button", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CANDO = new ABLNodeType(Proparse.CANDO, "can-do", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CANFIND = new ABLNodeType(Proparse.CANFIND, "can-find", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CANQUERY = new ABLNodeType(Proparse.CANQUERY, "can-query", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CANSET = new ABLNodeType(Proparse.CANSET, "can-set", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CAPS = new ABLNodeType(Proparse.CAPS, "caps", "upper", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CARET = new ABLNodeType(Proparse.CARET, "^", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType CASE = new ABLNodeType(Proparse.CASE, "case", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CASESENSITIVE = new ABLNodeType(Proparse.CASESENSITIVE, "case-sensitive", 8, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CAST = new ABLNodeType(Proparse.CAST, "cast", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CATCH = new ABLNodeType(Proparse.CATCH, "catch", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CDECL = new ABLNodeType(Proparse.CDECL_KW, "cdecl", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CENTERED = new ABLNodeType(Proparse.CENTERED, "centered", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CHAINED = new ABLNodeType(Proparse.CHAINED, "chained", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CHARACTER = new ABLNodeType(Proparse.CHARACTER, "character", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CHARACTERLENGTH = new ABLNodeType(Proparse.CHARACTERLENGTH, "characterlength", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CHARSET = new ABLNodeType(Proparse.CHARSET, "charset", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CHECK = new ABLNodeType(Proparse.CHECK, "check", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CHECKED = new ABLNodeType(Proparse.CHECKED, "checked", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CHOOSE = new ABLNodeType(Proparse.CHOOSE, "choose", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CHR = new ABLNodeType(Proparse.CHR, "chr", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CLASS = new ABLNodeType(Proparse.CLASS, "class", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CLEAR = new ABLNodeType(Proparse.CLEAR, "clear", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CLIENTPRINCIPAL = new ABLNodeType(Proparse.CLIENTPRINCIPAL, "client-principal", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CLIPBOARD = new ABLNodeType(Proparse.CLIPBOARD, "clipboard", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType CLOB = new ABLNodeType(Proparse.CLOB, "clob", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CLOSE = new ABLNodeType(Proparse.CLOSE, "close", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CODEBASELOCATOR = new ABLNodeType(Proparse.CODEBASELOCATOR, "codebase-locator", NodeTypesOption.KEYWORD,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType CODEPAGE = new ABLNodeType(Proparse.CODEPAGE, "codepage", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CODEPAGECONVERT = new ABLNodeType(Proparse.CODEPAGECONVERT, "codepage-convert", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType COLLATE = new ABLNodeType(Proparse.COLLATE, "collate", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType COLOF = new ABLNodeType(Proparse.COLOF, "col-of", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLON = new ABLNodeType(Proparse.COLON, "colon", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType COLONALIGNED = new ABLNodeType(Proparse.COLONALIGNED, "colon-aligned", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLOR = new ABLNodeType(Proparse.COLOR, "color", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType COLORTABLE = new ABLNodeType(Proparse.COLORTABLE, "color-table", NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType COLUMN = new ABLNodeType(Proparse.COLUMN, "column", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNBGCOLOR = new ABLNodeType(Proparse.COLUMNBGCOLOR, "column-bgcolor", 10, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNCODEPAGE = new ABLNodeType(Proparse.COLUMNCODEPAGE, "column-codepage", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNDCOLOR = new ABLNodeType(Proparse.COLUMNDCOLOR, "column-dcolor", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNFGCOLOR = new ABLNodeType(Proparse.COLUMNFGCOLOR, "column-fgcolor", 10, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNFONT = new ABLNodeType(Proparse.COLUMNFONT, "column-font", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNLABEL = new ABLNodeType(Proparse.COLUMNLABEL, "column-label", 10, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType COLUMNOF = new ABLNodeType(Proparse.COLUMNOF, "column-of", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNPFCOLOR = new ABLNodeType(Proparse.COLUMNPFCOLOR, "column-pfcolor", 10, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COLUMNS = new ABLNodeType(Proparse.COLUMNS, "columns", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COMBOBOX = new ABLNodeType(Proparse.COMBOBOX, "combo-box", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COMHANDLE = new ABLNodeType(Proparse.COMHANDLE, "com-handle", "component-handle", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COMMA = new ABLNodeType(Proparse.COMMA, ",", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType COMMAND = new ABLNodeType(Proparse.COMMAND, "command", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COMMENT = new ABLNodeType(Proparse.COMMENT, NodeTypesOption.NONPRINTABLE);
        public static readonly ABLNodeType COMMENTEND = new ABLNodeType(Proparse.COMMENTEND, NodeTypesOption.NONPRINTABLE);
        public static readonly ABLNodeType COMMENTSTART = new ABLNodeType(Proparse.COMMENTSTART, NodeTypesOption.NONPRINTABLE);
        public static readonly ABLNodeType COMPARE = new ABLNodeType(Proparse.COMPARE, "compare", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType COMPARES = new ABLNodeType(Proparse.COMPARES, "compares", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType COMPILE = new ABLNodeType(Proparse.COMPILE, "compile", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COMPILER = new ABLNodeType(Proparse.COMPILER, "compiler", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType COMPLETE = new ABLNodeType(Proparse.COMPLETE, "complete", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COMSELF = new ABLNodeType(Proparse.COMSELF, "com-self", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType CONFIGNAME = new ABLNodeType(Proparse.CONFIGNAME, "config-name", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONNECT = new ABLNodeType(Proparse.CONNECT, "connect", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONNECTED = new ABLNodeType(Proparse.CONNECTED, "connected", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CONSTRUCTOR = new ABLNodeType(Proparse.CONSTRUCTOR, "constructor", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTAINS = new ABLNodeType(Proparse.CONTAINS, "contains", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTENTS = new ABLNodeType(Proparse.CONTENTS, "contents", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTEXT = new ABLNodeType(Proparse.CONTEXT, "context", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTEXTHELP = new ABLNodeType(Proparse.CONTEXTHELP, "context-help", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTEXTHELPFILE = new ABLNodeType(Proparse.CONTEXTHELPFILE, "context-help-file", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTEXTHELPID = new ABLNodeType(Proparse.CONTEXTHELPID, "context-help-id", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTEXTPOPUP = new ABLNodeType(Proparse.CONTEXTPOPUP, "context-popup", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONTROL = new ABLNodeType(Proparse.CONTROL, "control", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CONTROLFRAME = new ABLNodeType(Proparse.CONTROLFRAME, "control-frame", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONVERT = new ABLNodeType(Proparse.CONVERT, "convert", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CONVERT3DCOLORS = new ABLNodeType(Proparse.CONVERT3DCOLORS, "convert-3d-colors", 10, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COPYDATASET = new ABLNodeType(Proparse.COPYDATASET, "copy-dataset", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COPYLOB = new ABLNodeType(Proparse.COPYLOB, "copy-lob", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType COPYTEMPTABLE = new ABLNodeType(Proparse.COPYTEMPTABLE, "copy-temp-table", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType COUNT = new ABLNodeType(Proparse.COUNT, "count", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType COUNTOF = new ABLNodeType(Proparse.COUNTOF, "count-of", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CREATE = new ABLNodeType(Proparse.CREATE, "create", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CREATELIKESEQUENTIAL = new ABLNodeType(Proparse.CREATELIKESEQUENTIAL, "create-like-sequential", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CREATETESTFILE = new ABLNodeType(Proparse.CREATETESTFILE, "create-test-file", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CURLYAMP = new ABLNodeType(Proparse.CURLYAMP);
        public static readonly ABLNodeType CURLYNUMBER = new ABLNodeType(Proparse.CURLYNUMBER);
        public static readonly ABLNodeType CURLYSTAR = new ABLNodeType(Proparse.CURLYSTAR);
        public static readonly ABLNodeType CURRENCY = new ABLNodeType(Proparse.CURRENCY, "currency", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CURRENT = new ABLNodeType(Proparse.CURRENT, "current", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType CURRENTCHANGED = new ABLNodeType(Proparse.CURRENTCHANGED, "current-changed", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CURRENTENVIRONMENT = new ABLNodeType(Proparse.CURRENTENVIRONMENT, "current-environment", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CURRENTLANGUAGE = new ABLNodeType(Proparse.CURRENTLANGUAGE, "current-language", 12, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType CURRENTQUERY = new ABLNodeType(Proparse.CURRENTQUERY, "current-query", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType CURRENTRESULTROW = new ABLNodeType(Proparse.CURRENTRESULTROW, "current-result-row", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CURRENTVALUE = new ABLNodeType(Proparse.CURRENTVALUE, "current-value", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType CURRENTWINDOW = new ABLNodeType(Proparse.CURRENTWINDOW, "current-window", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType CURSOR = new ABLNodeType(Proparse.CURSOR, "cursor", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType CODE_BLOCK = new ABLNodeType(Proparse.Code_block, NodeTypesOption.STRUCTURE);

        // D
        public static readonly ABLNodeType DATABASE = new ABLNodeType(Proparse.DATABASE, "database", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DATABIND = new ABLNodeType(Proparse.DATABIND, "data-bind", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DATARELATION = new ABLNodeType(Proparse.DATARELATION, "data-relation", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DATASERVERS = new ABLNodeType(Proparse.DATASERVERS, "dataservers", 11, "gateways", 7, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType DATASET = new ABLNodeType(Proparse.DATASET, "dataset", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DATASETHANDLE = new ABLNodeType(Proparse.DATASETHANDLE, "dataset-handle", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DATASOURCE = new ABLNodeType(Proparse.DATASOURCE, "data-source", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DATASOURCEMODIFIED = new ABLNodeType(Proparse.DATASOURCEMODIFIED, "data-source-modified", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DATASOURCEROWID = new ABLNodeType(Proparse.DATASOURCEROWID, "data-source-rowid", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DATE = new ABLNodeType(Proparse.DATE, "date", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DATETIME = new ABLNodeType(Proparse.DATETIME, "datetime", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DATETIMETZ = new ABLNodeType(Proparse.DATETIMETZ, "datetime-tz", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DAY = new ABLNodeType(Proparse.DAY, "day", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBCODEPAGE = new ABLNodeType(Proparse.DBCODEPAGE, "dbcodepage", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBCOLLATION = new ABLNodeType(Proparse.DBCOLLATION, "dbcollation", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBIMS = new ABLNodeType(Proparse.DBIMS, "dbims", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DBNAME = new ABLNodeType(Proparse.DBNAME, "dbname", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType DBPARAM = new ABLNodeType(Proparse.DBPARAM, "dbparam", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBREMOTEHOST = new ABLNodeType(Proparse.DBREMOTEHOST, "db-remote-host", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBRESTRICTIONS = new ABLNodeType(Proparse.DBRESTRICTIONS, "dbrestrictions", 6, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBTASKID = new ABLNodeType(Proparse.DBTASKID, "dbtaskid", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBTYPE = new ABLNodeType(Proparse.DBTYPE, "dbtype", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DBVERSION = new ABLNodeType(Proparse.DBVERSION, "dbversion", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DCOLOR = new ABLNodeType(Proparse.DCOLOR, "dcolor", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DDE = new ABLNodeType(Proparse.DDE, "dde", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DEBLANK = new ABLNodeType(Proparse.DEBLANK, "deblank", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DEBUG = new ABLNodeType(Proparse.DEBUG, "debug", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEBUGGER = new ABLNodeType(Proparse.DEBUGGER, "debugger", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType DEBUGLIST = new ABLNodeType(Proparse.DEBUGLIST, "debug-list", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DECIMAL = new ABLNodeType(Proparse.DECIMAL, "decimal", 3, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DECIMALS = new ABLNodeType(Proparse.DECIMALS, "decimals", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DECLARE = new ABLNodeType(Proparse.DECLARE, "declare", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DECRYPT = new ABLNodeType(Proparse.DECRYPT, "decrypt", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DEFAULT = new ABLNodeType(Proparse.DEFAULT, "default", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DEFAULTBUTTON = new ABLNodeType(Proparse.DEFAULTBUTTON, "default-button", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEFAULTEXTENSION = new ABLNodeType(Proparse.DEFAULTEXTENSION, "default-extension", 10, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEFAULTNOXLATE = new ABLNodeType(Proparse.DEFAULTNOXLATE, "default-noxlate", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEFAULTVALUE = new ABLNodeType(Proparse.DEFAULTVALUE, "default-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEFAULTWINDOW = new ABLNodeType(Proparse.DEFAULTWINDOW, "default-window", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType DEFERLOBFETCH = new ABLNodeType(Proparse.DEFERLOBFETCH, "defer-lob-fetch", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEFINE = new ABLNodeType(Proparse.DEFINE, "define", 3, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DEFINED = new ABLNodeType(Proparse.DEFINED, "defined", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DEFINETEXT = new ABLNodeType(Proparse.DEFINETEXT);
        public static readonly ABLNodeType DELEGATE = new ABLNodeType(Proparse.DELEGATE, "delegate", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DELETECHARACTER = new ABLNodeType(Proparse.DELETECHARACTER, "delete-character", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DELETERESULTLISTENTRY = new ABLNodeType(Proparse.DELETERESULTLISTENTRY, "delete-result-list-entry", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DELETE = new ABLNodeType(Proparse.DELETE_KW, "delete", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DELIMITER = new ABLNodeType(Proparse.DELIMITER, "delimiter", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DESCENDING = new ABLNodeType(Proparse.DESCENDING, "descending", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DESELECTION = new ABLNodeType(Proparse.DESELECTION, "deselection", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DESTRUCTOR = new ABLNodeType(Proparse.DESTRUCTOR, "destructor", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DIALOGBOX = new ABLNodeType(Proparse.DIALOGBOX, "dialog-box", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DIALOGHELP = new ABLNodeType(Proparse.DIALOGHELP, "dialog-help", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DICTIONARY = new ABLNodeType(Proparse.DICTIONARY, "dictionary", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DIGITS = new ABLNodeType(Proparse.DIGITS);
        public static readonly ABLNodeType DIGITSTART = new ABLNodeType(Proparse.DIGITSTART);
        public static readonly ABLNodeType DIR = new ABLNodeType(Proparse.DIR, "dir", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DISABLE = new ABLNodeType(Proparse.DISABLE, "disable", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DISABLEAUTOZAP = new ABLNodeType(Proparse.DISABLEAUTOZAP, "disable-auto-zap", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DISABLED = new ABLNodeType(Proparse.DISABLED, "disabled", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DISCONNECT = new ABLNodeType(Proparse.DISCONNECT, "disconnect", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DISPLAY = new ABLNodeType(Proparse.DISPLAY, "display", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DISTINCT = new ABLNodeType(Proparse.DISTINCT, "distinct", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DIVIDE = new ABLNodeType(Proparse.DIVIDE);
        public static readonly ABLNodeType DO = new ABLNodeType(Proparse.DO, "do", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DOS = new ABLNodeType(Proparse.DOS, "dos", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DOT_COMMENT = new ABLNodeType(Proparse.DOT_COMMENT);
        public static readonly ABLNodeType DOUBLE = new ABLNodeType(Proparse.DOUBLE, "double", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DOUBLECOLON = new ABLNodeType(Proparse.DOUBLECOLON, "::", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType DOUBLEQUOTE = new ABLNodeType(Proparse.DOUBLEQUOTE, "\"", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType DOWN = new ABLNodeType(Proparse.DOWN, "down", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DQSTRING = new ABLNodeType(Proparse.DQSTRING);
        public static readonly ABLNodeType DROP = new ABLNodeType(Proparse.DROP, "drop", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType DROPDOWN = new ABLNodeType(Proparse.DROPDOWN, "drop-down", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DROPDOWNLIST = new ABLNodeType(Proparse.DROPDOWNLIST, "drop-down-list", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DROPFILENOTIFY = new ABLNodeType(Proparse.DROPFILENOTIFY, "drop-file-notify", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DROPTARGET = new ABLNodeType(Proparse.DROPTARGET, "drop-target", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DUMP = new ABLNodeType(Proparse.DUMP, "dump", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DYNAMIC = new ABLNodeType(Proparse.DYNAMIC, "dynamic", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DYNAMICCAST = new ABLNodeType(Proparse.DYNAMICCAST, "dynamic-cast", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DYNAMICCURRENTVALUE = new ABLNodeType(Proparse.DYNAMICCURRENTVALUE, "dynamic-current-value", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DYNAMICFUNCTION = new ABLNodeType(Proparse.DYNAMICFUNCTION, "dynamic-function", 12, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DYNAMICINVOKE = new ABLNodeType(Proparse.DYNAMICINVOKE, "dynamic-invoke", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DYNAMICNEW = new ABLNodeType(Proparse.DYNAMICNEW, "dynamic-new", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType DYNAMICNEXTVALUE = new ABLNodeType(Proparse.DYNAMICNEXTVALUE, "dynamic-next-value", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType DYNAMICPROPERTY = new ABLNodeType(Proparse.DYNAMICPROPERTY, "dynamic-property", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        
        // E
        public static readonly ABLNodeType EACH = new ABLNodeType(Proparse.EACH, "each", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ECHO = new ABLNodeType(Proparse.ECHO, "echo", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EDGECHARS = new ABLNodeType(Proparse.EDGECHARS, "edge-chars", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EDGEPIXELS = new ABLNodeType(Proparse.EDGEPIXELS, "edge-pixels", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EDITING = new ABLNodeType(Proparse.EDITING, "editing", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType EDITOR = new ABLNodeType(Proparse.EDITOR, "editor", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EDITUNDO = new ABLNodeType(Proparse.EDITUNDO, "edit-undo", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ELSE = new ABLNodeType(Proparse.ELSE, "else", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType EMPTY = new ABLNodeType(Proparse.EMPTY, "empty", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENABLE = new ABLNodeType(Proparse.ENABLE, "enable", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ENABLEDFIELDS = new ABLNodeType(Proparse.ENABLEDFIELDS, "enabled-fields", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENCODE = new ABLNodeType(Proparse.ENCODE, "encode", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ENCRYPT = new ABLNodeType(Proparse.ENCRYPT, "encrypt", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ENCRYPTIONSALT = new ABLNodeType(Proparse.ENCRYPTIONSALT, "encryption-salt", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType END = new ABLNodeType(Proparse.END, "end", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ENDKEY = new ABLNodeType(Proparse.ENDKEY, "end-key", "endkey", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENDMOVE = new ABLNodeType(Proparse.ENDMOVE, "end-move", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENDRESIZE = new ABLNodeType(Proparse.ENDRESIZE, "end-resize", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENDROWRESIZE = new ABLNodeType(Proparse.ENDROWRESIZE, "end-row-resize", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENTERED = new ABLNodeType(Proparse.ENTERED, "entered", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ENTRY = new ABLNodeType(Proparse.ENTRY, "entry", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ENUM = new ABLNodeType(Proparse.ENUM, "enum", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EQ = new ABLNodeType(Proparse.EQ, "eq", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EQUAL = new ABLNodeType(Proparse.EQUAL, "=", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType ERROR = new ABLNodeType(Proparse.ERROR, "error", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ERRORCODE = new ABLNodeType(Proparse.ERRORCODE, "error-code", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ERRORSTACKTRACE = new ABLNodeType(Proparse.ERRORSTACKTRACE, "error-stack-trace", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ERRORSTATUS = new ABLNodeType(Proparse.ERRORSTATUS, "error-status", 10, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType ESCAPE = new ABLNodeType(Proparse.ESCAPE, "escape", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ESCAPED_QUOTE = new ABLNodeType(Proparse.ESCAPED_QUOTE);
        public static readonly ABLNodeType ETIME = new ABLNodeType(Proparse.ETIME_KW, "etime", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType EVENT = new ABLNodeType(Proparse.EVENT, "event", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EVENTPROCEDURE = new ABLNodeType(Proparse.EVENTPROCEDURE, "event-procedure", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EVENTS = new ABLNodeType(Proparse.EVENTS, "events", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXCEPT = new ABLNodeType(Proparse.EXCEPT, "except", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType EXCLAMATION = new ABLNodeType(Proparse.EXCLAMATION, "!", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType EXCLUSIVEID = new ABLNodeType(Proparse.EXCLUSIVEID, "exclusive-id", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXCLUSIVELOCK = new ABLNodeType(Proparse.EXCLUSIVELOCK, "exclusive-lock", 9, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType EXCLUSIVEWEBUSER = new ABLNodeType(Proparse.EXCLUSIVEWEBUSER, "exclusive-web-user", 13, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXECUTE = new ABLNodeType(Proparse.EXECUTE, "execute", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXISTS = new ABLNodeType(Proparse.EXISTS, "exists", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType EXP = new ABLNodeType(Proparse.EXP, "exp", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType EXPAND = new ABLNodeType(Proparse.EXPAND, "expand", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXPANDABLE = new ABLNodeType(Proparse.EXPANDABLE, "expandable", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXPLICIT = new ABLNodeType(Proparse.EXPLICIT, "explicit", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXPORT = new ABLNodeType(Proparse.EXPORT, "export", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType EXTENDED = new ABLNodeType(Proparse.EXTENDED, "extended", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EXTENT = new ABLNodeType(Proparse.EXTENT, "extent", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType EXTERNAL = new ABLNodeType(Proparse.EXTERNAL, "external", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType EDITING_PHRASE = new ABLNodeType(Proparse.Editing_phrase, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType ENTERED_FUNC = new ABLNodeType(Proparse.Entered_func, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType EVENT_LIST = new ABLNodeType(Proparse.Event_list, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType EXPR_STATEMENT = new ABLNodeType(Proparse.Expr_statement, NodeTypesOption.STRUCTURE);
        
        // F
        public static readonly ABLNodeType FALSELEAKS = new ABLNodeType(Proparse.FALSELEAKS, "false-leaks", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FALSE = new ABLNodeType(Proparse.FALSE_KW, "false", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FETCH = new ABLNodeType(Proparse.FETCH, "fetch", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FGCOLOR = new ABLNodeType(Proparse.FGCOLOR, "fgcolor", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FIELD = new ABLNodeType(Proparse.FIELD, "field", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FIELDS = new ABLNodeType(Proparse.FIELDS, "fields", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FILE = new ABLNodeType(Proparse.FILE, "file", 4, "file-name", "filename", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FILEINFORMATION = new ABLNodeType(Proparse.FILEINFORMATION, "file-information", 9, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType FILENAME = new ABLNodeType(Proparse.FILENAME);
        public static readonly ABLNodeType FILL = new ABLNodeType(Proparse.FILL, "fill", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FILLWHERESTRING = new ABLNodeType(Proparse.FILLWHERESTRING, "fill-where-string", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FILLIN = new ABLNodeType(Proparse.FILLIN, "fill-in", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FILTERS = new ABLNodeType(Proparse.FILTERS, "filters", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FINAL = new ABLNodeType(Proparse.FINAL, "final", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FINALLY = new ABLNodeType(Proparse.FINALLY, "finally", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FIND = new ABLNodeType(Proparse.FIND, "find", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FINDCASESENSITIVE = new ABLNodeType(Proparse.FINDCASESENSITIVE, "find-case-sensitive", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FINDER = new ABLNodeType(Proparse.FINDER, "finder", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FINDGLOBAL = new ABLNodeType(Proparse.FINDGLOBAL, "find-global", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FINDNEXTOCCURRENCE = new ABLNodeType(Proparse.FINDNEXTOCCURRENCE, "find-next-occurrence", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FINDPREVOCCURRENCE = new ABLNodeType(Proparse.FINDPREVOCCURRENCE, "find-prev-occurrence", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FINDSELECT = new ABLNodeType(Proparse.FINDSELECT, "find-select", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FINDWRAPAROUND = new ABLNodeType(Proparse.FINDWRAPAROUND, "find-wrap-around", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FIRST = new ABLNodeType(Proparse.FIRST, "first", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FIRSTFORM = new ABLNodeType(Proparse.FIRSTFORM, "first-form", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FIRSTOF = new ABLNodeType(Proparse.FIRSTOF, "first-of", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FITLASTCOLUMN = new ABLNodeType(Proparse.FITLASTCOLUMN, "fit-last-column", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FIXCHAR = new ABLNodeType(Proparse.FIXCHAR, "fixchar", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FIXCODEPAGE = new ABLNodeType(Proparse.FIXCODEPAGE, "fix-codepage", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FIXEDONLY = new ABLNodeType(Proparse.FIXEDONLY, "fixed-only", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FLAGS = new ABLNodeType(Proparse.FLAGS, "flags", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FLATBUTTON = new ABLNodeType(Proparse.FLATBUTTON, "flat-button", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FLOAT = new ABLNodeType(Proparse.FLOAT, "float", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FOCUS = new ABLNodeType(Proparse.FOCUS, "focus", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType FONT = new ABLNodeType(Proparse.FONT, "font", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FONTBASEDLAYOUT = new ABLNodeType(Proparse.FONTBASEDLAYOUT, "font-based-layout", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FONTTABLE = new ABLNodeType(Proparse.FONTTABLE, "font-table", NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType FOR = new ABLNodeType(Proparse.FOR, "for", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FORCEFILE = new ABLNodeType(Proparse.FORCEFILE, "force-file", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FOREIGNKEYHIDDEN = new ABLNodeType(Proparse.FOREIGNKEYHIDDEN, "foreign-key-hidden", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FORMAT = new ABLNodeType(Proparse.FORMAT, "format", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FORMINPUT = new ABLNodeType(Proparse.FORMINPUT, "forminput", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FORMLONGINPUT = new ABLNodeType(Proparse.FORMLONGINPUT, "form-long-input", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FORWARDS = new ABLNodeType(Proparse.FORWARDS, "forwards", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FRAME = new ABLNodeType(Proparse.FRAME, "frame", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FRAMECOL = new ABLNodeType(Proparse.FRAMECOL, "frame-col", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FRAMEDB = new ABLNodeType(Proparse.FRAMEDB, "frame-db", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType FRAMEDOWN = new ABLNodeType(Proparse.FRAMEDOWN, "frame-down", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FRAMEFIELD = new ABLNodeType(Proparse.FRAMEFIELD, "frame-field", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType FRAMEFILE = new ABLNodeType(Proparse.FRAMEFILE, "frame-file", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType FRAMEINDEX = new ABLNodeType(Proparse.FRAMEINDEX, "frame-index", 10, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType FRAMELINE = new ABLNodeType(Proparse.FRAMELINE, "frame-line", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FRAMENAME = new ABLNodeType(Proparse.FRAMENAME, "frame-name", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType FRAMEROW = new ABLNodeType(Proparse.FRAMEROW, "frame-row", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType FRAMEVALUE = new ABLNodeType(Proparse.FRAMEVALUE, "frame-value", 9, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType FREECHAR = new ABLNodeType(Proparse.FREECHAR);
        public static readonly ABLNodeType FREQUENCY = new ABLNodeType(Proparse.FREQUENCY, "frequency", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FROM = new ABLNodeType(Proparse.FROM, "from", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FROMCURRENT = new ABLNodeType(Proparse.FROMCURRENT, "from-current", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FUNCTION = new ABLNodeType(Proparse.FUNCTION, "function", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType FUNCTIONCALLTYPE = new ABLNodeType(Proparse.FUNCTIONCALLTYPE, "function-call-type", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType FIELD_LIST = new ABLNodeType(Proparse.Field_list, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType FIELD_REF = new ABLNodeType(Proparse.Field_ref, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType FORM_ITEM = new ABLNodeType(Proparse.Form_item, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType FORMAT_PHRASE = new ABLNodeType(Proparse.Format_phrase, NodeTypesOption.STRUCTURE);
        
        // G        
        public static readonly ABLNodeType GE = new ABLNodeType(Proparse.GE, "ge", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GENERATEMD5 = new ABLNodeType(Proparse.GENERATEMD5, "generate-md5", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GENERATEPBEKEY = new ABLNodeType(Proparse.GENERATEPBEKEY, "generate-pbe-key", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GENERATEPBESALT = new ABLNodeType(Proparse.GENERATEPBESALT, "generate-pbe-salt", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType GENERATERANDOMKEY = new ABLNodeType(Proparse.GENERATERANDOMKEY, "generate-random-key", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType GENERATEUUID = new ABLNodeType(Proparse.GENERATEUUID, "generate-uuid", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType GET = new ABLNodeType(Proparse.GET, "get", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETATTRCALLTYPE = new ABLNodeType(Proparse.GETATTRCALLTYPE, "get-attr-call-type", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GETBITS = new ABLNodeType(Proparse.GETBITS, "get-bits", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETBUFFERHANDLE = new ABLNodeType(Proparse.GETBUFFERHANDLE, "get-buffer-handle", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GETBYTE = new ABLNodeType(Proparse.GETBYTE, "get-byte", "getbyte", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETBYTEORDER = new ABLNodeType(Proparse.GETBYTEORDER, "get-byte-order", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETBYTES = new ABLNodeType(Proparse.GETBYTES, "get-bytes", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETCGILIST = new ABLNodeType(Proparse.GETCGILIST, "get-cgi-list", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETCGILONGVALUE = new ABLNodeType(Proparse.GETCGILONGVALUE, "get-cgi-long-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETCGIVALUE = new ABLNodeType(Proparse.GETCGIVALUE, "get-cgi-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETCLASS = new ABLNodeType(Proparse.GETCLASS, "get-class", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETCODEPAGE = new ABLNodeType(Proparse.GETCODEPAGE, "get-codepage", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETCODEPAGES = new ABLNodeType(Proparse.GETCODEPAGES, "get-codepages", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETCOLLATIONS = new ABLNodeType(Proparse.GETCOLLATIONS, "get-collations", 8, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETCONFIGVALUE = new ABLNodeType(Proparse.GETCONFIGVALUE, "get-config-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETDBCLIENT = new ABLNodeType(Proparse.GETDBCLIENT, "get-db-client", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType GETDIR = new ABLNodeType(Proparse.GETDIR, "get-dir", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETDOUBLE = new ABLNodeType(Proparse.GETDOUBLE, "get-double", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETEFFECTIVETENANTID = new ABLNodeType(Proparse.GETEFFECTIVETENANTID, "get-effective-tenant-id", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETEFFECTIVETENANTNAME = new ABLNodeType(Proparse.GETEFFECTIVETENANTNAME, "get-effective-tenant-name",
            NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETFILE = new ABLNodeType(Proparse.GETFILE, "get-file", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GETFLOAT = new ABLNodeType(Proparse.GETFLOAT, "get-float", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETINT64 = new ABLNodeType(Proparse.GETINT64, "get-int64", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETKEYVALUE = new ABLNodeType(Proparse.GETKEYVALUE, "get-key-value", 11, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GETLICENSE = new ABLNodeType(Proparse.GETLICENSE, "get-license", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETLONG = new ABLNodeType(Proparse.GETLONG, "get-long", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETPOINTERVALUE = new ABLNodeType(Proparse.GETPOINTERVALUE, "get-pointer-value", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETSHORT = new ABLNodeType(Proparse.GETSHORT, "get-short", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETSIZE = new ABLNodeType(Proparse.GETSIZE, "get-size", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETSTRING = new ABLNodeType(Proparse.GETSTRING, "get-string", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETUNSIGNEDLONG = new ABLNodeType(Proparse.GETUNSIGNEDLONG, "get-unsigned-long", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GETUNSIGNEDSHORT = new ABLNodeType(Proparse.GETUNSIGNEDSHORT, "get-unsigned-short", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType GLOBAL = new ABLNodeType(Proparse.GLOBAL, "global", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GLOBAL_DEFINE = new ABLNodeType(Proparse.GLOBALDEFINE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType GOON = new ABLNodeType(Proparse.GOON, "go-on", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GOPENDING = new ABLNodeType(Proparse.GOPENDING, "go-pending", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType GRANT = new ABLNodeType(Proparse.GRANT, "grant", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GRAPHICEDGE = new ABLNodeType(Proparse.GRAPHICEDGE, "graphic-edge", 9, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GROUP = new ABLNodeType(Proparse.GROUP, "group", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType GROUPBOX = new ABLNodeType(Proparse.GROUPBOX, "group-box", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GTHAN = new ABLNodeType(Proparse.GTHAN, "gt", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType GTOREQUAL = new ABLNodeType(Proparse.GTOREQUAL, ">=", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType GTORLT = new ABLNodeType(Proparse.GTORLT, "<>", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType GUID = new ABLNodeType(Proparse.GUID, "guid", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        
        // H
        public static readonly ABLNodeType HANDLE = new ABLNodeType(Proparse.HANDLE, "handle", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HAVING = new ABLNodeType(Proparse.HAVING, "having", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType HEADER = new ABLNodeType(Proparse.HEADER, "header", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType HEIGHT = new ABLNodeType(Proparse.HEIGHT, "height", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HEIGHTCHARS = new ABLNodeType(Proparse.HEIGHTCHARS, "height-chars", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HEIGHTPIXELS = new ABLNodeType(Proparse.HEIGHTPIXELS, "height-pixels", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HELP = new ABLNodeType(Proparse.HELP, "help", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType HELPTOPIC = new ABLNodeType(Proparse.HELPTOPIC, "help-topic", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HEXDECODE = new ABLNodeType(Proparse.HEXDECODE, "hex-decode", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType HEXENCODE = new ABLNodeType(Proparse.HEXENCODE, "hex-encode", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType HIDDEN = new ABLNodeType(Proparse.HIDDEN, "hidden", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HIDE = new ABLNodeType(Proparse.HIDE, "hide", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType HINT = new ABLNodeType(Proparse.HINT, "hint", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HORIZONTAL = new ABLNodeType(Proparse.HORIZONTAL, "horizontal", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HOSTBYTEORDER = new ABLNodeType(Proparse.HOSTBYTEORDER, "host-byte-order", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType HTMLENDOFLINE = new ABLNodeType(Proparse.HTMLENDOFLINE, "html-end-of-line", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HTMLFRAMEBEGIN = new ABLNodeType(Proparse.HTMLFRAMEBEGIN, "html-frame-begin", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HTMLFRAMEEND = new ABLNodeType(Proparse.HTMLFRAMEEND, "html-frame-end", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HTMLHEADERBEGIN = new ABLNodeType(Proparse.HTMLHEADERBEGIN, "html-header-begin", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HTMLHEADEREND = new ABLNodeType(Proparse.HTMLHEADEREND, "html-header-end", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HTMLTITLEBEGIN = new ABLNodeType(Proparse.HTMLTITLEBEGIN, "html-title-begin", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType HTMLTITLEEND = new ABLNodeType(Proparse.HTMLTITLEEND, "html-title-end", NodeTypesOption.KEYWORD);
        
        // I
        public static readonly ABLNodeType ID = new ABLNodeType(Proparse.ID);
        public static readonly ABLNodeType ID_THREE = new ABLNodeType(Proparse.ID_THREE);
        public static readonly ABLNodeType ID_TWO = new ABLNodeType(Proparse.ID_TWO);
        public static readonly ABLNodeType IF = new ABLNodeType(Proparse.IF, "if", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType IFCOND = new ABLNodeType(Proparse.IFCOND);
        public static readonly ABLNodeType IMAGE = new ABLNodeType(Proparse.IMAGE, "image", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMAGEDOWN = new ABLNodeType(Proparse.IMAGEDOWN, "image-down", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMAGEINSENSITIVE = new ABLNodeType(Proparse.IMAGEINSENSITIVE, "image-insensitive", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMAGESIZE = new ABLNodeType(Proparse.IMAGESIZE, "image-size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMAGESIZECHARS = new ABLNodeType(Proparse.IMAGESIZECHARS, "image-size-chars", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMAGESIZEPIXELS = new ABLNodeType(Proparse.IMAGESIZEPIXELS, "image-size-pixels", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMAGEUP = new ABLNodeType(Proparse.IMAGEUP, "image-up", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMPLEMENTS = new ABLNodeType(Proparse.IMPLEMENTS, "implements", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IMPORT = new ABLNodeType(Proparse.IMPORT, "import", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType IMPOSSIBLE_TOKEN = new ABLNodeType(Proparse.IMPOSSIBLE_TOKEN);
        public static readonly ABLNodeType INCLUDEREFARG = new ABLNodeType(Proparse.INCLUDEREFARG);
        public static readonly ABLNodeType INCREMENTEXCLUSIVEID = new ABLNodeType(Proparse.INCREMENTEXCLUSIVEID, "increment-exclusive-id", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INDEX = new ABLNodeType(Proparse.INDEX, "index", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType INDEXEDREPOSITION = new ABLNodeType(Proparse.INDEXEDREPOSITION, "indexed-reposition", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INDEXHINT = new ABLNodeType(Proparse.INDEXHINT, "index-hint", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INDICATOR = new ABLNodeType(Proparse.INDICATOR, "indicator", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType INFORMATION = new ABLNodeType(Proparse.INFORMATION, "information", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INHERITBGCOLOR = new ABLNodeType(Proparse.INHERITBGCOLOR, "inherit-bgcolor", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INHERITFGCOLOR = new ABLNodeType(Proparse.INHERITFGCOLOR, "inherit-fgcolor", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INHERITS = new ABLNodeType(Proparse.INHERITS, "inherits", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INITIAL = new ABLNodeType(Proparse.INITIAL, "initial", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INITIALDIR = new ABLNodeType(Proparse.INITIALDIR, "initial-dir", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INITIALFILTER = new ABLNodeType(Proparse.INITIALFILTER, "initial-filter", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INITIATE = new ABLNodeType(Proparse.INITIATE, "initiate", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INNER = new ABLNodeType(Proparse.INNER, "inner", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INNERCHARS = new ABLNodeType(Proparse.INNERCHARS, "inner-chars", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INNERLINES = new ABLNodeType(Proparse.INNERLINES, "inner-lines", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INPUT = new ABLNodeType(Proparse.INPUT, "input", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType INPUTOUTPUT = new ABLNodeType(Proparse.INPUTOUTPUT, "input-output", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType INSERT = new ABLNodeType(Proparse.INSERT, "insert", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType INT64 = new ABLNodeType(Proparse.INT64, "int64", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType INTEGER = new ABLNodeType(Proparse.INTEGER, "integer", 3, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType INTERFACE = new ABLNodeType(Proparse.INTERFACE, "interface", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INTERVAL = new ABLNodeType(Proparse.INTERVAL, "interval", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType INTO = new ABLNodeType(Proparse.INTO, "into", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType IN = new ABLNodeType(Proparse.IN_KW, "in", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType IS = new ABLNodeType(Proparse.IS, "is", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ISATTRSPACE = new ABLNodeType(Proparse.ISATTRSPACE, "is-attr-space", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType ISCODEPAGEFIXED = new ABLNodeType(Proparse.ISCODEPAGEFIXED, "is-codepage-fixed", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ISCOLUMNCODEPAGE = new ABLNodeType(Proparse.ISCOLUMNCODEPAGE, "is-column-codepage", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ISDBMULTITENANT = new ABLNodeType(Proparse.ISDBMULTITENANT, "is-db-multi-tenant", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ISLEADBYTE = new ABLNodeType(Proparse.ISLEADBYTE, "is-lead-byte", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ISMULTITENANT = new ABLNodeType(Proparse.ISMULTITENANT, "is-multi-tenant", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ISODATE = new ABLNodeType(Proparse.ISODATE, "iso-date", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ITEM = new ABLNodeType(Proparse.ITEM, "item", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType IUNKNOWN = new ABLNodeType(Proparse.IUNKNOWN, "iunknown", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType INLINE_DEFINITION = new ABLNodeType(Proparse.Inline_definition, NodeTypesOption.STRUCTURE);
        
        // J
        public static readonly ABLNodeType JOIN = new ABLNodeType(Proparse.JOIN, "join", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType JOINBYSQLDB = new ABLNodeType(Proparse.JOINBYSQLDB, "join-by-sqldb", NodeTypesOption.KEYWORD);
        
        // K
        public static readonly ABLNodeType KBLABEL = new ABLNodeType(Proparse.KBLABEL, "kblabel", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType KEEPMESSAGES = new ABLNodeType(Proparse.KEEPMESSAGES, "keep-messages", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType KEEPTABORDER = new ABLNodeType(Proparse.KEEPTABORDER, "keep-tab-order", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType KEY = new ABLNodeType(Proparse.KEY, "key", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType KEYCODE = new ABLNodeType(Proparse.KEYCODE, "key-code", "keycode", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType KEYFUNCTION = new ABLNodeType(Proparse.KEYFUNCTION, "key-function", 8, "keyfunction", 7, NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType KEYLABEL = new ABLNodeType(Proparse.KEYLABEL, "key-label", "keylabel", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType KEYS = new ABLNodeType(Proparse.KEYS, "keys", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType KEYWORD = new ABLNodeType(Proparse.KEYWORD, "keyword", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType KEYWORDALL = new ABLNodeType(Proparse.KEYWORDALL, "keyword-all", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        
        // L
        public static readonly ABLNodeType LABEL = new ABLNodeType(Proparse.LABEL, "label", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LABELBGCOLOR = new ABLNodeType(Proparse.LABELBGCOLOR, "label-bgcolor", 9, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LABELDCOLOR = new ABLNodeType(Proparse.LABELDCOLOR, "label-dcolor", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LABELFGCOLOR = new ABLNodeType(Proparse.LABELFGCOLOR, "label-fgcolor", 9, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LABELFONT = new ABLNodeType(Proparse.LABELFONT, "label-font", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LANDSCAPE = new ABLNodeType(Proparse.LANDSCAPE, "landscape", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LANGUAGES = new ABLNodeType(Proparse.LANGUAGES, "languages", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LARGE = new ABLNodeType(Proparse.LARGE, "large", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LARGETOSMALL = new ABLNodeType(Proparse.LARGETOSMALL, "large-to-small", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LAST = new ABLNodeType(Proparse.LAST, "last", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LASTBATCH = new ABLNodeType(Proparse.LASTBATCH, "last-batch", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LASTEVENT = new ABLNodeType(Proparse.LASTEVENT, "last-event", 9, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType LASTFORM = new ABLNodeType(Proparse.LASTFORM, "last-form", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LASTKEY = new ABLNodeType(Proparse.LASTKEY, "last-key", "lastkey", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType LASTOF = new ABLNodeType(Proparse.LASTOF, "last-of", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LC = new ABLNodeType(Proparse.LC, "lc", "lower", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LDBNAME = new ABLNodeType(Proparse.LDBNAME, "ldbname", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LE = new ABLNodeType(Proparse.LE, "le", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LEAKDETECTION = new ABLNodeType(Proparse.LEAKDETECTION, "leak-detection", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LEAVE = new ABLNodeType(Proparse.LEAVE, "leave", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LEFT = new ABLNodeType(Proparse.LEFT, "left", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LEFTALIGNED = new ABLNodeType(Proparse.LEFTALIGNED, "left-aligned", 10, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LEFTANGLE = new ABLNodeType(Proparse.LEFTANGLE, "<", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LEFTBRACE = new ABLNodeType(Proparse.LEFTBRACE, "[", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LEFTCURLY = new ABLNodeType(Proparse.LEFTCURLY, "{", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LEFTPAREN = new ABLNodeType(Proparse.LEFTPAREN, "(", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LEFTTRIM = new ABLNodeType(Proparse.LEFTTRIM, "left-trim", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LENGTH = new ABLNodeType(Proparse.LENGTH, "length", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LEXAT = new ABLNodeType(Proparse.LEXAT, "@", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LEXCOLON = new ABLNodeType(Proparse.LEXCOLON, ":", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LEXDATE = new ABLNodeType(Proparse.LEXDATE, NodeTypesOption.NONPRINTABLE);
        public static readonly ABLNodeType LEXOTHER = new ABLNodeType(Proparse.LEXOTHER, NodeTypesOption.NONPRINTABLE);
        public static readonly ABLNodeType LIBRARY = new ABLNodeType(Proparse.LIBRARY, "library", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LIKE = new ABLNodeType(Proparse.LIKE, "like", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LIKESEQUENTIAL = new ABLNodeType(Proparse.LIKESEQUENTIAL, "like-sequential", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LINECOUNTER = new ABLNodeType(Proparse.LINECOUNTER, "line-counter", 10, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LISTEVENTS = new ABLNodeType(Proparse.LISTEVENTS, "list-events", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LISTING = new ABLNodeType(Proparse.LISTING, "listing", 5, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LISTITEMPAIRS = new ABLNodeType(Proparse.LISTITEMPAIRS, "list-item-pairs", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LISTITEMS = new ABLNodeType(Proparse.LISTITEMS, "list-items", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LISTQUERYATTRS = new ABLNodeType(Proparse.LISTQUERYATTRS, "list-query-attrs", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LISTSETATTRS = new ABLNodeType(Proparse.LISTSETATTRS, "list-set-attrs", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LISTWIDGETS = new ABLNodeType(Proparse.LISTWIDGETS, "list-widgets", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LITTLEENDIAN = new ABLNodeType(Proparse.LITTLEENDIAN, "little-endian", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType LOAD = new ABLNodeType(Proparse.LOAD, "load", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LOADPICTURE = new ABLNodeType(Proparse.LOADPICTURE, "load-picture", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LOBDIR = new ABLNodeType(Proparse.LOBDIR, "lob-dir", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LOCAL_METHOD_REF = new ABLNodeType(Proparse.LOCAL_METHOD_REF, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType LOCKED = new ABLNodeType(Proparse.LOCKED, "locked", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LOG = new ABLNodeType(Proparse.LOG, "log", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LOGICAL = new ABLNodeType(Proparse.LOGICAL, "logical", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LOGMANAGER = new ABLNodeType(Proparse.LOGMANAGER, "log-manager", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType LONG = new ABLNodeType(Proparse.LONG, "long", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LONGCHAR = new ABLNodeType(Proparse.LONGCHAR, "longchar", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LOOKAHEAD = new ABLNodeType(Proparse.LOOKAHEAD, "lookahead", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LOOKUP = new ABLNodeType(Proparse.LOOKUP, "lookup", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType LTHAN = new ABLNodeType(Proparse.LTHAN, "lt", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType LTOREQUAL = new ABLNodeType(Proparse.LTOREQUAL, ">=", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType LOOSE_END_KEEPER = new ABLNodeType(Proparse.Loose_End_Keeper, NodeTypesOption.STRUCTURE);
        
        // M
        public static readonly ABLNodeType MACHINECLASS = new ABLNodeType(Proparse.MACHINECLASS, "machine-class", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType MAP = new ABLNodeType(Proparse.MAP, "map", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType MARGINEXTRA = new ABLNodeType(Proparse.MARGINEXTRA, "margin-extra", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MARKNEW = new ABLNodeType(Proparse.MARKNEW, "mark-new", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MARKROWSTATE = new ABLNodeType(Proparse.MARKROWSTATE, "mark-row-state", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MATCHES = new ABLNodeType(Proparse.MATCHES, "matches", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MAXCHARS = new ABLNodeType(Proparse.MAXCHARS, "max-chars", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MAXIMIZE = new ABLNodeType(Proparse.MAXIMIZE, "maximize", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MAXIMUM = new ABLNodeType(Proparse.MAXIMUM, "max", "maximum", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType MAXIMUMLEVEL = new ABLNodeType(Proparse.MAXIMUMLEVEL, "maximum-level", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MAXROWS = new ABLNodeType(Proparse.MAXROWS, "max-rows", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MAXSIZE = new ABLNodeType(Proparse.MAXSIZE, "max-size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MAXVALUE = new ABLNodeType(Proparse.MAXVALUE, "max-value", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MD5DIGEST = new ABLNodeType(Proparse.MD5DIGEST, "md5-digest", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType MEMBER = new ABLNodeType(Proparse.MEMBER, "member", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType MEMPTR = new ABLNodeType(Proparse.MEMPTR, "memptr", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MENU = new ABLNodeType(Proparse.MENU, "menu", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MENUBAR = new ABLNodeType(Proparse.MENUBAR, "menu-bar", "menubar", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MENUITEM = new ABLNodeType(Proparse.MENUITEM, "menu-item", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MERGEBYFIELD = new ABLNodeType(Proparse.MERGEBYFIELD, "merge-by-field", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MESSAGE = new ABLNodeType(Proparse.MESSAGE, "message", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType MESSAGEDIGEST = new ABLNodeType(Proparse.MESSAGEDIGEST, "message-digest", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MESSAGELINE = new ABLNodeType(Proparse.MESSAGELINE, "message-line", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MESSAGELINES = new ABLNodeType(Proparse.MESSAGELINES, "message-lines", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType METHOD = new ABLNodeType(Proparse.METHOD, "method", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MINIMUM = new ABLNodeType(Proparse.MINIMUM, "minimum", 3, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType MINSIZE = new ABLNodeType(Proparse.MINSIZE, "min-size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MINUS = new ABLNodeType(Proparse.MINUS, "-", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType MINVALUE = new ABLNodeType(Proparse.MINVALUE, "min-value", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MODULO = new ABLNodeType(Proparse.MODULO, "modulo", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MONTH = new ABLNodeType(Proparse.MONTH, "month", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType MOUSE = new ABLNodeType(Proparse.MOUSE, "mouse", NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType MOUSEPOINTER = new ABLNodeType(Proparse.MOUSEPOINTER, "mouse-pointer", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MPE = new ABLNodeType(Proparse.MPE, "mpe", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MTIME = new ABLNodeType(Proparse.MTIME, "mtime", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType MULTIPLE = new ABLNodeType(Proparse.MULTIPLE, "multiple", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MULTIPLEKEY = new ABLNodeType(Proparse.MULTIPLEKEY, "multiple-key", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType MULTIPLY = new ABLNodeType(Proparse.MULTIPLY, "*", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType MUSTEXIST = new ABLNodeType(Proparse.MUSTEXIST, "must-exist", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType METHOD_PARAM_LIST = new ABLNodeType(Proparse.Method_param_list, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType METHOD_PARAMETER = new ABLNodeType(Proparse.Method_parameter, NodeTypesOption.STRUCTURE);
        
        // N
        public static readonly ABLNodeType NAMEDOT = new ABLNodeType(Proparse.NAMEDOT, ".", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType NAMESPACEPREFIX = new ABLNodeType(Proparse.NAMESPACEPREFIX, "namespace-prefix", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NAMESPACEURI = new ABLNodeType(Proparse.NAMESPACEURI, "namespace-uri", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NATIVE = new ABLNodeType(Proparse.NATIVE, "native", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NE = new ABLNodeType(Proparse.NE, "ne", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NESTED = new ABLNodeType(Proparse.NESTED, "nested", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NEW = new ABLNodeType(Proparse.NEW, "new", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType NEWINSTANCE = new ABLNodeType(Proparse.NEWINSTANCE, "new-instance", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NEWLINE = new ABLNodeType(Proparse.NEWLINE, NodeTypesOption.NONPRINTABLE);
        public static readonly ABLNodeType NEXT = new ABLNodeType(Proparse.NEXT, "next", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NEXTPROMPT = new ABLNodeType(Proparse.NEXTPROMPT, "next-prompt", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NEXTVALUE = new ABLNodeType(Proparse.NEXTVALUE, "next-value", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType NO = new ABLNodeType(Proparse.NO, "no", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOAPPLY = new ABLNodeType(Proparse.NOAPPLY, "no-apply", NodeTypesOption.KEYWORD);        
        public static readonly ABLNodeType NOASSIGN = new ABLNodeType(Proparse.NOASSIGN, "no-assign", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOATTRLIST = new ABLNodeType(Proparse.NOATTRLIST, "no-attr-list", 9, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOATTRSPACE = new ABLNodeType(Proparse.NOATTRSPACE, "no-attr-space", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOAUTOVALIDATE = new ABLNodeType(Proparse.NOAUTOVALIDATE, "no-auto-validate", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOBINDWHERE = new ABLNodeType(Proparse.NOBINDWHERE, "no-bind-where", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOBOX = new ABLNodeType(Proparse.NOBOX, "no-box", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOCOLUMNSCROLLING = new ABLNodeType(Proparse.NOCOLUMNSCROLLING, "no-column-scrolling", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOCONSOLE = new ABLNodeType(Proparse.NOCONSOLE, "no-console", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOCONVERT = new ABLNodeType(Proparse.NOCONVERT, "no-convert", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOCONVERT3DCOLORS = new ABLNodeType(Proparse.NOCONVERT3DCOLORS, "no-convert-3d-colors", 13, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOCURRENTVALUE = new ABLNodeType(Proparse.NOCURRENTVALUE, "no-current-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NODEBUG = new ABLNodeType(Proparse.NODEBUG, "no-debug", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NODRAG = new ABLNodeType(Proparse.NODRAG, "no-drag", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOECHO = new ABLNodeType(Proparse.NOECHO, "no-echo", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOEMPTYSPACE = new ABLNodeType(Proparse.NOEMPTYSPACE, "no-empty-space", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOERROR = new ABLNodeType(Proparse.NOERROR_KW, "no-error", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOFILL = new ABLNodeType(Proparse.NOFILL, "no-fill", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOFOCUS = new ABLNodeType(Proparse.NOFOCUS, "no-focus", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOHELP = new ABLNodeType(Proparse.NOHELP, "no-help", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOHIDE = new ABLNodeType(Proparse.NOHIDE, "no-hide", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOINDEXHINT = new ABLNodeType(Proparse.NOINDEXHINT, "no-index-hint", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOINHERITBGCOLOR = new ABLNodeType(Proparse.NOINHERITBGCOLOR, "no-inherit-bgcolor", 14, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOINHERITFGCOLOR = new ABLNodeType(Proparse.NOINHERITFGCOLOR, "no-inherit-fgcolor", 14, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOJOINBYSQLDB = new ABLNodeType(Proparse.NOJOINBYSQLDB, "no-join-by-sqldb", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOLABELS = new ABLNodeType(Proparse.NOLABELS, "no-labels", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOLOBS = new ABLNodeType(Proparse.NOLOBS, "no-lobs", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOLOCK = new ABLNodeType(Proparse.NOLOCK, "no-lock", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOLOOKAHEAD = new ABLNodeType(Proparse.NOLOOKAHEAD, "no-lookahead", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOMAP = new ABLNodeType(Proparse.NOMAP, "no-map", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOMESSAGE = new ABLNodeType(Proparse.NOMESSAGE, "no-message", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NONE = new ABLNodeType(Proparse.NONE, "none", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NONSERIALIZABLE = new ABLNodeType(Proparse.NONSERIALIZABLE, "non-serializable", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOPAUSE = new ABLNodeType(Proparse.NOPAUSE, "no-pause", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOPREFETCH = new ABLNodeType(Proparse.NOPREFETCH, "no-prefetch", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NORETURNVALUE = new ABLNodeType(Proparse.NORETURNVALUE, "no-return-value", 13, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NORMAL = new ABLNodeType(Proparse.NORMAL, "normal", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NORMALIZE = new ABLNodeType(Proparse.NORMALIZE, "normalize", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType NOROWMARKERS = new ABLNodeType(Proparse.NOROWMARKERS, "no-row-markers", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOSCROLLBARVERTICAL = new ABLNodeType(Proparse.NOSCROLLBARVERTICAL, "no-scrollbar-vertical", 14, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOSEPARATECONNECTION = new ABLNodeType(Proparse.NOSEPARATECONNECTION, "no-separate-connection", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOSEPARATORS = new ABLNodeType(Proparse.NOSEPARATORS, "no-separators", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOT = new ABLNodeType(Proparse.NOT, "not", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOTACTIVE = new ABLNodeType(Proparse.NOTACTIVE, "not-active", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOTABSTOP = new ABLNodeType(Proparse.NOTABSTOP, "no-tab-stop", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOUNDERLINE = new ABLNodeType(Proparse.NOUNDERLINE, "no-underline", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NOUNDO = new ABLNodeType(Proparse.NOUNDO, "no-undo", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOVALIDATE = new ABLNodeType(Proparse.NOVALIDATE, "no-validate", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOW = new ABLNodeType(Proparse.NOW, "now", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType NOWAIT = new ABLNodeType(Proparse.NOWAIT, "no-wait", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NOWORDWRAP = new ABLNodeType(Proparse.NOWORDWRAP, "no-word-wrap", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NULL = new ABLNodeType(Proparse.NULL_KW, "null", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType NUMALIASES = new ABLNodeType(Proparse.NUMALIASES, "num-aliases", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType NUMBER = new ABLNodeType(Proparse.NUMBER, NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType NUMCOPIES = new ABLNodeType(Proparse.NUMCOPIES, "num-copies", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NUMDBS = new ABLNodeType(Proparse.NUMDBS, "num-dbs", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType NUMENTRIES = new ABLNodeType(Proparse.NUMENTRIES, "num-entries", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType NUMERIC = new ABLNodeType(Proparse.NUMERIC, "numeric", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType NUMRESULTS = new ABLNodeType(Proparse.NUMRESULTS, "num-results", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType NOT_CASESENS = new ABLNodeType(Proparse.Not_casesens, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType NOT_NULL = new ABLNodeType(Proparse.Not_null, NodeTypesOption.STRUCTURE);
        
        // O
        public static readonly ABLNodeType OBJCOLON = new ABLNodeType(Proparse.OBJCOLON, ":", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType OBJECT = new ABLNodeType(Proparse.OBJECT, "object", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OCTETLENGTH = new ABLNodeType(Proparse.OCTETLENGTH, "octet-length", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OF = new ABLNodeType(Proparse.OF, "of", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OFF = new ABLNodeType(Proparse.OFF, "off", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OK = new ABLNodeType(Proparse.OK, "ok", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OKCANCEL = new ABLNodeType(Proparse.OKCANCEL, "ok-cancel", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OLD = new ABLNodeType(Proparse.OLD, "old", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ON = new ABLNodeType(Proparse.ON, "on", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ONLY = new ABLNodeType(Proparse.ONLY, "only", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OPEN = new ABLNodeType(Proparse.OPEN, "open", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OPSYS = new ABLNodeType(Proparse.OPSYS, "opsys", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType OPTION = new ABLNodeType(Proparse.OPTION, "option", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OPTIONS = new ABLNodeType(Proparse.OPTIONS, "options", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OPTIONSFILE = new ABLNodeType(Proparse.OPTIONSFILE, "options-file", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OR = new ABLNodeType(Proparse.OR, "or", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ORDER = new ABLNodeType(Proparse.ORDER, "order", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ORDEREDJOIN = new ABLNodeType(Proparse.ORDEREDJOIN, "ordered-join", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ORDINAL = new ABLNodeType(Proparse.ORDINAL, "ordinal", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OS2 = new ABLNodeType(Proparse.OS2, "os2", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OS400 = new ABLNodeType(Proparse.OS400, "os400", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OSAPPEND = new ABLNodeType(Proparse.OSAPPEND, "os-append", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OSCOMMAND = new ABLNodeType(Proparse.OSCOMMAND, "os-command", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OSCOPY = new ABLNodeType(Proparse.OSCOPY, "os-copy", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OSCREATEDIR = new ABLNodeType(Proparse.OSCREATEDIR, "os-create-dir", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OSDELETE = new ABLNodeType(Proparse.OSDELETE, "os-delete", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OSDIR = new ABLNodeType(Proparse.OSDIR, "os-dir", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OSDRIVES = new ABLNodeType(Proparse.OSDRIVES, "os-drives", 8, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType OSERROR = new ABLNodeType(Proparse.OSERROR, "os-error", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType OSGETENV = new ABLNodeType(Proparse.OSGETENV, "os-getenv", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType OSRENAME = new ABLNodeType(Proparse.OSRENAME, "os-rename", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OTHERWISE = new ABLNodeType(Proparse.OTHERWISE, "otherwise", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OUTER = new ABLNodeType(Proparse.OUTER, "outer", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OUTERJOIN = new ABLNodeType(Proparse.OUTERJOIN, "outer-join", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType OUTPUT = new ABLNodeType(Proparse.OUTPUT, "output", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OVERLAY = new ABLNodeType(Proparse.OVERLAY, "overlay", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType OVERRIDE = new ABLNodeType(Proparse.OVERRIDE, "override", NodeTypesOption.KEYWORD);
        
        // P
        public static readonly ABLNodeType PAGE = new ABLNodeType(Proparse.PAGE, "page", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PAGEBOTTOM = new ABLNodeType(Proparse.PAGEBOTTOM, "page-bottom", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PAGED = new ABLNodeType(Proparse.PAGED, "paged", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PAGENUMBER = new ABLNodeType(Proparse.PAGENUMBER, "page-number", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType PAGESIZE = new ABLNodeType(Proparse.PAGESIZE_KW, "page-size", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType PAGETOP = new ABLNodeType(Proparse.PAGETOP, "page-top", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PAGEWIDTH = new ABLNodeType(Proparse.PAGEWIDTH, "page-width", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PARAMETER = new ABLNodeType(Proparse.PARAMETER, "parameter", 5, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PARENT = new ABLNodeType(Proparse.PARENT, "parent", NodeTypesOption.KEYWORD); // PARENT is not a reserved keyword (not what documentation says)
        public static readonly ABLNodeType PARENTFIELDSAFTER = new ABLNodeType(Proparse.PARENTFIELDSAFTER, "parent-fields-after", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PARENTFIELDSBEFORE = new ABLNodeType(Proparse.PARENTFIELDSBEFORE, "parent-fields-before", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PARENTIDFIELD = new ABLNodeType(Proparse.PARENTIDFIELD, "parent-id-field", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PARENTIDRELATION = new ABLNodeType(Proparse.PARENTIDRELATION, "parent-id-relation", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PARTIALKEY = new ABLNodeType(Proparse.PARTIALKEY, "partial-key", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PASCAL = new ABLNodeType(Proparse.PASCAL_KW, "pascal", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PASSWORDFIELD = new ABLNodeType(Proparse.PASSWORDFIELD, "password-field", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PAUSE = new ABLNodeType(Proparse.PAUSE, "pause", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PBEHASHALGORITHM = new ABLNodeType(Proparse.PBEHASHALGORITHM, "pbe-hash-algorithm", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PBEKEYROUNDS = new ABLNodeType(Proparse.PBEKEYROUNDS, "pbe-key-rounds", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PDBNAME = new ABLNodeType(Proparse.PDBNAME, "pdbname", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType PERFORMANCE = new ABLNodeType(Proparse.PERFORMANCE, "performance", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PERIOD = new ABLNodeType(Proparse.PERIOD, ".", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType PERIODSTART = new ABLNodeType(Proparse.PERIODSTART, ".", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType PERSISTENT = new ABLNodeType(Proparse.PERSISTENT, "persistent", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PFCOLOR = new ABLNodeType(Proparse.PFCOLOR, "pfcolor", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PINNABLE = new ABLNodeType(Proparse.PINNABLE, "pinnable", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PIPE = new ABLNodeType(Proparse.PIPE, "|", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType PLUS = new ABLNodeType(Proparse.PLUS, "+", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType PLUSMINUSSTART = new ABLNodeType(Proparse.PLUSMINUSSTART);
        public static readonly ABLNodeType PORTRAIT = new ABLNodeType(Proparse.PORTRAIT, "portrait", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType POSITION = new ABLNodeType(Proparse.POSITION, "position", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PRECISION = new ABLNodeType(Proparse.PRECISION, "precision", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PREFERDATASET = new ABLNodeType(Proparse.PREFERDATASET, "prefer-dataset", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PREPROCESS = new ABLNodeType(Proparse.PREPROCESS, "preprocess", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PREPROCESSDIRECTIVE = new ABLNodeType(Proparse.PREPROCESSDIRECTIVE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSELSE = new ABLNodeType(Proparse.PREPROCESSELSE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSELSEIF = new ABLNodeType(Proparse.PREPROCESSELSEIF, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSENDIF = new ABLNodeType(Proparse.PREPROCESSENDIF, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSIF = new ABLNodeType(Proparse.PREPROCESSIF, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSJMESSAGE = new ABLNodeType(Proparse.PREPROCESSJMESSAGE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSMESSAGE = new ABLNodeType(Proparse.PREPROCESSMESSAGE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSTOKEN = new ABLNodeType(Proparse.PREPROCESSTOKEN, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PREPROCESSUNDEFINE = new ABLNodeType(Proparse.PREPROCESSUNDEFINE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PRESELECT = new ABLNodeType(Proparse.PRESELECT, "preselect", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PREV = new ABLNodeType(Proparse.PREV, "prev", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PRIMARY = new ABLNodeType(Proparse.PRIMARY, "primary", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PRINTER = new ABLNodeType(Proparse.PRINTER, "printer", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PRINTERSETUP = new ABLNodeType(Proparse.PRINTERSETUP, "printer-setup", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PRIVATE = new ABLNodeType(Proparse.PRIVATE, "private", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PRIVILEGES = new ABLNodeType(Proparse.PRIVILEGES, "privileges", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PROCEDURE = new ABLNodeType(Proparse.PROCEDURE, "procedure", 5, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PROCEDURECALLTYPE = new ABLNodeType(Proparse.PROCEDURECALLTYPE, "procedure-call-type", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PROCESS = new ABLNodeType(Proparse.PROCESS, "process", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PROCESSARCHITECTURE = new ABLNodeType(Proparse.PROCESSARCHITECTURE, "process-architecture", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PROCHANDLE = new ABLNodeType(Proparse.PROCHANDLE, "proc-handle", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PROCSTATUS = new ABLNodeType(Proparse.PROCSTATUS, "proc-status", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PROCTEXT = new ABLNodeType(Proparse.PROCTEXT, "proc-text", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PROCTEXTBUFFER = new ABLNodeType(Proparse.PROCTEXTBUFFER, "proc-text-buffer", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PROFILER = new ABLNodeType(Proparse.PROFILER, "profiler", NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType PROGRAMNAME = new ABLNodeType(Proparse.PROGRAMNAME, "program-name", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType PROGRESS = new ABLNodeType(Proparse.PROGRESS, "progress", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PROMPT = new ABLNodeType(Proparse.PROMPT, "prompt", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PROMPTFOR = new ABLNodeType(Proparse.PROMPTFOR, "prompt-for", 8, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PROMSGS = new ABLNodeType(Proparse.PROMSGS, "promsgs", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PROPARSEDIRECTIVE = new ABLNodeType(Proparse.PROPARSEDIRECTIVE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType PROPATH = new ABLNodeType(Proparse.PROPATH, "propath", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PROPERTY = new ABLNodeType(Proparse.PROPERTY, "property", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PROPERTY_GETTER = new ABLNodeType(Proparse.Property_getter, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType PROPERTY_SETTER = new ABLNodeType(Proparse.Property_setter, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType PROTECTED = new ABLNodeType(Proparse.PROTECTED, "protected", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PROVERSION = new ABLNodeType(Proparse.PROVERSION, "proversion", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType PUBLIC = new ABLNodeType(Proparse.PUBLIC, "public", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUBLISH = new ABLNodeType(Proparse.PUBLISH, "publish", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUT = new ABLNodeType(Proparse.PUT, "put", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PUTBITS = new ABLNodeType(Proparse.PUTBITS, "put-bits", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTBYTE = new ABLNodeType(Proparse.PUTBYTE, "put-byte", "putbyte", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PUTBYTES = new ABLNodeType(Proparse.PUTBYTES, "put-bytes", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTDOUBLE = new ABLNodeType(Proparse.PUTDOUBLE, "put-double", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTFLOAT = new ABLNodeType(Proparse.PUTFLOAT, "put-float", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTINT64 = new ABLNodeType(Proparse.PUTINT64, "put-int64", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTKEYVALUE = new ABLNodeType(Proparse.PUTKEYVALUE, "put-key-value", 11, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType PUTLONG = new ABLNodeType(Proparse.PUTLONG, "put-long", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTSHORT = new ABLNodeType(Proparse.PUTSHORT, "put-short", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTSTRING = new ABLNodeType(Proparse.PUTSTRING, "put-string", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTUNSIGNEDLONG = new ABLNodeType(Proparse.PUTUNSIGNEDLONG, "put-unsigned-long", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PUTUNSIGNEDSHORT = new ABLNodeType(Proparse.PUTUNSIGNEDSHORT, "put-unsigned-short", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType PARAMETER_LIST = new ABLNodeType(Proparse.Parameter_list, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType PROGRAM_ROOT = new ABLNodeType(Proparse.Program_root, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType PROGRAM_TAIL = new ABLNodeType(Proparse.Program_tail, NodeTypesOption.STRUCTURE);
        
        // Q
        public static readonly ABLNodeType QSTRING = new ABLNodeType(Proparse.QSTRING);
        public static readonly ABLNodeType QUERY = new ABLNodeType(Proparse.QUERY, "query", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType QUERYCLOSE = new ABLNodeType(Proparse.QUERYCLOSE, "query-close", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType QUERYOFFEND = new ABLNodeType(Proparse.QUERYOFFEND, "query-off-end", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType QUERYPREPARE = new ABLNodeType(Proparse.QUERYPREPARE, "query-prepare", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType QUERYTUNING = new ABLNodeType(Proparse.QUERYTUNING, "query-tuning", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType QUESTION = new ABLNodeType(Proparse.QUESTION, "question", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType QUIT = new ABLNodeType(Proparse.QUIT, "quit", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType QUOTER = new ABLNodeType(Proparse.QUOTER, "quoter", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        
        // R
        public static readonly ABLNodeType RADIOBUTTONS = new ABLNodeType(Proparse.RADIOBUTTONS, "radio-buttons", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RADIOSET = new ABLNodeType(Proparse.RADIOSET, "radio-set", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RANDOM = new ABLNodeType(Proparse.RANDOM, "random", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RAW = new ABLNodeType(Proparse.RAW, "raw", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RAWTRANSFER = new ABLNodeType(Proparse.RAWTRANSFER, "raw-transfer", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RCODEINFORMATION = new ABLNodeType(Proparse.RCODEINFORMATION, "rcode-information", 10, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType READ = new ABLNodeType(Proparse.READ, "read", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType READAVAILABLE = new ABLNodeType(Proparse.READAVAILABLE, "read-available", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType READEXACTNUM = new ABLNodeType(Proparse.READEXACTNUM, "read-exact-num", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType READKEY = new ABLNodeType(Proparse.READKEY, "readkey", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType READONLY = new ABLNodeType(Proparse.READONLY, "read-only", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REAL = new ABLNodeType(Proparse.REAL, "real", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RECID = new ABLNodeType(Proparse.RECID, "recid", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RECORDLENGTH = new ABLNodeType(Proparse.RECORDLENGTH, "record-length", 10, NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RECORD_NAME = new ABLNodeType(Proparse.RECORD_NAME, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType RECTANGLE = new ABLNodeType(Proparse.RECTANGLE, "rectangle", 4, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType RECURSIVE = new ABLNodeType(Proparse.RECURSIVE, "recursive", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REFERENCEONLY = new ABLNodeType(Proparse.REFERENCEONLY, "reference-only", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REJECTED = new ABLNodeType(Proparse.REJECTED, "rejected", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RELATIONFIELDS = new ABLNodeType(Proparse.RELATIONFIELDS, "relation-fields", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RELEASE = new ABLNodeType(Proparse.RELEASE, "release", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REPEAT = new ABLNodeType(Proparse.REPEAT, "repeat", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REPLACE = new ABLNodeType(Proparse.REPLACE, "replace", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType REPLICATIONCREATE = new ABLNodeType(Proparse.REPLICATIONCREATE, "replication-create", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REPLICATIONDELETE = new ABLNodeType(Proparse.REPLICATIONDELETE, "replication-delete", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REPLICATIONWRITE = new ABLNodeType(Proparse.REPLICATIONWRITE, "replication-write", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REPOSITION = new ABLNodeType(Proparse.REPOSITION, "reposition", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REPOSITIONBACKWARD = new ABLNodeType(Proparse.REPOSITIONBACKWARD, "reposition-backward", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REPOSITIONFORWARD = new ABLNodeType(Proparse.REPOSITIONFORWARD, "reposition-forward", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REPOSITIONMODE = new ABLNodeType(Proparse.REPOSITIONMODE, "reposition-mode", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REPOSITIONTOROW = new ABLNodeType(Proparse.REPOSITIONTOROW, "reposition-to-row", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REPOSITIONTOROWID = new ABLNodeType(Proparse.REPOSITIONTOROWID, "reposition-to-rowid", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REQUEST = new ABLNodeType(Proparse.REQUEST, "request", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RESTARTROW = new ABLNodeType(Proparse.RESTARTROW, "restart-row", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RESULT = new ABLNodeType(Proparse.RESULT, "result", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RETAIN = new ABLNodeType(Proparse.RETAIN, "retain", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType RETAINSHAPE = new ABLNodeType(Proparse.RETAINSHAPE, "retain-shape", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RETRY = new ABLNodeType(Proparse.RETRY, "retry", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType RETRYCANCEL = new ABLNodeType(Proparse.RETRYCANCEL, "retry-cancel", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RETURN = new ABLNodeType(Proparse.RETURN, "return", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType RETURNS = new ABLNodeType(Proparse.RETURNS, "returns", NodeTypesOption.KEYWORD); // Not a reserved keyword
        public static readonly ABLNodeType RETURNTOSTARTDIR = new ABLNodeType(Proparse.RETURNTOSTARTDIR, "return-to-start-dir", 18, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RETURNVALUE = new ABLNodeType(Proparse.RETURNVALUE, "return-value", 10, NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType REVERSEFROM = new ABLNodeType(Proparse.REVERSEFROM, "reverse-from", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType REVERT = new ABLNodeType(Proparse.REVERT, "revert", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType REVOKE = new ABLNodeType(Proparse.REVOKE, "revoke", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType RGBVALUE = new ABLNodeType(Proparse.RGBVALUE, "rgb-value", 5, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RIGHT = new ABLNodeType(Proparse.RIGHT, "right", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RIGHTALIGNED = new ABLNodeType(Proparse.RIGHTALIGNED, "right-aligned", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RIGHTANGLE = new ABLNodeType(Proparse.RIGHTANGLE, ">", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType RIGHTBRACE = new ABLNodeType(Proparse.RIGHTBRACE, "]", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType RIGHTCURLY = new ABLNodeType(Proparse.RIGHTCURLY, "}", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType RIGHTPAREN = new ABLNodeType(Proparse.RIGHTPAREN, ")", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType RIGHTTRIM = new ABLNodeType(Proparse.RIGHTTRIM, "right-trim", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType RINDEX = new ABLNodeType(Proparse.RINDEX, "r-index", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ROUND = new ABLNodeType(Proparse.ROUND, "round", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ROUNDED = new ABLNodeType(Proparse.ROUNDED, "rounded", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ROUTINELEVEL = new ABLNodeType(Proparse.ROUTINELEVEL, "routine-level", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ROW = new ABLNodeType(Proparse.ROW, "row", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ROWCREATED = new ABLNodeType(Proparse.ROWCREATED, "row-created", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ROWDELETED = new ABLNodeType(Proparse.ROWDELETED, "row-deleted", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ROWHEIGHTCHARS = new ABLNodeType(Proparse.ROWHEIGHTCHARS, "row-height", 10, "row-height-chars", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ROWHEIGHTPIXELS = new ABLNodeType(Proparse.ROWHEIGHTPIXELS, "row-height-pixels", 12, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ROWID = new ABLNodeType(Proparse.ROWID, "rowid", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC); // Yes, ROWID is not a reserved keyword
        public static readonly ABLNodeType ROWMODIFIED = new ABLNodeType(Proparse.ROWMODIFIED, "row-modified", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType ROWOF = new ABLNodeType(Proparse.ROWOF, "row-of", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType ROWSTATE = new ABLNodeType(Proparse.ROWSTATE, "row-state", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType ROWUNMODIFIED = new ABLNodeType(Proparse.ROWUNMODIFIED, "row-unmodified", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType RULE = new ABLNodeType(Proparse.RULE, "rule", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType RUN = new ABLNodeType(Proparse.RUN, "run", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType RUNPROCEDURE = new ABLNodeType(Proparse.RUNPROCEDURE, "run-procedure", 8, NodeTypesOption.KEYWORD);
        
        // S
        public static readonly ABLNodeType SAVE = new ABLNodeType(Proparse.SAVE, "save", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAVEAS = new ABLNodeType(Proparse.SAVEAS, "save-as", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SAVECACHE = new ABLNodeType(Proparse.SAVECACHE, "savecache", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SAVEWHERESTRING = new ABLNodeType(Proparse.SAVEWHERESTRING, "save-where-string", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SAXATTRIBUTES = new ABLNodeType(Proparse.SAXATTRIBUTES, "sax-attributes", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SAXCOMPLETE = new ABLNodeType(Proparse.SAXCOMPLETE, "sax-complete", 10, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXPARSERERROR = new ABLNodeType(Proparse.SAXPARSERERROR, "sax-parser-error", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXREADER = new ABLNodeType(Proparse.SAXREADER, "sax-reader", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SAXRUNNING = new ABLNodeType(Proparse.SAXRUNNING, "sax-running", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXUNINITIALIZED = new ABLNodeType(Proparse.SAXUNINITIALIZED, "sax-uninitialized", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITER = new ABLNodeType(Proparse.SAXWRITER, "sax-writer", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SAXWRITEBEGIN = new ABLNodeType(Proparse.SAXWRITEBEGIN, "sax-write-begin", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITECOMPLETE = new ABLNodeType(Proparse.SAXWRITECOMPLETE, "sax-write-complete", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITECONTENT = new ABLNodeType(Proparse.SAXWRITECONTENT, "sax-write-content", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITEELEMENT = new ABLNodeType(Proparse.SAXWRITEELEMENT, "sax-write-element", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITEERROR = new ABLNodeType(Proparse.SAXWRITEERROR, "sax-write-error", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITEIDLE = new ABLNodeType(Proparse.SAXWRITEIDLE, "sax-write-idle", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SAXWRITETAG = new ABLNodeType(Proparse.SAXWRITETAG, "sax-write-tag", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SCHEMA = new ABLNodeType(Proparse.SCHEMA, "schema", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SCOPEDDEFINE = new ABLNodeType(Proparse.SCOPEDDEFINE, NodeTypesOption.PREPROCESSOR);
        public static readonly ABLNodeType SCREEN = new ABLNodeType(Proparse.SCREEN, "screen", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SCREENIO = new ABLNodeType(Proparse.SCREENIO, "screen-io", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SCREENLINES = new ABLNodeType(Proparse.SCREENLINES, "screen-lines", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType SCREENVALUE = new ABLNodeType(Proparse.SCREENVALUE, "screen-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SCROLL = new ABLNodeType(Proparse.SCROLL, "scroll", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SCROLLABLE = new ABLNodeType(Proparse.SCROLLABLE, "scrollable", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SCROLLBARHORIZONTAL = new ABLNodeType(Proparse.SCROLLBARHORIZONTAL, "scrollbar-horizontal", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SCROLLBARVERTICAL = new ABLNodeType(Proparse.SCROLLBARVERTICAL, "scrollbar-vertical", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SCROLLING = new ABLNodeType(Proparse.SCROLLING, "scrolling", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SDBNAME = new ABLNodeType(Proparse.SDBNAME, "sdbname", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SEARCH = new ABLNodeType(Proparse.SEARCH, "search", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SEARCHSELF = new ABLNodeType(Proparse.SEARCHSELF, "search-self", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SEARCHTARGET = new ABLNodeType(Proparse.SEARCHTARGET, "search-target", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SECTION = new ABLNodeType(Proparse.SECTION, "section", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SECURITYPOLICY = new ABLNodeType(Proparse.SECURITYPOLICY, "security-policy", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType SEEK = new ABLNodeType(Proparse.SEEK, "seek", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SELECT = new ABLNodeType(Proparse.SELECT, "select", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SELECTION = new ABLNodeType(Proparse.SELECTION, "selection", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SELECTIONLIST = new ABLNodeType(Proparse.SELECTIONLIST, "selection-list", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SELF = new ABLNodeType(Proparse.SELF, "self", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType SEMI = new ABLNodeType(Proparse.SEMI, ";", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType SEND = new ABLNodeType(Proparse.SEND, "send", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SENDSQLSTATEMENT = new ABLNodeType(Proparse.SENDSQLSTATEMENT, "send-sql-statement", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SENSITIVE = new ABLNodeType(Proparse.SENSITIVE, "sensitive", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SEPARATECONNECTION = new ABLNodeType(Proparse.SEPARATECONNECTION, "separate-connection", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SEPARATORS = new ABLNodeType(Proparse.SEPARATORS, "separators", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SERIALIZABLE = new ABLNodeType(Proparse.SERIALIZABLE, "serializable", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SERIALIZEHIDDEN = new ABLNodeType(Proparse.SERIALIZEHIDDEN, "serialize-hidden", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SERIALIZENAME = new ABLNodeType(Proparse.SERIALIZENAME, "serialize-name", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SERVER = new ABLNodeType(Proparse.SERVER, "server", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SERVERSOCKET = new ABLNodeType(Proparse.SERVERSOCKET, "server-socket", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SESSION = new ABLNodeType(Proparse.SESSION, "session", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType SET = new ABLNodeType(Proparse.SET, "set", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SETATTRCALLTYPE = new ABLNodeType(Proparse.SETATTRCALLTYPE, "set-attr-call-type", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SETBYTEORDER = new ABLNodeType(Proparse.SETBYTEORDER, "set-byte-order", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SETCONTENTS = new ABLNodeType(Proparse.SETCONTENTS, "set-contents", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SETCURRENTVALUE = new ABLNodeType(Proparse.SETCURRENTVALUE, "set-current-value", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SETDBCLIENT = new ABLNodeType(Proparse.SETDBCLIENT, "set-db-client", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SETEFFECTIVETENANT = new ABLNodeType(Proparse.SETEFFECTIVETENANT, "set-effective-tenant", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SETPOINTERVALUE = new ABLNodeType(Proparse.SETPOINTERVALUE, "set-pointer-value", 15, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SETSIZE = new ABLNodeType(Proparse.SETSIZE, "set-size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SETUSERID = new ABLNodeType(Proparse.SETUSERID, "setuserid", 7, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SHA1DIGEST = new ABLNodeType(Proparse.SHA1DIGEST, "sha1-digest", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SHARED = new ABLNodeType(Proparse.SHARED, "shared", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SHARELOCK = new ABLNodeType(Proparse.SHARELOCK, "share-lock", 5, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SHORT = new ABLNodeType(Proparse.SHORT, "short", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SHOWSTATS = new ABLNodeType(Proparse.SHOWSTATS, "show-stats", 9, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SIDELABELS = new ABLNodeType(Proparse.SIDELABELS, "side-labels", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SIGNATURE = new ABLNodeType(Proparse.SIGNATURE, "signature", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SILENT = new ABLNodeType(Proparse.SILENT, "silent", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SIMPLE = new ABLNodeType(Proparse.SIMPLE, "simple", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SINGLE = new ABLNodeType(Proparse.SINGLE, "single", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SINGLERUN = new ABLNodeType(Proparse.SINGLERUN, "single-run", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SINGLETON = new ABLNodeType(Proparse.SINGLETON, "singleton", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SINGLEQUOTE = new ABLNodeType(Proparse.SINGLEQUOTE, "'", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType SIZE = new ABLNodeType(Proparse.SIZE, "size", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SIZECHARS = new ABLNodeType(Proparse.SIZECHARS, "size-chars", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SIZEPIXELS = new ABLNodeType(Proparse.SIZEPIXELS, "size-pixels", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SKIP = new ABLNodeType(Proparse.SKIP, "skip", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SKIPDELETEDRECORD = new ABLNodeType(Proparse.SKIPDELETEDRECORD, "skip-deleted-record", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SKIPGROUPDUPLICATES = new ABLNodeType(Proparse.SKIPGROUPDUPLICATES, "skip-group-duplicates", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SLASH = new ABLNodeType(Proparse.SLASH, "/", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType SLIDER = new ABLNodeType(Proparse.SLIDER, "slider", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SMALLINT = new ABLNodeType(Proparse.SMALLINT, "smallint", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SOAPHEADER = new ABLNodeType(Proparse.SOAPHEADER, "soap-header", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SOAPHEADERENTRYREF = new ABLNodeType(Proparse.SOAPHEADERENTRYREF, "soap-header-entryref", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SOCKET = new ABLNodeType(Proparse.SOCKET, "socket", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SOME = new ABLNodeType(Proparse.SOME, "some", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SORT = new ABLNodeType(Proparse.SORT, "sort", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SOURCE = new ABLNodeType(Proparse.SOURCE, "source", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SOURCEPROCEDURE = new ABLNodeType(Proparse.SOURCEPROCEDURE, "source-procedure", NodeTypesOption.KEYWORD,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType SPACE = new ABLNodeType(Proparse.SPACE, "space", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SQL = new ABLNodeType(Proparse.SQL, "sql", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SQRT = new ABLNodeType(Proparse.SQRT, "sqrt", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SQSTRING = new ABLNodeType(Proparse.SQSTRING);
        public static readonly ABLNodeType SSLSERVERNAME = new ABLNodeType(Proparse.SSLSERVERNAME, "ssl-server-name", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType STAR = new ABLNodeType(Proparse.STAR, "*", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType START = new ABLNodeType(Proparse.START, "start", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STARTING = new ABLNodeType(Proparse.STARTING, "starting", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STARTMOVE = new ABLNodeType(Proparse.STARTMOVE, "start-move", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STARTRESIZE = new ABLNodeType(Proparse.STARTRESIZE, "start-resize", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STARTROWRESIZE = new ABLNodeType(Proparse.STARTROWRESIZE, "start-row-resize", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STATIC = new ABLNodeType(Proparse.STATIC, "static", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STATUS = new ABLNodeType(Proparse.STATUS, "status", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STATUSBAR = new ABLNodeType(Proparse.STATUSBAR, "status-bar", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STDCALL = new ABLNodeType(Proparse.STDCALL_KW, "stdcall", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STOMPDETECTION = new ABLNodeType(Proparse.STOMPDETECTION, "stomp-detection", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STOMPFREQUENCY = new ABLNodeType(Proparse.STOMPFREQUENCY, "stomp-frequency", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STOP = new ABLNodeType(Proparse.STOP, "stop", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STOPAFTER = new ABLNodeType(Proparse.STOPAFTER, "stop-after", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STOREDPROCEDURE = new ABLNodeType(Proparse.STOREDPROCEDURE, "stored-procedure", 11, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STREAM = new ABLNodeType(Proparse.STREAM, "stream", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STREAMHANDLE = new ABLNodeType(Proparse.STREAMHANDLE, "stream-handle", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STREAMIO = new ABLNodeType(Proparse.STREAMIO, "stream-io", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType STRETCHTOFIT = new ABLNodeType(Proparse.STRETCHTOFIT, "stretch-to-fit", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType STRING = new ABLNodeType(Proparse.STRING, "string", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType STRINGXREF = new ABLNodeType(Proparse.STRINGXREF, "string-xref", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBAVERAGE = new ABLNodeType(Proparse.SUBAVERAGE, "sub-average", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBCOUNT = new ABLNodeType(Proparse.SUBCOUNT, "sub-count", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBMAXIMUM = new ABLNodeType(Proparse.SUBMAXIMUM, "sub-maximum", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBMENU = new ABLNodeType(Proparse.SUBMENU, "sub-menu", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBMENUHELP = new ABLNodeType(Proparse.SUBMENUHELP, "sub-menu-help", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBMINIMUM = new ABLNodeType(Proparse.SUBMINIMUM, "sub-minimum", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBSCRIBE = new ABLNodeType(Proparse.SUBSCRIBE, "subscribe", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUBSTITUTE = new ABLNodeType(Proparse.SUBSTITUTE, "substitute", 5, NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SUBSTRING = new ABLNodeType(Proparse.SUBSTRING, "substring", 6, NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SUBTOTAL = new ABLNodeType(Proparse.SUBTOTAL, "sub-total", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUM = new ABLNodeType(Proparse.SUM, "sum", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType SUMMARY = new ABLNodeType(Proparse.SUMMARY, "summary", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SUPER = new ABLNodeType(Proparse.SUPER, "super", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC,
            NodeTypesOption.MAY_BE_REGULAR_FUNC, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType SYMMETRICENCRYPTIONALGORITHM = new ABLNodeType(Proparse.SYMMETRICENCRYPTIONALGORITHM, "symmetric-encryption-algorithm",
            NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SYMMETRICENCRYPTIONIV = new ABLNodeType(Proparse.SYMMETRICENCRYPTIONIV, "symmetric-encryption-iv", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SYMMETRICENCRYPTIONKEY = new ABLNodeType(Proparse.SYMMETRICENCRYPTIONKEY, "symmetric-encryption-key",
            NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SYMMETRICSUPPORT = new ABLNodeType(Proparse.SYMMETRICSUPPORT, "symmetric-support", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SYSTEMDIALOG = new ABLNodeType(Proparse.SYSTEMDIALOG, "system-dialog", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType SYSTEMHELP = new ABLNodeType(Proparse.SYSTEMHELP, "system-help", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType SCANNER_HEAD = new ABLNodeType(Proparse.Scanner_head, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SCANNER_TAIL = new ABLNodeType(Proparse.Scanner_tail, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_BEGINS = new ABLNodeType(Proparse.Sql_begins, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_BETWEEN = new ABLNodeType(Proparse.Sql_between, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_COMP_QUERY = new ABLNodeType(Proparse.Sql_comp_query, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_IN = new ABLNodeType(Proparse.Sql_in, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_LIKE = new ABLNodeType(Proparse.Sql_like, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_NULL_TEST = new ABLNodeType(Proparse.Sql_null_test, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType SQL_SELECT_WHAT = new ABLNodeType(Proparse.Sql_select_what, NodeTypesOption.STRUCTURE);
        
        // T
        public static readonly ABLNodeType TABLE = new ABLNodeType(Proparse.TABLE, "table", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TABLEHANDLE = new ABLNodeType(Proparse.TABLEHANDLE, "table-handle", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TABLENUMBER = new ABLNodeType(Proparse.TABLENUMBER, "table-number", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TABLESCAN = new ABLNodeType(Proparse.TABLESCAN, "table-scan", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TARGET = new ABLNodeType(Proparse.TARGET, "target", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TARGETPROCEDURE = new ABLNodeType(Proparse.TARGETPROCEDURE, "target-procedure", NodeTypesOption.KEYWORD,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType TEMPTABLE = new ABLNodeType(Proparse.TEMPTABLE, "temp-table", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TENANT = new ABLNodeType(Proparse.TENANT, "tenant", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TENANTID = new ABLNodeType(Proparse.TENANTID, "tenant-id", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType TENANTNAME = new ABLNodeType(Proparse.TENANTNAME, "tenant-name", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType TENANTNAMETOID = new ABLNodeType(Proparse.TENANTNAMETOID, "tenant-name-to-id", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType TENANTWHERE = new ABLNodeType(Proparse.TENANTWHERE, "tenant-where", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TERMINAL = new ABLNodeType(Proparse.TERMINAL, "term", "terminal", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType TERMINATE = new ABLNodeType(Proparse.TERMINATE, "terminate", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TEXT = new ABLNodeType(Proparse.TEXT, "text", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TEXTCURSOR = new ABLNodeType(Proparse.TEXTCURSOR, "text-cursor", NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType TEXTSEGGROW = new ABLNodeType(Proparse.TEXTSEGGROW, "text-seg-growth", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType THEN = new ABLNodeType(Proparse.THEN, "then", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType THISOBJECT = new ABLNodeType(Proparse.THISOBJECT, "this-object", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType THISPROCEDURE = new ABLNodeType(Proparse.THISPROCEDURE, "this-procedure", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType THREED = new ABLNodeType(Proparse.THREED, "three-d", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType THROUGH = new ABLNodeType(Proparse.THROUGH, "through", "thru", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType THROW = new ABLNodeType(Proparse.THROW, "throw", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TICMARKS = new ABLNodeType(Proparse.TICMARKS, "tic-marks", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TILDE = new ABLNodeType(Proparse.TILDE, "~", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType TIME = new ABLNodeType(Proparse.TIME, "time", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType TIMESTAMP = new ABLNodeType(Proparse.TIMESTAMP, "timestamp", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TIMEZONE = new ABLNodeType(Proparse.TIMEZONE, "timezone", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType TITLE = new ABLNodeType(Proparse.TITLE, "title", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TO = new ABLNodeType(Proparse.TO, "to", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TODAY = new ABLNodeType(Proparse.TODAY, "today", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType TOGGLEBOX = new ABLNodeType(Proparse.TOGGLEBOX, "toggle-box", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TOOLBAR = new ABLNodeType(Proparse.TOOLBAR, "tool-bar", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TOOLTIP = new ABLNodeType(Proparse.TOOLTIP, "tooltip", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TOP = new ABLNodeType(Proparse.TOP, "top", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TOPIC = new ABLNodeType(Proparse.TOPIC, "topic", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TOPNAVQUERY = new ABLNodeType(Proparse.TOPNAVQUERY, "top-nav-query", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TOPONLY = new ABLNodeType(Proparse.TOPONLY, "top-only", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TOROWID = new ABLNodeType(Proparse.TOROWID, "to-rowid", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType TOTAL = new ABLNodeType(Proparse.TOTAL, "total", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TRAILING = new ABLNodeType(Proparse.TRAILING, "trailing", 5, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TRANSACTION = new ABLNodeType(Proparse.TRANSACTION, "trans", 5, "transaction", 8, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED, NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        public static readonly ABLNodeType TRANSACTIONMODE = new ABLNodeType(Proparse.TRANSACTIONMODE, "transaction-mode", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TRANSINITPROCEDURE = new ABLNodeType(Proparse.TRANSINITPROCEDURE, "trans-init-procedure", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TRANSPARENT = new ABLNodeType(Proparse.TRANSPARENT, "transparent", 8, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TRIGGER = new ABLNodeType(Proparse.TRIGGER, "trigger", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TRIGGERS = new ABLNodeType(Proparse.TRIGGERS, "triggers", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TRIM = new ABLNodeType(Proparse.TRIM, "trim", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType TRUE = new ABLNodeType(Proparse.TRUE_KW, "true", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType TRUNCATE = new ABLNodeType(Proparse.TRUNCATE, "truncate", 5, NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType TTCODEPAGE = new ABLNodeType(Proparse.TTCODEPAGE, "ttcodepage", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType TYPE_NAME = new ABLNodeType(Proparse.TYPE_NAME, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType TYPELESS_TOKEN = new ABLNodeType(Proparse.TYPELESS_TOKEN, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType TYPEOF = new ABLNodeType(Proparse.TYPEOF, "type-of", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        
        // U
        public static readonly ABLNodeType UNARY_MINUS = new ABLNodeType(Proparse.UNARY_MINUS, "-", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType UNARY_PLUS = new ABLNodeType(Proparse.UNARY_PLUS, "+", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType UNBOX = new ABLNodeType(Proparse.UNBOX, "unbox", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType UNBUFFERED = new ABLNodeType(Proparse.UNBUFFERED, "unbuffered", 6, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType UNDERLINE = new ABLNodeType(Proparse.UNDERLINE, "underline", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNDO = new ABLNodeType(Proparse.UNDO, "undo", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNFORMATTED = new ABLNodeType(Proparse.UNFORMATTED, "unformatted", 6, NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNION = new ABLNodeType(Proparse.UNION, "union", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNIQUE = new ABLNodeType(Proparse.UNIQUE, "unique", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNIQUEMATCH = new ABLNodeType(Proparse.UNIQUEMATCH, "unique-match", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType UNIX = new ABLNodeType(Proparse.UNIX, "unix", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNKNOWNVALUE = new ABLNodeType(Proparse.UNKNOWNVALUE, "?", NodeTypesOption.SYMBOL);
        public static readonly ABLNodeType UNLESSHIDDEN = new ABLNodeType(Proparse.UNLESSHIDDEN, "unless-hidden", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UNLOAD = new ABLNodeType(Proparse.UNLOAD, "unload", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType UNQUOTEDSTRING = new ABLNodeType(Proparse.UNQUOTEDSTRING);
        public static readonly ABLNodeType UNSIGNEDBYTE = new ABLNodeType(Proparse.UNSIGNEDBYTE, "unsigned-byte", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType UNSIGNEDSHORT = new ABLNodeType(Proparse.UNSIGNEDSHORT, "unsigned-short", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType UNSUBSCRIBE = new ABLNodeType(Proparse.UNSUBSCRIBE, "unsubscribe", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType UP = new ABLNodeType(Proparse.UP, "up", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType UPDATE = new ABLNodeType(Proparse.UPDATE, "update", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType URLDECODE = new ABLNodeType(Proparse.URLDECODE, "url-decode", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType URLENCODE = new ABLNodeType(Proparse.URLENCODE, "url-encode", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USE = new ABLNodeType(Proparse.USE, "use", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USEDICTEXPS = new ABLNodeType(Proparse.USEDICTEXPS, "use-dict-exps", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USEFILENAME = new ABLNodeType(Proparse.USEFILENAME, "use-filename", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USEINDEX = new ABLNodeType(Proparse.USEINDEX, "use-index", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType USER = new ABLNodeType(Proparse.USER, "user", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_NO_ARG_FUNC,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType USEREVVIDEO = new ABLNodeType(Proparse.USEREVVIDEO, "use-revvideo", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USERID = new ABLNodeType(Proparse.USERID, "userid", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED,
            NodeTypesOption.MAY_BE_NO_ARG_FUNC, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType USER_FUNC = new ABLNodeType(Proparse.USER_FUNC, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType USETEXT = new ABLNodeType(Proparse.USETEXT, "use-text", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USEUNDERLINE = new ABLNodeType(Proparse.USEUNDERLINE, "use-underline", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USEWIDGETPOOL = new ABLNodeType(Proparse.USEWIDGETPOOL, "use-widget-pool", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType USING = new ABLNodeType(Proparse.USING, "using", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        
        // V
        public static readonly ABLNodeType V6FRAME = new ABLNodeType(Proparse.V6FRAME, "v6frame", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType VALIDATE = new ABLNodeType(Proparse.VALIDATE, "validate", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType VALIDEVENT = new ABLNodeType(Proparse.VALIDEVENT, "valid-event", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType VALIDHANDLE = new ABLNodeType(Proparse.VALIDHANDLE, "valid-handle", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType VALIDOBJECT = new ABLNodeType(Proparse.VALIDOBJECT, "valid-object", NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType VALUE = new ABLNodeType(Proparse.VALUE, "value", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType VALUECHANGED = new ABLNodeType(Proparse.VALUECHANGED, "value-changed", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType VALUES = new ABLNodeType(Proparse.VALUES, "values", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType VARIABLE = new ABLNodeType(Proparse.VARIABLE, "variable", 3, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType VERBOSE = new ABLNodeType(Proparse.VERBOSE, "verbose", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType VERTICAL = new ABLNodeType(Proparse.VERTICAL, "vertical", 4, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType VIEW = new ABLNodeType(Proparse.VIEW, "view", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType VIEWAS = new ABLNodeType(Proparse.VIEWAS, "view-as", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType VISIBLE = new ABLNodeType(Proparse.VISIBLE, "visible", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType VMS = new ABLNodeType(Proparse.VMS, "vms", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType VOID = new ABLNodeType(Proparse.VOID, "void", NodeTypesOption.KEYWORD);
        
        // W
        public static readonly ABLNodeType WAIT = new ABLNodeType(Proparse.WAIT, "wait", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WAITFOR = new ABLNodeType(Proparse.WAITFOR, "wait-for", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WARNING = new ABLNodeType(Proparse.WARNING, "warning", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WEBCONTEXT = new ABLNodeType(Proparse.WEBCONTEXT, "web-context", 7, NodeTypesOption.KEYWORD, NodeTypesOption.SYSHDL);
        public static readonly ABLNodeType WEEKDAY = new ABLNodeType(Proparse.WEEKDAY, "weekday", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType WHEN = new ABLNodeType(Proparse.WHEN, "when", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WHERE = new ABLNodeType(Proparse.WHERE, "where", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WHILE = new ABLNodeType(Proparse.WHILE, "while", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WIDGET = new ABLNodeType(Proparse.WIDGET, "widget", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WIDGETHANDLE = new ABLNodeType(Proparse.WIDGETHANDLE, "widget-handle", 8, NodeTypesOption.KEYWORD,
            NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType WIDGETID = new ABLNodeType(Proparse.WIDGETID, "widget-id", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WIDGETPOOL = new ABLNodeType(Proparse.WIDGETPOOL, "widget-pool", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WIDTH = new ABLNodeType(Proparse.WIDTH, "width", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WIDTHCHARS = new ABLNodeType(Proparse.WIDTHCHARS, "width-chars", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WIDTHPIXELS = new ABLNodeType(Proparse.WIDTHPIXELS, "width-pixels", 7, NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WINDOW = new ABLNodeType(Proparse.WINDOW, "window", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WINDOWDELAYEDMINIMIZE = new ABLNodeType(Proparse.WINDOWDELAYEDMINIMIZE, "window-delayed-minimize", 18,
            NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WINDOWMAXIMIZED = new ABLNodeType(Proparse.WINDOWMAXIMIZED, "window-maximized", 12, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WINDOWMINIMIZED = new ABLNodeType(Proparse.WINDOWMINIMIZED, "window-minimized", 12, NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WINDOWNAME = new ABLNodeType(Proparse.WINDOWNAME, "window-name", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WINDOWNORMAL = new ABLNodeType(Proparse.WINDOWNORMAL, "window-normal", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WITH = new ABLNodeType(Proparse.WITH, "with", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WORDINDEX = new ABLNodeType(Proparse.WORDINDEX, "word-index", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType WORKTABLE = new ABLNodeType(Proparse.WORKTABLE, "work-table", 8, "workfile", NodeTypesOption.KEYWORD,
            NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WRITE = new ABLNodeType(Proparse.WRITE, "write", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType WS = new ABLNodeType(Proparse.WS);
        public static readonly ABLNodeType WIDGET_REF = new ABLNodeType(Proparse.Widget_ref, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType WITH_COLUMNS = new ABLNodeType(Proparse.With_columns, NodeTypesOption.STRUCTURE);
        public static readonly ABLNodeType WITH_DOWN = new ABLNodeType(Proparse.With_down, NodeTypesOption.STRUCTURE);
        
        // X
        public static readonly ABLNodeType X = new ABLNodeType(Proparse.X, "x", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XCODE = new ABLNodeType(Proparse.XCODE, "xcode", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType XDOCUMENT = new ABLNodeType(Proparse.XDOCUMENT, "x-document", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XMLDATATYPE = new ABLNodeType(Proparse.XMLDATATYPE, "xml-data-type", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XMLNODENAME = new ABLNodeType(Proparse.XMLNODENAME, "xml-node-name", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XMLNODETYPE = new ABLNodeType(Proparse.XMLNODETYPE, "xml-node-type", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XNODEREF = new ABLNodeType(Proparse.XNODEREF, "x-noderef", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XOF = new ABLNodeType(Proparse.XOF, "x-of", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType XOR = new ABLNodeType(Proparse.XOR, "xor", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType XREF = new ABLNodeType(Proparse.XREF, "xref", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType XREFXML = new ABLNodeType(Proparse.XREFXML, "xref-xml", NodeTypesOption.KEYWORD);
        
        // Y
        public static readonly ABLNodeType Y = new ABLNodeType(Proparse.Y, "y", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType YEAR = new ABLNodeType(Proparse.YEAR, "year", NodeTypesOption.KEYWORD, NodeTypesOption.MAY_BE_REGULAR_FUNC);
        public static readonly ABLNodeType YES = new ABLNodeType(Proparse.YES, "yes", NodeTypesOption.KEYWORD, NodeTypesOption.RESERVED);
        public static readonly ABLNodeType YESNO = new ABLNodeType(Proparse.YESNO, "yes-no", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType YESNOCANCEL = new ABLNodeType(Proparse.YESNOCANCEL, "yes-no-cancel", NodeTypesOption.KEYWORD);
        public static readonly ABLNodeType YOF = new ABLNodeType(Proparse.YOF, "y-of", NodeTypesOption.KEYWORD);

        public static IEnumerable<ABLNodeType> Values
        {
            get
            {
                yield return EMPTY_NODE;
                yield return INVALID_NODE;
                yield return EOF_ANTLR4;
                yield return INCLUDEDIRECTIVE;
                yield return AACBIT;
                yield return AACONTROL;
                yield return AALIST;
                yield return AAMEMORY;
                yield return AAMSG;
                yield return AAPCONTROL;
                yield return AASERIAL;
                yield return AATRACE;
                yield return ABSOLUTE;
                yield return ABSTRACT;
                yield return ACCELERATOR;
                yield return ACCUMULATE;
                yield return ACTIVEFORM;
                yield return ACTIVEWINDOW;
                yield return ADD;
                yield return ADDINTERVAL;
                yield return ADVISE;
                yield return ALERTBOX;
                yield return ALIAS;
                yield return ALL;
                yield return ALLOWREPLICATION;
                yield return ALTER;
                yield return ALTERNATEKEY;
                yield return AMBIGUOUS;
                yield return AMPANALYZERESUME;
                yield return AMPANALYZESUSPEND;
                yield return AMPELSE;
                yield return AMPELSEIF;
                yield return AMPENDIF;
                yield return AMPGLOBALDEFINE;
                yield return AMPIF;
                yield return AMPMESSAGE;
                yield return AMPSCOPEDDEFINE;
                yield return AMPTHEN;
                yield return AMPUNDEFINE;
                yield return ANALYZE;
                yield return AND;
                yield return ANNOTATION;
                yield return ANSIONLY;
                yield return ANY;
                yield return ANYWHERE;
                yield return APPEND;
                yield return APPLICATION;
                yield return APPLY;
                yield return ARRAYMESSAGE;
                yield return AS;
                yield return ASC;
                yield return ASCENDING;
                yield return ASKOVERWRITE;
                yield return ASSEMBLY;
                yield return ASSIGN;
                yield return ASSIGN_DYNAMIC_NEW;
                yield return ASYNCHRONOUS;
                yield return AT;
                yield return ATTACHMENT;
                yield return ATTRSPACE;
                yield return AUDITCONTROL;
                yield return AUDITENABLED;
                yield return AUDITPOLICY;
                yield return AUTHORIZATION;
                yield return AUTOCOMPLETION;
                yield return AUTOENDKEY;
                yield return AUTOGO;
                yield return AUTOMATIC;
                yield return AUTORETURN;
                yield return AVAILABLE;
                yield return AVERAGE;
                yield return AVG;
                yield return AGGREGATE_PHRASE;
                yield return ARRAY_SUBSCRIPT;
                yield return ASSIGN_FROM_BUFFER;
                yield return AUTOMATION_OBJECT;
                yield return BACKGROUND;
                yield return BACKSLASH;
                yield return BACKTICK;
                yield return BACKWARDS;
                yield return BASE64;
                yield return BASE64DECODE;
                yield return BASE64ENCODE;
                yield return BASEKEY;
                yield return BATCHSIZE;
                yield return BEFOREHIDE;
                yield return BEFORETABLE;
                yield return BEGINS;
                yield return BELL;
                yield return BETWEEN;
                yield return BGCOLOR;
                yield return BIGENDIAN;
                yield return BIGINT;
                yield return BINARY;
                yield return BIND;
                yield return BINDWHERE;
                yield return BLANK;
                yield return BLOB;
                yield return BLOCK_LABEL;
                yield return BLOCKLEVEL;
                yield return BOTH;
                yield return BOTTOM;
                yield return BOX;
                yield return BREAK;
                yield return BROWSE;
                yield return BTOS;
                yield return BUFFER;
                yield return BUFFERCHARS;
                yield return BUFFERCOMPARE;
                yield return BUFFERCOPY;
                yield return BUFFERGROUPID;
                yield return BUFFERGROUPNAME;
                yield return BUFFERLINES;
                yield return BUFFERNAME;
                yield return BUFFERTENANTNAME;
                yield return BUFFERTENANTID;
                yield return BUTTON;
                yield return BUTTONS;
                yield return BY;
                yield return BYPOINTER;
                yield return BYREFERENCE;
                yield return BYTE;
                yield return BYVALUE;
                yield return BYVARIANTPOINTER;
                yield return BLOCK_ITERATOR;
                yield return CACHE;
                yield return CACHESIZE;
                yield return CALL;
                yield return CANCELBUTTON;
                yield return CANDO;
                yield return CANFIND;
                yield return CANQUERY;
                yield return CANSET;
                yield return CAPS;
                yield return CARET;
                yield return CASE;
                yield return CASESENSITIVE;
                yield return CAST;
                yield return CATCH;
                yield return CDECL;
                yield return CENTERED;
                yield return CHAINED;
                yield return CHARACTER;
                yield return CHARACTERLENGTH;
                yield return CHARSET;
                yield return CHECK;
                yield return CHECKED;
                yield return CHOOSE;
                yield return CHR;
                yield return CLASS;
                yield return CLEAR;
                yield return CLIENTPRINCIPAL;
                yield return CLIPBOARD;
                yield return CLOB;
                yield return CLOSE;
                yield return CODEBASELOCATOR;
                yield return CODEPAGE;
                yield return CODEPAGECONVERT;
                yield return COLLATE;
                yield return COLOF;
                yield return COLON;
                yield return COLONALIGNED;
                yield return COLOR;
                yield return COLORTABLE;
                yield return COLUMN;
                yield return COLUMNBGCOLOR;
                yield return COLUMNCODEPAGE;
                yield return COLUMNDCOLOR;
                yield return COLUMNFGCOLOR;
                yield return COLUMNFONT;
                yield return COLUMNLABEL;
                yield return COLUMNOF;
                yield return COLUMNPFCOLOR;
                yield return COLUMNS;
                yield return COMBOBOX;
                yield return COMHANDLE;
                yield return COMMA;
                yield return COMMAND;
                yield return COMMENT;
                yield return COMMENTEND;
                yield return COMMENTSTART;
                yield return COMPARE;
                yield return COMPARES;
                yield return COMPILE;
                yield return COMPILER;
                yield return COMPLETE;
                yield return COMSELF;
                yield return CONFIGNAME;
                yield return CONNECT;
                yield return CONNECTED;
                yield return CONSTRUCTOR;
                yield return CONTAINS;
                yield return CONTENTS;
                yield return CONTEXT;
                yield return CONTEXTHELP;
                yield return CONTEXTHELPFILE;
                yield return CONTEXTHELPID;
                yield return CONTEXTPOPUP;
                yield return CONTROL;
                yield return CONTROLFRAME;
                yield return CONVERT;
                yield return CONVERT3DCOLORS;
                yield return COPYDATASET;
                yield return COPYLOB;
                yield return COPYTEMPTABLE;
                yield return COUNT;
                yield return COUNTOF;
                yield return CREATE;
                yield return CREATELIKESEQUENTIAL;
                yield return CREATETESTFILE;
                yield return CURLYAMP;
                yield return CURLYNUMBER;
                yield return CURLYSTAR;
                yield return CURRENCY;
                yield return CURRENT;
                yield return CURRENTCHANGED;
                yield return CURRENTENVIRONMENT;
                yield return CURRENTLANGUAGE;
                yield return CURRENTQUERY;
                yield return CURRENTRESULTROW;
                yield return CURRENTVALUE;
                yield return CURRENTWINDOW;
                yield return CURSOR;
                yield return CODE_BLOCK;
                yield return DATABASE;
                yield return DATABIND;
                yield return DATARELATION;
                yield return DATASERVERS;
                yield return DATASET;
                yield return DATASETHANDLE;
                yield return DATASOURCE;
                yield return DATASOURCEMODIFIED;
                yield return DATASOURCEROWID;
                yield return DATE;
                yield return DATETIME;
                yield return DATETIMETZ;
                yield return DAY;
                yield return DBCODEPAGE;
                yield return DBCOLLATION;
                yield return DBIMS;
                yield return DBNAME;
                yield return DBPARAM;
                yield return DBREMOTEHOST;
                yield return DBRESTRICTIONS;
                yield return DBTASKID;
                yield return DBTYPE;
                yield return DBVERSION;
                yield return DCOLOR;
                yield return DDE;
                yield return DEBLANK;
                yield return DEBUG;
                yield return DEBUGGER;
                yield return DEBUGLIST;
                yield return DECIMAL;
                yield return DECIMALS;
                yield return DECLARE;
                yield return DECRYPT;
                yield return DEFAULT;
                yield return DEFAULTBUTTON;
                yield return DEFAULTEXTENSION;
                yield return DEFAULTNOXLATE;
                yield return DEFAULTVALUE;
                yield return DEFAULTWINDOW;
                yield return DEFERLOBFETCH;
                yield return DEFINE;
                yield return DEFINED;
                yield return DEFINETEXT;
                yield return DELEGATE;
                yield return DELETECHARACTER;
                yield return DELETERESULTLISTENTRY;
                yield return DELETE;
                yield return DELIMITER;
                yield return DESCENDING;
                yield return DESELECTION;
                yield return DESTRUCTOR;
                yield return DIALOGBOX;
                yield return DIALOGHELP;
                yield return DICTIONARY;
                yield return DIGITS;
                yield return DIGITSTART;
                yield return DIR;
                yield return DISABLE;
                yield return DISABLEAUTOZAP;
                yield return DISABLED;
                yield return DISCONNECT;
                yield return DISPLAY;
                yield return DISTINCT;
                yield return DIVIDE;
                yield return DO;
                yield return DOS;
                yield return DOT_COMMENT;
                yield return DOUBLE;
                yield return DOUBLECOLON;
                yield return DOUBLEQUOTE;
                yield return DOWN;
                yield return DQSTRING;
                yield return DROP;
                yield return DROPDOWN;
                yield return DROPDOWNLIST;
                yield return DROPFILENOTIFY;
                yield return DROPTARGET;
                yield return DUMP;
                yield return DYNAMIC;
                yield return DYNAMICCAST;
                yield return DYNAMICCURRENTVALUE;
                yield return DYNAMICFUNCTION;
                yield return DYNAMICINVOKE;
                yield return DYNAMICNEW;
                yield return DYNAMICNEXTVALUE;
                yield return DYNAMICPROPERTY;
                yield return EACH;
                yield return ECHO;
                yield return EDGECHARS;
                yield return EDGEPIXELS;
                yield return EDITING;
                yield return EDITOR;
                yield return EDITUNDO;
                yield return ELSE;
                yield return EMPTY;
                yield return ENABLE;
                yield return ENABLEDFIELDS;
                yield return ENCODE;
                yield return ENCRYPT;
                yield return ENCRYPTIONSALT;
                yield return END;
                yield return ENDKEY;
                yield return ENDMOVE;
                yield return ENDRESIZE;
                yield return ENDROWRESIZE;
                yield return ENTERED;
                yield return ENTRY;
                yield return ENUM;
                yield return EQ;
                yield return EQUAL;
                yield return ERROR;
                yield return ERRORCODE;
                yield return ERRORSTACKTRACE;
                yield return ERRORSTATUS;
                yield return ESCAPE;
                yield return ESCAPED_QUOTE;
                yield return ETIME;
                yield return EVENT;
                yield return EVENTPROCEDURE;
                yield return EVENTS;
                yield return EXCEPT;
                yield return EXCLAMATION;
                yield return EXCLUSIVEID;
                yield return EXCLUSIVELOCK;
                yield return EXCLUSIVEWEBUSER;
                yield return EXECUTE;
                yield return EXISTS;
                yield return EXP;
                yield return EXPAND;
                yield return EXPANDABLE;
                yield return EXPLICIT;
                yield return EXPORT;
                yield return EXTENDED;
                yield return EXTENT;
                yield return EXTERNAL;
                yield return EDITING_PHRASE;
                yield return ENTERED_FUNC;
                yield return EVENT_LIST;
                yield return EXPR_STATEMENT;
                yield return FALSELEAKS;
                yield return FALSE;
                yield return FETCH;
                yield return FGCOLOR;
                yield return FIELD;
                yield return FIELDS;
                yield return FILE;
                yield return FILEINFORMATION;
                yield return FILENAME;
                yield return FILL;
                yield return FILLWHERESTRING;
                yield return FILLIN;
                yield return FILTERS;
                yield return FINAL;
                yield return FINALLY;
                yield return FIND;
                yield return FINDCASESENSITIVE;
                yield return FINDER;
                yield return FINDGLOBAL;
                yield return FINDNEXTOCCURRENCE;
                yield return FINDPREVOCCURRENCE;
                yield return FINDSELECT;
                yield return FINDWRAPAROUND;
                yield return FIRST;
                yield return FIRSTFORM;
                yield return FIRSTOF;
                yield return FITLASTCOLUMN;
                yield return FIXCHAR;
                yield return FIXCODEPAGE;
                yield return FIXEDONLY;
                yield return FLAGS;
                yield return FLATBUTTON;
                yield return FLOAT;
                yield return FOCUS;
                yield return FONT;
                yield return FONTBASEDLAYOUT;
                yield return FONTTABLE;
                yield return FOR;
                yield return FORCEFILE;
                yield return FOREIGNKEYHIDDEN;
                yield return FORMAT;
                yield return FORMINPUT;
                yield return FORMLONGINPUT;
                yield return FORWARDS;
                yield return FRAME;
                yield return FRAMECOL;
                yield return FRAMEDB;
                yield return FRAMEDOWN;
                yield return FRAMEFIELD;
                yield return FRAMEFILE;
                yield return FRAMEINDEX;
                yield return FRAMELINE;
                yield return FRAMENAME;
                yield return FRAMEROW;
                yield return FRAMEVALUE;
                yield return FREECHAR;
                yield return FREQUENCY;
                yield return FROM;
                yield return FROMCURRENT;
                yield return FUNCTION;
                yield return FUNCTIONCALLTYPE;
                yield return FIELD_LIST;
                yield return FIELD_REF;
                yield return FORM_ITEM;
                yield return FORMAT_PHRASE;
                yield return GE;
                yield return GENERATEMD5;
                yield return GENERATEPBEKEY;
                yield return GENERATEPBESALT;
                yield return GENERATERANDOMKEY;
                yield return GENERATEUUID;
                yield return GET;
                yield return GETATTRCALLTYPE;
                yield return GETBITS;
                yield return GETBUFFERHANDLE;
                yield return GETBYTE;
                yield return GETBYTEORDER;
                yield return GETBYTES;
                yield return GETCGILIST;
                yield return GETCGILONGVALUE;
                yield return GETCGIVALUE;
                yield return GETCLASS;
                yield return GETCODEPAGE;
                yield return GETCODEPAGES;
                yield return GETCOLLATIONS;
                yield return GETCONFIGVALUE;
                yield return GETDBCLIENT;
                yield return GETDIR;
                yield return GETDOUBLE;
                yield return GETEFFECTIVETENANTID;
                yield return GETEFFECTIVETENANTNAME;
                yield return GETFILE;
                yield return GETFLOAT;
                yield return GETINT64;
                yield return GETKEYVALUE;
                yield return GETLICENSE;
                yield return GETLONG;
                yield return GETPOINTERVALUE;
                yield return GETSHORT;
                yield return GETSIZE;
                yield return GETSTRING;
                yield return GETUNSIGNEDLONG;
                yield return GETUNSIGNEDSHORT;
                yield return GLOBAL;
                yield return GLOBAL_DEFINE;
                yield return GOON;
                yield return GOPENDING;
                yield return GRANT;
                yield return GRAPHICEDGE;
                yield return GROUP;
                yield return GROUPBOX;
                yield return GTHAN;
                yield return GTOREQUAL;
                yield return GTORLT;
                yield return GUID;
                yield return HANDLE;
                yield return HAVING;
                yield return HEADER;
                yield return HEIGHT;
                yield return HEIGHTCHARS;
                yield return HEIGHTPIXELS;
                yield return HELP;
                yield return HELPTOPIC;
                yield return HEXDECODE;
                yield return HEXENCODE;
                yield return HIDDEN;
                yield return HIDE;
                yield return HINT;
                yield return HORIZONTAL;
                yield return HOSTBYTEORDER;
                yield return HTMLENDOFLINE;
                yield return HTMLFRAMEBEGIN;
                yield return HTMLFRAMEEND;
                yield return HTMLHEADERBEGIN;
                yield return HTMLHEADEREND;
                yield return HTMLTITLEBEGIN;
                yield return HTMLTITLEEND;
                yield return ID;
                yield return ID_THREE;
                yield return ID_TWO;
                yield return IF;
                yield return IFCOND;
                yield return IMAGE;
                yield return IMAGEDOWN;
                yield return IMAGEINSENSITIVE;
                yield return IMAGESIZE;
                yield return IMAGESIZECHARS;
                yield return IMAGESIZEPIXELS;
                yield return IMAGEUP;
                yield return IMPLEMENTS;
                yield return IMPORT;
                yield return IMPOSSIBLE_TOKEN;
                yield return INCLUDEREFARG;
                yield return INCREMENTEXCLUSIVEID;
                yield return INDEX;
                yield return INDEXEDREPOSITION;
                yield return INDEXHINT;
                yield return INDICATOR;
                yield return INFORMATION;
                yield return INHERITBGCOLOR;
                yield return INHERITFGCOLOR;
                yield return INHERITS;
                yield return INITIAL;
                yield return INITIALDIR;
                yield return INITIALFILTER;
                yield return INITIATE;
                yield return INNER;
                yield return INNERCHARS;
                yield return INNERLINES;
                yield return INPUT;
                yield return INPUTOUTPUT;
                yield return INSERT;
                yield return INT64;
                yield return INTEGER;
                yield return INTERFACE;
                yield return INTERVAL;
                yield return INTO;
                yield return IN;
                yield return IS;
                yield return ISATTRSPACE;
                yield return ISCODEPAGEFIXED;
                yield return ISCOLUMNCODEPAGE;
                yield return ISDBMULTITENANT;
                yield return ISLEADBYTE;
                yield return ISMULTITENANT;
                yield return ISODATE;
                yield return ITEM;
                yield return IUNKNOWN;
                yield return INLINE_DEFINITION;
                yield return JOIN;
                yield return JOINBYSQLDB;
                yield return KBLABEL;
                yield return KEEPMESSAGES;
                yield return KEEPTABORDER;
                yield return KEY;
                yield return KEYCODE;
                yield return KEYFUNCTION;
                yield return KEYLABEL;
                yield return KEYS;
                yield return KEYWORD;
                yield return KEYWORDALL;
                yield return LABEL;
                yield return LABELBGCOLOR;
                yield return LABELDCOLOR;
                yield return LABELFGCOLOR;
                yield return LABELFONT;
                yield return LANDSCAPE;
                yield return LANGUAGES;
                yield return LARGE;
                yield return LARGETOSMALL;
                yield return LAST;
                yield return LASTBATCH;
                yield return LASTEVENT;
                yield return LASTFORM;
                yield return LASTKEY;
                yield return LASTOF;
                yield return LC;
                yield return LDBNAME;
                yield return LE;
                yield return LEAKDETECTION;
                yield return LEAVE;
                yield return LEFT;
                yield return LEFTALIGNED;
                yield return LEFTANGLE;
                yield return LEFTBRACE;
                yield return LEFTCURLY;
                yield return LEFTPAREN;
                yield return LEFTTRIM;
                yield return LENGTH;
                yield return LEXAT;
                yield return LEXCOLON;
                yield return LEXDATE;
                yield return LEXOTHER;
                yield return LIBRARY;
                yield return LIKE;
                yield return LIKESEQUENTIAL;
                yield return LINECOUNTER;
                yield return LISTEVENTS;
                yield return LISTING;
                yield return LISTITEMPAIRS;
                yield return LISTITEMS;
                yield return LISTQUERYATTRS;
                yield return LISTSETATTRS;
                yield return LISTWIDGETS;
                yield return LITTLEENDIAN;
                yield return LOAD;
                yield return LOADPICTURE;
                yield return LOBDIR;
                yield return LOCAL_METHOD_REF;
                yield return LOCKED;
                yield return LOG;
                yield return LOGICAL;
                yield return LOGMANAGER;
                yield return LONG;
                yield return LONGCHAR;
                yield return LOOKAHEAD;
                yield return LOOKUP;
                yield return LTHAN;
                yield return LTOREQUAL;
                yield return LOOSE_END_KEEPER;
                yield return MACHINECLASS;
                yield return MAP;
                yield return MARGINEXTRA;
                yield return MARKNEW;
                yield return MARKROWSTATE;
                yield return MATCHES;
                yield return MAXCHARS;
                yield return MAXIMIZE;
                yield return MAXIMUM;
                yield return MAXIMUMLEVEL;
                yield return MAXROWS;
                yield return MAXSIZE;
                yield return MAXVALUE;
                yield return MD5DIGEST;
                yield return MEMBER;
                yield return MEMPTR;
                yield return MENU;
                yield return MENUBAR;
                yield return MENUITEM;
                yield return MERGEBYFIELD;
                yield return MESSAGE;
                yield return MESSAGEDIGEST;
                yield return MESSAGELINE;
                yield return MESSAGELINES;
                yield return METHOD;
                yield return MINIMUM;
                yield return MINSIZE;
                yield return MINUS;
                yield return MINVALUE;
                yield return MODULO;
                yield return MONTH;
                yield return MOUSE;
                yield return MOUSEPOINTER;
                yield return MPE;
                yield return MTIME;
                yield return MULTIPLE;
                yield return MULTIPLEKEY;
                yield return MULTIPLY;
                yield return MUSTEXIST;
                yield return METHOD_PARAM_LIST;
                yield return METHOD_PARAMETER;
                yield return NAMEDOT;
                yield return NAMESPACEPREFIX;
                yield return NAMESPACEURI;
                yield return NATIVE;
                yield return NE;
                yield return NESTED;
                yield return NEW;
                yield return NEWINSTANCE;
                yield return NEWLINE;
                yield return NEXT;
                yield return NEXTPROMPT;
                yield return NEXTVALUE;
                yield return NO;
                yield return NOAPPLY;
                yield return NOASSIGN;
                yield return NOATTRLIST;
                yield return NOATTRSPACE;
                yield return NOAUTOVALIDATE;
                yield return NOBINDWHERE;
                yield return NOBOX;
                yield return NOCOLUMNSCROLLING;
                yield return NOCONSOLE;
                yield return NOCONVERT;
                yield return NOCONVERT3DCOLORS;
                yield return NOCURRENTVALUE;
                yield return NODEBUG;
                yield return NODRAG;
                yield return NOECHO;
                yield return NOEMPTYSPACE;
                yield return NOERROR;
                yield return NOFILL;
                yield return NOFOCUS;
                yield return NOHELP;
                yield return NOHIDE;
                yield return NOINDEXHINT;
                yield return NOINHERITBGCOLOR;
                yield return NOINHERITFGCOLOR;
                yield return NOJOINBYSQLDB;
                yield return NOLABELS;
                yield return NOLOBS;
                yield return NOLOCK;
                yield return NOLOOKAHEAD;
                yield return NOMAP;
                yield return NOMESSAGE;
                yield return NONE;
                yield return NONSERIALIZABLE;
                yield return NOPAUSE;
                yield return NOPREFETCH;
                yield return NORETURNVALUE;
                yield return NORMAL;
                yield return NORMALIZE;
                yield return NOROWMARKERS;
                yield return NOSCROLLBARVERTICAL;
                yield return NOSEPARATECONNECTION;
                yield return NOSEPARATORS;
                yield return NOT;
                yield return NOTACTIVE;
                yield return NOTABSTOP;
                yield return NOUNDERLINE;
                yield return NOUNDO;
                yield return NOVALIDATE;
                yield return NOW;
                yield return NOWAIT;
                yield return NOWORDWRAP;
                yield return NULL;
                yield return NUMALIASES;
                yield return NUMBER;
                yield return NUMCOPIES;
                yield return NUMDBS;
                yield return NUMENTRIES;
                yield return NUMERIC;
                yield return NUMRESULTS;
                yield return NOT_CASESENS;
                yield return NOT_NULL;
                yield return OBJCOLON;
                yield return OBJECT;
                yield return OCTETLENGTH;
                yield return OF;
                yield return OFF;
                yield return OK;
                yield return OKCANCEL;
                yield return OLD;
                yield return ON;
                yield return ONLY;
                yield return OPEN;
                yield return OPSYS;
                yield return OPTION;
                yield return OPTIONS;
                yield return OPTIONSFILE;
                yield return OR;
                yield return ORDER;
                yield return ORDEREDJOIN;
                yield return ORDINAL;
                yield return OS2;
                yield return OS400;
                yield return OSAPPEND;
                yield return OSCOMMAND;
                yield return OSCOPY;
                yield return OSCREATEDIR;
                yield return OSDELETE;
                yield return OSDIR;
                yield return OSDRIVES;
                yield return OSERROR;
                yield return OSGETENV;
                yield return OSRENAME;
                yield return OTHERWISE;
                yield return OUTER;
                yield return OUTERJOIN;
                yield return OUTPUT;
                yield return OVERLAY;
                yield return OVERRIDE;
                yield return PAGE;
                yield return PAGEBOTTOM;
                yield return PAGED;
                yield return PAGENUMBER;
                yield return PAGESIZE;
                yield return PAGETOP;
                yield return PAGEWIDTH;
                yield return PARAMETER;
                yield return PARENT;
                yield return PARENTFIELDSAFTER;
                yield return PARENTFIELDSBEFORE;
                yield return PARENTIDFIELD;
                yield return PARENTIDRELATION;
                yield return PARTIALKEY;
                yield return PASCAL;
                yield return PASSWORDFIELD;
                yield return PAUSE;
                yield return PBEHASHALGORITHM;
                yield return PBEKEYROUNDS;
                yield return PDBNAME;
                yield return PERFORMANCE;
                yield return PERIOD;
                yield return PERIODSTART;
                yield return PERSISTENT;
                yield return PFCOLOR;
                yield return PINNABLE;
                yield return PIPE;
                yield return PLUS;
                yield return PLUSMINUSSTART;
                yield return PORTRAIT;
                yield return POSITION;
                yield return PRECISION;
                yield return PREFERDATASET;
                yield return PREPROCESS;
                yield return PREPROCESSDIRECTIVE;
                yield return PREPROCESSELSE;
                yield return PREPROCESSELSEIF;
                yield return PREPROCESSENDIF;
                yield return PREPROCESSIF;
                yield return PREPROCESSJMESSAGE;
                yield return PREPROCESSMESSAGE;
                yield return PREPROCESSTOKEN;
                yield return PREPROCESSUNDEFINE;
                yield return PRESELECT;
                yield return PREV;
                yield return PRIMARY;
                yield return PRINTER;
                yield return PRINTERSETUP;
                yield return PRIVATE;
                yield return PRIVILEGES;
                yield return PROCEDURE;
                yield return PROCEDURECALLTYPE;
                yield return PROCESS;
                yield return PROCESSARCHITECTURE;
                yield return PROCHANDLE;
                yield return PROCSTATUS;
                yield return PROCTEXT;
                yield return PROCTEXTBUFFER;
                yield return PROFILER;
                yield return PROGRAMNAME;
                yield return PROGRESS;
                yield return PROMPT;
                yield return PROMPTFOR;
                yield return PROMSGS;
                yield return PROPARSEDIRECTIVE;
                yield return PROPATH;
                yield return PROPERTY;
                yield return PROPERTY_GETTER;
                yield return PROPERTY_SETTER;
                yield return PROTECTED;
                yield return PROVERSION;
                yield return PUBLIC;
                yield return PUBLISH;
                yield return PUT;
                yield return PUTBITS;
                yield return PUTBYTE;
                yield return PUTBYTES;
                yield return PUTDOUBLE;
                yield return PUTFLOAT;
                yield return PUTINT64;
                yield return PUTKEYVALUE;
                yield return PUTLONG;
                yield return PUTSHORT;
                yield return PUTSTRING;
                yield return PUTUNSIGNEDLONG;
                yield return PUTUNSIGNEDSHORT;
                yield return PARAMETER_LIST;
                yield return PROGRAM_ROOT;
                yield return PROGRAM_TAIL;
                yield return QSTRING;
                yield return QUERY;
                yield return QUERYCLOSE;
                yield return QUERYOFFEND;
                yield return QUERYPREPARE;
                yield return QUERYTUNING;
                yield return QUESTION;
                yield return QUIT;
                yield return QUOTER;
                yield return RADIOBUTTONS;
                yield return RADIOSET;
                yield return RANDOM;
                yield return RAW;
                yield return RAWTRANSFER;
                yield return RCODEINFORMATION;
                yield return READ;
                yield return READAVAILABLE;
                yield return READEXACTNUM;
                yield return READKEY;
                yield return READONLY;
                yield return REAL;
                yield return RECID;
                yield return RECORDLENGTH;
                yield return RECORD_NAME;
                yield return RECTANGLE;
                yield return RECURSIVE;
                yield return REFERENCEONLY;
                yield return REJECTED;
                yield return RELATIONFIELDS;
                yield return RELEASE;
                yield return REPEAT;
                yield return REPLACE;
                yield return REPLICATIONCREATE;
                yield return REPLICATIONDELETE;
                yield return REPLICATIONWRITE;
                yield return REPOSITION;
                yield return REPOSITIONBACKWARD;
                yield return REPOSITIONFORWARD;
                yield return REPOSITIONMODE;
                yield return REPOSITIONTOROW;
                yield return REPOSITIONTOROWID;
                yield return REQUEST;
                yield return RESTARTROW;
                yield return RESULT;
                yield return RETAIN;
                yield return RETAINSHAPE;
                yield return RETRY;
                yield return RETRYCANCEL;
                yield return RETURN;
                yield return RETURNS;
                yield return RETURNTOSTARTDIR;
                yield return RETURNVALUE;
                yield return REVERSEFROM;
                yield return REVERT;
                yield return REVOKE;
                yield return RGBVALUE;
                yield return RIGHT;
                yield return RIGHTALIGNED;
                yield return RIGHTANGLE;
                yield return RIGHTBRACE;
                yield return RIGHTCURLY;
                yield return RIGHTPAREN;
                yield return RIGHTTRIM;
                yield return RINDEX;
                yield return ROUND;
                yield return ROUNDED;
                yield return ROUTINELEVEL;
                yield return ROW;
                yield return ROWCREATED;
                yield return ROWDELETED;
                yield return ROWHEIGHTCHARS;
                yield return ROWHEIGHTPIXELS;
                yield return ROWID;
                yield return ROWMODIFIED;
                yield return ROWOF;
                yield return ROWSTATE;
                yield return ROWUNMODIFIED;
                yield return RULE;
                yield return RUN;
                yield return RUNPROCEDURE;
                yield return SAVE;
                yield return SAVEAS;
                yield return SAVECACHE;
                yield return SAVEWHERESTRING;
                yield return SAXATTRIBUTES;
                yield return SAXCOMPLETE;
                yield return SAXPARSERERROR;
                yield return SAXREADER;
                yield return SAXRUNNING;
                yield return SAXUNINITIALIZED;
                yield return SAXWRITER;
                yield return SAXWRITEBEGIN;
                yield return SAXWRITECOMPLETE;
                yield return SAXWRITECONTENT;
                yield return SAXWRITEELEMENT;
                yield return SAXWRITEERROR;
                yield return SAXWRITEIDLE;
                yield return SAXWRITETAG;
                yield return SCHEMA;
                yield return SCOPEDDEFINE;
                yield return SCREEN;
                yield return SCREENIO;
                yield return SCREENLINES;
                yield return SCREENVALUE;
                yield return SCROLL;
                yield return SCROLLABLE;
                yield return SCROLLBARHORIZONTAL;
                yield return SCROLLBARVERTICAL;
                yield return SCROLLING;
                yield return SDBNAME;
                yield return SEARCH;
                yield return SEARCHSELF;
                yield return SEARCHTARGET;
                yield return SECTION;
                yield return SECURITYPOLICY;
                yield return SEEK;
                yield return SELECT;
                yield return SELECTION;
                yield return SELECTIONLIST;
                yield return SELF;
                yield return SEMI;
                yield return SEND;
                yield return SENDSQLSTATEMENT;
                yield return SENSITIVE;
                yield return SEPARATECONNECTION;
                yield return SEPARATORS;
                yield return SERIALIZABLE;
                yield return SERIALIZEHIDDEN;
                yield return SERIALIZENAME;
                yield return SERVER;
                yield return SERVERSOCKET;
                yield return SESSION;
                yield return SET;
                yield return SETATTRCALLTYPE;
                yield return SETBYTEORDER;
                yield return SETCONTENTS;
                yield return SETCURRENTVALUE;
                yield return SETDBCLIENT;
                yield return SETEFFECTIVETENANT;
                yield return SETPOINTERVALUE;
                yield return SETSIZE;
                yield return SETUSERID;
                yield return SHA1DIGEST;
                yield return SHARED;
                yield return SHARELOCK;
                yield return SHORT;
                yield return SHOWSTATS;
                yield return SIDELABELS;
                yield return SIGNATURE;
                yield return SILENT;
                yield return SIMPLE;
                yield return SINGLE;
                yield return SINGLERUN;
                yield return SINGLETON;
                yield return SINGLEQUOTE;
                yield return SIZE;
                yield return SIZECHARS;
                yield return SIZEPIXELS;
                yield return SKIP;
                yield return SKIPDELETEDRECORD;
                yield return SKIPGROUPDUPLICATES;
                yield return SLASH;
                yield return SLIDER;
                yield return SMALLINT;
                yield return SOAPHEADER;
                yield return SOAPHEADERENTRYREF;
                yield return SOCKET;
                yield return SOME;
                yield return SORT;
                yield return SOURCE;
                yield return SOURCEPROCEDURE;
                yield return SPACE;
                yield return SQL;
                yield return SQRT;
                yield return SQSTRING;
                yield return SSLSERVERNAME;
                yield return STAR;
                yield return START;
                yield return STARTING;
                yield return STARTMOVE;
                yield return STARTRESIZE;
                yield return STARTROWRESIZE;
                yield return STATIC;
                yield return STATUS;
                yield return STATUSBAR;
                yield return STDCALL;
                yield return STOMPDETECTION;
                yield return STOMPFREQUENCY;
                yield return STOP;
                yield return STOPAFTER;
                yield return STOREDPROCEDURE;
                yield return STREAM;
                yield return STREAMHANDLE;
                yield return STREAMIO;
                yield return STRETCHTOFIT;
                yield return STRING;
                yield return STRINGXREF;
                yield return SUBAVERAGE;
                yield return SUBCOUNT;
                yield return SUBMAXIMUM;
                yield return SUBMENU;
                yield return SUBMENUHELP;
                yield return SUBMINIMUM;
                yield return SUBSCRIBE;
                yield return SUBSTITUTE;
                yield return SUBSTRING;
                yield return SUBTOTAL;
                yield return SUM;
                yield return SUMMARY;
                yield return SUPER;
                yield return SYMMETRICENCRYPTIONALGORITHM;
                yield return SYMMETRICENCRYPTIONIV;
                yield return SYMMETRICENCRYPTIONKEY;
                yield return SYMMETRICSUPPORT;
                yield return SYSTEMDIALOG;
                yield return SYSTEMHELP;
                yield return SCANNER_HEAD;
                yield return SCANNER_TAIL;
                yield return SQL_BEGINS;
                yield return SQL_BETWEEN;
                yield return SQL_COMP_QUERY;
                yield return SQL_IN;
                yield return SQL_LIKE;
                yield return SQL_NULL_TEST;
                yield return SQL_SELECT_WHAT;
                yield return TABLE;
                yield return TABLEHANDLE;
                yield return TABLENUMBER;
                yield return TABLESCAN;
                yield return TARGET;
                yield return TARGETPROCEDURE;
                yield return TEMPTABLE;
                yield return TENANT;
                yield return TENANTID;
                yield return TENANTNAME;
                yield return TENANTNAMETOID;
                yield return TENANTWHERE;
                yield return TERMINAL;
                yield return TERMINATE;
                yield return TEXT;
                yield return TEXTCURSOR;
                yield return TEXTSEGGROW;
                yield return THEN;
                yield return THISOBJECT;
                yield return THISPROCEDURE;
                yield return THREED;
                yield return THROUGH;
                yield return THROW;
                yield return TICMARKS;
                yield return TILDE;
                yield return TIME;
                yield return TIMESTAMP;
                yield return TIMEZONE;
                yield return TITLE;
                yield return TO;
                yield return TODAY;
                yield return TOGGLEBOX;
                yield return TOOLBAR;
                yield return TOOLTIP;
                yield return TOP;
                yield return TOPIC;
                yield return TOPNAVQUERY;
                yield return TOPONLY;
                yield return TOROWID;
                yield return TOTAL;
                yield return TRAILING;
                yield return TRANSACTION;
                yield return TRANSACTIONMODE;
                yield return TRANSINITPROCEDURE;
                yield return TRANSPARENT;
                yield return TRIGGER;
                yield return TRIGGERS;
                yield return TRIM;
                yield return TRUE;
                yield return TRUNCATE;
                yield return TTCODEPAGE;
                yield return TYPE_NAME;
                yield return TYPELESS_TOKEN;
                yield return TYPEOF;
                yield return UNARY_MINUS;
                yield return UNARY_PLUS;
                yield return UNBOX;
                yield return UNBUFFERED;
                yield return UNDERLINE;
                yield return UNDO;
                yield return UNFORMATTED;
                yield return UNION;
                yield return UNIQUE;
                yield return UNIQUEMATCH;
                yield return UNIX;
                yield return UNKNOWNVALUE;
                yield return UNLESSHIDDEN;
                yield return UNLOAD;
                yield return UNQUOTEDSTRING;
                yield return UNSIGNEDBYTE;
                yield return UNSIGNEDSHORT;
                yield return UNSUBSCRIBE;
                yield return UP;
                yield return UPDATE;
                yield return URLDECODE;
                yield return URLENCODE;
                yield return USE;
                yield return USEDICTEXPS;
                yield return USEFILENAME;
                yield return USEINDEX;
                yield return USER;
                yield return USEREVVIDEO;
                yield return USERID;
                yield return USER_FUNC;
                yield return USETEXT;
                yield return USEUNDERLINE;
                yield return USEWIDGETPOOL;
                yield return USING;
                yield return V6FRAME;
                yield return VALIDATE;
                yield return VALIDEVENT;
                yield return VALIDHANDLE;
                yield return VALIDOBJECT;
                yield return VALUE;
                yield return VALUECHANGED;
                yield return VALUES;
                yield return VARIABLE;
                yield return VERBOSE;
                yield return VERTICAL;
                yield return VIEW;
                yield return VIEWAS;
                yield return VISIBLE;
                yield return VMS;
                yield return VOID;
                yield return WAIT;
                yield return WAITFOR;
                yield return WARNING;
                yield return WEBCONTEXT;
                yield return WEEKDAY;
                yield return WHEN;
                yield return WHERE;
                yield return WHILE;
                yield return WIDGET;
                yield return WIDGETHANDLE;
                yield return WIDGETID;
                yield return WIDGETPOOL;
                yield return WIDTH;
                yield return WIDTHCHARS;
                yield return WIDTHPIXELS;
                yield return WINDOW;
                yield return WINDOWDELAYEDMINIMIZE;
                yield return WINDOWMAXIMIZED;
                yield return WINDOWMINIMIZED;
                yield return WINDOWNAME;
                yield return WINDOWNORMAL;
                yield return WITH;
                yield return WORDINDEX;
                yield return WORKTABLE;
                yield return WRITE;
                yield return WS;
                yield return WIDGET_REF;
                yield return WITH_COLUMNS;
                yield return WITH_DOWN;
                yield return X;
                yield return XCODE;
                yield return XDOCUMENT;
                yield return XMLDATATYPE;
                yield return XMLNODENAME;
                yield return XMLNODETYPE;
                yield return XNODEREF;
                yield return XOF;
                yield return XOR;
                yield return XREF;
                yield return XREFXML;
                yield return Y;
                yield return YEAR;
                yield return YES;
                yield return YESNO;
                yield return YESNOCANCEL;
                yield return YOF;
            }
        }

        // Private attributes
        public int Type { get; private set; }
        public string Text { get; private set; }
        private readonly NodeTypesOption options;

        // Keywords can have up to two alternate syntax
        public string Alt1 { get; private set; }
        public string Alt2 { get; private set; }

        // And can be abbreviated too
        private readonly int abbrMain;
        private readonly int abbrAlt1;
        private readonly int abbrAlt2;

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type)
        private ABLNodeType(int type) : this(type, "")
        {
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text)
        private ABLNodeType(int type, string text) : this(type, text, text.Length)
        {
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text, int minAbbrev)
        private ABLNodeType(int type, string text, int minAbbrev)
        {
            this.Type = type;
            this.Text = text;
            this.abbrMain = minAbbrev;
            this.options = NodeTypesOption.NONE;
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, NodeTypesOption opt, params NodeTypesOption[] options) : this(type, "")
        {
            this.options = opt;
            foreach(NodeTypesOption supOpt in options)
            {
                this.options |= supOpt;
            }
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, string text, NodeTypesOption opt, params NodeTypesOption[] options)
        {
            this.Type = type;
            this.Text = text;
            this.abbrMain = text.Length;
            this.options = opt;
            foreach (NodeTypesOption supOpt in options)
            {
                this.options |= supOpt;
            }
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text, int minabbr, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, string text, int minabbr, NodeTypesOption opt, params NodeTypesOption[] options)
        {
            this.Type = type;
            this.Text = text;
            this.abbrMain = minabbr;
            this.options = opt;
            foreach (NodeTypesOption supOpt in options)
            {
                this.options |= supOpt;
            }
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text, String extraLiteral, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, string text, string extraLiteral, NodeTypesOption opt, params NodeTypesOption[] options) : this(type, text, text.Length, extraLiteral, opt, options)
        {
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text, int minabbr, String alt1, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, string text, int minabbr, string alt1, NodeTypesOption opt, params NodeTypesOption[] options) : this(type, text, minabbr, opt, options)
        {
            this.Alt1 = alt1;
            this.abbrAlt1 = alt1.Length;
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String text, int minAbbr, String alt1, int minAbbr1, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, string text, int minAbbr, string alt1, int minAbbr1, NodeTypesOption opt, params NodeTypesOption[] options) : this(type, text, minAbbr, opt, options)
        {
            this.Alt1 = alt1;
            this.abbrAlt1 = minAbbr1;
        }

        //JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
        //ORIGINAL LINE: private ABLNodeType(int type, String fullText, int minAbbr, String alt1, String alt2, NodeTypesOption opt, NodeTypesOption... options)
        private ABLNodeType(int type, string fullText, int minAbbr, string alt1, string alt2, NodeTypesOption opt, params NodeTypesOption[] options) : this(type, fullText, minAbbr, alt1, opt, options)
        {
            this.Alt2 = alt2;
            this.abbrAlt2 = alt2.Length;
        }

        /// <returns> True if node type is a keyword </returns>
        public virtual bool IsKeyword()
        {            
            return options.HasFlag(NodeTypesOption.KEYWORD);            
        }

        public virtual bool IsPreprocessor()
        {            
            return options.HasFlag(NodeTypesOption.PREPROCESSOR);            
        }

        /// <returns> True if node type is a keyword but can't be used as a variable name or field name among other things </returns>
        public virtual bool IsReservedKeyword()
        {            
            return options.HasFlag(NodeTypesOption.KEYWORD) && options.HasFlag(NodeTypesOption.RESERVED);            
        }

        /// <returns> True if node type is a keyword and can be used as a variable name or field name among other things </returns>
        public virtual bool IsUnreservedKeywordType()
        {            
            return options.HasFlag(NodeTypesOption.KEYWORD) && !options.HasFlag(NodeTypesOption.RESERVED);            
        }

        public virtual bool IsSystemHandleName()
        {            
            return options.HasFlag(NodeTypesOption.SYSHDL);            
        }

        public virtual bool MayBeNoArgFunc()
        {
            return options.HasFlag(NodeTypesOption.MAY_BE_NO_ARG_FUNC);
        }

        public virtual bool MayBeRegularFunc()
        {
            return options.HasFlag(NodeTypesOption.MAY_BE_REGULAR_FUNC);
        }

        public virtual bool IsAbbreviated(string txt)
        {
            if (String.IsNullOrEmpty(txt) || !IsKeyword())
            {
                return false;
            }
            string lowText = txt.ToLower();
            if (Text.StartsWith(lowText))
            {
                return Text.Length > lowText.Length;
            }
            else if ((Alt1 != null) && Alt1.StartsWith(lowText))
            {
                return Alt1.Length > lowText.Length;
            }
            else if ((Alt2 != null) && Alt2.StartsWith(lowText))
            {
                return Alt2.Length > lowText.Length;
            }
            return false;
        }

        static ABLNodeType()
        {
            foreach (ABLNodeType e in ABLNodeType.Values)
            {
                // No duplicates allowed in definition
                try
                {
                    typeMap.Add(e.Type, e);
                }
                catch (ArgumentException ae)
                {
                    throw new System.InvalidOperationException(ERR_INIT + e.Type, ae);
                }

                if (e.options.HasFlag(NodeTypesOption.KEYWORD))
                {
                    // Full-text map is only filled with keywords
                    for (int zz = e.abbrMain; zz <= e.Text.Length; zz++)
                    {
                        try
                        {
                            literalsMap.Add(e.Text.Substring(0, zz).ToLower(), e);
                        }
                        catch (ArgumentException ae)
                        {
                            throw new System.InvalidOperationException(ERR_INIT + e.Text.Substring(0, zz), ae);
                        }
                    }
                }
                if (e.Alt1 != null)
                {                    
                    for (int zz = e.abbrAlt1; zz <= e.Alt1.Length; zz++)
                    {
                        try
                        {
                            literalsMap.Add(e.Alt1.Substring(0, zz).ToLower(), e);
                        }
                        catch (ArgumentException ae)
                        {
                            throw new System.InvalidOperationException(ERR_INIT + e.Alt1.Substring(0, zz), ae);
                        }                        
                    }
                }
                if (e.Alt2 != null)
                {
                    for (int zz = e.abbrAlt2; zz <= e.Alt2.Length; zz++)
                    {
                        try
                        {
                            literalsMap.Add(e.Alt2.Substring(0, zz).ToLower(), e);
                        }
                        catch (ArgumentException ae)
                        {
                            throw new System.InvalidOperationException(ERR_INIT + e.Alt2.Substring(0, zz), ae);
                        }
                    }
                }                
            }
        }

        public static ABLNodeType GetNodeType(int type)
        {
            ABLNodeType nodeType = typeMap[type];
            return nodeType ?? INVALID_NODE;
        }

        internal static bool IsValidType(int type)
        {
            return typeMap.ContainsKey(type);
        }

        /// <summary>
        /// Returns uppercase of the type info record's full text. Returns null if there's no type info for the type number.
        /// Returns empty string if there's no text for the type.
        /// </summary>
        public static string GetFullText(int type)
        {
            ABLNodeType e = typeMap[type];
            if (e == null)
            {
                return null;
            }
            if (e.options.HasFlag(NodeTypesOption.PLACEHOLDER))
            {
                return null;
            }
            if (!e.options.HasFlag(NodeTypesOption.KEYWORD))
            {
                return "";
            }
            return (e.Text ?? "").ToUpper();
        }

        public static ABLNodeType GetLiteral(string text)
        {
            return GetLiteral(text, null);
        }

        public static ABLNodeType GetLiteral(string text, ABLNodeType defaultType)
        {
            if (text == null)
            {
                return defaultType;
            }
            if (literalsMap.ContainsKey(text.ToLower()))
                return literalsMap[text.ToLower()];
            else
                return defaultType;            
        }

        public static string GetFullText(string text)
        {
            if (text == null)
            {
                return "";
            }
            ABLNodeType type = literalsMap[text.ToLower()];
            if (type == null)
            {
                return "";
            }
            return type.Text.ToUpper();
        }

        /// <summary>
        /// Get the type number for a type name. For those type names that have it, the "_KW" suffix is optional.
        /// </summary>
        /// <param name="s"> type name </param>
        /// <returns> -1 if invalid type name is entered. </returns>
        public static int GetTypeNum(string s)
        {
            if (s == null)
            {
                return -1;
            }
            if (s.StartsWith("_", StringComparison.Ordinal))
            {
                return -1;
            }
            ABLNodeType ret = literalsMap[s.ToLower()];
            if (ret == null)
            {
                // It's possible that we've been passed a token type name which needs
                // to have the _KW suffix added to it.
                ret = literalsMap[s.ToLower() + "_KW"];
            }
            if (ret == null)
            {
                return -1;
            }
            return ret.Type;
        }

        public static bool IsKeywordType(int nodeType)
        {
            ABLNodeType type = typeMap[nodeType];
            if (type == null)
            {
                return false;
            }
            return type.IsKeyword();
        }

        /// <returns> True if node type can't be used as a variable name or field name among other things </returns>
        public static bool IsReserved(int nodeType)
        {
            ABLNodeType type = typeMap[nodeType];
            if (type == null)
            {
                return false;
            }
            return type.IsReservedKeyword();
        }

        internal static bool IsUnreservedKeywordType(int nodeType)
        {
            ABLNodeType type = typeMap[nodeType];
            if (type == null)
            {
                return false;
            }
            return type.IsUnreservedKeywordType();
        }

        public static bool IsSystemHandleName(int nodeType)
        {
            ABLNodeType type = typeMap[nodeType];
            if (type == null)
            {
                return false;
            }
            return type.IsSystemHandleName();
        }

        internal static bool MayBeNoArgFunc(int nodeType)
        {
            ABLNodeType type = typeMap[nodeType];
            if (type == null)
            {
                return false;
            }
            return type.MayBeNoArgFunc();
        }

        internal static bool MayBeRegularFunc(int nodeType)
        {
            ABLNodeType type = typeMap[nodeType];
            if (type == null)
            {
                return false;
            }
            return type.MayBeRegularFunc();
        }

        public override string ToString()
        {
            string proType;
            if (Type == EMPTY_NODE_TYPE)
                proType = nameof(EMPTY_NODE_TYPE);
            else if (Type == TokenConstants.InvalidType)
                proType = nameof(TokenConstants.InvalidType);
            else if (Type == TokenConstants.EOF)
                proType = nameof(TokenConstants.EOF);
            else
                proType = GetConstNameByValue<Proparse>(Type);
            return new StringBuilder().Append(proType).Append(".").Append(Text).ToString();
        }

        public string GetConstNameByValue<T>(int constValue) =>
           typeof(T)
                // Gets all public and static fields
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                // IsLiteral determines if its value is written at 
                //   compile time and not changeable
                // IsInitOnly determine if the field can be set 
                //   in the body of the constructor
                // for C# a field which is readonly keyword would have both true 
                //   but a const field would have only IsLiteral equal to true
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType.Equals(typeof(int)))
                .FirstOrDefault(f => (int)f.GetValue(null) == constValue)
                ?.Name;
    }
}
