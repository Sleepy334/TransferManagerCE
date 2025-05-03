using System.Collections.Generic;
using TransferManagerCE.TransferRules;

namespace TransferManagerCE.CustomManager
{
    public class TransferOfferBuildingRestrictions
    {
        // Building restrictions
        private HashSet<ushort>? m_allowedBuildings = null;
        private static readonly HashSet<ushort> s_emptyBuildingSet = new HashSet<ushort>(); // Used when empty

        // -------------------------------------------------------------------------------------------
        public void ResetCachedValues()
        {
            m_allowedBuildings = null;
        }

        // -------------------------------------------------------------------------------------------
        public HashSet<ushort> GetAllowedBuildingList(CustomTransferOffer offer, CustomTransferReason.Reason material)
        {
            if (m_allowedBuildings is null)
            {
                BuildingSettings? settings = BuildingSettingsStorage.GetSettings(offer.GetBuilding());
                if (settings is not null && settings.HasRestrictionSettings())
                {
                    int iRestrictionId = BuildingRuleSets.GetRestrictionId(offer.GetBuildingType(), material, offer.IsIncoming());
                    if (iRestrictionId != -1)
                    {
                        RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                        if (restrictions is not null)
                        {
                            if (offer.IsIncoming())
                            {
                                if (restrictions.m_incomingBuildingSettings.HasBuildingRestrictions())
                                {
                                    // Take a copy for thread safety
                                    m_allowedBuildings = restrictions.m_incomingBuildingSettings.GetBuildingRestrictionsCopy();
                                }
                            }
                            else
                            {
                                if (restrictions.m_outgoingBuildingSettings.HasBuildingRestrictions())
                                {
                                    // Take a copy for thread safety
                                    m_allowedBuildings = restrictions.m_outgoingBuildingSettings.GetBuildingRestrictionsCopy();
                                }
                            }
                        }
                    }
                }

                // Use the static empty set to save memory
                if (m_allowedBuildings is null)
                {
                    m_allowedBuildings = s_emptyBuildingSet;
                }
            }
            return m_allowedBuildings;
        }
    }
}
