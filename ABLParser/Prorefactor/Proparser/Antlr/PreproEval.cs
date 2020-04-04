using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using ABLParser.Prorefactor.Refactor.Settings;
using static ABLParser.Prorefactor.Proparser.Antlr.PreprocessorParser;
using log4net;
using Antlr4.Runtime.Misc;
using ABLParser.Prorefactor.Core;
using System.Text.RegularExpressions;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class PreproEval : PreprocessorParserBaseVisitor<object>
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(PreproEval));

        private readonly IProparseSettings settings;

        public PreproEval(IProparseSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Main entry point returning the evaluation of the preprocessor expression
        /// </summary>
        /// <returns> A Boolean object </returns>
        public override object VisitPreproIfEval([NotNull] PreproIfEvalContext ctx)
        {
            LOGGER.Debug("Entering VisitPreproIfEval()");
            object o = Visit(ctx.expr());
            LOGGER.Debug(string.Format("Exiting VisitPreproIfEval() with return value '{0}'", o));
            return (o != null) && GetBool(o);
        }

        // ****
        // Expr
        // ****
        public override object VisitAnd(AndContext ctx)
        {
            object o1 = Visit(ctx.expr(0));
            object o2 = Visit(ctx.expr(1));
            bool b1 = (o1 != null) && GetBool(o1);
            bool b2 = (o2 != null) && GetBool(o2);

            return b1 && b2;
        }

        public override object VisitOr(OrContext ctx)
        {
            object o1 = Visit(ctx.expr(0));
            object o2 = Visit(ctx.expr(1));
            bool b1 = (o1 != null) && GetBool(o1);
            bool b2 = (o2 != null) && GetBool(o2);

            return b1 || b2;
        }

        public override object VisitStringOp(StringOpContext ctx)
        {
            object o1 = Visit(ctx.expr(0));
            object o2 = Visit(ctx.expr(1));

            if (ctx.op.Type == PreprocessorParser.MATCHES)
            {
                return Matches(o1, o2);
            }
            else
            {
                return GetString(o1).ToLower().StartsWith(GetString(o2).ToLower(), StringComparison.Ordinal);
            }
        }

        public override object VisitComparison(ComparisonContext ctx)
        {
            object o1 = Visit(ctx.expr(0));
            object o2 = Visit(ctx.expr(1));

            switch (ctx.op.Type)
            {
                case PreprocessorParser.EQ:
                case PreprocessorParser.EQUAL:
                    return DoCompare(o1, o2, Compare.EQ);
                case PreprocessorParser.GTORLT:
                case PreprocessorParser.NE:
                    return DoCompare(o1, o2, Compare.NE);
                case PreprocessorParser.RIGHTANGLE:
                case PreprocessorParser.GTHAN:
                    return DoCompare(o1, o2, Compare.GT);
                case PreprocessorParser.LEFTANGLE:
                case PreprocessorParser.LTHAN:
                    return DoCompare(o1, o2, Compare.LT);
                case PreprocessorParser.GTOREQUAL:
                case PreprocessorParser.GE:
                    return DoCompare(o1, o2, Compare.GE);
                case PreprocessorParser.LTOREQUAL:
                case PreprocessorParser.LE:
                    return DoCompare(o1, o2, Compare.LE);
                default:
                    return null;
            }
        }

        public override object VisitPlus(PlusContext ctx)
        {
            if (ctx.op.Type == PreprocessorParser.PLUS)
            {
                return OpPlus(Visit(ctx.expr(0)), Visit(ctx.expr(1)));
            }
            else
            {
                return OpMinus(Visit(ctx.expr(0)), Visit(ctx.expr(1)));
            }
        }

        public override object VisitMultiply(MultiplyContext ctx)
        {
            switch (ctx.op.Type)
            {
                case PreprocessorParser.STAR:
                case PreprocessorParser.MULTIPLY:
                    return OpMultiply(Visit(ctx.expr(0)), Visit(ctx.expr(1)));
                case PreprocessorParser.SLASH:
                case PreprocessorParser.DIVIDE:
                    return OpDivide(Visit(ctx.expr(0)), Visit(ctx.expr(1)));
                case PreprocessorParser.MODULO:
                    double? m1 = GetFloat(Visit(ctx.expr(0))) + .5;
                    double? m2 = GetFloat(Visit(ctx.expr(1))) + .5;
                    return Convert.ToInt32(m1.Value % m2.Value);
                default:
                    return null;
            }
        }

        public override object VisitNot(NotContext ctx)
        {
            return Convert.ToBoolean(!GetBool(Visit(ctx.expr())));
        }

        public override object VisitUnaryMinus(UnaryMinusContext ctx)
        {
            object o = Visit(ctx.expr());
            if (o is int?)
            {
                return (int?)o * -1;
            }
            else
            {
                return (float?)o * -1;
            }
        }

        // ****
        // Atom
        // ****

        public override object VisitNumber(NumberContext ctx)
        {
            return GetNumber(ctx.NUMBER().GetText());
        }

        public override object VisitQuotedString(QuotedStringContext ctx)
        {
            return StringFuncs.QstringStrip(ctx.QSTRING().GetText());
        }

        public override object VisitTrueExpr(TrueExprContext ctx)
        {
            return true;
        }

        public override object VisitFalseExpr(FalseExprContext ctx)
        {
            return false;
        }

        public override object VisitExprInParen(ExprInParenContext ctx)
        {
            return Visit(ctx.expr());
        }

        public override object VisitUnknownExpr(UnknownExprContext ctx)
        {
            return null;
        }

        // *********
        // Functions
        // *********

        public override object VisitIntegerFunction(IntegerFunctionContext ctx)
        {
            return Integer(Visit(ctx.expr()));
        }

        public override object VisitInt64Function(Int64FunctionContext ctx)
        {
            return Integer(Visit(ctx.expr()));
        }

        public override object VisitDecimalFunction(DecimalFunctionContext ctx)
        {
            return Decimal(Visit(ctx.expr()));
        }

        public override object VisitLeftTrimFunction(LeftTrimFunctionContext ctx)
        {
            object ch = null;
            if (ctx.trimChars != null)
            {
                ch = Visit(ctx.trimChars);
            }

            return Lefttrim(Visit(ctx.expr(0)), ch);
        }
        public override object VisitRightTrimFunction(RightTrimFunctionContext ctx)
        {
            if (ctx.trimChars != null)
            {
                return StringFuncs.Rtrim(GetString(Visit(ctx.expr(0))), GetString(Visit(ctx.trimChars)));
            }
            else
            {
                return StringFuncs.Rtrim(GetString(Visit(ctx.expr(0))));
            }
        }

        public override object VisitPropathFunction(PropathFunctionContext ctx)
        {
            return Propath(settings);
        }

        public override object VisitProversionFunction(ProversionFunctionContext ctx)
        {
            return settings.Proversion;
        }

        public override object VisitProcessArchitectureFunction(ProcessArchitectureFunctionContext ctx)
        {
            return settings.ProcessArchitecture;
        }

        public override object VisitEntryFunction(EntryFunctionContext ctx)
        {
            object element = Visit(ctx.element);
            object list = Visit(ctx.list);
            object ch = null;
            if (ctx.character != null)
            {
                ch = Visit(ctx.character);
            }

            return Entry(element, list, ch);
        }

        public override object VisitIndexFunction(IndexFunctionContext ctx)
        {
            object source = Visit(ctx.source);
            object target = Visit(ctx.target);
            object start = null;
            if (ctx.starting != null)
            {
                start = Visit(ctx.starting);
            }

            return Index(source, target, start);
        }

        public override object VisitLengthFunction(LengthFunctionContext ctx)
        {
            return Visit(ctx.expr(0)).ToString().Length;
        }

        public override object VisitLookupFunction(LookupFunctionContext ctx)
        {
            object expr = Visit(ctx.expr(0));
            object list = Visit(ctx.list);
            object ch = null;
            if (ctx.character != null)
            {
                ch = Visit(ctx.character);
            }

            return Lookup(expr, list, ch);
        }

        public override object VisitMaximumFunction(MaximumFunctionContext ctx)
        {
            object ret = null;
            foreach (ExprContext expr in ctx.expr())
            {
                object o = Visit(expr);
                if ((ret == null) || (DoCompare(o, ret, Compare.GT) == true))
                {
                    ret = o;
                }
            }

            return ret;
        }

        public override object VisitMinimumFunction(MinimumFunctionContext ctx)
        {
            object ret = null;
            foreach (ExprContext expr in ctx.expr())
            {
                object o = Visit(expr);
                if ((ret == null) || (DoCompare(o, ret, Compare.LT) == true))
                {
                    ret = o;
                }
            }

            return ret;
        }

        public override object VisitNumEntriesFunction(NumEntriesFunctionContext ctx)
        {
            object list = Visit(ctx.list);
            object ch = null;
            if (ctx.character != null)
            {
                ch = Visit(ctx.character);
            }

            return NumEntries(list, ch);
        }


        public override object VisitOpsysFunction(OpsysFunctionContext ctx)
        {
            return settings.OpSys.Name;
        }

        /// <summary>
        /// Perfect :-)
        /// See https://xkcd.com/221/
        /// </summary>
        public override object VisitRandomFunction(RandomFunctionContext ctx)
        {
            return Convert.ToInt32(4);
        }

        public override object VisitReplaceFunction(ReplaceFunctionContext ctx)
        {
            return Replace(GetString(Visit(ctx.source)), GetString(Visit(ctx.from)), GetString(Visit(ctx.to)));
        }

        public override object VisitRIndexFunction(RIndexFunctionContext ctx)
        {
            object source = Visit(ctx.source);
            object target = Visit(ctx.target);
            object start = null;
            if (ctx.starting != null)
            {
                start = Visit(ctx.starting);
            }

            return RIndex(source, target, start);
        }

        public override object VisitSubstringFunction(SubstringFunctionContext ctx)
        {
            object o = Visit(ctx.expr(0));
            object pos = Visit(ctx.position);
            object len = (ctx.length == null ? null : Visit(ctx.length));
            SubstringType type = SubstringType.CHARACTER;
            if ((ctx.type != null) && (ctx.type.GetText() != null))
            {
                Enum.TryParse(StringFuncs.QstringStrip(ctx.type.GetText()).ToUpper().Trim(), true, out type);
            }
            if (type != SubstringType.CHARACTER)
            {
                throw new ProEvalException("FIXED / COLUMN / RAW options of SUBSTRING function not yet supported");
            }

            return Substring(o, pos, len, type);
        }

        public override object VisitTrimFunction(TrimFunctionContext ctx)
        {
            object expr = Visit(ctx.expr(0));
            if (ctx.trimChars != null)
            {
                return StringFuncs.Trim(GetString(expr), GetString(Visit(ctx.trimChars)));
            }
            else
            {
                return GetString(expr).Trim();
            }
        }

        public override object VisitKeywordFunction(KeywordFunctionContext ctx)
        {
            string str = GetString(Visit(ctx.expr()));
            ABLNodeType nodeType = ABLNodeType.GetLiteral(str);
            if (nodeType == null)
            {
                return null;
            }
            else if (nodeType.IsReservedKeyword())
            {
                return nodeType.Text.ToUpper();
            }
            else
            {
                return null;
            }
        }

        public override object VisitKeywordAllFunction(KeywordAllFunctionContext ctx)
        {
            string str = GetString(Visit(ctx.expr()));
            ABLNodeType nodeType = ABLNodeType.GetLiteral(str);
            if (nodeType == null)
            {
                return null;
            }
            else
            {
                return nodeType.Text.ToUpper();
            }
        }

        public override object VisitDbTypeFunction(DbTypeFunctionContext ctx)
        {
            return "PROGRESS";
        }

        // *****************
        // Support functions
        // *****************

        internal enum Compare
        {
            EQ,
            NE,
            GT,
            GE,
            LT,
            LE
        }
        /// <summary>
        /// Test DoCompare for two objects.
        /// </summary>
        /// <seealso cref= #compare(object, object) </seealso>
        internal static bool? DoCompare(object left, object right, Compare test)
        {
            int? result = DoCompare(left, right);
            if (result == null)
            {
                if (test == Compare.NE)
                {
                    return true;
                }
                return null;
            }
            switch (test)
            {
                case Compare.EQ:
                    return result == 0;
                case Compare.GE:
                    return result >= 0;
                case Compare.GT:
                    return result > 0;
                case Compare.LE:
                    return result <= 0;
                case Compare.LT:
                    return result < 0;
                case Compare.NE:
                    return result != 0;
                default:
                    throw new System.ArgumentException("Undefined state for Compare object");
            }            
        }

        /// <summary>
        /// Use compareTo() from String, Integer, or Float for two objects. Returns 0 for comparison of two nulls. Returns null
        /// if only one of the two is null.
        /// </summary>
        private static int? DoCompare(object left, object right)
        {            
            if (left == null && right == null)
            {
                return 0;
            }
            if (left == null || right == null)
            {
                return null;
            }
            if ((left is bool) && (right is bool))
            {
                return Convert.ToInt32(left) - Convert.ToInt32(right);
            }
            if ((left is string) && (right is string))
            {
                return CompareStringHelper(left).CompareTo(CompareStringHelper(right));
            }
            if ((IsNumber(left)) && (IsNumber(right)))
            {
                double? fl = Convert.ToDouble(left);
                double? fr = Convert.ToDouble(right);
                return Nullable.Compare(fl, fr);
            }
            throw new ProEvalException("Incompatible data types in comparison expression.");
        }

        internal static string CompareStringHelper(object o)
        {
            // Lowercase and right-trim the string for comparison, like Progress does.
            return StringFuncs.Rtrim(((string)o).ToLower());
        }

        internal static float? Decimal(object o)
        {      
            switch (o)
            {
                case null:
                    return null;                
                case string s:
                    return Convert.ToSingle(GetNumber(s));
                case bool b:
                    return b ? 1f : 0f;
            }
            if (IsNumber(o))
            {
                return Convert.ToSingle(o);
            }
            throw new ProEvalException("Error converting to DECIMAL.");
        }

        internal static string Entry(object oa, object ob, object odelim)
        {
            if (oa == null || ob == null)
            {
                return null;
            }
            int pos = GetInt(oa);
            string b = GetString(ob);
            string delim;
            if (odelim == null)
            {
                delim = ",";
            }
            else
            {
                delim = GetString(odelim);
            }
            if (delim.Length == 0)
            {
                delim = " ";
            }
            // Progress position numbers start at 1
            if (pos < 1)
            {
                throw new ProEvalException("ENTRY function received non-positive number");
            }
            delim = delim.Substring(0, 1);            
            string[] array = Regex.Split(b, delim, RegexOptions.IgnoreCase);
            if (pos > array.Length)
            {
                return "";
            }
            return array[pos - 1];
        }

        internal static bool GetBool(object obj)
        {
            // Implicit conversion from int or ProString to bool
            // Note that Progress does /not/ do implicit conversion to bool from decimal.
            if (obj is string)
            {
                return ((string)obj).Length != 0;
            }
            if (obj is bool?)
            {
                return ((bool?)obj).Value;
            }
            if (obj is int?)
            {
                return ((int?)obj) != 0;
            }
            throw new ProEvalException("Unknown datatype passed to getBool");
        }

        internal static float GetFloat(object obj)
        {
            // Implicit conversion from int to float, but no others.
            if (obj is float?)
            {
                return ((float?)obj).Value;
            }
            if (obj is int?)
            {
                return ((int?)obj).Value;
            }
            throw new ProEvalException("Incompatible datatype");
        }

        internal static int GetInt(object obj)
        {
            // No implicit conversion to int.
            if (obj is int?)
            {
                return ((int?)obj).Value;
            }
            throw new ProEvalException("Incompatible datatype");
        }

        internal static object GetNumber(string str)
        {
            string nbr = str.Trim();
            if (nbr.Length == 0)
            {
                // Empty string returns 0
                return 0;
            }
            if (nbr.EndsWith("-", StringComparison.Ordinal))
            {
                // Progress allows negative numbers to be represented like: 256-
                nbr = "-" + nbr.Substring(0, nbr.Length - 1);
            }
            if (nbr.StartsWith("+", StringComparison.Ordinal))
            {
                nbr = nbr.Substring(1);
            }
            try
            {
                if (nbr.IndexOf('.') > -1)
                {
                    return float.Parse(nbr);
                }
                else
                {
                    return int.Parse(nbr);
                }
            }
            catch (System.FormatException)
            {
                throw new ProEvalException("Lexical cast to number from '" + str + "' failed");
            }
        }

        internal static string GetString(object obj)
        {
            // No implicit conversion to String.
            if (obj is string)
            {
                return (string)obj;
            }
            throw new ProEvalException("Incompatible datatype");
        }

        internal static int? Index(object x, object y, object z)
        {
            if (x == null || y == null)
            {
                return 0;
            }
            string a = GetString(x);
            string b = GetString(y);
            if (a.Length == 0 || b.Length == 0)
            {
                return 0;
            }
            int startIndex = 1;
            if (z != null)
            {
                startIndex = GetInt(z);
            }
            string source = a.ToLower();
            string target = b.ToLower();
            // Progress counts from one, returns zero if not found.
            // (Java counts from zero, returns -1 if not found.)
            return source.IndexOf(target, startIndex - 1, StringComparison.Ordinal) + 1;
        }

        internal static int? Integer(object o)
        {           
            switch (o)
            {
                case null:
                    return null;                                
                case string s:
                    return Convert.ToInt32(GetNumber(s));
                case bool b:
                    return b ? 1 : 0;
            }
            if (IsNumber(o))
            {                
                return Convert.ToInt32(o);
            }
            throw new ProEvalException("Error converting to INTEGER.");            
        }

        internal static string Lefttrim(object a, object b)
        {
            if (b != null)
            {
                string t = GetString(b);
                return StringFuncs.Ltrim(GetString(a), t);
            }
            return StringFuncs.Ltrim(GetString(a));
        }

        internal static int? Lookup(object x, object y, object z)
        {
            if (x == null || y == null)
            {
                return null;
            }
            string a = GetString(x);
            string b = GetString(y);
            if (a.Length == 0 && b.Length == 0)
            {
                return 1;
            }
            a = a.ToLower();
            b = b.ToLower();
            string delim;
            if (z == null)
            {
                delim = ",";
            }
            else
            {
                delim = ((string)z).ToLower();
            }            
            IList<string> expr = Regex.Split(a, delim).ToList();
            List<string> list = Regex.Split(b, delim).ToList();
            if (expr.Count == 1)
            {
                return list.IndexOf(a) + 1;
            }
            // From the docs:
            // If expression contains a delimiter, LOOKUP returns the beginning
            // of a series of entries in list.
            // For example, LOOKUP("a,b,c","x,a,b,c") returns a 2.
            int exprSize = expr.Count;
            int listSize = list.Count;
            for (int index = 0; index < listSize; ++index)
            {
                int end = index + exprSize;
                if (end >= listSize)
                {
                    return 0;
                }
                if (list.GetRange(index, end).Equals(expr))
                {
                    return index + 1;
                }
            }
            return 0;
        }

        internal static bool? Matches(object y, object z)
        {
            string a = GetString(y).ToLower();
            string b = GetString(z).ToLower();
            // Completion conditions
            if (b.Length == 1 && b[0] == '*')
            {
                return true;
            }
            if (a.Length == 0)
            {
                return b.Length == 0;
            }
            if (b.Length == 0)
            {
                return false;
            }

            // Match any single char
            if (b[0] == '.')
            {
                return Matches(a.Substring(1), b.Substring(1));
            }

            // Match any number of chars
            if (b[0] == '*')
            {
                return Matches(a, b.Substring(1)).Value || Matches(a.Substring(1), b).Value;
            }

            // Match an escaped char
            if (b[0] == '~')
            {
                return a[0] == b[1] && Matches(a.Substring(1), b.Substring(2)).Value;
            }

            // Match a single specific char
            return a[0] == b[0] && Matches(a.Substring(1), b.Substring(1)).Value;
        }

        internal static int? NumEntries(object a, object b)
        {
            string sa = GetString(a);
            if (sa.Length == 0)
            {
                return 0;
            }
            string sb;
            if (b != null)
            {
                sb = GetString(b);
            }
            else
            {
                sb = ",";
            }            
            return Regex.Split(sa, sb, RegexOptions.IgnoreCase).Length;
        }

        internal static object OpDivide(object left, object right)
        {
            if (left == null || right == null)
            {
                return null;
            }
            if ((left is int?) && (right is int?))
            {
                return (int?)left / (int?)right;
            }
            if (IsNumber(left) && IsNumber(right))
            {
                double? fl = Convert.ToDouble(left);
                double? fr = Convert.ToDouble(right);
                return fl.Value / fr.Value;
            }
            throw new ProEvalException("Incompatible data type in expression.");
        }

        internal static object OpMinus(object left, object right)
        {
            if (left == null || right == null)
            {
                return null;
            }
            if ((left is int?) && (right is int?))
            {
                return (int?)left - (int?)right;
            }
            if (IsNumber(left) && IsNumber(right))
            {
                double? fl = Convert.ToDouble(left);
                double? fr = Convert.ToDouble(right);
                return fl.Value - fr.Value;
            }
            throw new ProEvalException("Incompatible data type in expression.");
        }

        internal static object OpMultiply(object left, object right)
        {
            if (left == null || right == null)
            {
                return null;
            }
            if ((left is int?) && (right is int?))
            {
                return (int?)left * (int?)right;
            }
            if (IsNumber(left) && IsNumber(right))
            {
                double? fl = Convert.ToDouble(left);
                double? fr = Convert.ToDouble(right);
                return fl.Value * fr.Value;
            }
            throw new ProEvalException("Incompatible data type in expression.");
        }

        internal static object OpPlus(object left, object right)
        {
            if (left == null || right == null)
            {
                return null;
            }
            if ((left is string) && (right is string))
            {
                return (string)left + right;
            }
            if ((left is int?) && (right is int?))
            {
                return (int?)left + (int?)right;
            }
            if (IsNumber(left) && IsNumber(right))
            {
                double? fl = Convert.ToDouble(left);
                double? fr = Convert.ToDouble(right);
                return fl.Value + fr.Value;
            }
            throw new ProEvalException("Incompatible data type in expression.");
        }

        // TODO Verify if this not a duplicate of settings#getPropath()
        internal static string Propath(IProparseSettings settings)
        {
            StringBuilder bldr = new StringBuilder();
            bool delim = false;
            foreach (string p in settings.PropathAsList)
            {
                if (delim)
                {
                    bldr.Append(',');
                }
                bldr.Append(p);
                delim = true;
            }
            return bldr.ToString();
        }

        // Case-insensitive sourceString.replace(from, to).
        internal static string Replace(string source, string from, string to)
        {            
            return Regex.Replace(source, from, to, RegexOptions.IgnoreCase);            
        }

        internal static int? RIndex(object a, object b, object c)
        {
            // Notes: Progress counts from one, but Java counts from zero.
            // R-INDEX returns zero if not found, Java returns -1, so
            // adding 1 to Java's lastIndexOf() works the way we want.
            string source = GetString(a).ToLower();
            string target = GetString(b).ToLower();
            // If either string is empty, Progress returns zero
            if (source.Length == 0 || target.Length == 0)
            {
                return 0;
            }
            if (c != null)
            {
                return source.LastIndexOf(target, GetInt(c) - 1, StringComparison.Ordinal) + 1;
            }
            return source.LastIndexOf(target, StringComparison.Ordinal) + 1;
        }

        internal static string @String(object a)
        {
            if (a == null)
            {
                return "?";
            }
            if (a is bool)
            {
                return (bool)a ? "yes" : "no";
            }
            return a.ToString();
        }
        internal static string Substring(object a, object b, object c, SubstringType type)
        {
            string str = GetString(a);
            int pos = GetInt(b) - 1;
            if (pos >= str.Length)
            {
                return "";
            }
            int len = -1;
            if (c != null)
            {
                len = GetInt(c);
            }
            if (len == -1)
            {
                return str.Substring(pos);
            }
            int endpos = pos + len;
            if (endpos > str.Length)
            {
                endpos = str.Length;
            }
            return str.Substring(pos, endpos - pos);
        }

        internal enum SubstringType
        {
            CHARACTER,
            FIXED,
            COLUMN,
            RAW
        }

        private static bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
    }

}
