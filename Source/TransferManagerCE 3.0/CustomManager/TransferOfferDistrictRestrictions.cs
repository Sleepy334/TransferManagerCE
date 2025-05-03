using System.Collections.Generic;
using TransferManagerCE.Settings;
using TransferManagerCE.TransferRules;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    public class TransferOfferDistrictRestrictions
    {
        // Districts settings
        private byte? m_district = null;
        private byte? m_area = null;
        private DistrictRestrictionSettings.PreferLocal m_preferLocal = DistrictRestrictionSettings.PreferLocal.Unknown;
        private HashSet<DistrictData>? m_actualDistricts = null;
        private HashSet<DistrictData>? m_allowedDistricts = null;
        private static readonly HashSet<DistrictData> s_emptyDistrictSet = new HashSet<DistrictData>(); // Used when empty

        // -------------------------------------------------------------------------------------------
        public void ResetCachedValues()
        {
            // Districts settings
            m_district = null;
            m_area = null;
            m_preferLocal = DistrictRestrictionSettings.PreferLocal.Unknown;
            m_actualDistricts = null;
            m_allowedDistricts = null;
        }

        // -------------------------------------------------------------------------------------------
        public byte GetDistrict(CustomTransferOffer offer)
        {
            if (m_district is null)
            {
                m_district = DistrictManager.instance.GetDistrict(offer.Position);
            }
            return m_district.Value;
        }

        // -------------------------------------------------------------------------------------------
        public byte GetArea(CustomTransferOffer offer)
        {
            if (m_area is null)
            {
                // Handle non-building types as well
                if (offer.LocalPark > 0)
                {
                    m_area = (byte)offer.LocalPark;
                }
                else if (offer.m_object.Type == InstanceType.Park)
                {
                    m_area = offer.Park;
                }
                else
                {
                    m_area = DistrictManager.instance.GetPark(offer.Position);
                }
            }
            return m_area.Value;
        }

        // -------------------------------------------------------------------------------------------
        public HashSet<DistrictData> GetActualDistrictList(CustomTransferOffer offer)
        {
            if (m_actualDistricts is null)
            {
                // get districts
                byte district = offer.GetDistrict();
                byte area = offer.GetArea();

                if (district != 0 || area != 0)
                {
                    m_actualDistricts = new HashSet<DistrictData>();

                    if (district != 0)
                    {
                        m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.District, district));
                    }
                    if (area != 0)
                    {
                        m_actualDistricts.Add(new DistrictData(DistrictData.DistrictType.Park, area));
                    }
                }
                else
                {
                    // Use the empty set so we save on memeory allocations
                    m_actualDistricts = s_emptyDistrictSet;
                }
            }

            return m_actualDistricts;
        }

        // -------------------------------------------------------------------------------------------
        public HashSet<DistrictData> GetAllowedDistrictList(CustomTransferOffer offer, CustomTransferReason.Reason material)
        {
            if (m_allowedDistricts is null)
            {
                BuildingSettings? settings = BuildingSettingsStorage.GetSettings(offer.GetBuilding());
                if (settings is not null && settings.HasRestrictionSettings())
                {
                    int iRestrictionId = BuildingRuleSets.GetRestrictionId(offer.GetBuildingType(), material, offer.IsIncoming());
                    RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                    if (restrictions is not null)
                    {
                        if (offer.IsIncoming())
                        {
                            m_allowedDistricts = restrictions.m_incomingDistrictSettings.GetAllowedDistricts(offer.GetBuilding(), GetDistrict(offer), GetArea(offer));
                        }
                        else
                        {
                            m_allowedDistricts = restrictions.m_outgoingDistrictSettings.GetAllowedDistricts(offer.GetBuilding(), GetDistrict(offer), GetArea(offer));
                        }
                    }
                }

                // Use the static empty set instead so we dont use extra memory
                if (m_allowedDistricts is null)
                {
                    m_allowedDistricts = s_emptyDistrictSet;
                }
            }
            return m_allowedDistricts;
        }

        // -------------------------------------------------------------------------------------------
        public DistrictRestrictionSettings.PreferLocal GetDistrictRestriction(CustomTransferOffer offer, CustomTransferReason.Reason material)
        {
            if (m_preferLocal == DistrictRestrictionSettings.PreferLocal.Unknown)
            {
                // Default is all districts
                m_preferLocal = DistrictRestrictionSettings.PreferLocal.AllDistricts;

                // Local setting
                ushort buildingId = offer.GetBuilding();
                if (buildingId != 0)
                {
                    BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
                    if (settings is not null)
                    {
                        BuildingType eBuildingType = offer.GetBuildingType();
                        if (offer.IsIncoming())
                        {
                            if (BuildingRuleSets.HasIncomingDistrictRules(eBuildingType, material))
                            {
                                RestrictionSettings? restrictions = BuildingSettingsStorage.GetRestrictions(buildingId, eBuildingType, material, offer.IsIncoming());
                                if (restrictions is not null)
                                {
                                    m_preferLocal = restrictions.m_incomingDistrictSettings.m_iPreferLocalDistricts;
                                }
                            }
                        }
                        else
                        {
                            if (BuildingRuleSets.HasOutgoingDistrictRules(eBuildingType, material))
                            {
                                RestrictionSettings? restrictions = BuildingSettingsStorage.GetRestrictions(buildingId, eBuildingType, material, offer.IsIncoming());
                                if (restrictions is not null)
                                {
                                    m_preferLocal = restrictions.m_outgoingDistrictSettings.m_iPreferLocalDistricts;
                                }
                            }
                        }
                    }
                }
            }

            return m_preferLocal;
        }
    }
}
