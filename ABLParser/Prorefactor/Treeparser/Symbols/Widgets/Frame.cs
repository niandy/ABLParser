using ABLParser.Prorefactor.Proparser.Antlr;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Treeparser.Symbols.Widgets
{
    public class Frame : FieldContainer
    {

        private bool initialized = false;
        private Block frameScopeBlock = null;

        /// <summary>
        /// Unlike other symbols, Frames are automatically added to the scope, right here at creation time. </summary>
        public Frame(string name, TreeParserSymbolScope scope) : base(name, scope)
        {
            scope.Add(this);
        }

        public virtual Block FrameScopeBlock => frameScopeBlock;

        /// <returns> NodeTypes.FRAME </returns>
        public override int ProgressType => Proparse.FRAME;

        /// <summary>
        /// Initialize the frame and set the frame scope if not done already. Returns the frameScopeBlock.
        /// </summary>
        /// <seealso cref= #isInitialized() </seealso>
        public virtual Block Initialize(Block block)
        {
            if (initialized)
            {
                return frameScopeBlock;
            }
            initialized = true;
            if (frameScopeBlock == null)
            {
                frameScopeBlock = block.AddFrame(this);
            }
            return frameScopeBlock;
        }

        /// <summary>
        /// Has this frame been "referenced"? In other words, has it or any of its fields been displayed yet? Has its scope
        /// been determined?
        /// </summary>
        public virtual bool Initialized => initialized;

        /// <summary>
        /// This should be called for a block with an explicit default. i.e. {DO|FOR|REPEAT} WITH FRAME.
        /// </summary>
        public virtual Block FrameScopeBlockExplicitDefault
        {
            set
            {
                frameScopeBlock = value;
                value.SetDefaultFrameExplicit(this);
            }
        }

        /// <summary>
        /// This should be called when we need to set a block with this unnamed frame as that block's implicit default. Returns
        /// the block that this unnamed/default frame got scoped to. That would be a REPEAT or FOR block, or else the frame's
        /// symbol scope.
        /// </summary>
        public virtual Block SetFrameScopeUnnamedDefault(Block block)
        {
            frameScopeBlock = block.SetDefaultFrameImplicit(this);
            return frameScopeBlock;
        }

        internal override bool IsInstanceOfType(Symbol s) => s is Frame;        
    }
}
