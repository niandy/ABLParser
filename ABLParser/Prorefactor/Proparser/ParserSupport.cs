using ABLParser.Prorefactor.Core;
using ABLParser.Prorefactor.Proparser.Antlr;
using ABLParser.Prorefactor.Refactor;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using log4net;
using System;
using System.Collections.Generic;
using static ABLParser.Prorefactor.Proparser.SymbolScope;

namespace ABLParser.Prorefactor.Proparser
{

    /// <summary>
    /// Helper class when parsing procedure or class. One instance per ParseUnit.
    /// </summary>
    public class ParserSupport
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ParserSupport));

        private readonly RefactorSession session;
        private readonly ClassFinder classFinder;
        // Scope for the compile unit or class. It might be "sub" to a super scope in a class hierarchy
        private readonly RootSymbolScope unitScope;

        // Current scope might be "unitScope" or an inner method/subprocedure scope
        private SymbolScope currentScope;

        private bool schemaTablePriority = false;
        private bool unitIsInterface = false;
        private bool allowUnknownMethodCalls = true;

        private IDictionary<string, SymbolScope> funcScopeMap = new Dictionary<string, SymbolScope>();

        private string className = "";

        // Last field referenced. Used for inline defines using LIKE or AS.
        private string lastFieldIDStr;

        private ParseTreeProperty<FieldType> recordExpressions = new ParseTreeProperty<FieldType>();
        private ParseTreeProperty<JPNode> nodes = new ParseTreeProperty<JPNode>();

        private IList<SymbolScope> innerScopes = new List<SymbolScope>();
        private IDictionary<RuleContext, SymbolScope> innerScopesMap = new Dictionary<RuleContext, SymbolScope>();

        public ParserSupport(RefactorSession session)
        {
            this.session = session;
            this.unitScope = new RootSymbolScope(session);
            this.currentScope = unitScope;
            this.classFinder = new ClassFinder(session);
        }

        public virtual string ClassName
        {
            get
            {
                return className;
            }
        }

        /// <summary>
        /// An AS phrase allows further abbreviations on the datatype names. Input a token's text, this returns 0 if it is not
        /// a datatype abbreviation, otherwise returns the integer token type for the abbreviation. Here's the normal keyword
        /// abbreviation, with what AS phrase allows:
        /// <ul>
        /// <li>char: c
        /// <li>date: da
        /// <li>dec: de
        /// <li>int: i
        /// <li>logical: l
        /// <li>recid: rec
        /// <li>rowid: rowi
        /// <li>widget-h: widg
        /// </ul>
        /// </summary>
        public virtual int AbbrevDatatype(string text)
        {
            string s = text.ToLower();
            if ("cha".StartsWith(s, StringComparison.Ordinal))
            {
                return Proparse.CHARACTER;
            }
            if ("da".Equals(s) || "dat".Equals(s))
            {
                return Proparse.DATE;
            }
            if ("de".Equals(s))
            {
                return Proparse.DECIMAL;
            }
            if ("i".Equals(s) || "in".Equals(s))
            {
                return Proparse.INTEGER;
            }
            if ("logical".StartsWith(s, StringComparison.Ordinal))
            {
                return Proparse.LOGICAL;
            }
            if ("rec".Equals(s) || "reci".Equals(s))
            {
                return Proparse.RECID;
            }
            if ("rowi".Equals(s))
            {
                return Proparse.ROWID;
            }
            if ("widget-h".StartsWith(s, StringComparison.Ordinal) && s.Length >= 4)
            {
                return Proparse.WIDGETHANDLE;
            }
            return 0;
        }

        // TEMP-ANTLR4
        public virtual void VisitorEnterScope(RuleContext ctx)
        {            
            if (innerScopesMap.TryGetValue(ctx, out SymbolScope scope))
            {
                currentScope = scope;
            }
        }

        // TEMP-ANTLR4
        public virtual void VisitorExitScope(RuleContext ctx)
        {
            if (innerScopesMap.TryGetValue(ctx, out _))
            {
                currentScope = currentScope.SuperScope;
            }
        }

        public virtual void AddInnerScope()
        {
            currentScope = new SymbolScope(session, currentScope);
            innerScopes.Add(currentScope);
        }

        // TEMP-ANTLR4
        public virtual void AddInnerScope(RuleContext ctx)
        {
            AddInnerScope();
            innerScopesMap[ctx] = currentScope;
        }


        // TEMP-ANTLR4
        public virtual RootSymbolScope UnitScope
        {
            get
            {
                return unitScope;
            }
        }

        // TEMP-ANTLR4
        public virtual IList<SymbolScope> InnerScopes
        {
            get
            {
                return innerScopes;
            }
        }

        // Functions triggered from proparse.g

        public virtual void DefBuffer(string bufferName, string tableName)
        {
            LOG.Debug($"defBuffer {bufferName} to {tableName}");
            currentScope.DefineBuffer(bufferName, tableName);
        }

        public virtual void DefineClass(string name)
        {
            LOG.Debug($"defineClass '{name}'");
            className = ClassFinder.Dequote(name);
            unitScope.AttachTypeInfo(session.GetTypeInfo(className));
        }

        public virtual void DefInterface(string name)
        {
            LOG.Debug("defineInterface");
            unitIsInterface = true;
            className = ClassFinder.Dequote(name);
        }

        public virtual void DefTable(string name, SymbolScope.FieldType ttype)
        {
            // I think the compiler will only allow table defs at the class/unit scope,
            // but we don't need to enforce that here. It'll go in the right spot by the
            // nature of the code.
            currentScope.DefineTable(name.ToLower(), ttype);
        }

        public virtual void DefVar(string name)
        {
            currentScope.DefineVar(name);
        }

        public virtual void DefVarInlineAntlr4()
        {
            if (lastFieldIDStr == null)
            {
                LOG.Warn("Trying to define inline variable, but no ID symbol available");
            }
            else
            {
                currentScope.DefineInlineVar(lastFieldIDStr);
            }
        }

        public virtual void DropInnerScope()
        {
            currentScope = currentScope.SuperScope;
        }

        public virtual void FieldReference(string idNode)
        {
            lastFieldIDStr = idNode;
        }

        public virtual void FuncBegin(string name, RuleContext ctx)
        {
            string lowername = name.ToLower();            
            if (funcScopeMap.TryGetValue(lowername, out SymbolScope ss))
            {
                currentScope = ss;
            }
            else
            {
                currentScope = new SymbolScope(session, currentScope);
                innerScopes.Add(currentScope);
                if (ctx != null)
                {
                    innerScopesMap[ctx] = currentScope;
                }
                funcScopeMap[lowername] = currentScope;
                // User functions are always at the "unit" scope.
                unitScope.defFunc(lowername);
                // User funcs are not inheritable.
            }
        }

        public virtual void FuncEnd()
        {
            currentScope = currentScope.SuperScope;
        }

        public virtual void UsingState(string typeName)
        {
            classFinder.AddPath(typeName);
        }

        // End of functions triggered from proparse.g

        public virtual bool RecordSemanticPredicate(IToken lt1, IToken lt2, IToken lt3)
        {
            string recname = lt1.Text;
            // Since ANTLR4 migration, NAMEDOT doesn't exist anymore in the token stream, as they're filtered out by
            // NameDotTokenFilter
            // So this 'if' block can probably be removed...
            if (lt2.Type == ABLNodeType.NAMEDOT.Type)
            {
                recname += ".";
                recname += lt3.Text;
            }
            return (schemaTablePriority ? IsTableSchemaFirst(recname.ToLower()) : IsTable(recname.ToLower())) != null;
        }

        public virtual void PushNode(IParseTree ctx, JPNode node)
        {
            nodes.Put(ctx, node);
        }

        public virtual JPNode GetNode(IParseTree ctx)
        {
            return nodes.Get(ctx);
        }

        public virtual void PushRecordExpression(RuleContext ctx, string recName)
        {
            recordExpressions.Put(ctx, schemaTablePriority ? currentScope.IsTableSchemaFirst(recName.ToLower()) : currentScope.IsTable(recName.ToLower()));
        }

        public virtual FieldType GetRecordExpression(RuleContext ctx)
        {
            return recordExpressions.Get(ctx);
        }

        public virtual FieldType IsTable(string inName)
        {
            return currentScope.IsTable(inName);
        }

        public virtual FieldType IsTableSchemaFirst(string inName)
        {
            return currentScope.IsTableSchemaFirst(inName);
        }

        /// <summary>
        /// Returns true if the lookahead is a table name, and not a var name. </summary>
        public virtual bool IsTableName(IToken token)
        {
            int numDots = CountChars(token.Text, '.');
            if (numDots >= 2)
            {
                return false;
            }
            if (IsVar(token.Text))
            {
                return false;
            }
            return null != IsTable(token.Text.ToLower());
        }

        public virtual bool IsVar(string name)
        {
            return currentScope.IsVariable(name);
        }

        public virtual bool IsInlineVar(string name)
        {
            return currentScope.IsInlineVariable(name);
        }

        public virtual int IsMethodOrFunc(string name)
        {
            // Methods and user functions are only at the "unit" (class) scope.
            // Methods can also be inherited from superclasses.
            return unitScope.IsMethodOrFunction(name);
        }
        public virtual int IsMethodOrFunc(IToken token)
        {
            if (token == null)
            {
                return 0;
            }
            return unitScope.IsMethodOrFunction(token.Text);
        }

        /// <returns> True if parsing a class or interface </returns>
        public virtual bool Class
        {
            get
            {
                return !string.IsNullOrEmpty(className);
            }
        }

        public virtual string GetClassName(string name)
        {
            return classFinder.Lookup(name);
        }

        /// <returns> True if parsing an interface </returns>
        public virtual bool Interface => unitIsInterface;
            
        public virtual bool SchemaTablePriority
        {
            get => schemaTablePriority;            
            set => this.schemaTablePriority = value;            
        }


        public virtual void AllowUnknownMethodCalls()
        {
            this.allowUnknownMethodCalls = true;
        }

        public virtual void DisallowUnknownMethodCalls()
        {
            this.allowUnknownMethodCalls = false;
        }

        /// <returns> False if unknown method calls are not allowed in exprt2 rule. Usually returns true except when in
        ///         DYNAMIC-NEW or RUN ... IN|ON statements </returns>
        public virtual bool UnknownMethodCallsAllowed()
        {
            return allowUnknownMethodCalls;
        }

        // TODO Speed issue in this function, multiplied JPNode tree generation time by a factor 10
        public virtual string LookupClassName(string text)
        {
            return classFinder.Lookup(text);
        }

        public virtual bool HasHiddenBefore(ITokenStream stream)
        {
            int currIndex = stream.Index;
            // Obviously no hidden token for first token
            if (currIndex == 0)
            {
                return false;
            }
            // Otherwise see if token is in different channel
            return stream.Get(currIndex - 1).Channel != TokenConstants.DefaultChannel;
        }

        public virtual bool HasHiddenAfter(ITokenStream stream)
        {
            int currIndex = stream.Index;
            // Obviously no hidden token for last token
            if (currIndex == stream.Size - 1)
            {
                return false;
            }
            // Otherwise see if token is in different channel
            return stream.Get(currIndex + 1).Channel != TokenConstants.DefaultChannel;
        }

        private int CountChars(string s, char c)
        {
            int ret = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    ret++;
                }
            }
            return ret;
        }
    }

}
