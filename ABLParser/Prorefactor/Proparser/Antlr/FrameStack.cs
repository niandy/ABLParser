using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.Diagnostics;
using ABLParser.Prorefactor.Treeparser.Symbols.Widgets;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Treeparser;
using Antlr4.Runtime.Tree;
using ABLParser.Prorefactor.Core.Schema;
using ABLParser.Prorefactor.Core.NodeTypes;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    /// <summary>
    /// Keeps a stack of most recently "referenced" frames. A frame may be "referenced" at up to two different occassions.
    /// Once when the frame is created (like in a DEFINE FRAME statement), and once when the frame is "initialized" (like in
    /// a DISPLAY statement). The frame's scope is determined at the time it is initialized. Also deals with BROWSE widgets
    /// and the fields in those.
    /// </summary>
    public class FrameStack
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(FrameStack));        

        private bool currStatementIsEnabler = false;
        private readonly LinkedList<Frame> frameMRU = new LinkedList<Frame>();
        private FieldContainer containerForCurrentStatement = null;
        private JPNode currStatementWholeTableFormItemNode = null;

        protected internal FrameStack()
        {
            // Only from TreeParser
        }

        /// <summary>
        /// The ID node in a BROWSE ID pair. The ID node might have already had the symbol assigned to it at the point where
        /// the statement head was processed.
        /// </summary>
        internal virtual void BrowseRefNode(JPNode idNode, TreeParserSymbolScope symbolScope)
        {
            LOG.Debug("Enter FrameStack#browseRefNode");

            if (idNode.Symbol == null)
            {
                BrowseRefSet(idNode, symbolScope);
            }
        }

        private Browse BrowseRefSet(JPNode idNode, TreeParserSymbolScope symbolScope)
        {
            Browse browse = (Browse)symbolScope.LookupFieldLevelWidget(idNode.Text);
            idNode.Symbol = browse;
            return browse;
        }

        /// <summary>
        /// For a Form_item node which is for a whole table reference, get a list of the FieldBuffers that would be added to
        /// the frame, respecting any EXCEPT fields list.
        /// </summary>
        private IList<FieldBuffer> CalculateFormItemTableFields(JPNode formItemNode)
        {
            Debug.Assert(formItemNode.Type == Proparse.Form_item);
            Debug.Assert(formItemNode.FirstChild.Type == Proparse.RECORD_NAME);
            RecordNameNode recordNameNode = (RecordNameNode)formItemNode.FirstChild;
            TableBuffer tableBuffer = recordNameNode.TableBuffer;
            HashSet<IField> fieldSet = new HashSet<IField>(tableBuffer.Table.FieldSet);
            JPNode exceptNode = formItemNode.Parent.FindDirectChild(Proparse.EXCEPT);
            if (exceptNode != null)
            {
                for (JPNode n = exceptNode.FirstChild; n != null; n = n.NextSibling)
                {
                    if (!(n is FieldRefNode))
                    {
                        continue;
                    }
                    IField f = ((FieldBuffer)((FieldRefNode)n).Symbol).Field;
                    fieldSet.Remove(f);
                }
            }
            List<FieldBuffer> returnList = new List<FieldBuffer>();
            foreach (IField field in fieldSet)
            {
                returnList.Add(tableBuffer.GetFieldBuffer(field));
            }
            return returnList;
        }

        /// <summary>
        /// Create a frame object. Adds the new frame object to the MRU list.
        /// </summary>
        private Frame CreateFrame(string frameName, TreeParserSymbolScope symbolScope)
        {
            Frame frame = new Frame(frameName, symbolScope);
            frameMRU.AddFirst(frame);
            return frame;
        }


        /// <summary>
        /// Recieve a Form_item node for a field which should be referenceable on the frame|browse. This checks for LEXAT
        /// (DISPLAY thisField @ anotherField) which would keep thisField from being added to the frame. The LEXAT is dealt
        /// with in a separate call. This must be called <b>after</b> any Field_ref symbols have been resolved. This only does
        /// anything if the first child of the Form_item is RECORD_NAME or Field_ref. Tree parser rules like display_item and
        /// form_item sometimes get used in statements that don't actually affect frames. In those cases,
        /// containerForCurrentStatement==null, and this function is a no-op.
        /// </summary>
        internal virtual void FormItem(JPNode formItemNode)
        {
            LOG.Debug("Enter FrameStack#formItem");

            if (containerForCurrentStatement == null)
            {
                return;
            }
            Debug.Assert(formItemNode.Type == Proparse.Form_item);
            JPNode firstChild = formItemNode.FirstChild;
            if (firstChild.Type == Proparse.RECORD_NAME)
            {
                // Delay processing until the end of the statement. We need any EXCEPT fields resolved first.
                currStatementWholeTableFormItemNode = formItemNode;
            }
            else
            {
                FieldRefNode fieldRefNode = null;
                JPNode tempNode = formItemNode.FindDirectChild(Proparse.Format_phrase);
                if (tempNode != null)
                {
                    tempNode = tempNode.FindDirectChild(Proparse.LEXAT);
                    if (tempNode != null)
                    {
                        return;
                    }
                }
                if (fieldRefNode == null && firstChild.Type == Proparse.Field_ref)
                {
                    fieldRefNode = (FieldRefNode)firstChild;
                }
                if (fieldRefNode != null)
                {
                    containerForCurrentStatement.addSymbol(fieldRefNode.Symbol, currStatementIsEnabler);
                }
            }
        }

        /// <summary>
        /// The ID node in a FRAME ID pair. For "WITH FRAME id", the ID was already set when we processed the statement head.
        /// </summary>
        internal virtual void FrameRefNode(JPNode idNode, TreeParserSymbolScope symbolScope)
        {
            LOG.Debug("Enter FrameStack#frameRefNode");

            if (idNode.Symbol == null)
            {
                FrameRefSet(idNode, symbolScope);
            }
        }

        private Frame FrameRefSet(JPNode idNode, TreeParserSymbolScope symbolScope)
        {
            string frameName = idNode.Text;
            Frame frame = (Frame)symbolScope.LookupWidget(Proparse.FRAME, frameName);
            if (frame == null)
            {
                frame = CreateFrame(frameName, symbolScope);
            }
            idNode.Symbol = frame;
            return frame;
        }

        /// <summary>
        /// For a statement that might have #(WITH ... #([FRAME|BROWSE] ID)), get the FRAME|BROWSE node. </summary>
        private JPNode GetContainerTypeNode(JPNode stateNode)
        {
            JPNode withNode = stateNode.FindDirectChild(Proparse.WITH);
            if (withNode == null)
            {
                return null;
            }
            JPNode typeNode = withNode.FindDirectChild(Proparse.FRAME);
            if (typeNode == null)
            {
                typeNode = withNode.FindDirectChild(Proparse.BROWSE);
            }
            return typeNode;
        }

        /// <summary>
        /// Create the frame if necessary, set its scope if that hasn't already been done. </summary>
        private Frame InitializeFrame(Frame frame, Block currentBlock)
        {
            // If we don't have a frame then get or create the unnamed default frame for the block.
            if (frame == null)
            {
                frame = currentBlock.DefaultFrame;
            }
            bool newFrame = frame == null;
            if (newFrame)
            {
                frame = CreateFrame("", currentBlock.SymbolScope);
                frame.SetFrameScopeUnnamedDefault(currentBlock);
            }
            if (!frame.Initialized)
            {
                frame.Initialize(currentBlock);
                if (!newFrame)
                {
                    frameMRU.Remove(frame);
                    frameMRU.AddFirst(frame);
                }
            }
            return frame;
        }


        /// <summary>
        /// Deals with frame fields referenced by INPUT and USING. For a Field_ref node where it matches #(Field_ref INPUT
        /// ...), determine which frame field is being referenced. This is also called for #(RECORD_NAME ... #(USING
        /// #(Field_ref...))). Sets the FieldContainer attribute (a Frame or Browse object) on the Field_ref node.
        /// </summary>
        /// <seealso cref= org.prorefactor.core.JPNode#getFieldContainer(). </seealso>
        internal virtual FieldLookupResult InputFieldLookup(FieldRefNode fieldRefNode, TreeParserSymbolScope currentScope)
        {
            JPNode idNode = fieldRefNode.IdNode;
            Field.Name inputName = new Field.Name(idNode.Text.ToLower());
            FieldContainer fieldContainer = null;
            Symbol fieldOrVariable = null;
            JPNode tempNode = fieldRefNode.FirstChild;
            int tempType = tempNode.Type;
            if (tempType == Proparse.INPUT)
            {
                tempNode = tempNode.NextSibling;
                tempType = tempNode.Type;
            }
            if (tempType == Proparse.BROWSE || tempType == Proparse.FRAME)
            {
                fieldContainer = (FieldContainer)tempNode.NextNode.Symbol;
                fieldOrVariable = fieldContainer.lookupFieldOrVar(inputName);
            }
            else
            {
                foreach (Frame frame in frameMRU)
                {
                    if (!frame.Scope.IsActiveIn(currentScope))
                    {
                        continue;
                    }
                    fieldOrVariable = frame.lookupFieldOrVar(inputName);
                    if (fieldOrVariable != null)
                    {
                        fieldContainer = frame;
                        break;
                    }
                }
            }
            if (fieldOrVariable == null)
            {
                LOG.Error(String.Format("Could not find input field {0} {1}:{2}", idNode.Text, idNode.FileIndex, idNode.Line));
                return null;
            }
            fieldRefNode.FieldContainer = fieldContainer;
            FieldLookupResult.Builder result = (new FieldLookupResult.Builder()).SetSymbol(fieldOrVariable);
            if (!(fieldOrVariable is Variable))
            {
                Field.Name resName = new Field.Name(fieldOrVariable.FullName);
                if (inputName.Table == null)
                {
                    result.SetUnqualified();
                }
                if (inputName.Field.Length < resName.Field.Length || (inputName.Table != null && (inputName.Table.Length < resName.Table.Length)))
                {
                    result.SetAbbreviated();
                }
            }
            return result.Build();
        }

        /// <summary>
        /// Receive the node (will be a Field_ref) that follows an @ in a frame phrase. </summary>
        internal virtual void LexAt(JPNode fieldRefNode)
        {
            LOG.Debug("Enter FrameStack#lexAt");

            if (containerForCurrentStatement != null)
            {
                containerForCurrentStatement.addSymbol(fieldRefNode.Symbol, currStatementIsEnabler);
            }
        }

        /// <summary>
        /// FOR|REPEAT|DO blocks need to be checked for explicit WITH FRAME phrase. </summary>
        internal virtual void NodeOfBlock(JPNode blockNode, Block currentBlock)
        {
            LOG.Debug("Enter FrameStack#nodeOfBlock");

            JPNode containerTypeNode = GetContainerTypeNode(blockNode);
            if (containerTypeNode == null)
            {
                return;
            }
            // No such thing as DO WITH BROWSE...
            Debug.Assert(containerTypeNode.Type == Proparse.FRAME);
            JPNode frameIDNode = containerTypeNode.NextNode;
            Debug.Assert(frameIDNode.Type == Proparse.ID);
            Frame frame = FrameRefSet(frameIDNode, currentBlock.SymbolScope);
            frame.FrameScopeBlockExplicitDefault = ((BlockNode)blockNode).Block;
            blockNode.FieldContainer = frame;
            containerForCurrentStatement = frame;
        }

        /// <summary>
        /// Called at tree parser DEFINE BROWSE statement. </summary>
        internal virtual void NodeOfDefineBrowse(Browse newBrowseSymbol, JPNode defNode, IParseTree defNode2)
        {
            LOG.Debug("Enter FrameStack#nodeOfDefineBrowse");

            containerForCurrentStatement = newBrowseSymbol;
            containerForCurrentStatement.addStatement(defNode2);
        }

        /// <summary>
        /// Called at tree parser DEFINE FRAME statement. A DEFINE FRAME statement might hide a frame symbol from a higher
        /// symbol scope. A DEFINE FRAME statement is legal for a frame symbol already in use, sort of like how you can have
        /// multiple FORM statements, I suppose. A DEFINE FRAME statement does not initialize the frame's scope.
        /// </summary>
        internal virtual void NodeOfDefineFrame(IParseTree defNode2, JPNode defNode, JPNode idNode, string frameName, TreeParserSymbolScope currentSymbolScope)
        {
            LOG.Debug("Enter FrameStack#nodeOfDefineFrame");

            Frame frame = (Frame)currentSymbolScope.LookupSymbolLocally(Proparse.FRAME, frameName);
            if (frame == null)
            {
                frame = CreateFrame(frameName, currentSymbolScope);
            }
            frame.DefinitionNode = defNode.IdNode;
            defNode.Symbol = frame;
            defNode.FieldContainer = frame;
            containerForCurrentStatement = frame;
            containerForCurrentStatement.addStatement(defNode2);
        }

        /// <summary>
        /// For an IO/UI statement which would initialize a frame, compute the frame and set the frame attribute on the
        /// statement head node. This is not used from DEFINE FRAME, HIDE FRAME, or any other "frame" statements which would
        /// not count as a "reference" for frame scoping purposes.
        /// </summary>
        internal virtual void NodeOfInitializingStatement(IParseTree stateNode2, JPNode stateNode, Block currentBlock)
        {
            LOG.Debug("Enter FrameStack#nodeOfInitializingStatement");

            JPNode containerTypeNode = GetContainerTypeNode(stateNode);
            JPNode idNode = null;
            if (containerTypeNode != null)
            {
                idNode = containerTypeNode.NextNode;
                Debug.Assert(idNode.Type == Proparse.ID);
            }
            if (containerTypeNode != null && containerTypeNode.Type == Proparse.BROWSE)
            {
                containerForCurrentStatement = BrowseRefSet(idNode, currentBlock.SymbolScope);
            }
            else
            {
                Frame frame = null;
                if (idNode != null)
                {
                    frame = FrameRefSet(idNode, currentBlock.SymbolScope);
                }
                // This returns the frame whether it already exists or it creates it new.
                frame = InitializeFrame(frame, currentBlock);
                containerForCurrentStatement = frame;
            }
            stateNode.FieldContainer = containerForCurrentStatement;
            containerForCurrentStatement.addStatement(stateNode2);
        }

        /// <summary>
        /// For frame init statements like VIEW and CLEAR which have no frame phrase. Called at the end of the statement, after
        /// all symbols (including FRAME ID) have been resolved.
        /// </summary>
        internal virtual void SimpleFrameInitStatement(IParseTree headNode2, JPNode headNode, JPNode frameIDNode, Block currentBlock)
        {
            LOG.Debug("Enter FrameStack#simpleFrameInitStatement");

            Frame frame = (Frame)frameIDNode.NextNode.Symbol;
            Debug.Assert(frame != null);
            InitializeFrame(frame, currentBlock);
            headNode.FieldContainer = frame;
            frame.addStatement(headNode2);
        }

        /// <summary>
        /// Called at the end of a frame affecting statement. </summary>
        internal virtual void StatementEnd()
        {
            LOG.Debug("Enter FrameStack#statementEnd");

            // For something like DISPLAY customer, we delay adding the fields to the frame until the end of the statement.
            // That's because any fields in an EXCEPT fields phrase need to have their symbols resolved first.
            if (currStatementWholeTableFormItemNode != null)
            {
                IList<FieldBuffer> fields = CalculateFormItemTableFields(currStatementWholeTableFormItemNode);
                foreach (FieldBuffer fieldBuffer in fields)
                {
                    containerForCurrentStatement.addSymbol(fieldBuffer, currStatementIsEnabler);
                }
                currStatementWholeTableFormItemNode = null;
            }
            containerForCurrentStatement = null;
            currStatementIsEnabler = false;
        }

        /// <summary>
        /// Used only by the tree parser, for ENABLE|UPDATE|PROMPT-FOR. </summary>
        internal virtual void StatementIsEnabler()
        {
            currStatementIsEnabler = true;
        }

    }

}
