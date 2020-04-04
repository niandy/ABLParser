using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Core;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static ABLParser.Prorefactor.Proparser.Antlr.Proparse;
using Builder = ABLParser.Prorefactor.Core.JPNode.Builder;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class JPNodeVisitor : ProparseBaseVisitor<Builder>
    {
        private readonly ParserSupport support;
        private readonly BufferedTokenStream stream;

        public JPNodeVisitor(ParserSupport support, BufferedTokenStream stream)
        {
            this.support = support;
            this.stream = stream;
        }

        public override Builder VisitProgram(ProgramContext ctx)
        {
            return createTree(ctx, ABLNodeType.PROGRAM_ROOT, ABLNodeType.PROGRAM_TAIL);
        }

        public override Builder VisitCodeBlock(CodeBlockContext ctx)
        {
            support.VisitorEnterScope(ctx.Parent);
            Builder retVal = CreateTree(ctx, ABLNodeType.CODE_BLOCK);
            support.VisitorExitScope(ctx.Parent);

            return retVal;
        }

        public override Builder VisitClassCodeBlock(ClassCodeBlockContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.CODE_BLOCK);
        }

        public override Builder VisitEmptyStatement(EmptyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDotComment(DotCommentContext ctx)
        {
            Builder node = VisitTerminal(ctx.NAMEDOT()).ChangeType(ABLNodeType.DOT_COMMENT).SetStatement().SetRuleNode(ctx);

            ProToken.Builder tok = new ProToken.Builder(node.Token);
            IList<NotStatementEndContext> list = ctx.notStatementEnd();
            for (int zz = 0; zz < list.Count; zz++)
            {
                tok.MergeWith((ProToken)list[zz].Start);
            }
            node.UpdateToken(tok.Build());
            node.SetDown(Visit(ctx.statementEnd()));

            return node;
        }

        public override Builder VisitFunctionCallStatement(FunctionCallStatementContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.EXPR_STATEMENT).SetStatement().SetRuleNode(ctx);
        }

        public override Builder VisitFunctionCallStatementSub(FunctionCallStatementSubContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).ChangeType(ABLNodeType.GetNodeType(support.IsMethodOrFunc(ctx.fname.GetText())));
        }

        public override Builder VisitExpressionStatement(ExpressionStatementContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.EXPR_STATEMENT).SetStatement().SetRuleNode(ctx);
        }

        public override Builder VisitLabeledBlock(LabeledBlockContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBlockFor(BlockForContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBlockOptionIterator(BlockOptionIteratorContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.BLOCK_ITERATOR);
        }

        public override Builder VisitBlockOptionWhile(BlockOptionWhileContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBlockOptionGroupBy(BlockOptionGroupByContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBlockPreselect(BlockPreselectContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitPseudoFunction(PseudoFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitMemoryManagementFunction(MemoryManagementFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBuiltinFunction(BuiltinFunctionContext ctx)
        {
            if (ctx.GetChild(0) is ITerminalNode)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitArgFunction(ArgFunctionContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (holder.NodeType == ABLNodeType.COMPARES)
            {
                holder.ChangeType(ABLNodeType.COMPARE);
            }
            return holder;
        }

        public override Builder VisitOptionalArgFunction(OptionalArgFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRecordFunction(RecordFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitParameterBufferFor(ParameterBufferForContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitParameterBufferRecord(ParameterBufferRecordContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitParameterOther(ParameterOtherContext ctx)
        {
            if (ctx.p == null)
            {
                return CreateTree(ctx, ABLNodeType.INPUT);
            }
            else
            {
                return CreateTreeFromFirstNode(ctx);
            }
        }

        public override Builder VisitParameterList(ParameterListContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.PARAMETER_LIST);
        }

        public override Builder VisitEventList(EventListContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.EVENT_LIST);
        }

        public override Builder VisitAnyOrValueValue(AnyOrValueValueContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitAnyOrValueAny(AnyOrValueAnyContext ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.TYPELESS_TOKEN);
        }

        public override Builder VisitValueExpression(ValueExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        // ----------
        // EXPRESSION
        // ----------

        public override Builder VisitExpressionMinus(ExpressionMinusContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            holder.ChangeType(ABLNodeType.UNARY_MINUS);
            return holder;
        }

        public override Builder VisitExpressionPlus(ExpressionPlusContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            holder.ChangeType(ABLNodeType.UNARY_PLUS);
            return holder;
        }
        public override Builder VisitExpressionOp1(ExpressionOp1Context ctx)
        {
            Builder holder = CreateTreeFromSecondNode(ctx).SetOperator();
            if (holder.NodeType == ABLNodeType.STAR)
            {
                holder.ChangeType(ABLNodeType.MULTIPLY);
            }
            else if (holder.NodeType == ABLNodeType.SLASH)
            {
                holder.ChangeType(ABLNodeType.DIVIDE);
            }
            return holder;
        }

        public override Builder VisitExpressionOp2(ExpressionOp2Context ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitExpressionComparison(ExpressionComparisonContext ctx)
        {
            Builder holder = CreateTreeFromSecondNode(ctx).SetOperator();
            if (holder.NodeType == ABLNodeType.LEFTANGLE)
            {
                holder.ChangeType(ABLNodeType.LTHAN);
            }
            else if (holder.NodeType == ABLNodeType.LTOREQUAL)
            {
                holder.ChangeType(ABLNodeType.LE);
            }
            else if (holder.NodeType == ABLNodeType.RIGHTANGLE)
            {
                holder.ChangeType(ABLNodeType.GTHAN);
            }
            else if (holder.NodeType == ABLNodeType.GTOREQUAL)
            {
                holder.ChangeType(ABLNodeType.GE);
            }
            else if (holder.NodeType == ABLNodeType.GTORLT)
            {
                holder.ChangeType(ABLNodeType.NE);
            }
            else if (holder.NodeType == ABLNodeType.EQUAL)
            {
                holder.ChangeType(ABLNodeType.EQ);
            }

            return holder;
        }

        public override Builder VisitExpressionStringComparison(ExpressionStringComparisonContext ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitExpressionNot(ExpressionNotContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExpressionAnd(ExpressionAndContext ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitExpressionOr(ExpressionOrContext ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        // ---------------
        // EXPRESSION BITS
        // ---------------

        public override Builder VisitExprtNoReturnValue(ExprtNoReturnValueContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.WIDGET_REF);
        }

        public override Builder VisitExprtWidName(ExprtWidNameContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.WIDGET_REF);
        }

        public override Builder VisitExprtExprt2(ExprtExprt2Context ctx)
        {
            if (ctx.colonAttribute() != null)
            {
                return CreateTree(ctx, ABLNodeType.WIDGET_REF);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitExprt2ParenExpr(Exprt2ParenExprContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExprt2ParenCall(Exprt2ParenCallContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            holder.ChangeType(ABLNodeType.GetNodeType(support.IsMethodOrFunc(ctx.fname.GetText())));
            return holder;
        }

        public override Builder VisitExprt2New(Exprt2NewContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExprt2ParenCall2(Exprt2ParenCall2Context ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            holder.ChangeType(ABLNodeType.LOCAL_METHOD_REF);
            return holder;
        }
        public override Builder VisitExprt2Field(Exprt2FieldContext ctx)
        {
            if (ctx.ENTERED() != null)
            {
                return CreateTree(ctx, ABLNodeType.ENTERED_FUNC);
            }
            else
            {
                return VisitChildren(ctx);
            }
        }

        public override Builder VisitWidattrWidName(WidattrWidNameContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.WIDGET_REF);
        }

        public override Builder VisitWidattrExprt2(WidattrExprt2Context ctx)
        {
            return CreateTree(ctx, ABLNodeType.WIDGET_REF);
        }

        public override Builder VisitGWidget(GWidgetContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.WIDGET_REF);
        }

        public override Builder VisitFiln(FilnContext ctx)
        {
            return VisitChildren(ctx);
        }

        public override Builder VisitFieldn(FieldnContext ctx)
        {
            return VisitChildren(ctx);
        }

        public override Builder VisitField(FieldContext ctx)
        {
            Builder holder = CreateTree(ctx, ABLNodeType.FIELD_REF).SetRuleNode(ctx);
            if ((ctx.Parent is MessageOptionContext) && support.IsInlineVar(ctx.GetText()))
            {
                holder.SetInlineVar();
            }
            return holder;
        }

        public override Builder VisitFieldFrameOrBrowse(FieldFrameOrBrowseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitArraySubscript(ArraySubscriptContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.ARRAY_SUBSCRIPT);
        }

        public override Builder VisitMethodParamList(MethodParamListContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.METHOD_PARAM_LIST);
        }

        public override Builder VisitInuic(InuicContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitRecordAsFormItem(RecordAsFormItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }


        public override Builder VisitRecord(RecordContext ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.RECORD_NAME).SetStoreType(support.GetRecordExpression(ctx)).SetRuleNode(ctx);
        }

        public override Builder VisitBlockLabel(BlockLabelContext ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.BLOCK_LABEL);
        }

        public override Builder VisitIdentifierUKW(IdentifierUKWContext ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.ID);
        }

        public override Builder VisitNewIdentifier(NewIdentifierContext ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.ID);
        }


        public override Builder VisitFilename(FilenameContext ctx)
        {
            ProToken.Builder tok = (new ProToken.Builder((ProToken)ctx.t1.Start)).SetType(ABLNodeType.FILENAME);
            for (int zz = 1; zz < ctx.filenamePart().Length; zz++)
            {
                tok.MergeWith((ProToken)ctx.filenamePart(zz).Start);
            }

            return (new Builder(tok.Build())).SetRuleNode(ctx);
        }

        public override Builder VisitTypeName(TypeNameContext ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.TYPE_NAME).SetRuleNode(ctx).SetClassname(support.LookupClassName(ctx.nonPunctuating().GetText()));
        }

        public override Builder VisitTypeName2(TypeName2Context ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.TYPE_NAME).SetRuleNode(ctx);
        }

        public override Builder VisitWidName(WidNameContext ctx)
        {
            return VisitChildren(ctx).SetRuleNode(ctx);
        }

        // **********
        // Statements
        // **********

        public override Builder VisitAaTraceCloseStatement(AaTraceCloseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.CLOSE);
        }

        public override Builder VisitAaTraceOnOffStatement(AaTraceOnOffStatementContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (ctx.OFF() != null)
            {
                holder.SetStatement(ABLNodeType.OFF);
            }
            else
            {
                holder.SetStatement(ABLNodeType.ON);
            }
            return holder;
        }

        public override Builder VisitAaTraceStatement(AaTraceStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitAccumulateStatement(AccumulateStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitAggregatePhrase(AggregatePhraseContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.AGGREGATE_PHRASE);
        }

        public override Builder VisitAggregateOption(AggregateOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitAllExceptFields(AllExceptFieldsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitAnalyzeStatement(AnalyzeStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitAnnotation(AnnotationContext ctx)
        {
            Builder node = VisitTerminal(ctx.ANNOTATION()).SetStatement().SetRuleNode(ctx);

            IList<NotStatementEndContext> list = ctx.notStatementEnd();
            if (list.Count > 0)
            {
                ProToken.Builder tok = (new ProToken.Builder((ProToken)list[0].Start)).SetType(ABLNodeType.UNQUOTEDSTRING);
                for (int zz = 1; zz < list.Count; zz++)
                {
                    tok.MergeWith(Visit(list[zz]).Token);
                }

                Builder ch = new Builder(tok.Build());
                node.SetDown(ch);
                ch.SetRight(Visit(ctx.statementEnd()));
            }
            else
            {
                node.SetDown(Visit(ctx.statementEnd()));
            }

            return node;
        }

        public override Builder VisitApplyStatement(ApplyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitApplyStatementSub(ApplyStatementSubContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitAssignOption(AssignOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitAssignOptionSub(AssignOptionSubContext ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitAssignStatement(AssignStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitAssignStatement2(AssignStatement2Context ctx)
        {
            // Unset rule node, as it's in fact assigned to the parent node
            Builder node1 = CreateTreeFromSecondNode(ctx).SetOperator().UnsetRuleNode();

            Builder holder = (new Builder(ABLNodeType.ASSIGN)).SetStatement().SetDown(node1).SetRuleNode(ctx);
            Builder lastNode = node1;
            for (int zz = 3; zz < ctx.ChildCount; zz++)
            {
                lastNode = lastNode.SetRight(Visit(ctx.GetChild(zz))).Last;
            }

            return holder;
        }

        public override Builder VisitAssignEqual(AssignEqualContext ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitAssignField(AssignFieldContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.ASSIGN_FROM_BUFFER);
        }

        public override Builder VisitAtExpression(AtExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitAtPhrase(AtPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitAtPhraseSub(AtPhraseSubContext ctx)
        {
            Builder builder = CreateTreeFromFirstNode(ctx);
            if (builder.NodeType == ABLNodeType.COLUMNS)
            {
                builder.ChangeType(ABLNodeType.COLUMN);
            }
            else if (builder.NodeType == ABLNodeType.COLOF)
            {
                builder.ChangeType(ABLNodeType.COLUMNOF);
            }

            return builder;
        }

        public override Builder VisitBellStatement(BellStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitBlockLevelStatement(BlockLevelStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitBufferCompareStatement(BufferCompareStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitBufferCompareSave(BufferCompareSaveContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBufferCompareResult(BufferCompareResultContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBufferComparesBlock(BufferComparesBlockContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.CODE_BLOCK);
        }

        public override Builder VisitBufferCompareWhen(BufferCompareWhenContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitBufferComparesEnd(BufferComparesEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitBufferCopyStatement(BufferCopyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitBufferCopyAssign(BufferCopyAssignContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitByExpr(ByExprContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCacheExpr(CacheExprContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCallStatement(CallStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitCasesensNot(CasesensNotContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.NOT_CASESENS);
        }

        public override Builder VisitCaseStatement(CaseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitCaseBlock(CaseBlockContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.CODE_BLOCK);
        }

        public override Builder VisitCaseWhen(CaseWhenContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCaseExpression1(CaseExpression1Context ctx)
        {
            return VisitChildren(ctx);
        }

        public override Builder VisitCaseExpression2(CaseExpression2Context ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitCaseExprTerm(CaseExprTermContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCaseOtherwise(CaseOtherwiseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCaseEnd(CaseEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCatchStatement(CatchStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitCatchEnd(CatchEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitChooseStatement(ChooseStatementContext ctx)
        {
            Builder node = CreateStatementTreeFromFirstNode(ctx);
            if (node.Down.NodeType == ABLNodeType.FIELDS)
            {
                node.Down.ChangeType(ABLNodeType.FIELD);
            }
            return node;
        }

        public override Builder VisitChooseField(ChooseFieldContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitChooseOption(ChooseOptionContext ctx)
        {
            if (ctx.KEYS() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            else
            {
                return VisitChildren(ctx);
            }
        }
        public override Builder VisitEnumStatement(EnumStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDefEnumStatement(DefEnumStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.ENUM);
        }

        public override Builder VisitEnumEnd(EnumEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitClassStatement(ClassStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitClassInherits(ClassInheritsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitClassImplements(ClassImplementsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitClassEnd(ClassEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitClearStatement(ClearStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitCloseQueryStatement(CloseQueryStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.QUERY);
        }

        public override Builder VisitCloseStoredProcedureStatement(CloseStoredProcedureStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.STOREDPROCEDURE);
        }

        public override Builder VisitCloseStoredWhere(CloseStoredWhereContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCollatePhrase(CollatePhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitColorAnyOrValue(ColorAnyOrValueContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitColorExpression(ColorExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitColorSpecification(ColorSpecificationContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitColorDisplay(ColorDisplayContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitColorPrompt(ColorPromptContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (holder.NodeType == ABLNodeType.PROMPTFOR)
            {
                holder.ChangeType(ABLNodeType.PROMPT);
            }
            return holder;
        }

        public override Builder VisitColorStatement(ColorStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitColumnExpression(ColumnExpressionContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (holder.NodeType == ABLNodeType.COLUMNS)
            {
                holder.ChangeType(ABLNodeType.COLUMN);
            }
            return holder;
        }
        public override Builder VisitColumnFormat(ColumnFormatContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORMAT_PHRASE);
        }

        public override Builder VisitColumnFormatOption(ColumnFormatOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitComboBoxPhrase(ComboBoxPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitComboBoxOption(ComboBoxOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCompileStatement(CompileStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitCompileOption(CompileOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCompileLang(CompileLangContext ctx)
        {
            return VisitChildren(ctx);
        }

        public override Builder VisitCompileLang2(CompileLang2Context ctx)
        {
            return VisitChildren(ctx).ChangeType(ABLNodeType.TYPELESS_TOKEN);
        }

        public override Builder VisitCompileInto(CompileIntoContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCompileEqual(CompileEqualContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCompileAppend(CompileAppendContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCompilePage(CompilePageContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitConnectStatement(ConnectStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitConstructorStatement(ConstructorStatementContext ctx)
        {
            Builder holder = CreateStatementTreeFromFirstNode(ctx);
            Builder typeName = holder.Down;
            if (typeName.NodeType != ABLNodeType.TYPE_NAME)
            {
                typeName = typeName.Right;
            }
            if (typeName.NodeType == ABLNodeType.TYPE_NAME)
            {
                typeName.SetClassname(support.ClassName);
            }
            return holder;
        }

        public override Builder VisitConstructorEnd(ConstructorEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitContextHelpIdExpression(ContextHelpIdExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitConvertPhrase(ConvertPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCopyLobStatement(CopyLobStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }
        public override Builder VisitCopyLobFor(CopyLobForContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCopyLobStarting(CopyLobStartingContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitForTenant(ForTenantContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCreateStatement(CreateStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitCreateWhateverStatement(CreateWhateverStatementContext ctx)
        {
            Builder holder = CreateStatementTreeFromFirstNode(ctx);
            holder.SetStatement(holder.Down.NodeType);
            return holder;
        }

        public override Builder VisitCreateAliasStatement(CreateAliasStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.ALIAS);
        }

        public override Builder VisitCreateConnect(CreateConnectContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCreateBrowseStatement(CreateBrowseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.BROWSE);
        }

        public override Builder VisitCreateQueryStatement(CreateQueryStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.QUERY);
        }

        public override Builder VisitCreateBufferStatement(CreateBufferStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.BUFFER);
        }

        public override Builder VisitCreateBufferName(CreateBufferNameContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCreateDatabaseStatement(CreateDatabaseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.DATABASE);
        }

        public override Builder VisitCreateDatabaseFrom(CreateDatabaseFromContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitCreateServerStatement(CreateServerStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SERVER);
        }

        public override Builder VisitCreateServerSocketStatement(CreateServerSocketStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SERVERSOCKET);
        }

        public override Builder VisitCreateSocketStatement(CreateSocketStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SOCKET);
        }

        public override Builder VisitCreateTempTableStatement(CreateTempTableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.TEMPTABLE);
        }

        public override Builder VisitCreateWidgetStatement(CreateWidgetStatementContext ctx)
        {
            if (ctx.createConnect() == null)
            {
                return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.WIDGET);
            }
            else
            {
                return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.AUTOMATION_OBJECT);
            }
        }
        public override Builder VisitCreateWidgetPoolStatement(CreateWidgetPoolStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.WIDGETPOOL);
        }

        public override Builder VisitCanFindFunction(CanFindFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitCurrentValueFunction(CurrentValueFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDatatypeDll(DatatypeDllContext ctx)
        {
            Builder node = VisitChildren(ctx);
            if ((ctx.id != null) && (support.AbbrevDatatype(ctx.id.Text) == CHARACTER))
            {
                node.ChangeType(ABLNodeType.CHARACTER);
            }

            return node;
        }

        public override Builder VisitDatatypeVar(DatatypeVarContext ctx)
        {
            Builder builder = VisitChildren(ctx);
            if (builder.NodeType == ABLNodeType.IN)
            {
                builder.ChangeType(ABLNodeType.INTEGER);
            }
            else if (builder.NodeType == ABLNodeType.LOG)
            {
                builder.ChangeType(ABLNodeType.LOGICAL);
            }
            else if (builder.NodeType == ABLNodeType.ROW)
            {
                builder.ChangeType(ABLNodeType.ROWID);
            }
            else if (builder.NodeType == ABLNodeType.WIDGET)
            {
                builder.ChangeType(ABLNodeType.WIDGETHANDLE);
            }
            else if (ctx.id != null)
            {
                builder.ChangeType(ABLNodeType.GetNodeType(support.AbbrevDatatype(ctx.id.Text)));
            }

            return builder.SetRuleNode(ctx);
        }

        public override Builder VisitDdeAdviseStatement(DdeAdviseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.ADVISE);
        }

        public override Builder VisitDdeExecuteStatement(DdeExecuteStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.EXECUTE);
        }

        public override Builder VisitDdeGetStatement(DdeGetStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.GET);
        }

        public override Builder VisitDdeInitiateStatement(DdeInitiateStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.INITIATE);
        }

        public override Builder VisitDdeRequestStatement(DdeRequestStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.REQUEST);
        }

        public override Builder VisitDdeSendStatement(DdeSendStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SEND);
        }

        public override Builder VisitDdeTerminateStatement(DdeTerminateStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.TERMINATE);
        }

        public override Builder VisitDecimalsExpr(DecimalsExprContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDefaultExpr(DefaultExprContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDefineBrowseStatement(DefineBrowseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.BROWSE);
        }

        public override Builder VisitDefineBufferStatement(DefineBufferStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.BUFFER);
        }
        public override Builder VisitDefineDatasetStatement(DefineDatasetStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.DATASET);
        }

        public override Builder VisitDefineDataSourceStatement(DefineDataSourceStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.DATASOURCE);
        }

        public override Builder VisitDefineEventStatement(DefineEventStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.EVENT);
        }

        public override Builder VisitDefineFrameStatement(DefineFrameStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.FRAME);
        }

        public override Builder VisitDefineImageStatement(DefineImageStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.IMAGE);
        }

        public override Builder VisitDefineMenuStatement(DefineMenuStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.MENU);
        }

        public override Builder VisitDefineParameterStatement(DefineParameterStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.PARAMETER);
        }

        public override Builder VisitDefineParamVar(DefineParamVarContext ctx)
        {
            Builder retVal = VisitChildren(ctx).MoveRightToDown();
            if (retVal.Down.NodeType == ABLNodeType.CLASS)
            {
                retVal.MoveRightToDown();
            }

            return retVal;
        }

        public override Builder VisitDefinePropertyStatement(DefinePropertyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.PROPERTY);
        }

        public override Builder VisitDefinePropertyAccessorGetBlock(DefinePropertyAccessorGetBlockContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.PROPERTY_GETTER).SetRuleNode(ctx);
        }

        public override Builder VisitDefinePropertyAccessorSetBlock(DefinePropertyAccessorSetBlockContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.PROPERTY_SETTER).SetRuleNode(ctx);
        }

        public override Builder VisitDefineQueryStatement(DefineQueryStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.QUERY);
        }

        public override Builder VisitDefineRectangleStatement(DefineRectangleStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.RECTANGLE);
        }

        public override Builder VisitDefineStreamStatement(DefineStreamStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.STREAM);
        }

        public override Builder VisitDefineSubMenuStatement(DefineSubMenuStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SUBMENU);
        }

        public override Builder VisitDefineTempTableStatement(DefineTempTableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.TEMPTABLE);
        }

        public override Builder VisitDefineWorkTableStatement(DefineWorkTableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.WORKTABLE);
        }

        public override Builder VisitDefineVariableStatement(DefineVariableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.VARIABLE);
        }

        public override Builder VisitDefineShare(DefineShareContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitDefBrowseDisplay(DefBrowseDisplayContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDefBrowseDisplayItem(DefBrowseDisplayItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitDefBrowseEnable(DefBrowseEnableContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDefBrowseEnableItem(DefBrowseEnableItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitDefineButtonStatement(DefineButtonStatementContext ctx)
        {
            Builder builder = CreateStatementTreeFromFirstNode(ctx, ABLNodeType.BUTTON);
            if (builder.Down.NodeType == ABLNodeType.BUTTONS)
            {
                builder.Down.ChangeType(ABLNodeType.BUTTON);
            }
            return builder;
        }

        public override Builder VisitButtonOption(ButtonOptionContext ctx)
        {
            if ((ctx.IMAGEDOWN() != null) || (ctx.IMAGE() != null) || (ctx.IMAGEUP() != null) || (ctx.IMAGEINSENSITIVE() != null) || (ctx.MOUSEPOINTER() != null) || (ctx.NOFOCUS() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitDataRelation(DataRelationContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitParentIdRelation(ParentIdRelationContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFieldMappingPhrase(FieldMappingPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDataRelationNested(DataRelationNestedContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitEventSignature(EventSignatureContext ctx)
        {
            if (ctx.SIGNATURE() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            else
            {
                return CreateTree(ctx, ABLNodeType.SIGNATURE);
            }
        }

        public override Builder VisitEventDelegate(EventDelegateContext ctx)
        {
            if (ctx.DELEGATE() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            else
            {
                return CreateTree(ctx, ABLNodeType.DELEGATE);
            }
        }

        public override Builder VisitDefineImageOption(DefineImageOptionContext ctx)
        {
            if (ctx.STRETCHTOFIT() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            else
            {
                return VisitChildren(ctx);
            }
        }

        public override Builder VisitMenuListItem(MenuListItemContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitMenuItemOption(MenuItemOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRectangleOption(RectangleOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitDefTableBeforeTable(DefTableBeforeTableContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitDefTableLike(DefTableLikeContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDefTableUseIndex(DefTableUseIndexContext ctx)
        {
            Builder builder = CreateTreeFromFirstNode(ctx);
            builder.Down.SetRuleNode(ctx.identifier());

            return builder;
        }

        public override Builder VisitDefTableField(DefTableFieldContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
            if (holder.NodeType == ABLNodeType.FIELDS)
            {
                holder.ChangeType(ABLNodeType.FIELD);
            }
            return holder;
        }

        public override Builder VisitDefTableIndex(DefTableIndexContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDeleteStatement(DeleteStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDeleteAliasStatement(DeleteAliasStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.ALIAS);
        }

        public override Builder VisitDeleteObjectStatement(DeleteObjectStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.OBJECT);
        }

        public override Builder VisitDeleteProcedureStatement(DeleteProcedureStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.PROCEDURE);
        }

        public override Builder VisitDeleteWidgetStatement(DeleteWidgetStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.WIDGET);
        }

        public override Builder VisitDeleteWidgetPoolStatement(DeleteWidgetPoolStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.WIDGETPOOL);
        }

        public override Builder VisitDelimiterConstant(DelimiterConstantContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDestructorStatement(DestructorStatementContext ctx)
        {
            Builder holder = CreateStatementTreeFromFirstNode(ctx);
            Builder typeName = holder.Down;
            if (typeName.NodeType != ABLNodeType.TYPE_NAME)
            {
                typeName = typeName.Right;
            }
            if (typeName.NodeType == ABLNodeType.TYPE_NAME)
            {
                typeName.SetClassname(support.ClassName);
            }

            return holder;
        }

        public override Builder VisitDestructorEnd(DestructorEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDictionaryStatement(DictionaryStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDisableStatement(DisableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDisableTriggersStatement(DisableTriggersStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.TRIGGERS);
        }
        public override Builder VisitDisconnectStatement(DisconnectStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDisplayStatement(DisplayStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDisplayItem(DisplayItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitDisplayWith(DisplayWithContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDoStatement(DoStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDownStatement(DownStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitDynamicCurrentValueFunction(DynamicCurrentValueFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitDynamicNewStatement(DynamicNewStatementContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.ASSIGN_DYNAMIC_NEW).SetStatement();
        }

        public override Builder VisitDynamicPropertyFunction(DynamicPropertyFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFieldEqualDynamicNew(FieldEqualDynamicNewContext ctx)
        {
            return CreateTreeFromSecondNode(ctx).SetOperator();
        }

        public override Builder VisitDynamicNew(DynamicNewContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitEditorPhrase(EditorPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitEditorOption(EditorOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitEmptyTempTableStatement(EmptyTempTableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitEnableStatement(EnableStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitEditingPhrase(EditingPhraseContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.EDITING_PHRASE);
        }

        public override Builder VisitEntryFunction(EntryFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExceptFields(ExceptFieldsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExceptUsingFields(ExceptUsingFieldsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExportStatement(ExportStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }
        public override Builder VisitExtentPhrase(ExtentPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitExtentPhrase2(ExtentPhrase2Context ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFieldFormItem(FieldFormItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitFieldList(FieldListContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FIELD_LIST);
        }

        public override Builder VisitFieldsFields(FieldsFieldsContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (holder.NodeType == ABLNodeType.FIELD)
            {
                holder.ChangeType(ABLNodeType.FIELDS);
            }
            return holder;
        }

        public override Builder VisitFieldOption(FieldOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFillInPhrase(FillInPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFinallyStatement(FinallyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitFinallyEnd(FinallyEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFindStatement(FindStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitFontExpression(FontExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitForStatement(ForStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitFormatExpression(FormatExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFormItem(FormItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitFormStatement(FormStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitFormatPhrase(FormatPhraseContext ctx)
        {
            // TODO Add IConstants.INLINE_VAR_DEF to JPNode objects when in 'AS datatypeVar' or 'LIKE field' cases
            return CreateTree(ctx, ABLNodeType.FORMAT_PHRASE);
        }

        public override Builder VisitFormatOption(FormatOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFrameWidgetName(FrameWidgetNameContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitFramePhrase(FramePhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitFrameExpressionCol(FrameExpressionColContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.WITH_COLUMNS);
        }

        public override Builder VisitFrameExpressionDown(FrameExpressionDownContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.WITH_DOWN);
        }

        public override Builder VisitBrowseOption(BrowseOptionContext ctx)
        {
            if (ctx.DOWN() != null)
            {
                return VisitChildren(ctx);
            }
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFrameOption(FrameOptionContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (holder.NodeType == ABLNodeType.COLUMNS)
            {
                holder.ChangeType(ABLNodeType.COLUMN);
            }
            return holder;
        }

        public override Builder VisitFrameViewAs(FrameViewAsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFrameViewAsOption(FrameViewAsOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFromPos(FromPosContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFunctionStatement(FunctionStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitExternalFunctionStatement(ExternalFunctionStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitFunctionEnd(FunctionEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitFunctionParams(FunctionParamsContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.PARAMETER_LIST);
        }

        public override Builder VisitFunctionParamBufferFor(FunctionParamBufferForContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitFunctionParamStandard(FunctionParamStandardContext ctx)
        {
            if (ctx.qualif == null)
            {
                return CreateTree(ctx, ABLNodeType.INPUT);
            }
            else
            {
                return CreateTreeFromFirstNode(ctx);
            }
        }

        public override Builder VisitFunctionParamStandardAs(FunctionParamStandardAsContext ctx)
        {
            return VisitChildren(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitFunctionParamStandardLike(FunctionParamStandardLikeContext ctx)
        {
            return VisitChildren(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitFunctionParamStandardTableHandle(FunctionParamStandardTableHandleContext ctx)
        {
            return VisitChildren(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitFunctionParamStandardDatasetHandle(FunctionParamStandardDatasetHandleContext ctx)
        {
            return VisitChildren(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitGetStatement(GetStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }
        public override Builder VisitGetKeyValueStatement(GetKeyValueStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitGoOnPhrase(GoOnPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitHeaderBackground(HeaderBackgroundContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitHelpConstant(HelpConstantContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitHideStatement(HideStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitIfStatement(IfStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitIfElse(IfElseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitInExpression(InExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitInWindowExpression(InWindowExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitImagePhraseOption(ImagePhraseOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitImportStatement(ImportStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitInWidgetPoolExpression(InWidgetPoolExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitInitialConstant(InitialConstantContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitInputClearStatement(InputClearStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.CLEAR);
        }

        public override Builder VisitInputCloseStatement(InputCloseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.CLOSE);
        }

        public override Builder VisitInputFromStatement(InputFromStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.FROM);
        }

        public override Builder VisitInputThroughStatement(InputThroughStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.THROUGH);
        }

        public override Builder VisitInputOutputCloseStatement(InputOutputCloseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.CLOSE);
        }

        public override Builder VisitInputOutputThroughStatement(InputOutputThroughStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.THROUGH);
        }

        public override Builder VisitInsertStatement(InsertStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitInterfaceStatement(InterfaceStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitInterfaceInherits(InterfaceInheritsContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitInterfaceEnd(InterfaceEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitIoPhraseAnyTokensSub3(IoPhraseAnyTokensSub3Context ctx)
        {
            Builder node = VisitChildren(ctx.fname1).ChangeType(ABLNodeType.FILENAME);

            ProToken.Builder tok = new ProToken.Builder(node.Token);
            IList<NotIoOptionContext> list = ctx.notIoOption();
            for (int zz = 0; zz < list.Count; zz++)
            {
                tok.MergeWith(Visit(list[zz]).Token);
            }
            node.UpdateToken(tok.Build());
            for (int zz = 0; zz < ctx.ioOption().Length; zz++)
            {
                node.Last.SetRight(Visit(ctx.ioOption(zz)));
            }
            node.Last.SetRight(Visit(ctx.statementEnd()));

            return node;
        }

        public override Builder VisitIoOption(IoOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitIoOsDir(IoOsDirContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitIoPrinter(IoPrinterContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLabelConstant(LabelConstantContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLdbnameFunction(LdbnameFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLdbnameOption(LdbnameOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLeaveStatement(LeaveStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitLengthFunction(LengthFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLikeField(LikeFieldContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLikeWidgetName(LikeWidgetNameContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitLoadStatement(LoadStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitLoadOption(LoadOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitMessageStatement(MessageStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }
        public override Builder VisitMessageItem(MessageItemContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.FORM_ITEM).SetRuleNode(ctx);
        }

        public override Builder VisitMessageOption(MessageOptionContext ctx)
        {
            Builder builder = CreateTreeFromFirstNode(ctx);
            Builder tmp = builder.Down;
            while (tmp != null)
            {
                if (tmp.NodeType == ABLNodeType.BUTTON)
                {
                    tmp.ChangeType(ABLNodeType.BUTTONS);
                }
                tmp = tmp.Right;
            }
            return builder;
        }

        public override Builder VisitMethodStatement(MethodStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitMethodEnd(MethodEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitNamespacePrefix(NamespacePrefixContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitNamespaceUri(NamespaceUriContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitNextStatement(NextStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitNextPromptStatement(NextPromptStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitNextValueFunction(NextValueFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitNullPhrase(NullPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitOnStatement(OnStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOnAssign(OnAssignContext ctx)
        {
            return VisitChildren(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitOnstateRunParams(OnstateRunParamsContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.PARAMETER_LIST);
        }

        public override Builder VisitOnPhrase(OnPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitOnUndo(OnUndoContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitOnAction(OnActionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitOpenQueryStatement(OpenQueryStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.QUERY);
        }

        public override Builder VisitOpenQueryOption(OpenQueryOptionContext ctx)
        {
            if (ctx.MAXROWS() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }
        public override Builder VisitOsAppendStatement(OsAppendStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOsCommandStatement(OsCommandStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOsCopyStatement(OsCopyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOsCreateDirStatement(OsCreateDirStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOsDeleteStatement(OsDeleteStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOsRenameStatement(OsRenameStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitOutputCloseStatement(OutputCloseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.CLOSE);
        }

        public override Builder VisitOutputThroughStatement(OutputThroughStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.THROUGH);
        }

        public override Builder VisitOutputToStatement(OutputToStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.TO);
        }

        public override Builder VisitPageStatement(PageStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitPauseExpression(PauseExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitPauseStatement(PauseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitPauseOption(PauseOptionContext ctx)
        {
            if (ctx.MESSAGE() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitProcedureExpression(ProcedureExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitProcedureStatement(ProcedureStatementContext ctx)
        {
            Builder holder = CreateStatementTreeFromFirstNode(ctx);
            holder.Down.ChangeType(ABLNodeType.ID);
            return holder;
        }

        public override Builder VisitExternalProcedureStatement(ExternalProcedureStatementContext ctx)
        {
            Builder holder = CreateStatementTreeFromFirstNode(ctx);
            holder.Down.ChangeType(ABLNodeType.ID);
            holder.Down.Right.MoveRightToDown();

            return holder;
        }

        public override Builder VisitProcedureOption(ProcedureOptionContext ctx)
        {
            if (ctx.EXTERNAL() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }
        public override Builder VisitProcedureDllOption(ProcedureDllOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitProcedureEnd(ProcedureEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitProcessEventsStatement(ProcessEventsStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitPromptForStatement(PromptForStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx).ChangeType(ABLNodeType.PROMPTFOR);
        }

        public override Builder VisitPublishStatement(PublishStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitPublishOption(PublishOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitPutStatement(PutStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitPutCursorStatement(PutCursorStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.CURSOR);
        }

        public override Builder VisitPutScreenStatement(PutScreenStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SCREEN);
        }

        public override Builder VisitPutKeyValueStatement(PutKeyValueStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitQueryName(QueryNameContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitQueryTuningPhrase(QueryTuningPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitQueryTuningOption(QueryTuningOptionContext ctx)
        {
            if ((ctx.CACHESIZE() != null) || (ctx.DEBUG() != null) || (ctx.HINT() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitQuitStatement(QuitStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRadiosetPhrase(RadiosetPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRadiosetOption(RadiosetOptionContext ctx)
        {
            if (ctx.RADIOBUTTONS() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            else
            {
                return VisitChildren(ctx);
            }
        }

        public override Builder VisitRadioLabel(RadioLabelContext ctx)
        {
            Builder holder = VisitChildren(ctx);
            if (holder.NodeType != ABLNodeType.QSTRING)
            {
                holder.ChangeType(ABLNodeType.UNQUOTEDSTRING);
            }
            return holder;
        }

        public override Builder VisitRawFunction(RawFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitRawTransferStatement(RawTransferStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitReadkeyStatement(ReadkeyStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRepeatStatement(RepeatStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRecordFields(RecordFieldsContext ctx)
        {
            Builder holder = CreateTreeFromFirstNode(ctx);
            if (holder.NodeType == ABLNodeType.FIELD)
            {
                holder.ChangeType(ABLNodeType.FIELDS);
            }
            return holder;
        }

        public override Builder VisitRecordphrase(RecordphraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRecordOption(RecordOptionContext ctx)
        {
            if ((ctx.LEFT() != null) || (ctx.OF() != null) || (ctx.WHERE() != null) || (ctx.USEINDEX() != null) || (ctx.USING() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitReleaseStatement(ReleaseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitReleaseExternalStatement(ReleaseExternalStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.EXTERNAL);
        }

        public override Builder VisitReleaseObjectStatement(ReleaseObjectStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.OBJECT);
        }

        public override Builder VisitRepositionStatement(RepositionStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRepositionOption(RepositionOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitReturnStatement(ReturnStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRoutineLevelStatement(RoutineLevelStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRowExpression(RowExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunStatement(RunStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunOptPersistent(RunOptPersistentContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunOptSingleRun(RunOptSingleRunContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunOptSingleton(RunOptSingletonContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunOptServer(RunOptServerContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitRunOptAsync(RunOptAsyncContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunEvent(RunEventContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunSet(RunSetContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitRunStoredProcedureStatement(RunStoredProcedureStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.STOREDPROCEDURE);
        }

        public override Builder VisitRunSuperStatement(RunSuperStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.SUPER);
        }

        public override Builder VisitSaveCacheStatement(SaveCacheStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitScrollStatement(ScrollStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitSeekStatement(SeekStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitSelectionlistphrase(SelectionlistphraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSelectionListOption(SelectionListOptionContext ctx)
        {
            if ((ctx.LISTITEMS() != null) || (ctx.LISTITEMPAIRS() != null) || (ctx.INNERCHARS() != null) || (ctx.INNERLINES() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitSerializeName(SerializeNameContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSetStatement(SetStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitShowStatsStatement(ShowStatsStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitSizePhrase(SizePhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSkipPhrase(SkipPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSliderPhrase(SliderPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSliderOption(SliderOptionContext ctx)
        {
            if ((ctx.MAXVALUE() != null) || (ctx.MINVALUE() != null) || (ctx.TICMARKS() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitSliderFrequency(SliderFrequencyContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSpacePhrase(SpacePhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }
        public override Builder VisitStatusStatement(StatusStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitStatusOption(StatusOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitStopAfter(StopAfterContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitStopStatement(StopStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitStreamNameOrHandle(StreamNameOrHandleContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSubscribeStatement(SubscribeStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitSubscribeRun(SubscribeRunContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSubstringFunction(SubstringFunctionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSystemDialogColorStatement(SystemDialogColorStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.COLOR);
        }

        public override Builder VisitSystemDialogFontStatement(SystemDialogFontStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.FONT);
        }

        public override Builder VisitSystemDialogFontOption(SystemDialogFontOptionContext ctx)
        {
            if ((ctx.MAXSIZE() != null) || (ctx.MINSIZE() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitSystemDialogGetDirStatement(SystemDialogGetDirStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.GETDIR);
        }

        public override Builder VisitSystemDialogGetDirOption(SystemDialogGetDirOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSystemDialogGetFileStatement(SystemDialogGetFileStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.GETFILE);
        }

        public override Builder VisitSystemDialogGetFileOption(SystemDialogGetFileOptionContext ctx)
        {
            if ((ctx.FILTERS() != null) || (ctx.DEFAULTEXTENSION() != null) || (ctx.INITIALDIR() != null) || (ctx.UPDATE() != null))
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitSystemDialogGetFileInitFilter(SystemDialogGetFileInitFilterContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSystemDialogPrinterSetupStatement(SystemDialogPrinterSetupStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx, ABLNodeType.PRINTERSETUP);
        }

        public override Builder VisitSystemDialogPrinterOption(SystemDialogPrinterOptionContext ctx)
        {
            if (ctx.NUMCOPIES() != null)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            return VisitChildren(ctx);
        }

        public override Builder VisitSystemHelpStatement(SystemHelpStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }
        public override Builder VisitSystemHelpWindow(SystemHelpWindowContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitSystemHelpOption(SystemHelpOptionContext ctx)
        {
            if (ctx.children.Count > 1)
            {
                return CreateTreeFromFirstNode(ctx);
            }
            else
            {
                return VisitChildren(ctx);
            }
        }

        public override Builder VisitTextOption(TextOptionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTextPhrase(TextPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitThisObjectStatement(ThisObjectStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitTitleExpression(TitleExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTimeExpression(TimeExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTitlePhrase(TitlePhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitToExpression(ToExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitToggleBoxPhrase(ToggleBoxPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTooltipExpression(TooltipExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTransactionModeAutomaticStatement(TransactionModeAutomaticStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitTriggerPhrase(TriggerPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTriggerBlock(TriggerBlockContext ctx)
        {
            return CreateTree(ctx, ABLNodeType.CODE_BLOCK);
        }

        public override Builder VisitTriggerOn(TriggerOnContext ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }

        public override Builder VisitTriggersEnd(TriggersEndContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTriggerProcedureStatement(TriggerProcedureStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitTriggerOfSub1(TriggerOfSub1Context ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTriggerOfSub2(TriggerOfSub2Context ctx)
        {
            support.DefVar(ctx.id.GetText());
            return CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
        }
        public override Builder VisitTriggerTableLabel(TriggerTableLabelContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitTriggerOld(TriggerOldContext ctx)
        {
            Builder node = CreateTreeFromFirstNode(ctx).SetRuleNode(ctx);
            support.DefVar(ctx.id.GetText());
            return node;
        }

        public override Builder VisitUnderlineStatement(UnderlineStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUndoStatement(UndoStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUndoAction(UndoActionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitUnloadStatement(UnloadStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUnsubscribeStatement(UnsubscribeStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUpStatement(UpStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUpdateField(UpdateFieldContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitUpdateStatement(UpdateStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUseStatement(UseStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUsingRow(UsingRowContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitUsingStatement(UsingStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitUsingFrom(UsingFromContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitValidatePhrase(ValidatePhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitValidateStatement(ValidateStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitViewStatement(ViewStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx);
        }

        public override Builder VisitViewAsPhrase(ViewAsPhraseContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitWaitForStatement(WaitForStatementContext ctx)
        {
            return CreateStatementTreeFromFirstNode(ctx).ChangeType(ABLNodeType.WAITFOR);
        }
        public override Builder VisitWaitForOr(WaitForOrContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitWaitForFocus(WaitForFocusContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitWaitForSet(WaitForSetContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitWhenExpression(WhenExpressionContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitWidgetId(WidgetIdContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitXmlDataType(XmlDataTypeContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitXmlNodeName(XmlNodeNameContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        public override Builder VisitXmlNodeType(XmlNodeTypeContext ctx)
        {
            return CreateTreeFromFirstNode(ctx);
        }

        // ------------------
        // Internal functions
        // ------------------

        /// <summary>
        /// Default behavior for each ParseTree node is to create an array of JPNode.
        /// ANTLR2 construct ruleName: TOKEN TOKEN | rule TOKEN | rule ...
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override @Nonnull public Builder VisitChildren(RuleNode ctx)
        public override Builder VisitChildren(IRuleNode ctx)
        {
            if (ctx.ChildCount == 0)
            {
                return new Builder(ABLNodeType.EMPTY_NODE);
            }

            Builder firstNode = Visit(ctx.GetChild(0));
            Builder lastNode = firstNode.Last;

            for (int zz = 1; zz < ctx.ChildCount; zz++)
            {
                lastNode = lastNode.SetRight(Visit(ctx.GetChild(zz))).Last;
            }
            return firstNode;
        }

        /// <summary>
        /// Attach hidden tokens to current token, then generate Builder with only one JPNode object
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override @Nonnull public Builder VisitTerminal(TerminalNode node)
        public override Builder VisitTerminal(ITerminalNode node)
        {
            ProToken tok = (ProToken)node.Symbol;

            ProToken lastHiddenTok = null;
            ProToken firstHiddenTok = null;

            ProToken t = node.Symbol.TokenIndex > 0 ? (ProToken)stream.Get(node.Symbol.TokenIndex - 1) : null;
            while ((t != null) && (t.Channel != TokenConstants.DefaultChannel))
            {
                if (firstHiddenTok == null)
                {
                    firstHiddenTok = t;
                }
                else
                {
                    lastHiddenTok.HiddenBefore = t;
                }
                lastHiddenTok = t;

                t = t.TokenIndex > 0 ? (ProToken)stream.Get(t.TokenIndex - 1) : null;
            }
            if (firstHiddenTok != null)
            {
                tok.HiddenBefore = firstHiddenTok;
            }

            return new Builder(tok);
        }
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Override @Nonnull public Builder VisitErrorNode(ErrorNode node)
        public override Builder VisitErrorNode(IErrorNode node)
        {
            // Better return an empty node rather than nothing or an error
            return new Builder(ABLNodeType.EMPTY_NODE);
        }

        protected override Builder AggregateResult(Builder aggregate, Builder nextResult)
        {
            throw new System.NotSupportedException("Not implemented");
        }

        protected override Builder DefaultResult => throw new System.NotSupportedException("Not implemented");

        /// <summary>
        /// ANTLR2 construct ruleName: TOKEN^ (TOKEN | rule)....
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nonnull private Builder createTreeFromFirstNode(RuleNode ctx)
        private Builder CreateTreeFromFirstNode(IRuleNode ctx)
        {
            Builder node = Visit(ctx.GetChild(0));

            Builder firstChild = node.Down;
            Builder lastChild = firstChild == null ? null : firstChild.Last;

            for (int zz = 1; zz < ctx.ChildCount; zz++)
            {
                Builder xx = Visit(ctx.GetChild(zz));
                if (lastChild != null)
                {
                    lastChild = lastChild.SetRight(xx).Last;
                }
                else if (xx != null)
                {
                    firstChild = xx;
                    lastChild = firstChild.Last;
                }
            }
            node.SetDown(firstChild);
            return node;
        }

        /// <summary>
        /// ANTLR2 construct ruleName: TOKEN^ (TOKEN | rule).... { ##.setStatementHead(); }
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nonnull private Builder createStatementTreeFromFirstNode(RuleNode ctx)
        private Builder CreateStatementTreeFromFirstNode(IRuleNode ctx)
        {
            return CreateTreeFromFirstNode(ctx).SetStatement().SetRuleNode(ctx);
        }

        /// <summary>
        /// ANTLR2 construct ruleName: TOKEN^ (TOKEN | rule).... { ##.setStatementHead(state2); }
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nonnull private Builder createStatementTreeFromFirstNode(RuleNode ctx, ABLNodeType state2)
        private Builder CreateStatementTreeFromFirstNode(IRuleNode ctx, ABLNodeType state2)
        {
            return CreateTreeFromFirstNode(ctx).SetStatement(state2).SetRuleNode(ctx);
        }

        /// <summary>
        /// ANTLR2 construct ruleName: exp OR^ exp ...
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nonnull private Builder createTreeFromSecondNode(RuleNode ctx)
        private Builder CreateTreeFromSecondNode(IRuleNode ctx)
        {
            Builder node = Visit(ctx.GetChild(1));
            Builder left = Visit(ctx.GetChild(0));
            Builder right = Visit(ctx.GetChild(2));

            node.SetDown(left);
            left.Last.SetRight(right);
            Builder lastNode = node.Last;
            for (int zz = 3; zz < ctx.ChildCount; zz++)
            {
                lastNode = lastNode.SetRight(Visit(ctx.GetChild(zz))).Last;
            }
            node.SetRuleNode(ctx);
            return node;
        }

        /// <summary>
        /// ANTLR2 construct ruleName: rule | token ... {## = #([NodeType], ##);}
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nonnull private Builder createTree(RuleNode ctx, ABLNodeType parentType)
        private Builder CreateTree(IRuleNode ctx, ABLNodeType parentType)
        {
            return (new Builder(parentType)).SetDown(VisitChildren(ctx));
        }

        /// <summary>
        /// ANTLR2 construct ruleName: rule | token ... {## = #([NodeType], ##, [TailNodeType]);}
        /// </summary>
        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nonnull private Builder createTree(RuleNode ctx, ABLNodeType parentType, ABLNodeType tail)
        private Builder createTree(IRuleNode ctx, ABLNodeType parentType, ABLNodeType tail)
        {
            Builder node = new Builder(parentType);
            Builder down = VisitChildren(ctx);
            node.SetDown(down);
            down.Last.SetRight(new Builder(tail));
            node.SetRuleNode(ctx);
            return node;
        }

    }

}
