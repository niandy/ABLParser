namespace ABLParser.Prorefactor.Core
{
    public class JPNodeMetrics
    {
        private readonly int loc;
        private readonly int comments;

        public JPNodeMetrics(int loc, int comments)
        {
            this.loc = loc;
            this.comments = comments;
        }

        public virtual int Loc => loc;
        public virtual int Comments => comments;
    }

}
