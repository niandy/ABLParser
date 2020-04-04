using ABLParser.Progress.Xref;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.NodeTypes;
using ABLParser.Prorefactor.Macrolevel;
using ABLParser.Prorefactor.Proparser;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using ABLParser.RCodeReader.Elements;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using static ABLParser.Prorefactor.Macrolevel.PreprocessorEventListener;

namespace ABLParser.Prorefactor.Treeparser
{


    /// <summary>
    /// Provides parse unit information, such as the symbol table and a reference to the AST. TreeParser01 calls
    /// symbolUsage() in this class in order to build the symbol table.
    /// </summary>
    public class ParseUnit
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(ParseUnit));

        private readonly RefactorSession session;
        private readonly FileInfo file;
        private readonly Stream input;
        private readonly string relativeName;

        private IntegerIndex<string> fileNameList;
        private IParseTree tree;        

        private ProgramRootNode topNode;
        private IncludeRef macroGraph;
        private bool appBuilderCode;
        private IList<EditableCodeSection> sections;
        private TreeParserRootSymbolScope rootScope;
        private JPNodeMetrics metrics;
        private XmlDocument doc = null;
        private Crossreference xref = null;
        private ITypeInfo typeInfo = null;
        private IList<int> trxBlocks;
        private ParserSupport support;

        public ParseUnit(FileInfo file, RefactorSession session) : this(file, file.FullName, session)
        {
        }

        public ParseUnit(FileInfo file, string relativeName, RefactorSession session)
        {
            this.file = file;
            this.input = null;
            this.relativeName = relativeName;
            this.session = session;
        }

        public ParseUnit(Stream input, string relativeName, RefactorSession session)
        {
            this.file = null;
            this.input = input;
            this.relativeName = relativeName;
            this.session = session;
        }

        public virtual TreeParserRootSymbolScope RootScope
        {
            get => rootScope;
            set => rootScope = value;
        }


        /// <summary>
        /// Get the syntax tree top (Program_root) node </summary>
        public virtual ProgramRootNode TopNode => topNode;

        public virtual JPNodeMetrics Metrics => metrics;

        public virtual string GetIncludeFileName(int index) => fileNameList?.GetValue(index) ?? "";        

        /// <returns> IncludeRef object </returns>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public @Nullable IncludeRef getMacroGraph()
        public virtual IncludeRef MacroGraph => macroGraph;

        /// <summary>
        /// This is just a shortcut for calling getMacroGraph() and MacroLevel.sourceArray(). Build and return an array of the
        /// MacroRef objects, which would map to the SOURCENUM attribute from JPNode. Built simply by walking the tree and
        /// adding every MacroRef to the array.
        /// </summary>
        /// <seealso cref= org.prorefactor.macrolevel.MacroLevel#sourceArray(MacroRef) </seealso>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: public @Nonnull MacroRef[] getMacroSourceArray()
        public virtual MacroRef[] GetMacroSourceArray() => macroGraph == null ? Array.Empty<MacroRef>() : MacroLevel.SourceArray(macroGraph);

        /// <summary>
        /// Returns a TokenSource object for the main file. Include files are not expanded, and preprocessor is not used
        /// </summary>
        /// <exception cref="UncheckedIOException"> If main file can't be opened </exception>
        public virtual ITokenSource Lex()
        {
            return new ProgressLexer(session, ByteSource, relativeName, true);
        }

        public virtual ITokenSource Preprocess()
        {
            ProgressLexer lexer = new ProgressLexer(session, ByteSource, relativeName, false);
            fileNameList = lexer.FilenameList;
            macroGraph = lexer.MacroGraph;
            appBuilderCode = ((PreprocessorEventListener)lexer.LstListener).AppBuilderCode;
            sections = ((PreprocessorEventListener)lexer.LstListener).EditableCodeSections;
            metrics = lexer.Metrics;

            return lexer;
        }

        /// <summary>
        /// Generate metrics for the main file
        /// </summary>
        /// <exception cref="UncheckedIOException"> If main file can't be opened </exception>
        public virtual void LexAndGenerateMetrics()
        {
            LOGGER.Debug("Entering ParseUnit#lexAndGenerateMetrics()");
            ProgressLexer lexer = new ProgressLexer(session, ByteSource, relativeName, true);
            IToken tok = lexer.NextToken();
            while (tok.Type != TokenConstants.EOF)
            {
                tok = lexer.NextToken();
            }
            this.metrics = lexer.Metrics;
            LOGGER.Debug("Exiting ParseUnit#lex()");
        }


        public virtual void Parse()
        {
            LOGGER.Debug("Entering ParseUnit#parse()");

            ProgressLexer lexer = new ProgressLexer(session, ByteSource, relativeName, false);
            Proparse parser = new Proparse(new CommonTokenStream(lexer));
            parser.InitAntlr4(session);
            parser.Interpreter.PredictionMode = PredictionMode.SLL;
            parser.ErrorHandler = new BailErrorStrategy();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new DescriptiveErrorListener());

            try
            {
                tree = parser.program();
            }
            catch (ParseCanceledException)
            {
                parser.ErrorHandler = new ProparseErrorStrategy(session.ProparseSettings.AllowAntlrTokenDeletion(), session.ProparseSettings.AllowAntlrTokenInsertion(), session.ProparseSettings.AllowAntlrRecover());
                parser.Interpreter.PredictionMode = PredictionMode.LL;
                // Another ParseCancellationException can be thrown in recover fails again
                tree = parser.program();
            }
            lexer.ParseComplete();
            topNode = (ProgramRootNode)(new JPNodeVisitor(parser.ParserSupport, (BufferedTokenStream)parser.InputStream)).Visit(tree).Build(parser.ParserSupport);

            fileNameList = lexer.FilenameList;
            macroGraph = lexer.MacroGraph;
            appBuilderCode = ((PreprocessorEventListener)lexer.LstListener).AppBuilderCode;
            sections = ((PreprocessorEventListener)lexer.LstListener).EditableCodeSections;
            metrics = lexer.Metrics;
            support = parser.ParserSupport;

            LOGGER.Debug("Exiting ParseUnit#parse()");
        }

        public virtual void TreeParser01()
        {
            if (topNode == null)
            {
                Parse();
            }
            ParseTreeWalker walker = new ParseTreeWalker();
            TreeParser parser = new TreeParser(support, session);
            walker.Walk(parser, tree);
            rootScope = parser.RootScope;
        }

        public virtual void AttachXref(XmlDocument doc)
        {
            this.doc = doc;
        }

        public virtual void AttachXref(Crossreference xref)
        {
            this.xref = xref;
            if (xref == null)
            {
                return;
            }
            AttachXrefToTreeParser(TopNode, xref);
        }

        public static void AttachXrefToTreeParser(ProgramRootNode root, Crossreference xref)
        {
            IList<JPNode> recordNodes = root.Query(ABLNodeType.RECORD_NAME);
            foreach (CrossreferenceSource src in xref.Source)
            {
                FileInfo srcFile = new FileInfo(src.Filename);
                foreach (CrossreferenceSourceReference @ref in src.Reference)
                {
                    if ("search".Equals(@ref.Referencetype, StringComparison.OrdinalIgnoreCase))
                    {
                        string tableName = @ref.Objectidentifier;
                        bool tempTable = "T".Equals(@ref.Tempref, StringComparison.OrdinalIgnoreCase);
                        int tableType = tempTable ? IConstants.ST_TTABLE : IConstants.ST_DBTABLE;
                        if (tempTable && (tableName.LastIndexOf(':') != -1))
                        {
                            // Temp-table defined in classes are prefixed by the class name
                            tableName = tableName.Substring(tableName.LastIndexOf(':') + 1);
                        }
                        if (!tempTable && (tableName.IndexOf("._", StringComparison.Ordinal) != -1))
                        {
                            // DBName._Metaschema -> skip
                            continue;
                        }

                        bool lFound = false;
                        foreach (RecordNameNode recNode in recordNodes)
                        {                            
                            try
                            {
                                if ((recNode.Statement.FirstNaturalChild.Line == @ref.Linenum)
                                    && (recNode.TableBuffer != null)
                                    && tableName.Equals(recNode.TableBuffer.TargetFullName, StringComparison.OrdinalIgnoreCase)
                                    && (recNode.AttrGet(IConstants.STORETYPE) == tableType)
                                    && ((src.Filenum == 1 && recNode.FileIndex == 0)
                                        || (Path.GetFullPath(srcFile.FullName) == Path.GetFullPath(recNode.Statement.FileName))))
                                {
                                    recNode.WholeIndex = "WHOLE-INDEX".Equals(@ref.Detail);
                                    recNode.SearchIndexName = recNode.TableBuffer.Table.GetName() + "." + @ref.Objectcontext;
                                    lFound = true;
                                    break;
                                }
                            }
                            catch (IOException)
                            {
                                // Nothing
                            }
                        }
                        if (!lFound && "WHOLE-INDEX".Equals(@ref.Detail))
                        {
                            LOGGER.Debug($"WHOLE-INDEX search on '{tableName}' with index '{@ref.Objectcontext}' couldn't be assigned to {srcFile.FullName} at line {@ref.Linenum}");
                        }
                    }
                    else if ("sort-access".Equals(@ref.Referencetype, StringComparison.OrdinalIgnoreCase))
                    {
                        string tableName = @ref.Objectidentifier;
                        bool tempTable = "T".Equals(@ref.Tempref, StringComparison.OrdinalIgnoreCase);
                        int tableType = tempTable ? IConstants.ST_TTABLE : IConstants.ST_DBTABLE;
                        if (tempTable && (tableName.LastIndexOf(':') != -1))
                        {
                            tableName = tableName.Substring(tableName.LastIndexOf(':') + 1);
                        }
                        if (!tempTable && (tableName.IndexOf("._", StringComparison.Ordinal) != -1))
                        {
                            // DBName._Metaschema -> skip
                            continue;
                        }

                        foreach (RecordNameNode recNode in recordNodes)
                        {                            
                            try
                            {
                                if ((recNode.Statement.FirstNaturalChild.Line == @ref.Linenum)
                                    && tableName.Equals(recNode.TableBuffer.TargetFullName, StringComparison.OrdinalIgnoreCase)
                                    && (recNode.AttrGet(IConstants.STORETYPE) == tableType)
                                    && ((src.Filenum == 1 && recNode.FileIndex == 0)
                                        || (Path.GetFullPath(srcFile.FullName) == Path.GetFullPath(recNode.Statement.FileName))))
                                {
                                    recNode.SortAccess = @ref.Objectcontext;
                                    break;
                                }
                            }
                            catch (IOException)
                            {
                                // Nothing
                            }
                        }
                    }
                }
            }
        }


        public virtual void AttachTypeInfo(ITypeInfo unit)
        {
            this.typeInfo = unit;
        }

        public virtual void AttachTransactionBlocks(IList<int> blocks)
        {
            this.trxBlocks = blocks;
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable public Document getXRefDocument()
        public virtual XmlDocument XRefDocument => doc;            

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable public CrossReference getXref()
        public virtual Crossreference Xref => xref;
            
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable public ITypeInfo getTypeInfo()
        public virtual ITypeInfo TypeInfo => typeInfo;            

        public virtual IList<int> TransactionBlocks => trxBlocks;
            
        public virtual IParseTree ParseTree => tree;
            
        public virtual ParserSupport Support => support;
            

        public virtual RefactorSession Session => session;            

        public virtual bool AppBuilderCode => appBuilderCode;            

        public virtual bool IsInEditableSection(int file, int line)
        {
            if (!appBuilderCode || (file > 0))
            {
                return true;
            }
            foreach (EditableCodeSection range in sections)
            {
                if ((range.FileNum == file) && (range.StartLine <= line) && (range.EndLine >= line))
                {
                    return true;
                }
            }
            return false;
        }

        private Stream ByteSource
        {
            get
            {
                try
                {                    
                    return input ?? file.Open(FileMode.Open, FileAccess.Read);
                }
                catch (IOException caught)
                {
                    throw new FileLoadException("Can't open file", caught);
                }
            }
        }

    }

}
