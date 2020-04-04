using System;
using System.Collections.Generic;
using System.Text;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public interface IPreprocessor
    {

        /// <summary>
        /// Implementation of the DEFINED preprocessor function </summary>
        /// <param name="argName"> </param>
        /// <returns> Either 0, 1, 2 or 3 </returns>
        string Defined(string argName);

        /// <summary>
        /// Implementation of &amp;GLOBAL-DEFINE preprocessor function </summary>
        /// <param name="argName"> Variable name </param>
        /// <param name="argVal"> And value </param>
        void DefGlobal(string argName, string argVal);

        /// <summary>
        /// Implementation of &amp;SCOPED-DEFINE preprocessor function </summary>
        /// <param name="argName"> Variable name </param>
        /// <param name="argVal"> And value </param>
        void DefScoped(string argName, string argVal);

        /// <summary>
        /// Returns the value of the n-th argument (in include file) </summary>
        /// <param name="argNum"> </param>
        /// <returns> Empty string if argument not defined, otherwise its value </returns>
        string GetArgText(int argNum);

        /// <summary>
        /// Returns the value of a preprocessor variable </summary>
        /// <param name="argName"> </param>
        /// <returns> Empty string if variable not defined, otherwise its value </returns>
        string GetArgText(string argName);

        /// <summary>
        /// Implementation of &amp;UNDEFINE preprocessor function </summary>
        /// <param name="argName"> </param>
        void Undef(string argName);

        /// <summary>
        /// Implementation of &amp;ANALYZE-SUSPEND </summary>
        /// <param name="analyzeSuspend"> Attributes </param>
        void AnalyzeSuspend(string analyzeSuspend);

        /// <summary>
        /// Implementation of &amp;ANALYZE-RESUME 
        /// </summary>
        void AnalyzeResume();
    }

}
