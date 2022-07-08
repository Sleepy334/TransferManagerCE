using ColossalFramework;
using System.Reflection;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferManagerStats
    {
        public const int iSTATS_ARRAY_SIZE = TRANSFER_REASON_COUNT + 1;
        public const int iMATERIAL_TOTAL_LOCATION = TRANSFER_REASON_COUNT;

        public static StatsContainer[] s_Stats = new StatsContainer[iSTATS_ARRAY_SIZE];

        public static void Init()
        {
            if (s_Stats != null)
            {
                for (int i = 0; i < s_Stats.Length; i++)
                {
                    s_Stats[i] = new StatsContainer((TransferReason)i);
                }

                CountExistingTransfers();
            }
        }

        private static void CountExistingTransfers()
        {
            TransferManager manager = Singleton<TransferManager>.instance;

            // Reflect transfer offer fields.
            FieldInfo incomingAmountField = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo outgoingAmountField = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo incomingCountField = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo outgoingCountField = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);

            int[] incomingAmount = (int[])incomingAmountField.GetValue(manager);
            int[] outgoingAmount = (int[])outgoingAmountField.GetValue(manager);
            ushort[] incomingCount = (ushort[])incomingCountField.GetValue(manager);
            ushort[] outgoingCount = (ushort[])outgoingCountField.GetValue(manager);

            // Add counts from already existing transfers
            if (incomingAmount != null && outgoingAmount != null && incomingCount != null && outgoingCount != null)
            {
                for (int material = 0; material < TRANSFER_REASON_COUNT; material++)
                {
                    // Incoming
                    s_Stats[material].TotalIncomingCount += incomingCount[material];
                    s_Stats[material].TotalIncomingAmount += incomingAmount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingCount += incomingCount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalIncomingAmount += incomingAmount[material];

                    // Outgoing
                    s_Stats[material].TotalOutgoingCount += outgoingCount[material];
                    s_Stats[material].TotalOutgoingAmount += outgoingAmount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingCount += outgoingCount[material];
                    s_Stats[iMATERIAL_TOTAL_LOCATION].TotalOutgoingAmount += outgoingAmount[material];
                }
            }
        }
    }
}
