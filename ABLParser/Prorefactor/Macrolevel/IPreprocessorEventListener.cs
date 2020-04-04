using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Macrolevel
{
    /// <summary>
    /// Catch preprocessor, lexer and post-lexer events in order to store preprocessor variables, include file references,
    /// analyze-* statements, ...
    /// </summary>
    public interface IPreprocessorEventListener
    {
        void MacroRef(int line, int column, string macroName);
        void MacroRefEnd();
        void Include(int line, int column, int currentFile, string incFile);
        void IncludeArgument(string argName, string value, bool undefined);
        void IncludeEnd();
        void Define(int line, int column, string name, string value, MacroDefinitionType type);
        void Undefine(int line, int column, string name);
        void PreproIf(int line, int column, bool value);
        void PreproElse(int line, int column);
        void PreproElseIf(int line, int column);
        void PreproEndIf(int line, int column);
        void AnalyzeSuspend(string str, int line);
        void AnalyzeResume(int line);
    }

}
