using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public class StringFuncs
    {

        private StringFuncs()
        {
            // No-op
        }

        /// <summary>
        /// Escape line breaks with backslashes. Replaces \ with \\, newline with \n, and linefeed with \r. Specifically
        /// written for the listing file, which uses one line per record, and cannot have extra line breaks in the output.
        /// </summary>
        public static string EscapeLineBreaks(string s)
        {
            string ret = s.Replace("\\", "\\\\");
            ret = ret.Replace("\n", "\\n");
            ret = ret.Replace("\r", "\\r");
            return ret;
        }

        public static string Ltrim(string s)
        {
            char[] c = s.ToCharArray();
            int begin = 0;
            int end = c.Length;
            while (begin < end && char.IsWhiteSpace(c[begin]))
            {
                ++begin;
            }
            return s.Substring(begin);
        }

        public static string Ltrim(string s, string t)
        {
            HashSet<char> trimSet = SetOfMatchChars(t);
            char[] c = s.ToCharArray();
            int begin = 0;
            int end = c.Length;
            while (begin < end && trimSet.Contains(c[begin]))
            {
                ++begin;
            }
            return s.Substring(begin);
        }

        /// <summary>
        /// Given a QSTRING node's text: strip string attributes, strip quotes, and trim. </summary>
        public static string QstringStrip(string s)
        {
            if (s.Length < 2)
            {
                return s;
            }
            char quoteType = s[0];
            if (quoteType != '"' && quoteType != '\'')
            {
                return s;
            }
            int endQuotePos = s.LastIndexOf(quoteType);
            if (endQuotePos < 1)
            {
                return s;
            }
            return s.Substring(1, endQuotePos - 1);
        }

        internal static HashSet<char> SetOfMatchChars(string s)
        {
            HashSet<char> set = new HashSet<char>();
            foreach (char c in s.ToLower().ToCharArray())
            {
                set.Add(c);
            }
            foreach (char c in s.ToUpper().ToCharArray())
            {
                set.Add(c);
            }
            return set;
        }

        public static string StripComments(string orig)
        {
            StringBuilder bldr = new StringBuilder();
            int it = 0;
            int commentLevel = 0;
            char[] c = orig.ToCharArray();
            int end = c.Length;
            char prev;
            char curr = (char)0;
            char next;
            while (it < end)
            {
                prev = curr;
                curr = c[it];
                ++it;
                next = it < end ? c[it] : (char)0;
                if (commentLevel > 0 && curr == '/' && prev == '*')
                {
                    --commentLevel;
                }
                else if (curr == '/' && next == '*')
                {
                    ++commentLevel;
                }
                else if (commentLevel == 0)
                {
                    bldr.Append(curr);
                }
            }
            return bldr.ToString();
        }

        public static string Rtrim(string s)
        {
            char[] c = s.ToCharArray();
            int end = c.Length;
            while (end > 0 && char.IsWhiteSpace(c[end - 1]))
            {
                end--;
            }
            return s.Substring(0, end);
        }


        public static string Rtrim(string s, string t)
        {
            HashSet<char> trimSet = SetOfMatchChars(t);
            char[] c = s.ToCharArray();
            int end = c.Length;
            while (end > 0 && trimSet.Contains(c[end - 1]))
            {
                end--;
            }
            return s.Substring(0, end);
        }

        public static string Trim(string s, string t)
        {
            HashSet<char> trimSet = SetOfMatchChars(t);
            char[] c = s.ToCharArray();
            int begin = 0;
            int end = c.Length;
            while (begin < end && trimSet.Contains(c[begin]))
            {
                ++begin;
            }
            while (end >= begin && trimSet.Contains(c[end - 1]))
            {
                --end;
            }
            return s.Substring(begin, end - begin);
        }

    }

}
