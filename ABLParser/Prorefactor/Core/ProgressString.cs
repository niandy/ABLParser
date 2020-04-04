using System.Text;

namespace ABLParser.Prorefactor.Core
{
    /// <summary>
    /// This class is for working with the text of Proparse's QSTRING nodes. Proparse's QSTRING nodes contain the string
    /// literal, including the delimiting quotation marks as well as any string attributes. This class will allow us to
    /// easily fetch and work with things like just the text portion, just the attributes portion, check if the delimiting
    /// quotes are single-quotes or double-quotes, etc.
    /// </summary>
    public class ProgressString
    {
        private readonly char quote;
        private readonly string text;
        private readonly string attributes;

        /// <summary>
        /// Constructor - should generally only be constructed by passing in the results of parser.getNodeText()
        /// </summary>
        public ProgressString(string quotedString)
        {
            quote = quotedString[0];
            int secondQuote = quotedString.LastIndexOf(quote);
            text = quotedString.Substring(1, secondQuote - 1);
            if (secondQuote < (quotedString.Length - 1))
            {
                attributes = quotedString.Substring(secondQuote + 1);
            }
            else
            {
                attributes = "";
            }
        }

        /// <summary>
        /// Get the string attributes, including the colon. </summary>
        public virtual string Attributes => attributes;

        /// <summary>
        /// Get the character quotation mark. </summary>
        public virtual char Quote => quote;

        /// <summary>
        /// Get the text stripped of quotes and attributes. </summary>
        public virtual string Text => text;

        /// <summary>
        /// Is this string translatable?
        /// </summary>
        /// <returns> True if translatable </returns>
        public virtual bool Trans => attributes.IndexOf('U') < 0 && attributes.IndexOf('u') < 0;

        public override string ToString()
        {
            StringBuilder buff = new StringBuilder();
            buff.Append(quote).Append(text).Append(quote).Append(attributes);
            return buff.ToString();
        }

        /// <summary>
        /// Convenience method to check if the first character of a String is a quote character. </summary>
        public static bool IsQuoted(string checkMe)
        {
            char c = checkMe[0];
            return c == '\'' || c == '"';
        }

        /// <summary>
        /// Strip attributes and quotes, if quoted. </summary>
        public static string Dequote(string orig)
        {
            if (IsQuoted(orig))
            {
                ProgressString pstring = new ProgressString(orig);
                return pstring.Text.Trim();
            }
            else
            {
                return orig;
            }
        }
    }
}
