namespace TransferManagerCE
{
    public class MatchStatsSeparator : MatchStatsData
    {
        public MatchStatsSeparator()
        {
        }

        public override bool IsSeparator()
        {
            return true;
        }

        public override string GetMaterialDescription()
        {
            return "";
        }
    }
}