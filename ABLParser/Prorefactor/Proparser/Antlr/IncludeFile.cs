using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{


    /// <summary>
    /// <ul>
    /// <li>Every time the lexer has to scan an include file, we create an IncludeFile object, for managing include file arguments and preprocessor scopes.</li>
    /// <li>We keep an InputSource object, which has an input stream.</li>
    /// <li>Each IncludeFile object will have one or more InputSource objects.</li>
    /// <li>The bottom InputSource object for an IncludeFile is the input for the include file itself.</li>
    /// </ul>
    /// </summary>
    public class IncludeFile
    {        
        private static readonly ILog LOGGER = LogManager.GetLogger(typeof(IncludeFile));

        private readonly IDictionary<string, string> defdNames = new Dictionary<string, string>();
        private readonly LinkedList<InputSource> inputVector = new LinkedList<InputSource>();

        private readonly IList<string> numberedArgs = new List<string>();
        private readonly IDictionary<string, string> namedArgs = new Dictionary<string, string>();
        private readonly IList<NamedArgument> namedArgsIn = new List<NamedArgument>();

        public IncludeFile(string referencedWithName, InputSource @is)
        {
            LOGGER.Debug(String.Format("New IncludeFile object for '{0}'", referencedWithName));

            inputVector.AddLast(@is);
            // {0} must return the name that this include file was referenced with.
            numberedArgs.Add(referencedWithName);
        }

        public virtual void AddInputSource(InputSource source)
        {
            inputVector.AddLast(source);
        }

        public virtual InputSource Pop()
        {
            if (inputVector.Count > 1)
            {
                inputVector.RemoveLast();
                return inputVector.Last.Value;
            }
            else
            {
                return null;
            }
        }

        public virtual InputSource LastSource
        {
            get
            {
                return inputVector.Last.Value;
            }
        }

        public virtual void AddArgument(string arg)
        {
            numberedArgs.Add(arg);
        }

        public virtual void AddNamedArgument(string name, string arg)
        {
            namedArgsIn.Add(new NamedArgument(name, arg));
            string lname = name.ToLower();
            // The first one defined is the one that gets used
            if (!namedArgs.ContainsKey(lname))
            {
                namedArgs[lname] = arg;
            }
            // Named include arguments can also be referenced by number.
            numberedArgs.Add(arg);
        }

        public virtual string AllNamedArgs
        {
            get
            {
                StringBuilder @out = new StringBuilder();
                foreach (NamedArgument arg in namedArgsIn)
                {
                    if (@out.Length > 0)
                    {
                        @out.Append(' ');
                    }
                    @out.Append('&').Append(arg.Name).Append("=\"");
                    // Parameters are read again by the lexer, so double quotes have to be escaped
                    @out.Append(arg.Arg.Replace("\"", "\"\""));
                    @out.Append("\"");
                }
                return @out.ToString();
            }
        }

        /// <summary>
        /// Returns space-separated list of all include arguments
        /// </summary>
        public virtual string AllArguments
        {
            get
            {
                if (numberedArgs.Count <= 1)
                {
                    return "";
                }

                StringBuilder sb = new StringBuilder();
                // Note: starts from 1. Doesn't include arg[0], which is the filename.
                foreach (string str in ((List<string>)numberedArgs).GetRange(1, numberedArgs.Count))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(str);
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// Get value of numbered argument (i.e. {&amp;2})
        /// </summary>
        /// <param name="num"> Arg number </param>
        /// <returns> String value </returns>
        public virtual string GetNumberedArgument(int num)
        {
            if (num >= numberedArgs.Count)
            {
                return "";
            }
            else
            {
                return numberedArgs[num];
            }
        }

        /// <summary>
        /// Get value of named argument (i.e. {&amp;FOO}).
        /// </summary>
        /// <param name="name"> Arg name. If blank, returns first blank (undefined) named arg. </param>
        /// <returns> null if not found. </returns>
        public virtual string GetNamedArg(string name)
        {
            // If name is blank, return the first blank (undefined) named argument (if any).
            if (name.Length == 0)
            {
                foreach (NamedArgument nargin in namedArgsIn)
                {
                    if (nargin.Name.Length == 0)
                    {
                        return nargin.Arg;
                    }
                }
                return null;
            }
            namedArgs.TryGetValue(name.ToLower(), out string arg);
            return arg;
        }

        /// <summary>
        /// Returns value of scope-defined variable
        /// </summary>
        public virtual string GetValue(string name)
        {
            defdNames.TryGetValue(name, out string val);
            return val;
        }

        public virtual bool IsNameDefined(string name)
        {
            return defdNames.ContainsKey(name);
        }

        public virtual void ScopeDefine(string name, string value)
        {
            defdNames[name] = value;
        }

        public virtual void RemoveVariable(string name)
        {
            defdNames.Remove(name);
        }

        internal virtual bool UndefNamedArg(string name)
        {
            string lname = name.ToLower();
            // Find the first one and clobber it
            bool found = false;
            foreach (NamedArgument nargin in namedArgsIn)
            {
                if (nargin.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Erase the argument name, which seems to be what Progress does.
                    nargin.EraseName();
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return false;
            }
            // Now see if that named argument got assigned more than once
            found = false;
            foreach (NamedArgument nargin in namedArgsIn)
            {
                if (nargin.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    namedArgs[lname] = nargin.Arg;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                namedArgs.Remove(lname);
            }
            return true;
        }

        private sealed class NamedArgument
        {
            public string Name { get; private set; }
            public string Arg { get; private set; }

            public void EraseName() => Name = "";

            internal NamedArgument(string name, string arg)
            {
                this.Name = name;
                this.Arg = arg;
            }
        }

    }

}
