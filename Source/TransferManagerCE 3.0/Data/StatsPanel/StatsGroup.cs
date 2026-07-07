using SleepyCommon;

namespace TransferManagerCore
{
    public class StatsGroup : StatsBase
    {
        public StatsGroup(string sDescription, string sValue) : base(sDescription, sValue)
        {
            m_color = KnownColor.lightGrey;
        }
    }
}
