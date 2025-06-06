namespace TransferManagerCE
{
    public class StatsHeader : StatsBase
    {
        public StatsHeader(string sDescription) : 
            base(sDescription, "")
        {
        }

        public override bool IsHeader()
        {
            return true;
        }
    }
}
