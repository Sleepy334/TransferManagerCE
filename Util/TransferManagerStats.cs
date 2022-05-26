using ColossalFramework;
using System.Reflection;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferManagerStats
    {
        const int iTOTAL_MATERIAL = 255;

        public static StatsContainer[] s_Stats = new StatsContainer[256];

        public static void Init()
        {
            for (int i = 0; i < s_Stats.Length; i++)
            {
                s_Stats[i] = new StatsContainer((TransferReason)i);
            }

            CountExistingTransfers();
        }

        private static void CountExistingTransfers()
        {
            TransferManager manager = Singleton<TransferManager>.instance;

            // Reflect transfer offer fields.
            FieldInfo incomingOfferField = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo outgoingOfferField = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.Instance | BindingFlags.NonPublic);
            TransferOffer[] incomingOffers = incomingOfferField.GetValue(manager) as TransferOffer[];
            TransferOffer[] outgoingOffers = outgoingOfferField.GetValue(manager) as TransferOffer[];

            // Find offers to this building.
            if (incomingOffers != null && outgoingOffers != null)
            {
                for (int i = 0; i < incomingOffers.Length; ++i)
                {
                    // Calculate reason
                    long material = (i & 0xFFFFF800) >> 11;

                    if (incomingOffers[i].Amount > 0)
                    {
                        s_Stats[material].m_stats.TotalIncomingCount++;
                        s_Stats[material].m_stats.TotalIncomingAmount += incomingOffers[i].Amount;

                        s_Stats[iTOTAL_MATERIAL].m_stats.TotalIncomingCount++;
                        s_Stats[iTOTAL_MATERIAL].m_stats.TotalIncomingAmount += incomingOffers[i].Amount;
                    }

                    if (outgoingOffers[i].Amount > 0)
                    {
                        s_Stats[material].m_stats.TotalOutgoingCount++;
                        s_Stats[material].m_stats.TotalOutgoingAmount += outgoingOffers[i].Amount;

                        s_Stats[iTOTAL_MATERIAL].m_stats.TotalOutgoingCount++;
                        s_Stats[iTOTAL_MATERIAL].m_stats.TotalOutgoingAmount += outgoingOffers[i].Amount;
                    }
                }
            }
        }
    }
}
