using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Core.NodeTypes;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Refactor;
using ABLParser.Prorefactor.Treeparser;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.Prorefactor.Treeparser.Symbols.Widgets;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using static ABLParser.Prorefactor.Proparser.Antlr.Proparse;

namespace ABLParser.Prorefactor.Proparser.Antlr
{


    public class TreeParser : ProparseBaseListener
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TreeParser));

        private const string LOG_ADDING_QUAL_TO = "{0}> Adding {1} qualifier to '{2}'";

        private readonly ParserSupport support;
        private readonly RefactorSession refSession;
        private readonly TreeParserRootSymbolScope rootScope;

        private int currentLevel;

        private Block currentBlock;
        private TreeParserSymbolScope currentScope;
        private Routine currentRoutine;
        private Routine rootRoutine;
        private JPNode lastStatement;
        /// <summary>
        /// The symbol last, or currently being, defined. Needed when we have complex syntax like DEFINE id ... LIKE, where we
        /// want to track the LIKE but it's not in the same grammar production as the DEFINE.
        /// </summary>
        private ISymbol currSymbol;

        private TableBuffer lastTableReferenced;
        private TableBuffer prevTableReferenced;
        private FrameStack frameStack = new FrameStack();

        private TableBuffer currDefTable;
        private Index currDefIndex;
        // LIKE tables management for index copy
        private bool currDefTableUseIndex = false;
        private TableBuffer currDefTableLike = null;

        private bool formItem2 = false;

        // This tree parser's stack. I think it is best to keep the stack
        // in the tree parser grammar for visibility sake, rather than hide
        // it in the support class. If we move grammar and actions around
        // within this .g, the effect on the stack should be highly visible.
        // Deque implementation has to support null elements
        private LinkedList<Symbol> stack = new LinkedList<Symbol>();
        // Since there can be more than one WIP Call, there can be more than one WIP Parameter
        private LinkedList<Parameter> wipParameters = new LinkedList<Parameter>();

        /*
         * Note that blockStack is *only* valid for determining the current block - the stack itself cannot be used for
         * determining a block's parent, buffer scopes, etc. That logic is found within the Block class. Conversely, we cannot
         * use Block.parent to find the current block when we close out a block. That is because a scope's root block parent
         * is always the program block, but a programmer may code a scope into a non-root block... which we need to make
         * current again once done inside the scope.
         */
        private IList<Block> blockStack = new List<Block>();
        private IDictionary<string, TreeParserSymbolScope> funcForwards = new Dictionary<string, TreeParserSymbolScope>();
        private ParseTreeProperty<ContextQualifier> contextQualifiers = new ParseTreeProperty<ContextQualifier>();
        private ParseTreeProperty<TableNameResolution> nameResolution = new ParseTreeProperty<TableNameResolution>();

        // Temporary work-around
        private bool inDefineEvent = false;

        public TreeParser(ParserSupport support, RefactorSession session)
        {
            this.support = support;
            this.refSession = session;
            this.rootScope = new TreeParserRootSymbolScope(refSession);
            this.currentScope = rootScope;
        }

        public virtual TreeParserRootSymbolScope RootScope
        {
            get
            {
                return rootScope;
            }
        }

        private void SetContextQualifier(IParseTree ctx, ContextQualifier cq)
        {
            if ((cq == null) || (ctx == null))
            {
                return;
            }
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug(String.Format(LOG_ADDING_QUAL_TO, Indent(), cq, ctx.GetText()));
            }
            contextQualifiers.Put(ctx, cq);
        }
        public override void EnterProgram(ProgramContext ctx)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Entering program");
            }

            if (rootRoutine != null)
            {
                // Executing TreeParser more than once on a ParseTree would just result in meaningless result
                throw new System.InvalidOperationException("TreeParser has already been executed...");
            }

            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            currentBlock = PushBlock(new Block(rootScope, blockNode));
            rootScope.RootBlock = currentBlock;
            blockNode.Block = currentBlock;

            Routine routine = new Routine("", rootScope, rootScope)
            {
                DefinitionNode = blockNode
            };
            routine.SetProgressType(ABLNodeType.PROGRAM_ROOT);
            blockNode.Symbol = routine;

            rootScope.Add(routine);
            currentRoutine = routine;
            rootRoutine = routine;
        }

        public override void ExitProgram(ProgramContext ctx)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Exiting program");
            }
        }

        public override void EnterBlockFor(BlockForContext ctx)
        {
            foreach (RecordContext record in ctx.record())
            {
                SetContextQualifier(record, ContextQualifier.BUFFERSYMBOL);
            }
        }

        public override void ExitBlockFor(BlockForContext ctx)
        {
            foreach (RecordContext record in ctx.record())
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug($"{Indent()}> Adding strong buffer scope for {record.GetText()} to current block");
                }

                RecordNameNode recNode = (RecordNameNode)support.GetNode(record);
                currentBlock.AddStrongBufferScope(recNode);
            }
        }

        public override void EnterRecord(RecordContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            if (qual != null)
            {
                RecordNameNode((RecordNameNode)support.GetNode(ctx), qual);
            }
            else
            {
                LOG.Info($"No context qualifier found for {ctx.GetText()} in {Proparse.ruleNames[ctx.Parent.Parent.Parent.RuleIndex]} => {Proparse.ruleNames[ctx.Parent.Parent.RuleIndex]} => {Proparse.ruleNames[ctx.Parent.RuleIndex]} => {Proparse.ruleNames[ctx.RuleIndex]}");
            }
        }

        public override void EnterBlockOptionIterator(BlockOptionIteratorContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.REFUP);
            SetContextQualifier(ctx.expression(0), ContextQualifier.REF);
            SetContextQualifier(ctx.expression(1), ContextQualifier.REF);
        }

        public override void EnterBlockOptionWhile(BlockOptionWhileContext ctx)
        {
            SetContextQualifier(ctx.expression(), ContextQualifier.REF);
        }

        public override void EnterBlockPreselect(BlockPreselectContext ctx)
        {
            SetContextQualifier(ctx.forRecordSpec(), ContextQualifier.INITWEAK);
        }

        public override void EnterPseudoFunction(PseudoFunctionContext ctx)
        {
            if (ctx.entryFunction() != null)
            {
                SetContextQualifier(ctx.entryFunction().functionArgs().expression(1), ContextQualifier.UPDATING);
            }
            if (ctx.lengthFunction() != null)
            {
                SetContextQualifier(ctx.lengthFunction().functionArgs().expression(0), ContextQualifier.UPDATING);
            }
            if (ctx.rawFunction() != null)
            {
                SetContextQualifier(ctx.rawFunction().functionArgs().expression(0), ContextQualifier.UPDATING);
            }
            if (ctx.substringFunction() != null)
            {
                SetContextQualifier(ctx.substringFunction().functionArgs().expression(0), ContextQualifier.UPDATING);
            }
        }
        public override void EnterMemoryManagementFunction(MemoryManagementFunctionContext ctx)
        {
            SetContextQualifier(ctx.functionArgs().expression(0), ContextQualifier.UPDATING);
        }

        public override void EnterFunctionArgs(FunctionArgsContext ctx)
        {
            foreach (ExpressionContext exp in ctx.expression())
            {
                ContextQualifier qual = contextQualifiers.Get(exp);
                if (qual == null)
                {
                    SetContextQualifier(exp, ContextQualifier.REF);
                }
            }
        }

        public override void EnterRecordFunction(RecordFunctionContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.REF);
        }

        public override void EnterParameterBufferFor(ParameterBufferForContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.REF);
        }

        public override void EnterParameterBufferRecord(ParameterBufferRecordContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.INIT);
        }

        public override void EnterParameterOther(ParameterOtherContext ctx)
        {
            if (ctx.p != null)
            {
                if (ctx.OUTPUT() != null)
                {
                    SetContextQualifier(ctx.parameterArg(), ContextQualifier.UPDATING);
                }
                else if (ctx.INPUTOUTPUT() != null)
                {
                    SetContextQualifier(ctx.parameterArg(), ContextQualifier.REFUP);
                }
                else
                {
                    SetContextQualifier(ctx.parameterArg(), ContextQualifier.REF);
                }
            }
        }

        public override void EnterParameterArgTableHandle(ParameterArgTableHandleContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.INIT);
        }

        public override void ExitParameterArgTableHandle(ParameterArgTableHandleContext ctx)
        {
            noteReference(support.GetNode(ctx.field()), contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterParameterArgTable(ParameterArgTableContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.TEMPTABLESYMBOL);
        }

        public override void EnterParameterArgDatasetHandle(ParameterArgDatasetHandleContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.INIT);
        }

        public override void ExitParameterArgDatasetHandle(ParameterArgDatasetHandleContext ctx)
        {
            noteReference(support.GetNode(ctx.field()), contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterParameterArgAs(ParameterArgAsContext ctx)
        {
            Variable variable = new Variable("", currentScope);
            if (ctx.datatypeComNative() != null)
            {
                variable.SetDataType(DataType.GetDataType(ctx.datatypeComNative().Start.Type));
            }
            else if (ctx.datatypeVar() != null)
            {
                variable.SetDataType(DataType.GetDataType(ctx.datatypeVar().Start.Type));
            }
            else
            {
                variable.SetDataType(DataType.CLASS);
                variable.SetClassName(ctx.typeName().GetText());
            }
            currSymbol = variable;
        }

        public override void EnterParameterArgComDatatype(ParameterArgComDatatypeContext ctx)
        {
            SetContextQualifier(ctx.expression(), contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterFunctionParamBufferFor(FunctionParamBufferForContext ctx)
        {
            Parameter param = new Parameter();
            param.DirectionNode = null;
            currentRoutine.AddParameter(param);
            wipParameters.AddFirst(param);

            wipParameters.First.Value.DirectionNode = ABLNodeType.BUFFER;
            wipParameters.First.Value.ProgressType = ABLNodeType.BUFFER.Type;

            SetContextQualifier(ctx.record(), ContextQualifier.SYMBOL);
        }
        public override void ExitFunctionParamBufferFor(FunctionParamBufferForContext ctx)
        {
            if (ctx.bn != null)
            {
                TableBuffer buf = DefineBuffer(support.GetNode(ctx), support.GetNode(ctx), ctx.bn.GetText(), support.GetNode(ctx.record()), true);
                wipParameters.First.Value.Symbol = buf;
            }
            wipParameters.RemoveFirst();
        }

        public override void EnterFunctionParamStandard(FunctionParamStandardContext ctx)
        {
            Parameter param = new Parameter();
            param.DirectionNode = null;
            currentRoutine.AddParameter(param);
            wipParameters.AddFirst(param);
            if (ctx.qualif == null)
            {
                param.DirectionNode = ABLNodeType.INPUT;
            }
            else
            {
                param.DirectionNode = ABLNodeType.GetNodeType(ctx.qualif.Type);
            }
        }

        public override void ExitFunctionParamStandard(FunctionParamStandardContext ctx)
        {
            wipParameters.RemoveFirst();
        }

        public override void ExitFunctionParamStandardAs(FunctionParamStandardAsContext ctx)
        {
            Variable var = DefineVariable(ctx, support.GetNode(ctx), ctx.n.GetText(), true);
            wipParameters.First.Value.Symbol = var;
            AddToSymbolScope(var);
            DefAs(ctx.asDataTypeVar());
        }

        public override void EnterFunctionParamStandardLike(FunctionParamStandardLikeContext ctx)
        {
            Variable var = DefineVariable(ctx, support.GetNode(ctx), ctx.n2.GetText(), true);
            wipParameters.First.Value.Symbol = var;
            stack.AddFirst(var);
        }

        public override void ExitFunctionParamStandardLike(FunctionParamStandardLikeContext ctx)
        {
            DefLike(support.GetNode(ctx.likeField().field()));
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterFunctionParamStandardTable(FunctionParamStandardTableContext ctx)
        {
            wipParameters.First.Value.ProgressType = ABLNodeType.TEMPTABLE.Type;
            SetContextQualifier(ctx.record(), ContextQualifier.TEMPTABLESYMBOL);
        }

        public override void EnterFunctionParamStandardTableHandle(FunctionParamStandardTableHandleContext ctx)
        {
            Variable var = DefineVariable(ctx, support.GetNode(ctx), ctx.hn.GetText(), DataType.HANDLE, true);
            wipParameters.First.Value.Symbol = var;
            wipParameters.First.Value.ProgressType = ABLNodeType.TABLEHANDLE.Type;
            AddToSymbolScope(var);
        }

        public override void EnterFunctionParamStandardDatasetHandle(FunctionParamStandardDatasetHandleContext ctx)
        {
            Variable var = DefineVariable(ctx, support.GetNode(ctx), ctx.hn2.GetText(), DataType.HANDLE, true);
            wipParameters.First.Value.Symbol = var;
            wipParameters.First.Value.ProgressType = ABLNodeType.DATASETHANDLE.Type;

            AddToSymbolScope(var);
        }

        private void EnterExpression(ExpressionContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            if (qual == null)
            {
                qual = ContextQualifier.REF;
            }
            foreach (ExpressionTermContext c in ctx.GetRuleContexts<ExpressionTermContext>())
            {
                SetContextQualifier(c, qual);
            }
            foreach (ExpressionContext c in ctx.GetRuleContexts<ExpressionContext>())
            {
                SetContextQualifier(c, qual);
            }
        }

        public override void EnterExpressionMinus(ExpressionMinusContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionPlus(ExpressionPlusContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionOp1(ExpressionOp1Context ctx)
        {
            EnterExpression(ctx);
        }
        public override void EnterExpressionOp2(ExpressionOp2Context ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionComparison(ExpressionComparisonContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionStringComparison(ExpressionStringComparisonContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionNot(ExpressionNotContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionAnd(ExpressionAndContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionOr(ExpressionOrContext ctx)
        {
            EnterExpression(ctx);
        }

        public override void EnterExpressionExprt(ExpressionExprtContext ctx)
        {
            EnterExpression(ctx);
        }

        // Expression term

        public override void EnterExprtNoReturnValue(ExprtNoReturnValueContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            SetContextQualifier(ctx.sWidget(), qual);
            SetContextQualifier(ctx.colonAttribute(), qual);
        }

        public override void EnterExprtWidName(ExprtWidNameContext ctx)
        {
            Widattr(ctx, contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterExprtExprt2(ExprtExprt2Context ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            if ((ctx.colonAttribute() != null) && (ctx.expressionTerm2() is Exprt2FieldContext) && (ctx.colonAttribute().OBJCOLON(0) != null))
            {
                Widattr((Exprt2FieldContext)ctx.expressionTerm2(), qual, ctx.colonAttribute().id.Text);
            }
            else
            {
                SetContextQualifier(ctx.expressionTerm2(), qual);
                if (ctx.colonAttribute() != null)
                {
                    SetContextQualifier(ctx.colonAttribute(), qual);
                }
            }
        }

        public override void EnterExprt2ParenExpr(Exprt2ParenExprContext ctx)
        {
            SetContextQualifier(ctx.expression(), contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterExprt2Field(Exprt2FieldContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            if ((qual == null) || (qual == ContextQualifier.SYMBOL))
            {
                qual = ContextQualifier.REF;
            }
            SetContextQualifier(ctx.field(), qual);
        }

        public override void EnterWidattrExprt2(WidattrExprt2Context ctx)
        {
            if ((ctx.expressionTerm2() is Exprt2FieldContext) && (ctx.colonAttribute().OBJCOLON(0) != null))
            {
                Widattr((Exprt2FieldContext)ctx.expressionTerm2(), contextQualifiers.RemoveFrom(ctx), ctx.colonAttribute().id.Text);
            }
        }

        public override void EnterWidattrWidName(WidattrWidNameContext ctx)
        {
            Widattr(ctx, contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterGWidget(GWidgetContext ctx)
        {
            if (ctx.inuic() != null)
            {
                if (ctx.inuic().FRAME() != null)
                {
                    FrameRef(support.GetNode(ctx.inuic()).NextNode.NextNode);
                }
                else if (ctx.inuic().BROWSE() != null)
                {
                    BrowseRef(support.GetNode(ctx.inuic()).NextNode.NextNode);
                }
            }
        }
        public override void EnterSWidget(SWidgetContext ctx)
        {
            if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.REF);
            }
        }

        public override void EnterWidName(WidNameContext ctx)
        {
            // TODO Verify missing cases
            if (ctx.FRAME() != null)
            {
                FrameRef(support.GetNode(ctx).NextNode);
            }
            else if (ctx.BROWSE() != null)
            {
                BrowseRef(support.GetNode(ctx).NextNode);
            }
            else if ((ctx.BUFFER() != null) || (ctx.TEMPTABLE() != null))
            {
                BufferRef(ctx.filn().GetText());
            }
            else if (ctx.FIELD() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.REF);
            }
        }

        public override void EnterAggregateOption(AggregateOptionContext ctx)
        {
            AddToSymbolScope(DefineVariable(ctx, support.GetNode(ctx), ABLNodeType.GetFullText(ctx.accumulateWhat().Start.Type), ctx.accumulateWhat().COUNT() != null ? DataType.INTEGER : DataType.DECIMAL, false));
        }

        public override void EnterAssignmentList(AssignmentListContext ctx)
        {
            if (ctx.record() != null)
            {
                SetContextQualifier(ctx.record(), ContextQualifier.UPDATING);
            }
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void EnterAssignStatement2(AssignStatement2Context ctx)
        {
            if (ctx.widattr() != null)
            {
                SetContextQualifier(ctx.widattr(), ContextQualifier.UPDATING);
            }
            else if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            }
            SetContextQualifier(ctx.expression(), ContextQualifier.REF);
        }

        public override void EnterAssignEqual(AssignEqualContext ctx)
        {
            if (ctx.widattr() != null)
            {
                SetContextQualifier(ctx.widattr(), ContextQualifier.UPDATING);
            }
            else if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterReferencePoint(ReferencePointContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
        }

        public override void EnterBufferCompareStatement(BufferCompareStatementContext ctx)
        {
            SetContextQualifier(ctx.record(0), ContextQualifier.REF);

            if ((ctx.exceptUsingFields() != null) && (ctx.exceptUsingFields().field() != null))
            {
                ContextQualifier qual = ctx.exceptUsingFields().USING() == null ? ContextQualifier.SYMBOL : ContextQualifier.REF;
                foreach (FieldContext field in ctx.exceptUsingFields().field())
                {
                    SetContextQualifier(field, qual);
                    nameResolution.Put(field, TableNameResolution.LAST);
                }
            }

            SetContextQualifier(ctx.record(1), ContextQualifier.REF);
        }

        public override void EnterBufferCompareSave(BufferCompareSaveContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        public override void EnterBufferCopyStatement(BufferCopyStatementContext ctx)
        {
            SetContextQualifier(ctx.record(0), ContextQualifier.REF);

            if ((ctx.exceptUsingFields() != null) && (ctx.exceptUsingFields().field() != null))
            {
                ContextQualifier qual = ctx.exceptUsingFields().USING() == null ? ContextQualifier.SYMBOL : ContextQualifier.REF;
                foreach (FieldContext field in ctx.exceptUsingFields().field())
                {
                    SetContextQualifier(field, qual);
                    nameResolution.Put(field, TableNameResolution.LAST);
                }
            }

            SetContextQualifier(ctx.record(1), ContextQualifier.UPDATING);
        }
        public override void EnterChooseStatement(ChooseStatementContext ctx)
        {
            FrameInitializingStatement(ctx);
        }

        public override void EnterChooseField(ChooseFieldContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            frameStack.FormItem(support.GetNode(ctx.field()));
        }

        public override void EnterChooseOption(ChooseOptionContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        public override void ExitChooseStatement(ChooseStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void EnterClassStatement(ClassStatementContext ctx)
        {
            rootScope.ClassName = ctx.tn.GetText();
            rootScope.TypeInfo = refSession.GetTypeInfo(ctx.tn.GetText());
            rootScope.AbstractClass = (ctx.ABSTRACT().Length != 0);
            rootScope.SerializableClass = (ctx.SERIALIZABLE().Length != 0);
            rootScope.FinalClass = (ctx.FINAL().Length != 0);
        }

        public override void EnterInterfaceStatement(InterfaceStatementContext ctx)
        {
            rootScope.ClassName = ctx.name.GetText();
            rootScope.TypeInfo = refSession.GetTypeInfo(ctx.name.GetText());
            rootScope.Interface = true;
        }

        public override void ExitClearStatement(ClearStatementContext ctx)
        {
            if (ctx.frameWidgetName() != null)
            {
                frameStack.SimpleFrameInitStatement(ctx, support.GetNode(ctx), support.GetNode(ctx.frameWidgetName()), currentBlock);
            }
        }

        public override void EnterCatchStatement(CatchStatementContext ctx)
        {
            ScopeAdd(support.GetNode(ctx));
            AddToSymbolScope(DefineVariable(ctx, support.GetNode(ctx).FirstChild, ctx.n.Text));
            DefAs(ctx.classTypeName());
        }

        public override void ExitCatchStatement(CatchStatementContext ctx)
        {
            ScopeClose();
        }

        public override void EnterCloseStoredField(CloseStoredFieldContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.REF);
        }

        public override void EnterCloseStoredWhere(CloseStoredWhereContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.REF);
        }

        public override void EnterColorStatement(ColorStatementContext ctx)
        {
            FrameInitializingStatement(ctx);
            foreach (FieldFormItemContext item in ctx.fieldFormItem())
            {
                SetContextQualifier(item, ContextQualifier.SYMBOL);
                frameStack.FormItem(support.GetNode(item));
            }
        }

        public override void ExitColorStatement(ColorStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void ExitColumnFormatOption(ColumnFormatOptionContext ctx)
        {
            if ((ctx.LEXAT() != null) && (ctx.field() != null))
            {
                SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
                frameStack.LexAt(support.GetNode(ctx.field()));
            }
        }
        public override void EnterConstructorStatement(ConstructorStatementContext ctx)
        {
            /*
             * Since 'structors don't have a name, we don't add them to any sort of map in the parent scope.
             */
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            TreeParserSymbolScope definingScope = currentScope;
            ScopeAdd(blockNode);

            // 'structors don't have names, so use empty string.
            Routine r = new Routine("", definingScope, currentScope);
            r.SetProgressType(blockNode.NodeType);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            currentRoutine = r;
        }

        public override void ExitConstructorStatement(ConstructorStatementContext ctx)
        {
            ScopeClose();
            currentRoutine = rootRoutine;
        }

        public override void EnterCopyLobFrom(CopyLobFromContext ctx)
        {
            SetContextQualifier(ctx.expression(), ContextQualifier.REF);
        }

        public override void EnterCopyLobTo(CopyLobToContext ctx)
        {
            if (ctx.FILE() == null)
            {
                SetContextQualifier(ctx.expression(0), ContextQualifier.UPDATING);
            }
            else
            {
                // COPY-LOB ... TO FILE xxx : xxx is only referenced in this case, the value is not updated
                SetContextQualifier(ctx.expression(0), ContextQualifier.REF);
            }
        }

        public override void EnterCreateStatement(CreateStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateWhateverStatement(CreateWhateverStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateBrowseStatement(CreateBrowseStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateBufferStatement(CreateBufferStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateQueryStatement(CreateQueryStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateServerStatement(CreateServerStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateSocketStatement(CreateSocketStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateServerSocketStatement(CreateServerSocketStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateTempTableStatement(CreateTempTableStatementContext ctx)
        {
            SetContextQualifier(ctx.expressionTerm(), ContextQualifier.UPDATING);
        }

        public override void EnterCreateWidgetStatement(CreateWidgetStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }
        public override void EnterCanFindFunction(CanFindFunctionContext ctx)
        {
            RecordNameNode recordNode = (RecordNameNode)support.GetNode(ctx.recordphrase().record());
            // Keep a ref to the current block...
            Block b = currentBlock;
            // ...create a can-find scope and block (assigns currentBlock)...
            ScopeAdd(support.GetNode(ctx));
            // ...and then set this "can-find block" to use it as its parent.
            currentBlock.Parent = b;
            string buffName = ctx.recordphrase().record().GetText();
            ITable table;
            bool isDefault;
            TableBuffer tableBuffer = currentScope.LookupBuffer(buffName);
            if (tableBuffer != null)
            {
                table = tableBuffer.Table;
                isDefault = tableBuffer.Default;
                // Notify table buffer that it's used in a CAN-FIND
                tableBuffer.NoteReference(ContextQualifier.INIT);
            }
            else
            {
                table = refSession.Schema.LookupTable(buffName);
                isDefault = true;
            }
            TableBuffer newBuff = currentScope.DefineBuffer(isDefault ? "" : buffName, table);
            recordNode.TableBuffer = newBuff;
            currentBlock.AddHiddenCursor(recordNode);

            SetContextQualifier(ctx.recordphrase().record(), ContextQualifier.INIT);
        }

        public override void ExitCanFindFunction(CanFindFunctionContext ctx)
        {
            ScopeClose();
        }

        public override void EnterDdeGetStatement(DdeGetStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        public override void EnterDdeInitiateStatement(DdeInitiateStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        public override void EnterDdeRequestStatement(DdeRequestStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        public override void EnterDefineBrowseStatement(DefineBrowseStatementContext ctx)
        {
            stack.AddFirst(DefineBrowse(ctx, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void ExitDefineBrowseStatement(DefineBrowseStatementContext ctx)
        {
            AddToSymbolScope(stack.Last.Value);
            stack.RemoveLast();
        }

        public override void EnterDefBrowseDisplay(DefBrowseDisplayContext ctx)
        {
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void EnterDefBrowseDisplayItemsOrRecord(DefBrowseDisplayItemsOrRecordContext ctx)
        {
            if (ctx.recordAsFormItem() != null)
            {
                SetContextQualifier(ctx.recordAsFormItem(), ContextQualifier.INIT);
            }
        }

        public override void ExitDefBrowseDisplayItemsOrRecord(DefBrowseDisplayItemsOrRecordContext ctx)
        {
            if (ctx.recordAsFormItem() != null)
            {
                frameStack.FormItem(support.GetNode(ctx.recordAsFormItem()));
            }
            foreach (DefBrowseDisplayItemContext item in ctx.defBrowseDisplayItem())
            {
                frameStack.FormItem(support.GetNode(item));
            }
        }

        public override void EnterDefBrowseEnable(DefBrowseEnableContext ctx)
        {
            if ((ctx.allExceptFields() != null) && (ctx.allExceptFields().exceptFields() != null))
            {
                foreach (FieldContext fld in ctx.allExceptFields().exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                }
            }
        }

        public override void EnterDefBrowseEnableItem(DefBrowseEnableItemContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
            frameStack.FormItem(support.GetNode(ctx));
        }
        public override void EnterDefineBufferStatement(DefineBufferStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ctx.TEMPTABLE() == null ? ContextQualifier.SYMBOL : ContextQualifier.TEMPTABLESYMBOL);
        }

        public override void EnterFieldsFields(FieldsFieldsContext ctx)
        {
            foreach (FieldContext fld in ctx.field())
            {
                SetContextQualifier(fld, ContextQualifier.SYMBOL);
                nameResolution.Put(fld, TableNameResolution.LAST);
            }
        }

        public override void ExitDefineBufferStatement(DefineBufferStatementContext ctx)
        {
            DefineBuffer(support.GetNode(ctx), support.GetNode(ctx).IdNode, ctx.n.GetText(), support.GetNode(ctx.record()), false);
        }

        public override void EnterDefineButtonStatement(DefineButtonStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.BUTTON, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void EnterButtonOption(ButtonOptionContext ctx)
        {
            if (ctx.likeField() != null)
            {
                SetContextQualifier(ctx.likeField().field(), ContextQualifier.SYMBOL);
            }
        }

        public override void ExitDefineButtonStatement(DefineButtonStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineDatasetStatement(DefineDatasetStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.DATASET, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
            foreach (RecordContext record in ctx.record())
            {
                SetContextQualifier(record, ContextQualifier.INIT);
            }
        }

        public override void ExitDefineDatasetStatement(DefineDatasetStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDataRelation(DataRelationContext ctx)
        {
            foreach (RecordContext record in ctx.record())
            {
                SetContextQualifier(record, ContextQualifier.INIT);
            }
        }

        public override void EnterParentIdRelation(ParentIdRelationContext ctx)
        {
            foreach (RecordContext record in ctx.record())
            {
                SetContextQualifier(record, ContextQualifier.INIT);
            }
            foreach (FieldContext fld in ctx.field())
            {
                SetContextQualifier(fld, ContextQualifier.SYMBOL);
            }
        }

        public override void EnterFieldMappingPhrase(FieldMappingPhraseContext ctx)
        {
            for (int zz = 0; zz < ctx.field().Length; zz += 2)
            {
                SetContextQualifier(ctx.field()[zz], ContextQualifier.SYMBOL);
                nameResolution.Put(ctx.field()[zz], TableNameResolution.PREVIOUS);
                SetContextQualifier(ctx.field()[zz + 1], ContextQualifier.SYMBOL);
                nameResolution.Put(ctx.field()[zz + 1], TableNameResolution.LAST);
            }
        }

        public override void EnterDefineDataSourceStatement(DefineDataSourceStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.DATASOURCE, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void ExitDefineDataSourceStatement(DefineDataSourceStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterSourceBufferPhrase(SourceBufferPhraseContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.INIT);
            if (ctx.field() != null)
            {
                foreach (FieldContext fld in ctx.field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                }
            }
        }
        public override void EnterDefineEventStatement(DefineEventStatementContext ctx)
        {
            this.inDefineEvent = true;
            stack.AddFirst(DefineEvent(support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void ExitDefineEventStatement(DefineEventStatementContext ctx)
        {
            this.inDefineEvent = false;
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineFrameStatement(DefineFrameStatementContext ctx)
        {
            formItem2 = true;
            frameStack.NodeOfDefineFrame(ctx, support.GetNode(ctx), null, ctx.identifier().GetText(), currentScope);
            SetContextQualifier(ctx.formItemsOrRecord(), ContextQualifier.SYMBOL);

            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void ExitDefineFrameStatement(DefineFrameStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterDefineImageStatement(DefineImageStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.IMAGE, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void EnterDefineImageOption(DefineImageOptionContext ctx)
        {
            if (ctx.likeField() != null)
            {
                SetContextQualifier(ctx.likeField().field(), ContextQualifier.SYMBOL);
            }
        }

        public override void ExitDefineImageStatement(DefineImageStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineMenuStatement(DefineMenuStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.MENU, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void ExitDefineMenuStatement(DefineMenuStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineParameterStatement(DefineParameterStatementContext ctx)
        {
            Parameter param = new Parameter();
            if (ctx.defineParameterStatementSub2() != null)
            {
                if (ctx.qualif != null)
                {
                    param.DirectionNode = ABLNodeType.GetNodeType(ctx.qualif.Type);
                }
                else
                {
                    param.DirectionNode = ABLNodeType.INPUT;
                }
            }
            currentRoutine.AddParameter(param);
            wipParameters.AddFirst(param);
        }

        public override void ExitDefineParameterStatement(DefineParameterStatementContext ctx)
        {
            wipParameters.RemoveFirst();
        }

        public override void EnterDefineParameterStatementSub1(DefineParameterStatementSub1Context ctx)
        {
            wipParameters.First.Value.DirectionNode = ABLNodeType.BUFFER;
            wipParameters.First.Value.ProgressType = ABLNodeType.BUFFER.Type;
            SetContextQualifier(ctx.record(), ctx.TEMPTABLE() == null ? ContextQualifier.SYMBOL : ContextQualifier.TEMPTABLESYMBOL);
        }

        public override void ExitDefineParameterStatementSub1(DefineParameterStatementSub1Context ctx)
        {
            DefineBuffer(support.GetNode(ctx.Parent), support.GetNode(ctx.Parent).IdNode, ctx.bn.GetText(), support.GetNode(ctx.record()), true);
        }

        public override void EnterDefineParameterStatementSub2Variable(DefineParameterStatementSub2VariableContext ctx)
        {
            stack.AddFirst(DefineVariable(ctx.Parent, support.GetNode(ctx.Parent), ctx.identifier().GetText(), true));
        }
        public override void ExitDefineParameterStatementSub2Variable(DefineParameterStatementSub2VariableContext ctx)
        {
            wipParameters.First.Value.Symbol = stack.First.Value;
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineParameterStatementSub2VariableLike(DefineParameterStatementSub2VariableLikeContext ctx)
        {
            stack.AddFirst(DefineVariable(ctx.Parent, support.GetNode(ctx.Parent), ctx.identifier().GetText(), true));
        }

        public override void ExitDefineParameterStatementSub2VariableLike(DefineParameterStatementSub2VariableLikeContext ctx)
        {
            wipParameters.First.Value.Symbol = stack.First.Value;
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineParamVar(DefineParamVarContext ctx)
        {
            DefAs(ctx);
        }

        public override void ExitDefineParamVarLike(DefineParamVarLikeContext ctx)
        {
            DefLike(support.GetNode(ctx.field()));
        }

        public override void EnterDefineParameterStatementSub2Table(DefineParameterStatementSub2TableContext ctx)
        {
            wipParameters.First.Value.ProgressType = ABLNodeType.TEMPTABLE.Type;
            SetContextQualifier(ctx.record(), ContextQualifier.TEMPTABLESYMBOL);
        }

        public override void EnterDefineParameterStatementSub2TableHandle(DefineParameterStatementSub2TableHandleContext ctx)
        {
            wipParameters.First.Value.ProgressType = ABLNodeType.TABLEHANDLE.Type;
            AddToSymbolScope(DefineVariable(ctx, support.GetNode(ctx.Parent), ctx.pn2.GetText(), DataType.HANDLE, true));
        }

        public override void EnterDefineParameterStatementSub2DatasetHandle(DefineParameterStatementSub2DatasetHandleContext ctx)
        {
            wipParameters.First.Value.ProgressType = ABLNodeType.DATASETHANDLE.Type;
            AddToSymbolScope(DefineVariable(ctx, support.GetNode(ctx.Parent), ctx.dsh.GetText(), DataType.HANDLE, true));
        }

        public override void EnterDefinePropertyStatement(DefinePropertyStatementContext ctx)
        {
            stack.AddFirst(DefineVariable(ctx, support.GetNode(ctx), ctx.n.GetText()));

        }

        public override void EnterDefinePropertyAs(DefinePropertyAsContext ctx)
        {
            DefAs(ctx.datatype());
        }

        public override void ExitDefinePropertyAs(DefinePropertyAsContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefinePropertyAccessorGetBlock(DefinePropertyAccessorGetBlockContext ctx)
        {
            if (ctx.codeBlock() != null)
            {
                PropGetSetBegin(support.GetNode(ctx));
            }
        }

        public override void EnterDefinePropertyAccessorSetBlock(DefinePropertyAccessorSetBlockContext ctx)
        {
            if (ctx.codeBlock() != null)
            {
                PropGetSetBegin(support.GetNode(ctx));
            }
        }

        public override void ExitDefinePropertyAccessorGetBlock(DefinePropertyAccessorGetBlockContext ctx)
        {
            if (ctx.codeBlock() != null)
            {
                PropGetSetEnd();
            }
        }

        public override void ExitDefinePropertyAccessorSetBlock(DefinePropertyAccessorSetBlockContext ctx)
        {
            if (ctx.codeBlock() != null)
            {
                PropGetSetEnd();
            }
        }

        public override void EnterDefineQueryStatement(DefineQueryStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.QUERY, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
            foreach (RecordContext record in ctx.record())
            {
                SetContextQualifier(record, ContextQualifier.INIT);
            }
        }

        public override void ExitDefineQueryStatement(DefineQueryStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }
        public override void EnterDefineRectangleStatement(DefineRectangleStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.RECTANGLE, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void EnterRectangleOption(RectangleOptionContext ctx)
        {
            if (ctx.likeField() != null)
            {
                SetContextQualifier(ctx.likeField().field(), ContextQualifier.SYMBOL);
            }
        }

        public override void ExitDefineRectangleStatement(DefineRectangleStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void ExitDefineStreamStatement(DefineStreamStatementContext ctx)
        {
            AddToSymbolScope(DefineSymbol(ABLNodeType.STREAM, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void EnterDefineSubMenuStatement(DefineSubMenuStatementContext ctx)
        {
            stack.AddFirst(DefineSymbol(ABLNodeType.SUBMENU, support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText()));
        }

        public override void ExitDefineSubMenuStatement(DefineSubMenuStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefineTempTableStatement(DefineTempTableStatementContext ctx)
        {
            DefineTempTable(support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText());
        }

        public override void EnterDefTableBeforeTable(DefTableBeforeTableContext ctx)
        {
            DefineBuffer(support.GetNode(ctx), support.GetNode(ctx), ctx.i.GetText(), support.GetNode(ctx.Parent), false);
        }

        public override void ExitDefineTempTableStatement(DefineTempTableStatementContext ctx)
        {
            PostDefineTempTable();
        }

        public override void EnterDefTableLike(DefTableLikeContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.SYMBOL);
        }

        public override void ExitDefTableLike(DefTableLikeContext ctx)
        {
            DefineTableLike(ctx.record());
            foreach (DefTableUseIndexContext useIndex in ctx.defTableUseIndex())
            {
                DefineUseIndex(support.GetNode(ctx.record()), support.GetNode(useIndex.identifier()), useIndex.identifier().GetText());
            }
        }

        public override void EnterDefTableField(DefTableFieldContext ctx)
        {
            stack.AddFirst(DefineTableFieldInitialize(support.GetNode(ctx), ctx.identifier().GetText()));
        }

        public override void ExitDefTableField(DefTableFieldContext ctx)
        {
            DefineTableFieldFinalize(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDefTableIndex(DefTableIndexContext ctx)
        {
            DefineIndexInitialize(ctx.identifier(0).GetText(), ctx.UNIQUE() != null, ctx.PRIMARY() != null, false);
            for (int zz = 1; zz < ctx.identifier().Length; zz++)
            {
                DefineIndexField(ctx.identifier(zz).GetText());
            }
        }

        public override void EnterDefineWorkTableStatement(DefineWorkTableStatementContext ctx)
        {
            defineWorktable(support.GetNode(ctx), support.GetNode(ctx.identifier()), ctx.identifier().GetText());
        }

        public override void EnterDefineVariableStatement(DefineVariableStatementContext ctx)
        {
            stack.AddFirst(DefineVariable(ctx, support.GetNode(ctx), ctx.n.GetText()));
        }

        public override void ExitDefineVariableStatement(DefineVariableStatementContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterDeleteStatement(DeleteStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.UPDATING);
        }
        public override void EnterDestructorStatement(DestructorStatementContext ctx)
        {
            /*
             * Since 'structors don't have a name, we don't add them to any sort of map in the parent scope.
             */
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            TreeParserSymbolScope definingScope = currentScope;
            ScopeAdd(blockNode);

            // 'structors don't have names, so use empty string.
            Routine r = new Routine("", definingScope, currentScope);
            r.SetProgressType(blockNode.NodeType);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            currentRoutine = r;
        }

        public override void ExitDestructorStatement(DestructorStatementContext ctx)
        {
            ScopeClose();
            currentRoutine = rootRoutine;
        }

        public override void EnterDisableStatement(DisableStatementContext ctx)
        {
            formItem2 = true;
            FrameEnablingStatement(ctx);
            foreach (FormItemContext form in ctx.formItem())
            {
                SetContextQualifier(form, ContextQualifier.SYMBOL);
            }
            if ((ctx.allExceptFields() != null) && (ctx.allExceptFields().exceptFields() != null))
            {
                foreach (FieldContext fld in ctx.allExceptFields().exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                }
            }
        }

        public override void ExitDisableStatement(DisableStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterDisableTriggersStatement(DisableTriggersStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.SYMBOL);
        }

        public override void EnterDisplayStatement(DisplayStatementContext ctx)
        {
            FrameInitializingStatement(ctx);
            SetContextQualifier(ctx.displayItemsOrRecord(), ContextQualifier.REF);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void EnterDisplayItemsOrRecord(DisplayItemsOrRecordContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            for (int kk = 0; kk < ctx.ChildCount; kk++)
            {
                SetContextQualifier(ctx.GetChild(kk), qual);
            }
        }

        public override void EnterDisplayItem(DisplayItemContext ctx)
        {
            if (ctx.expression() != null)
            {
                SetContextQualifier(ctx.expression(), contextQualifiers.RemoveFrom(ctx));
            }
        }

        public override void ExitDisplayItem(DisplayItemContext ctx)
        {
            if (ctx.expression() != null)
            {
                frameStack.FormItem(support.GetNode(ctx));
            }
        }

        public override void ExitDisplayStatement(DisplayStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void EnterFieldEqualDynamicNew(FieldEqualDynamicNewContext ctx)
        {
            if (ctx.widattr() != null)
            {
                SetContextQualifier(ctx.widattr(), ContextQualifier.UPDATING);
            }
            else if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterDoStatement(DoStatementContext ctx)
        {
            BlockBegin(ctx);
            FrameBlockCheck(support.GetNode(ctx));
        }
        public override void EnterDoStatementSub(DoStatementSubContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void ExitDoStatement(DoStatementContext ctx)
        {
            BlockEnd();
        }

        public override void EnterDownStatement(DownStatementContext ctx)
        {
            FrameEnablingStatement(ctx);
        }

        public override void ExitDownStatement(DownStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void EnterEmptyTempTableStatement(EmptyTempTableStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.TEMPTABLESYMBOL);
        }

        public override void EnterEnableStatement(EnableStatementContext ctx)
        {
            formItem2 = true;
            FrameEnablingStatement(ctx);

            foreach (FormItemContext form in ctx.formItem())
            {
                SetContextQualifier(form, ContextQualifier.SYMBOL);
            }
            if ((ctx.allExceptFields() != null) && (ctx.allExceptFields().exceptFields() != null))
            {
                foreach (FieldContext fld in ctx.allExceptFields().exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                }
            }
        }

        public override void ExitEnableStatement(EnableStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterExportStatement(ExportStatementContext ctx)
        {
            SetContextQualifier(ctx.displayItemsOrRecord(), ContextQualifier.REF);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void EnterExtentPhrase2(ExtentPhrase2Context ctx)
        {
            if (ctx.constant() != null)
            {
                defExtent(ctx.constant().GetText());
            }
        }

        public override void EnterFieldOption(FieldOptionContext ctx)
        {
            if (ctx.AS() != null)
            {
                DefAs(ctx.asDataTypeField());
            }
            else if (ctx.LIKE() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
            }
        }

        public override void ExitFieldOption(FieldOptionContext ctx)
        {
            if (ctx.LIKE() != null)
            {
                DefLike(support.GetNode(ctx.field()));
            }
        }

        public override void EnterFindStatement(FindStatementContext ctx)
        {
            SetContextQualifier(ctx.recordphrase().record(), ContextQualifier.INIT);
        }

        public override void EnterForStatement(ForStatementContext ctx)
        {
            BlockBegin(ctx);
            FrameBlockCheck(support.GetNode(ctx));

            SetContextQualifier(ctx.forRecordSpec(), ContextQualifier.INITWEAK);
        }

        public override void EnterForstate_sub(Forstate_subContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void ExitForStatement(ForStatementContext ctx)
        {
            BlockEnd();
        }
        public override void EnterForRecordSpec(ForRecordSpecContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            foreach (RecordphraseContext rec in ctx.recordphrase())
            {
                SetContextQualifier(rec.record(), qual);
            }
        }

        public override void EnterFormItem(FormItemContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), qual);
            }
            else if (ctx.recordAsFormItem() != null)
            {
                SetContextQualifier(ctx.recordAsFormItem(), qual);
            }
        }

        public override void ExitFormItem(FormItemContext ctx)
        {
            if ((ctx.field() != null) || (ctx.recordAsFormItem() != null))
            {
                frameStack.FormItem(support.GetNode(ctx));
            }
        }

        public override void EnterFormItemsOrRecord(FormItemsOrRecordContext ctx)
        {
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            for (int kk = 0; kk < ctx.ChildCount; kk++)
            {
                if (formItem2 && (ctx.GetChild(kk) is RecordAsFormItemContext) && (qual == ContextQualifier.SYMBOL))
                {
                    SetContextQualifier(ctx.GetChild(kk), ContextQualifier.BUFFERSYMBOL);
                }
                else
                {
                    SetContextQualifier(ctx.GetChild(kk), qual);
                }
            }
        }

        public override void EnterVarRecField(VarRecFieldContext ctx)
        {
            if (ctx.record() != null)
            {
                SetContextQualifier(ctx.record(), contextQualifiers.RemoveFrom(ctx));
            }
            else
            {
                SetContextQualifier(ctx.field(), contextQualifiers.RemoveFrom(ctx));
            }
        }

        public override void EnterRecordAsFormItem(RecordAsFormItemContext ctx)
        {
            SetContextQualifier(ctx.record(), contextQualifiers.RemoveFrom(ctx));
        }

        public override void EnterFormStatement(FormStatementContext ctx)
        {
            formItem2 = true;
            FrameInitializingStatement(ctx);
            SetContextQualifier(ctx.formItemsOrRecord(), ContextQualifier.SYMBOL);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void ExitFormStatement(FormStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterFormatOption(FormatOptionContext ctx)
        {
            if ((ctx.LEXAT() != null) && (ctx.field() != null))
            {
                SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
                frameStack.LexAt(support.GetNode(ctx.field()));
            }
            else if ((ctx.LIKE() != null) && (ctx.field() != null))
            {
                SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
            }
        }

        public override void EnterFrameWidgetName(FrameWidgetNameContext ctx)
        {
            frameStack.FrameRefNode(support.GetNode(ctx).FirstChild, currentScope);
        }

        public override void EnterFrameOption(FrameOptionContext ctx)
        {
            if (((ctx.CANCELBUTTON() != null) || (ctx.DEFAULTBUTTON() != null)) && (ctx.field() != null))
            {
                SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
            }
        }
        public override void EnterFunctionStatement(FunctionStatementContext ctx)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> New function definition '{ ctx.id.GetText()}'");
            }

            TreeParserSymbolScope definingScope = currentScope;
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            ScopeAdd(blockNode);

            Routine r = new Routine(ctx.id.GetText(), definingScope, currentScope);
            if (ctx.typeName() != null)
            {
                r.ReturnDatatypeNode = DataType.CLASS;
            }
            else
            {
                r.ReturnDatatypeNode = DataType.GetDataType(ctx.datatypeVar().Start.Type);
            }
            r.SetProgressType(ABLNodeType.FUNCTION);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            definingScope.Add(r);
            currentRoutine = r;

            if (ctx.FORWARDS() != null)
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug($"{Indent()}> FORWARDS definition");
                }
                funcForwards[ctx.id.GetText()] = currentScope;
            }
            else if ((ctx.functionParams() == null) || (ctx.functionParams().ChildCount == 2))
            {
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug($"{Indent()}> No parameter, trying to find them in FORWARDS declaration");
                }
                // No parameter defined, then we inherit from FORWARDS declaration (if available)                
                if (funcForwards.TryGetValue(ctx.id.GetText(), out TreeParserSymbolScope forwardScope))
                {
                    if (LOG.IsDebugEnabled)
                    {
                        LOG.Debug($"{Indent()}> Inherits from FORWARDS definition");
                    }
                    Routine r2 = forwardScope.Routine;
                    ScopeSwap(forwardScope);
                    blockNode.Block = currentBlock;
                    blockNode.Symbol = r2;
                    r2.DefinitionNode = blockNode;
                    definingScope.Add(r2);
                    currentRoutine = r2;
                }
            }
        }

        public override void ExitFunctionStatement(FunctionStatementContext ctx)
        {
            ScopeClose();
            currentRoutine = rootRoutine;
        }

        public override void EnterGetKeyValueStatement(GetKeyValueStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        public override void EnterImportStatement(ImportStatementContext ctx)
        {
            foreach (FieldContext fld in ctx.field())
            {
                SetContextQualifier(fld, ContextQualifier.UPDATING);
            }
            if (ctx.varRecField() != null)
            {
                SetContextQualifier(ctx.varRecField(), ContextQualifier.UPDATING);
            }
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void EnterInsertStatement(InsertStatementContext ctx)
        {
            FrameInitializingStatement(ctx);

            SetContextQualifier(ctx.record(), ContextQualifier.UPDATING);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void ExitInsertStatement(InsertStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void EnterLdbnameOption(LdbnameOptionContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.BUFFERSYMBOL);
        }

        public override void EnterMessageOption(MessageOptionContext ctx)
        {
            if ((ctx.SET() != null) && (ctx.field() != null))
            {
                SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            }
            else if ((ctx.UPDATE() != null) && (ctx.field() != null))
            {
                SetContextQualifier(ctx.field(), ContextQualifier.REFUP);
            }
        }
        public override void EnterMethodStatement(MethodStatementContext ctx)
        {
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            TreeParserSymbolScope definingScope = currentScope;
            ScopeAdd(blockNode);

            Routine r = new Routine(ctx.id.GetText(), definingScope, currentScope);
            r.SetProgressType(ABLNodeType.METHOD);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            definingScope.Add(r);
            currentRoutine = r;

            if (ctx.VOID() != null)
            {
                currentRoutine.ReturnDatatypeNode = DataType.VOID;
            }
            else
            {
                if (ctx.datatype().CLASS() != null)
                {
                    currentRoutine.ReturnDatatypeNode = DataType.CLASS;
                }
                else
                {
                    if (ctx.datatype().datatypeVar().typeName() != null)
                    {
                        currentRoutine.ReturnDatatypeNode = DataType.CLASS;
                    }
                    else
                    {
                        currentRoutine.ReturnDatatypeNode = DataType.GetDataType(support.GetNode(ctx.datatype().datatypeVar()).Type);
                    }
                }
            }
        }

        public override void ExitMethodStatement(MethodStatementContext ctx)
        {
            ScopeClose();
            currentRoutine = rootRoutine;
        }

        public override void EnterNextPromptStatement(NextPromptStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
        }

        public override void EnterOpenQueryStatement(OpenQueryStatementContext ctx)
        {
            SetContextQualifier(ctx.forRecordSpec(), ContextQualifier.INIT);
        }

        public override void EnterExternalProcedureStatement(ExternalProcedureStatementContext ctx)
        {
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            TreeParserSymbolScope definingScope = currentScope;
            ScopeAdd(blockNode);

            Routine r = new Routine(ctx.filename().GetText(), definingScope, currentScope);
            r.SetProgressType(ABLNodeType.PROCEDURE);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            definingScope.Add(r);
            currentRoutine = r;
        }

        public override void ExitExternalProcedureStatement(ExternalProcedureStatementContext ctx)
        {
            ScopeClose();
            currentRoutine = rootRoutine;
        }

        public override void EnterProcedureStatement(ProcedureStatementContext ctx)
        {
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            TreeParserSymbolScope definingScope = currentScope;
            ScopeAdd(blockNode);

            Routine r = new Routine(ctx.filename().GetText(), definingScope, currentScope);
            r.SetProgressType(ABLNodeType.PROCEDURE);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            definingScope.Add(r);
            currentRoutine = r;
        }

        public override void ExitProcedureStatement(ProcedureStatementContext ctx)
        {
            ScopeClose();
            currentRoutine = rootRoutine;
        }

        public override void EnterPromptForStatement(PromptForStatementContext ctx)
        {
            formItem2 = true;
            FrameEnablingStatement(ctx);

            SetContextQualifier(ctx.formItemsOrRecord(), ContextQualifier.SYMBOL);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void ExitPromptForStatement(PromptForStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterRawTransferStatement(RawTransferStatementContext ctx)
        {
            SetContextQualifier(ctx.rawTransferElement(0), ContextQualifier.REF);
            SetContextQualifier(ctx.rawTransferElement(1), ContextQualifier.UPDATING);
        }
        public override void EnterRawTransferElement(RawTransferElementContext ctx)
        {
            if (ctx.record() != null)
            {
                SetContextQualifier(ctx.record(), contextQualifiers.RemoveFrom(ctx));
            }
            else if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), contextQualifiers.RemoveFrom(ctx));
            }
            else
            {
                SetContextQualifier(ctx.varRecField(), contextQualifiers.RemoveFrom(ctx));
            }
        }

        public override void EnterFieldFrameOrBrowse(FieldFrameOrBrowseContext ctx)
        {
            if (ctx.FRAME() != null)
            {
                FrameRef(support.GetNode(ctx).FirstChild);
            }
            else if (ctx.BROWSE() != null)
            {
                BrowseRef(support.GetNode(ctx).FirstChild);
            }
        }

        public override void ExitField(FieldContext ctx)
        {
            TableNameResolution? tnr = nameResolution.RemoveFrom(ctx);
            if (tnr == null)
            {
                tnr = TableNameResolution.ANY;
            }
            ContextQualifier qual = contextQualifiers.RemoveFrom(ctx);
            if (qual == null)
            {
                qual = ContextQualifier.REF;
            }
            Field(ctx, (FieldRefNode)support.GetNode(ctx), null, ctx.id.GetText(), qual, (TableNameResolution)tnr);
        }

        public override void EnterRecordFields(RecordFieldsContext ctx)
        {
            foreach (FieldContext fld in ctx.field())
            {
                SetContextQualifier(fld, ContextQualifier.SYMBOL);
                nameResolution.Put(fld, TableNameResolution.LAST);
            }
        }

        public override void EnterRecordOption(RecordOptionContext ctx)
        {
            if ((ctx.OF() != null) && (ctx.record() != null))
            {
                SetContextQualifier(ctx.record(), ContextQualifier.REF);
            }
            if ((ctx.USING() != null) && (ctx.field() != null))
            {
                foreach (FieldContext field in ctx.field())
                {
                    SetContextQualifier(field, ContextQualifier.SYMBOL);
                    nameResolution.Put(field, TableNameResolution.LAST);
                }
            }
        }

        public override void EnterOnStatement(OnStatementContext ctx)
        {
            ScopeAdd(support.GetNode(ctx));
        }

        public override void ExitOnStatement(OnStatementContext ctx)
        {
            ScopeClose();
        }

        public override void EnterOnOtherOfDbObject(OnOtherOfDbObjectContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.SYMBOL);
        }

        public override void ExitOnOtherOfDbObject(OnOtherOfDbObjectContext ctx)
        {
            DefineBufferForTrigger(support.GetNode(ctx.record()));
        }

        public override void EnterOnWriteOfDbObject(OnWriteOfDbObjectContext ctx)
        {
            SetContextQualifier(ctx.bf, ContextQualifier.SYMBOL);
        }

        public override void ExitOnWriteOfDbObject(OnWriteOfDbObjectContext ctx)
        {
            if (ctx.n != null)
            {
                DefineBuffer(support.GetNode(ctx.Parent.Parent).FindDirectChild(ABLNodeType.NEW), null, ctx.n.GetText(), support.GetNode(ctx.bf), true);
            }
            else
            {
                DefineBufferForTrigger(support.GetNode(ctx.bf));
            }

            if (ctx.o != null)
            {
                DefineBuffer(support.GetNode(ctx.Parent.Parent).FindDirectChild(ABLNodeType.OLD), null, ctx.o.GetText(), support.GetNode(ctx.bf), true);
            }
        }

        public override void EnterOnAssign(OnAssignContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.INIT);
        }

        public override void EnterOnAssignOldValue(OnAssignOldValueContext ctx)
        {
            Variable var = DefineVariable(ctx, support.GetNode(ctx.Parent), ctx.f.GetText());
            currSymbol = var;
            stack.AddFirst(var);
        }

        public override void ExitOnAssignOldValue(OnAssignOldValueContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
            currSymbol = null;
        }

        public override void EnterReleaseStatement(ReleaseStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.REF);
        }

        public override void EnterRepeatStatement(RepeatStatementContext ctx)
        {
            BlockBegin(ctx);
            FrameBlockCheck(support.GetNode(ctx));
        }

        public override void EnterRepeatStatementSub(RepeatStatementSubContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void ExitRepeatStatement(RepeatStatementContext ctx)
        {
            BlockEnd();
        }

        public override void EnterRunSet(RunSetContext ctx)
        {
            if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterScrollStatement(ScrollStatementContext ctx)
        {
            FrameInitializingStatement(ctx);
        }

        public override void ExitScrollStatement(ScrollStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void EnterSetStatement(SetStatementContext ctx)
        {
            formItem2 = true;
            FrameInitializingStatement(ctx);

            SetContextQualifier(ctx.formItemsOrRecord(), ContextQualifier.REFUP);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void ExitSetStatement(SetStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterSystemDialogColorStatement(SystemDialogColorStatementContext ctx)
        {
            if (ctx.updateField() != null)
            {
                SetContextQualifier(ctx.updateField().field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterSystemDialogFontOption(SystemDialogFontOptionContext ctx)
        {
            if (ctx.updateField() != null)
            {
                SetContextQualifier(ctx.updateField().field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterSystemDialogGetDirStatement(SystemDialogGetDirStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.REFUP);
        }
        public override void EnterSystemDialogGetDirOption(SystemDialogGetDirOptionContext ctx)
        {
            if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.REFUP);
            }
        }

        public override void EnterSystemDialogGetFileStatement(SystemDialogGetFileStatementContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.REFUP);
        }

        public override void EnterSystemDialogGetFileOption(SystemDialogGetFileOptionContext ctx)
        {
            if (ctx.field() != null)
            {
                SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterSystemDialogPrinterOption(SystemDialogPrinterOptionContext ctx)
        {
            if (ctx.updateField() != null)
            {
                SetContextQualifier(ctx.updateField().field(), ContextQualifier.UPDATING);
            }
        }

        public override void EnterTriggerProcedureStatementSub1(TriggerProcedureStatementSub1Context ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.SYMBOL);
        }

        public override void ExitTriggerProcedureStatementSub1(TriggerProcedureStatementSub1Context ctx)
        {
            DefineBufferForTrigger(support.GetNode(ctx.record()));
        }

        public override void EnterTriggerProcedureStatementSub2(TriggerProcedureStatementSub2Context ctx)
        {
            SetContextQualifier(ctx.buff, ContextQualifier.SYMBOL);
        }

        public override void ExitTriggerProcedureStatementSub2(TriggerProcedureStatementSub2Context ctx)
        {
            if (ctx.newBuff != null)
            {
                DefineBuffer(support.GetNode(ctx.Parent).FindDirectChild(ABLNodeType.NEW), null, ctx.newBuff.GetText(), support.GetNode(ctx.buff), true);
            }
            else
            {
                DefineBufferForTrigger(support.GetNode(ctx.buff));
            }

            if (ctx.oldBuff != null)
            {
                DefineBuffer(support.GetNode(ctx.Parent).FindDirectChild(ABLNodeType.OLD), null, ctx.oldBuff.GetText(), support.GetNode(ctx.buff), true);
            }
        }

        public override void EnterTriggerOfSub1(TriggerOfSub1Context ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.SYMBOL);
        }

        public override void EnterTriggerOfSub2(TriggerOfSub2Context ctx)
        {
            stack.AddFirst(DefineVariable(ctx, support.GetNode(ctx), ctx.id.GetText()));
        }

        public override void ExitTriggerOfSub2(TriggerOfSub2Context ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterTriggerOld(TriggerOldContext ctx)
        {
            stack.AddFirst(DefineVariable(ctx, support.GetNode(ctx), ctx.id.GetText()));
        }

        public override void ExitTriggerOld(TriggerOldContext ctx)
        {
            AddToSymbolScope(stack.First.Value);
            stack.RemoveFirst();
        }

        public override void EnterTriggerOn(TriggerOnContext ctx)
        {
            ScopeAdd(support.GetNode(ctx));
        }

        public override void ExitTriggerOn(TriggerOnContext ctx)
        {
            ScopeClose();
        }

        public override void EnterUnderlineStatement(UnderlineStatementContext ctx)
        {
            FrameInitializingStatement(ctx);

            foreach (FieldFormItemContext field in ctx.fieldFormItem())
            {
                SetContextQualifier(field, ContextQualifier.SYMBOL);
                frameStack.FormItem(support.GetNode(field));
            }
        }

        public override void ExitUnderlineStatement(UnderlineStatementContext ctx)
        {
            frameStack.StatementEnd();
        }

        public override void EnterUpStatement(UpStatementContext ctx)
        {
            FrameInitializingStatement(ctx);
        }

        public override void ExitUpStatement(UpStatementContext ctx)
        {
            frameStack.StatementEnd();
        }
        public override void EnterUpdateStatement(UpdateStatementContext ctx)
        {
            formItem2 = true;
            FrameEnablingStatement(ctx);
            SetContextQualifier(ctx.formItemsOrRecord(), ContextQualifier.REFUP);
            if (ctx.exceptFields() != null)
            {
                foreach (FieldContext fld in ctx.exceptFields().field())
                {
                    SetContextQualifier(fld, ContextQualifier.SYMBOL);
                    nameResolution.Put(fld, TableNameResolution.LAST);
                }
            }
        }

        public override void ExitUpdateStatement(UpdateStatementContext ctx)
        {
            frameStack.StatementEnd();
            formItem2 = false;
        }

        public override void EnterValidateStatement(ValidateStatementContext ctx)
        {
            SetContextQualifier(ctx.record(), ContextQualifier.REF);
        }

        public override void ExitViewStatement(ViewStatementContext ctx)
        {
            // The VIEW statement grammar uses gwidget, so we have to do some
            // special searching for FRAME to initialize.
            JPNode headNode = support.GetNode(ctx);
            foreach (JPNode frameNode in headNode.Query(ABLNodeType.FRAME))
            {
                ABLNodeType parentType = frameNode.Parent.NodeType;
                if (parentType == ABLNodeType.WIDGET_REF || parentType == ABLNodeType.IN)
                {
                    frameStack.SimpleFrameInitStatement(ctx, headNode, frameNode, currentBlock);
                    return;
                }
            }
        }

        public override void EnterWaitForSet(WaitForSetContext ctx)
        {
            SetContextQualifier(ctx.field(), ContextQualifier.UPDATING);
        }

        // ******************
        // INTERNAL METHODS
        // ******************

        /// <summary>
        /// Called at the *end* of the statement that defines the symbol. </summary>
        private void AddToSymbolScope(Symbol symbol)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Adding symbol '{symbol}' to current scope");
            }
            if (inDefineEvent)
            {
                return;
            }
            currentScope.Add(symbol);
        }

        private Block PushBlock(Block block)
        {
            return PushBlock(block, false);
        }

        private Block PushBlock(Block block, bool fromSwap)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Pushing block '{block}' to stack");
            }
            blockStack.Add(block);

            if (!fromSwap && (lastStatement != null))
            {
                lastStatement.NextStatement = block.Node;
                block.Node.PreviousStatement = lastStatement;
            }
            lastStatement = null;

            return block;
        }

        private Block PopBlock()
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Popping block from stack");
            }
            Block bb = blockStack[blockStack.Count - 1];
            blockStack.RemoveAt(blockStack.Count - 1);
            lastStatement = bb.Node;
            return blockStack[blockStack.Count - 1];
        }

        private void RecordNameNode(RecordNameNode recordNode, ContextQualifier contextQualifier)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"Entering recordNameNode {recordNode} {contextQualifier}");
            }

            recordNode.ContextQualifier = contextQualifier;
            TableBuffer buffer = null;
            switch (contextQualifier.innerEnumValue)
            {
                case ContextQualifier.InnerEnum.INIT:
                case ContextQualifier.InnerEnum.INITWEAK:
                case ContextQualifier.InnerEnum.REF:
                case ContextQualifier.InnerEnum.REFUP:
                case ContextQualifier.InnerEnum.UPDATING:
                case ContextQualifier.InnerEnum.BUFFERSYMBOL:
                    buffer = currentScope.GetBufferSymbol(recordNode.Text);
                    break;
                case ContextQualifier.InnerEnum.SYMBOL:
                    buffer = currentScope.LookupTableOrBufferSymbol(recordNode.Text);
                    break;
                case ContextQualifier.InnerEnum.TEMPTABLESYMBOL:
                    buffer = currentScope.LookupTempTable(recordNode.Text);
                    break;
                case ContextQualifier.InnerEnum.SCHEMATABLESYMBOL:
                    ITable table = refSession.Schema.LookupTable(recordNode.Text);
                    if (table != null)
                    {
                        buffer = currentScope.GetUnnamedBuffer(table);
                    }
                    break;
                case ContextQualifier.InnerEnum.STATIC:
                    break;
            }
            if (buffer == null)
            {
                return;
            }
            RecordNodeSymbol(recordNode, buffer); // Does checks, sets attributes.
            recordNode.TableBuffer = buffer;
            switch (contextQualifier.innerEnumValue)
            {
                case ContextQualifier.InnerEnum.INIT:
                case ContextQualifier.InnerEnum.REF:
                case ContextQualifier.InnerEnum.REFUP:
                case ContextQualifier.InnerEnum.UPDATING:
                    recordNode.BufferScope = currentBlock.GetBufferForReference(buffer);
                    break;
                case ContextQualifier.InnerEnum.INITWEAK:
                    recordNode.BufferScope = currentBlock.AddWeakBufferScope(buffer);
                    break;
                default:
                    break;
            }
            buffer.NoteReference(contextQualifier);
        }
        /// <summary>
        /// For a RECORD_NAME node, do checks and assignments for the TableBuffer. </summary>
        private void RecordNodeSymbol(RecordNameNode node, TableBuffer buffer)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"Entering recordNodeSymbol {node} {buffer}");
            }

            string nodeText = node.Text;
            if (buffer == null)
            {
                LOG.Error($"Could not resolve table '{nodeText}' in file #{node.FileIndex}:{node.Line}:{node.Column}");
                return;
            }

            ITable table = buffer.Table;
            prevTableReferenced = lastTableReferenced;
            lastTableReferenced = buffer;

            // For an unnamed buffer, determine if it's abbreviated.
            // Note that named buffers, temp and work table names cannot be abbreviated.
            if (buffer.Default && table.Storetype == IConstants.ST_DBTABLE)
            {
                string[] nameParts = nodeText.Split('.');
                int tableNameLen = nameParts[nameParts.Length - 1].Length;
                if (table.GetName().Length > tableNameLen)
                {
                    node.AttrSet(IConstants.ABBREVIATED, 1);
                }
            }
        }

        private void BlockBegin(IParseTree ctx)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Creating new block");
            }
            BlockNode blockNode = (BlockNode)support.GetNode(ctx);
            currentBlock = PushBlock(new Block(currentBlock, blockNode));
            blockNode.Block = currentBlock;
        }

        private void BlockEnd()
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> End of block");
            }
            currentBlock = PopBlock();
        }

        private void ScopeAdd(JPNode anode)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Creating new scope for block {anode.NodeType}");
            }

            BlockNode blockNode = (BlockNode)anode;
            currentScope = currentScope.AddScope();
            currentBlock = PushBlock(new Block(currentScope, blockNode));
            currentScope.RootBlock = currentBlock;
            blockNode.Block = currentBlock;
        }

        private void ScopeClose()
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> End of scope");
            }

            currentScope = currentScope.ParentScope;
            BlockEnd();
        }

        /// <summary>
        /// In the case of a function definition that comes some time after a function forward declaration, we want to use the
        /// scope that was created with the forward declaration, because it is the scope that has all of the parameter
        /// definitions. We have to do this because the definition itself may have left out the parameter list - it's not
        /// required - it just uses the parameter list from the declaration.
        /// </summary>
        private void ScopeSwap(TreeParserSymbolScope scope)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Swapping scope...");
            }

            currentScope = scope;
            BlockEnd(); // pop the unused block from the stack
            currentBlock = PushBlock(scope.RootBlock, true);
        }

        /// <summary>
        /// This is a specialization of frameInitializingStatement, called for ENABLE|UPDATE|PROMPT-FOR. </summary>
        private void FrameEnablingStatement(IParseTree ctx)
        {
            LOG.Debug($"Entering frameEnablingStatement");

            // Flip this flag before calling nodeOfInitializingStatement.
            frameStack.StatementIsEnabler();
            frameStack.NodeOfInitializingStatement(ctx, support.GetNode(ctx), currentBlock);
        }

        public virtual void FrameInitializingStatement(IParseTree ctx)
        {
            frameStack.NodeOfInitializingStatement(ctx, support.GetNode(ctx), currentBlock);
        }

        private void FrameBlockCheck(JPNode ast)
        {
            LOG.Debug($"Entering frameBlockCheck {ast}");
            frameStack.NodeOfBlock(ast, currentBlock);
        }

        private Variable DefineVariable(IParseTree ctx, JPNode defAST, string name)
        {
            return DefineVariable(ctx, defAST, name, false);
        }

        private Variable DefineVariable(IParseTree ctx, JPNode defAST, string name, DataType dataType, bool parameter)
        {
            Variable v = DefineVariable(ctx, defAST, name, parameter);
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Adding datatype {dataType}");
            }
            v.SetDataType(dataType);
            return v;
        }

        private Variable DefineVariable(IParseTree ctx, JPNode defNode, string name, bool parameter)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> New variable {name} (parameter: {parameter})");
            }

            // We need to create the Variable Symbol right away, because further actions in the grammar might need to set
            // attributes on it. We can't add it to the scope yet, because of statements like this: def var xyz like xyz.
            // The tree parser is responsible for calling addToScope at the end of the statement or when it is otherwise safe to
            // do so.
            Variable variable = new Variable(name, currentScope, parameter);
            if (defNode == null)
            {
                LOG.Warn($"Unable to set JPNode symbol for variable {ctx.GetText()}");
            }
            else
            {
                defNode.IdNode.Symbol = variable;
                variable.DefinitionNode = defNode.IdNode;
            }
            currSymbol = variable;
            return variable;
        }

        /// <summary>
        /// The tree parser calls this at an AS node </summary>
        public virtual void DefAs(ParserRuleContext ctx)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Variable AS '{ctx.GetText()}'");
            }

            Primative primative = (Primative)currSymbol;
            if ((ctx.Start.Type == ABLNodeType.CLASS.Type) || (ctx.Stop.Type == ABLNodeType.TYPE_NAME.Type))
            {
                primative.SetDataType(DataType.CLASS);
                primative.SetClassName(ctx.Stop.Text);
            }
            else
            {
                primative.SetDataType(DataType.GetDataType(ctx.Stop.Type));
            }
        }

        public virtual void defExtent(string text)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Variable extent '{text}'");
            }

            Primative primative = (Primative)currSymbol;
            if (primative == null)
            {
                return;
            }
            try
            {
                primative.SetExtent(int.Parse(text));
            }
            catch (System.FormatException)
            {
                primative.SetExtent(-1);
            }
        }

        public virtual void DefLike(JPNode likeNode)
        {
            LOG.Debug($"Entering defLike {likeNode}");
            Primative likePrim = (Primative)likeNode.Symbol;
            Primative newPrim = (Primative)currSymbol;
            if (likePrim != null)
            {
                newPrim.AssignAttributesLike(likePrim);
                currSymbol.LikeSymbol = likeNode.Symbol;
            }
            else
            {
                LOG.Error("Failed to find LIKE datatype at {likeNode.FileIndex} line {likeNode.Line}");
            }
        }

        public virtual Symbol DefineSymbol(ABLNodeType symbolType, JPNode defNode, JPNode idNode, string name)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Entering defineSymbol {symbolType} - {defNode}");
            }
            /*
             * Some notes: We need to create the Symbol right away, because further actions in the grammar might need to set
             * attributes on it. We can't add it to the scope yet, because of statements like this: def var xyz like xyz. The
             * tree parser is responsible for calling addToScope at the end of the statement or when it is otherwise safe to do
             * so.
             */
            Symbol symbol = SymbolFactory.create(symbolType, name, currentScope);
            currSymbol = symbol;
            currSymbol.DefinitionNode = defNode.IdNode;
            defNode.IdNode.Symbol = symbol;
            return symbol;
        }

        /// <summary>
        /// Called at the start of a DEFINE BROWSE statement. </summary>
        public virtual Browse DefineBrowse(IParseTree defSymbol, JPNode defAST, JPNode idAST, string name)
        {
            LOG.Debug($"Entering defineBrowse {defAST} - {idAST}");
            Browse browse = (Browse)DefineSymbol(ABLNodeType.BROWSE, defAST, idAST, name);
            frameStack.NodeOfDefineBrowse(browse, (JPNode)defAST, defSymbol);
            return browse;
        }

        public virtual Event DefineEvent(JPNode defNode, JPNode idNode, string name)
        {
            LOG.Debug($"Entering defineEvent {defNode} - {idNode}");
            Event @event = new Event(name, currentScope);
            @event.DefinitionNode = defNode.IdNode;
            currSymbol = @event;
            defNode.IdNode.Symbol = @event;

            return @event;
        }

        /// <summary>
        /// Defining a table field is done in two steps. The first step creates the field and field buffer but does not assign
        /// the field to the table yet. The second step assigns the field to the table. We don't want the field assigned to the
        /// table until we're done examining the field options, because we don't want the field available for lookup due to
        /// situations like this: def temp-table tt1 field DependentCare like DependentCare.
        /// </summary>
        /// <returns> The Object that is expected to be passed as an argument to defineTableFieldFinalize. </returns>
        /// <seealso cref= #defineTableFieldFinalize(Object) </seealso>
        public virtual Symbol DefineTableFieldInitialize(JPNode idNode, string text)
        {
            LOG.Debug($"Entering defineTableFieldInitialize {idNode}");
            FieldBuffer fieldBuff = rootScope.DefineTableFieldDelayedAttach(text, currDefTable);
            currSymbol = fieldBuff;
            fieldBuff.DefinitionNode = idNode.FirstChild;
            idNode.FirstChild.Symbol = fieldBuff;
            return fieldBuff;
        }

        public virtual void DefineTableFieldFinalize(object obj)
        {
            LOG.Debug($"Entering defineTableFieldFinalize {obj}");
            ((FieldBuffer)obj).Field.Table = currDefTable.Table;
        }

        private void DefineTableLike(IParseTree ctx)
        {
            // Get table for "LIKE table"
            currDefTableLike = AstTableBufferLink(support.GetNode(ctx));
            currDefTable.LikeSymbol = currDefTableLike;
            if (currDefTableLike != null)
            {
                // For each field in "table", create a field def in currDefTable
                foreach (IField field in currDefTableLike.Table.FieldPosOrder)
                {
                    rootScope.DefineTableField(field.GetName(), currDefTable).AssignAttributesLike(field);
                }
            }
        }
        private void DefineUseIndex(JPNode recNode, JPNode idNode, string name)
        {
            ITable table = AstTableLink(recNode);
            if (table == null)
            {
                return;
            }
            IIndex idx = table.LookupIndex(name);
            if (idx != null)
            {
                // ABL compiler quirk: idNode doesn't have to be a real index. Undefined behavior in this case
                currDefTable.Table.Add(new Index(currDefTable.Table, idx.Name, idx.Unique, idx.Primary));
            }
            else
            {
                // Mark idNode as INVALID_INDEX
                idNode.AttrSet(IConstants.INVALID_USEINDEX, IConstants.TRUE);
            }
            currDefTableUseIndex = true;
        }

        private void DefineIndexInitialize(string name, bool unique, bool primary, bool word)
        {
            currDefIndex = new Index(currDefTable.Table, name, unique, primary);
            currDefTable.Table.Add(currDefIndex);
        }

        private void DefineIndexField(string name)
        {
            IField fld = currDefTable.Table.LookupField(name);
            if (fld != null)
            {
                currDefIndex.AddField(fld);
            }
        }

        private void DefineTable(JPNode defNode, JPNode idNode, string name, int storeType)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> Table definition {defNode} {storeType}");
            }

            TableBuffer buffer = rootScope.DefineTable(name, storeType);
            currSymbol = buffer;
            currSymbol.DefinitionNode = defNode.IdNode;
            currDefTable = buffer;
            currDefTableLike = null;
            currDefTableUseIndex = false;

            defNode.IdNode.Symbol = buffer;
        }

        private void PostDefineTempTable()
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> End of table definition");
            }

            // In case of DEFINE TT LIKE, indexes are copied only if USE-INDEX and INDEX are never used
            if ((currDefTableLike != null) && !currDefTableUseIndex && (currDefTable.Table.Indexes.Count == 0))
            {
                LOG.Debug($"Copying all indexes from {currDefTableLike.Name}");
                foreach (IIndex idx in currDefTableLike.Table.Indexes)
                {
                    Index newIdx = new Index(currDefTable.Table, idx.Name, idx.Unique, idx.Primary);
                    foreach (IField fld in idx.Fields)
                    {
                        IField ifld = newIdx.Table.LookupField(fld.GetName());
                        if (ifld == null)
                        {
                            LOG.Info($"Unable to find field name {fld.GetName()} in table {currDefTable.Table.GetName()}");
                        }
                        else
                        {
                            newIdx.AddField(ifld);
                        }
                    }
                    currDefTable.Table.Add(newIdx);
                }
            }
        }

        private void DefineTempTable(JPNode defAST, JPNode idAST, string name)
        {
            DefineTable(defAST, idAST, name, IConstants.ST_TTABLE);
        }

        /// <summary>
        /// Get the Table symbol linked from a RECORD_NAME AST. </summary>
        private ITable AstTableLink(JPNode tableAST)
        {
            LOG.Debug($"Entering astTableLink {tableAST}");
            TableBuffer buffer = (TableBuffer)tableAST.Symbol;
            return buffer == null ? null : buffer.Table;
        }

        /// <summary>
        /// Get the TableBuffer symbol linked from a RECORD_NAME AST. </summary>
        private TableBuffer AstTableBufferLink(JPNode tableAST)
        {
            return (TableBuffer)tableAST.Symbol;
        }

        /// <summary>
        /// Define a buffer. If the buffer is initialized at the same time it is defined (as in a buffer parameter), then
        /// parameter init should be true.
        /// </summary>
        public virtual TableBuffer DefineBuffer(JPNode defAST, JPNode idNode, string name, JPNode tableAST, bool init)
        {
            LOG.Debug($"Entering defineBuffer {defAST} {tableAST} {init}");
            ITable table = AstTableLink(tableAST.IdNode);
            if (table == null)
            {
                return null;
            }

            TableBuffer bufSymbol = currentScope.DefineBuffer(name, table);
            currSymbol = bufSymbol;
            currSymbol.DefinitionNode = defAST.IdNode;
            defAST.IdNode.Symbol = bufSymbol;
            if (init)
            {
                BufferScope bufScope = currentBlock.GetBufferForReference(bufSymbol);
                defAST.BufferScope = bufScope;
            }
            return bufSymbol;
        }

        /// <summary>
        /// Define an unnamed buffer which is scoped (symbol and buffer) to the trigger scope/block.
        /// </summary>
        public virtual void DefineBufferForTrigger(JPNode tableAST)
        {
            LOG.Debug($"Entering defineBufferForTrigger {tableAST}");
            ITable table = AstTableLink(tableAST);
            TableBuffer bufSymbol = currentScope.DefineBuffer("", table);
            currentBlock.GetBufferForReference(bufSymbol); // Create the BufferScope
            currSymbol = bufSymbol;
        }

        private void defineWorktable(JPNode defAST, JPNode idAST, string name)
        {
            DefineTable(defAST, idAST, name, IConstants.ST_WTABLE);
        }

        public virtual void noteReference(JPNode node, ContextQualifier cq)
        {
            if ((node.Symbol != null) && ((cq == ContextQualifier.UPDATING) || (cq == ContextQualifier.REFUP)))
            {
                node.Symbol.NoteReference(cq);
            }
        }

        public virtual void PropGetSetBegin(JPNode propAST)
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"Entering propGetSetBegin {propAST}");
            }

            ScopeAdd(propAST);
            BlockNode blockNode = (BlockNode)propAST;
            TreeParserSymbolScope definingScope = currentScope.ParentScope;

            Routine r = new Routine(propAST.Text, definingScope, currentScope);
            r.SetProgressType(propAST.NodeType);
            r.DefinitionNode = blockNode;
            blockNode.Symbol = r;
            definingScope.Add(r);
            currentRoutine = r;
        }

        private void PropGetSetEnd()
        {
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"Entering propGetSetEnd");
            }

            ScopeClose();
            currentRoutine = rootRoutine;
        }

        // Called from expressionTerm2 rule, exprtWidName option
        // Check if THIS-OBJECT:SomeAttribute 
        private void Widattr(ExprtWidNameContext ctx, ContextQualifier cq)
        {            
            if ((ctx.widName().systemHandleName() != null) && (ctx.widName().systemHandleName().THISOBJECT() != null) && (ctx.colonAttribute().OBJCOLON().Length != 0)  && (ctx.colonAttribute().OBJCOLON(0) != null))
            {
                FieldLookupResult result = currentBlock.LookupField(ctx.colonAttribute().id.Text, true);
                if (result == null)
                {
                    return;
                }

                // Variable
                if (result.Symbol is Variable)
                {
                    result.Symbol.NoteReference(cq);
                }
            }
        }

        // Called from widattr rule, widattrWidName option
        // Check if THIS-OBJECT:SomeAttribute (same as previous rule)
        private void Widattr(WidattrWidNameContext ctx, ContextQualifier cq)
        {            
            if ((ctx.widName().systemHandleName() != null) && (ctx.widName().systemHandleName().THISOBJECT() != null) && (ctx.colonAttribute().OBJCOLON().Length != 0) && (ctx.colonAttribute().OBJCOLON(0) != null))
            {
                FieldLookupResult result = currentBlock.LookupField(ctx.colonAttribute().id.Text, true);
                if (result == null)
                {
                    return;
                }

                // Variable
                if (result.Symbol is Variable)
                {
                    result.Symbol.NoteReference(cq);
                }
            }
        }

        // Called from expressionTerm rule (expressionTerm2 option) and widattr rule (widattrExprt2 option)
        // Tries to add references to variables/properties of current class
        // Or references to static classes
        private void Widattr(Exprt2FieldContext ctx2, ContextQualifier cq, string right)
        {

            string clsRef = ctx2.field().GetText();
            string clsName = rootScope.ClassName;
            if ((!string.ReferenceEquals(clsRef, null)) && (!string.ReferenceEquals(clsName, null)) && (clsRef.IndexOf('.') == -1) && (clsName.IndexOf('.') != -1))
            {
                clsName = clsName.Substring(clsName.IndexOf('.') + 1);
            }

            if ((!string.ReferenceEquals(clsRef, null)) && (!string.ReferenceEquals(clsName, null)) && clsRef.Equals(clsName, StringComparison.OrdinalIgnoreCase))
            {
                FieldLookupResult result = currentBlock.LookupField(right, true);
                if (result == null)
                {
                    return;
                }

                // Variable
                if (result.Symbol is Variable)
                {
                    result.Symbol.NoteReference(cq);
                }
            }

            if ((ctx2.ENTERED() == null) && !string.IsNullOrEmpty(support.GetClassName(ctx2.field().GetText())))
            {
                SetContextQualifier(ctx2, ContextQualifier.STATIC);
            }
        }

        private void FrameRef(JPNode idAST)
        {
            frameStack.FrameRefNode(idAST, currentScope);
        }

        private void BrowseRef(JPNode idAST)
        {
            LOG.Debug($"Entering browseRef {idAST}");
            frameStack.BrowseRefNode(idAST, currentScope);
        }

        private void BufferRef(string name)
        {
            TableBuffer tableBuffer = currentScope.LookupBuffer(name);
            if (tableBuffer != null)
            {
                tableBuffer.NoteReference(ContextQualifier.SYMBOL);
            }
        }
        private void Field(IParseTree ctx, FieldRefNode refNode, JPNode idNode, string name, ContextQualifier cq, TableNameResolution resolution)
        {
            LOG.Debug($"Entering field {refNode} {idNode} {cq} {resolution}");
            FieldLookupResult result = null;

            refNode.ContextQualifier = cq;

            // Check if this is a Field_ref being "inline defined"
            // If so, we define it right now.
            if (refNode.AttrGet(IConstants.INLINE_VAR_DEF) == 1)
            {
                AddToSymbolScope(DefineVariable(ctx, refNode, name));
            }

            if (cq == ContextQualifier.STATIC)
            {
                // Nothing with static for now, but at least we don't check for external tables
                if (LOG.IsDebugEnabled)
                {
                    LOG.Debug($"Static reference to {refNode.IdNode.Text}");
                }
            }
            else if ((refNode.Parent.NodeType == ABLNodeType.USING && refNode.Parent.Parent.NodeType == ABLNodeType.RECORD_NAME) || (refNode.FirstChild.NodeType == ABLNodeType.INPUT && (refNode.NextSibling == null || refNode.NextSibling.NodeType != ABLNodeType.OBJCOLON)))
            {
                // First condition : there seems to be an implicit INPUT in USING phrases in a record phrase.
                // Second condition :I've seen at least one instance of "INPUT objHandle:attribute" in code,
                // which for some reason compiled clean. As far as I'm aware, the INPUT was
                // meaningless, and the compiler probably should have complained about it.
                // At any rate, the handle:attribute isn't an input field, and we don't want
                // to try to look up the handle using frame field rules.
                // Searching the frames for an existing INPUT field is very different than
                // the usual field/variable lookup rules. It is done based on what is in
                // the referenced FRAME or BROWSE, or what is found in the frames most
                // recently referenced list.
                result = frameStack.InputFieldLookup(refNode, currentScope);
            }
            else if (resolution == TableNameResolution.ANY)
            {
                // Lookup the field, with special handling for FIELDS/USING/EXCEPT phrases
                bool getBufferScope = (cq != ContextQualifier.SYMBOL);
                result = currentBlock.LookupField(name, getBufferScope);
            }
            else
            {
                // If we are in a FIELDS phrase, then we know which table the field is from.
                // The field lookup in Table expects an unqualified name.
                string[] parts = (name ?? "").Split('.');
                string fieldPart = parts.Length > 0 ? parts[parts.Length - 1] : "";
                TableBuffer ourBuffer = resolution == TableNameResolution.PREVIOUS ? prevTableReferenced : lastTableReferenced;
                IField field = ourBuffer.Table.LookupField(fieldPart);
                if (field == null)
                {
                    // The OpenEdge compiler seems to ignore invalid tokens in a FIELDS phrase.
                    // As a result, some questionable code will fail to parse here if we don't also ignore those here.
                    // Sigh. This would be a good lint rule.
                    ABLNodeType parentType = refNode.Parent.NodeType;
                    if (parentType == ABLNodeType.FIELDS || parentType == ABLNodeType.EXCEPT)
                    {
                        return;
                    }
                }
                FieldBuffer fieldBuffer = ourBuffer.GetFieldBuffer(field);
                result = (new FieldLookupResult.Builder()).SetSymbol(fieldBuffer).Build();
            }

            if (result == null)
            {
                return;
            }

            if (result.Unqualified)
            {
                refNode.AttrSet(IConstants.UNQUALIFIED_FIELD, IConstants.TRUE);
            }
            if (result.Abbreviated)
            {
                refNode.AttrSet(IConstants.ABBREVIATED, IConstants.TRUE);
            }

            // Buffer attributes
            if (result.BufferScope != null)
            {
                refNode.BufferScope = result.BufferScope;
            }

            refNode.Symbol = (Symbol)result.Symbol;
            result.Symbol.NoteReference(cq);
            if (result.Symbol is FieldBuffer)
            {
                FieldBuffer fb = (FieldBuffer)result.Symbol;
                refNode.AttrSet(IConstants.STORETYPE, fb.Field.Table.Storetype);
                if (fb.Buffer != null)
                {
                    fb.Buffer.NoteReference(cq);
                }
            }
            else
            {
                refNode.AttrSet(IConstants.STORETYPE, IConstants.ST_VAR);
            }
        }

        public override void EnterEveryRule(ParserRuleContext ctx)
        {
            currentLevel++;
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug($"{Indent()}> {Proparse.ruleNames[ctx.RuleIndex]}");
            }
        }

        public override void ExitEveryRule(ParserRuleContext ctx)
        {
            JPNode n = support.GetNode(ctx);
            if ((n != null) && n.StateHead && !(ctx is FunctionStatementContext))
            {
                EnterNewStatement(ctx, n);
            }
            currentLevel--;
        }

        private string Indent()
        {
            return new string(' ', currentLevel);
        }

        // Attach current statement to the previous one
        private void EnterNewStatement(ParserRuleContext ctx, JPNode node)
        {
            if ((lastStatement != null) && (node != lastStatement))
            {
                lastStatement.NextStatement = node;
                node.PreviousStatement = lastStatement;
            }
            lastStatement = node;
            node.InBlock = currentBlock;
            if (currentBlock.Node.FirstStatement == null)
            {
                currentBlock.Node.FirstStatement = node;
            }
        }
    }

}
