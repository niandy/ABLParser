using ABLParser.Prorefactor.Refactor;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Proparser
{

	public class ClassFinder
	{
		private static readonly ILog LOGGER = LogManager.GetLogger(typeof(ClassFinder));

		private RefactorSession session;
		private IList<string> paths = new List<string>();
		private IDictionary<string, string> namesMap = new Dictionary<string, string>();

		public ClassFinder(RefactorSession session)
		{
			this.session = session;
		}

		/// <summary>
		/// Add a USING class name or path glob to the class finder.
		/// </summary>
		internal virtual void AddPath(string nodeText)
		{
			LOGGER.Debug($"Entering addPath {nodeText}");
			string dequoted = Dequote(nodeText);
			if (dequoted.Length == 0)
			{
				return;
			}
			if (dequoted.EndsWith("*", StringComparison.Ordinal))
			{
				paths.Add(dequoted.Replace('.', '/').Substring(0, dequoted.Length - 1));
			}
			else
			{
				int dotPos = dequoted.LastIndexOf('.');
				string unqualified = dotPos > 0 ? dequoted.Substring(dotPos + 1) : dequoted;
				unqualified = unqualified.ToLower();
				// First match takes precedence.
				if (!namesMap.ContainsKey(unqualified))
				{
					namesMap[unqualified] = dequoted;
				}
			}
		}

		/// <summary>
		/// Returns a string with quotes and string attributes removed. Can't just use StringFuncs.qstringStrip because a class
		/// name might have embedded quoted text, to quote embedded spaces and such in the file name. The embedded quotation
		/// marks have to be stripped too.
		/// </summary>
		internal static string Dequote(string s1)
		{
			StringBuilder s2 = new StringBuilder();
			int len = s1.Length;
			char[] c1 = s1.ToCharArray();
			int numQuotes = 0;
			for (int i = 0; i < len; ++i)
			{
				char c = c1[i];
				if (c == '"' || c == '\'')
				{
					// If we have a colon after a quote, assume we have string
					// attributes at the end of a quoted class name, and we're done.
					if (++numQuotes > 1 && i + 1 < len && c1[i + 1] == ':')
					{
						break;
					}
				}
				else
				{
					s2.Append(c);
				}
			}
			return s2.ToString();
		}

		/// <summary>
		/// Find a class file for a *qualified* class name.
		/// </summary>
		internal virtual string FindClassFile(string qualClassName)
		{
			string slashName = qualClassName.Replace('.', '/');
			return session.FindFile(slashName + ".cls");
		}

		/// <summary>
		/// Lookup a qualified class name on the USING list and/or PROPATH:<ul>
		/// <li>If input name is already qualified, just returns that name dequoted</li>
		/// <li>Checks for explicit USING</li>
		/// <li>Checks for USING globs on PROPATH</li>
		/// <li>Checks for "no package" class file on PROPATH</li>
		/// <li>Returns empty String if all of the above fail</li>
		/// </ul>
		/// </summary>
		internal virtual string Lookup(string rawRefName)
		{
			LOGGER.Debug("Entering lookup {rawRefName}");
			string dequotedName = Dequote(rawRefName);

			// If already qualified, then return the dequoted name, no check against USING.
			if (dequotedName.Contains("."))
			{
				return dequotedName;
			}

			// Check if USING class name, or if the class file has already been found.			
			if (namesMap.TryGetValue(dequotedName.ToLower(), out string ret))
			{
				return ret;
			}

			// Check USING package globs and classes injected in RefactorSession
			foreach (string path in paths)
			{
				if (session.GetTypeInfo(path.Replace('/', '.') + dequotedName) != null)
				{
					return path.Replace('/', '.') + dequotedName;
				}
			}

			// Check USING package globs and files on the PROPATH.
			string withExtension = dequotedName + ".cls";
			foreach (string path in paths)
			{
				string classFile = session.FindFile(path + withExtension);
				if (classFile.Length != 0)
				{
					ret = path.Replace('/', '.') + dequotedName;
					namesMap[dequotedName.ToLower()] = ret;
					return ret;
				}
			}

			// The last chance is for a "no package" name in RefactorSession and on the path
			if (session.GetTypeInfo(dequotedName) != null)
			{
				return dequotedName;
			}
			if (session.FindFile(dequotedName + ".cls").Length > 0)
			{
				namesMap[dequotedName.ToLower()] = dequotedName;
				return dequotedName;
			}

			// No class source was found, return empty String.
			return "";
		}

	}

}
