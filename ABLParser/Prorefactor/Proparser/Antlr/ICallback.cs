using ABLParser.Prorefactor.Core;

namespace ABLParser.Prorefactor.Proparser.Antlr
{
    public interface ICallback<T>
    {

        /// <returns> The result of processing all the nodes </returns>
        T Result { get; }

        /// <summary>
        /// Callback action
        /// </summary>
        /// <param name="node"> Node to be visited </param>
        /// <returns> True if children have to be visited </returns>
        bool VisitNode(JPNode node);

    }

}
