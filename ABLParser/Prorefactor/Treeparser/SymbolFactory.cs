using System;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Treeparser.Symbols;
using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Treeparser
{
    using ABLParser.Prorefactor.Treeparser.Symbols.Widgets;
    using System.Diagnostics;

    /// <summary>
    /// Create a Symbol of the appropriate subclass. </summary>
    public sealed class SymbolFactory
    {

        private SymbolFactory()
        {
            // Shouldn't be instantiated
        }

        public static Symbol create(ABLNodeType symbolType, string name, TreeParserSymbolScope scope)
        {            
            if (symbolType == ABLNodeType.DATASET)
            {
                return new Dataset(name, scope);
            }
            else if (symbolType == ABLNodeType.DATASOURCE)
            {
                return new Datasource(name, scope);
            }
            else if (symbolType == ABLNodeType.QUERY)
            {
                return new Query(name, scope);
            }
            else if (symbolType == ABLNodeType.STREAM)
            {
                return new Stream(name, scope);
            }
            else if (symbolType == ABLNodeType.BROWSE)
            {
                return new Browse(name, scope);
            }
            else if (symbolType == ABLNodeType.BUTTON)
            {
                return new Button(name, scope);
            }
            else if (symbolType == ABLNodeType.FRAME)
            {
                return new Frame(name, scope);
            }
            else if (symbolType == ABLNodeType.IMAGE)
            {
                return new Image(name, scope);
            }
            else if (symbolType == ABLNodeType.MENU)
            {
                return new Menu(name, scope);
            }
            else if (symbolType == ABLNodeType.MENUITEM)
            {
                return new MenuItem(name, scope);
            }
            else if (symbolType == ABLNodeType.RECTANGLE)
            {
                return new Rectangle(name, scope);
            }
            else if (symbolType == ABLNodeType.SUBMENU)
            {
                return new Submenu(name, scope);
            }
            else
            {
                Debug.Assert(false, "Unexpected values for SymbolFactory" + " " + symbolType + " " + name);
                return null;
            };
        }
    }
}
