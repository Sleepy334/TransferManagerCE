using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE;

public class StatData
{
    public StatData()
    {

    }

    public StatData(StatData oSecond)
    {
        TotalMatches = oSecond.TotalMatches;
        TotalMatchAmount = oSecond.TotalMatchAmount;
        TotalIncomingCount = oSecond.TotalIncomingCount;
        TotalIncomingAmount = oSecond.TotalIncomingAmount;
        TotalOutgoingCount = oSecond.TotalOutgoingCount;
        TotalOutgoingAmount = oSecond.TotalOutgoingAmount;
    }

    public int TotalMatches = 0;
    public int TotalMatchAmount = 0;
    public int TotalIncomingCount = 0;
    public int TotalIncomingAmount = 0;
    public int TotalOutgoingCount = 0;
    public int TotalOutgoingAmount = 0;
}