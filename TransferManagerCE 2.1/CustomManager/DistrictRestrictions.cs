using System;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    public class DistrictRestrictions
    {
        // Services subject to global prefer local services:
        public static bool IsGlobalDistrictRestrictionsSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Cash:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Dead:
                    return true;

                // Goods subject to prefer local:
                // -none- it is too powerful, city will fall apart

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        public static bool CanTransfer(CustomTransferOffer offerIn, CustomTransferOffer offerOut, TransferReason material)
        {
            // 4 is the maximum value for warehouse matching eg. 2/2.
            // This allows warehouse transfer between districts but only when both really want it.
            const int PREFER_LOCAL_DISTRICT_THRESHOLD = 4;     

            // Check if it is an Import/Export
            if (offerIn.IsOutside() || offerOut.IsOutside())
            {
                // Don't restrict Import/Export with district restrictions
                return true;
            }

            // Find the maximum setting from both buildings
            RestrictionSettings.PreferLocal eInBuildingLocalDistrict = offerIn.GetDistrictRestriction(true, material);
            RestrictionSettings.PreferLocal eOutBuildingLocalDistrict = offerOut.GetDistrictRestriction(false, material);

            // Check max priority of both buildings
            RestrictionSettings.PreferLocal ePreferLocalDistrict = (RestrictionSettings.PreferLocal)Math.Max((int)eInBuildingLocalDistrict, (int)eOutBuildingLocalDistrict);
            switch (ePreferLocalDistrict)
            {
                case RestrictionSettings.PreferLocal.ALL_DISTRICTS:
                    {
                        return true;// Any match is fine
                    }
                case RestrictionSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                    {
                        // Combined priority needs to be equal or greater than PREFER_LOCAL_DISTRICT_THRESHOLD
                        int priority = offerIn.Priority + offerOut.Priority;
                        if (priority >= PREFER_LOCAL_DISTRICT_THRESHOLD)
                        {
                            return true; // Priority is high enough to allow match
                        }
                        break;
                    }
                case RestrictionSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case RestrictionSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case RestrictionSettings.PreferLocal.UNKNOWN:
                    {
                        Debug.Log("Error district restriction unknown");
                        return true;
                    }
            }

            // get respective districts
            HashSet<DistrictData> incomingActualDistricts = offerIn.GetActualDistrictList();
            HashSet<DistrictData> outgoingActualDistricts = offerOut.GetActualDistrictList();

            if (SaveGameSettings.GetSettings().PreferLocalService && ePreferLocalDistrict == RestrictionSettings.PreferLocal.PREFER_LOCAL_DISTRICT)
            {
                // If setting is prefer local district and it's from the global settings then also allow matching Active offers, where it's not our vehicle
                if (offerIn.Active && incomingActualDistricts.Count == 0)
                {
                    // Active side is outside any district so allow it
                    return true;
                }
                else if (offerOut.Active && outgoingActualDistricts.Count == 0)
                {
                    // Active side is outside any district so allow it
                    return true;
                }
            }

            // Now we check allowed districts against actual districts for both sides
            bool bInIsValid = false;
            switch (eInBuildingLocalDistrict)
            {
                case RestrictionSettings.PreferLocal.ALL_DISTRICTS:
                    {
                        bInIsValid = true;
                        break;
                    }
                case RestrictionSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                case RestrictionSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        HashSet<DistrictData> incomingAllowedDistricts = offerIn.GetAllowedIncomingDistrictList(material);
                        bInIsValid = DistrictData.Intersect(incomingAllowedDistricts, outgoingActualDistricts);
                        break;
                    }
                case RestrictionSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        HashSet<DistrictData> incomingBannedDistricts = offerIn.GetAllowedIncomingDistrictList(material);
                        bInIsValid = !DistrictData.Intersect(incomingBannedDistricts, outgoingActualDistricts); // Valid if they DON'T intersect
                        break;
                    }
            }

            bool bOutIsValid = false;
            if (bInIsValid)
            {
                // Finally check outgoing district restrictions are fine
                switch (eOutBuildingLocalDistrict)
                {
                    case RestrictionSettings.PreferLocal.ALL_DISTRICTS:
                        {
                            bOutIsValid = true;
                            break;
                        }
                    case RestrictionSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                    case RestrictionSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                        {
                            HashSet<DistrictData> outgoingAllowedDistricts = offerOut.GetAllowedOutgoingDistrictList(material);
                            bOutIsValid = DistrictData.Intersect(outgoingAllowedDistricts, incomingActualDistricts);
                            break;
                        }
                    case RestrictionSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                        {
                            HashSet<DistrictData> outgoingBannedDistricts = offerOut.GetAllowedOutgoingDistrictList(material);
                            bOutIsValid = !DistrictData.Intersect(outgoingBannedDistricts, incomingActualDistricts); // Valid if they DON'T intersect
                            break;
                        }
                }
            }

            return (bInIsValid && bOutIsValid);
        }

        // Determine current local district setting by combining building and global settings
        public static RestrictionSettings.PreferLocal GetPreferLocal(bool bIncoming, TransferReason material, CustomTransferOffer offer)
        {
            RestrictionSettings.PreferLocal ePreferLocalDistrict = RestrictionSettings.PreferLocal.ALL_DISTRICTS;

            // Global setting is only applied to certain services as it is too powerful otherwise.
            if (bIncoming && SaveGameSettings.GetSettings().PreferLocalService && IsGlobalDistrictRestrictionsSupported(material))
            {
                ePreferLocalDistrict = RestrictionSettings.PreferLocal.PREFER_LOCAL_DISTRICT;
            }

            // Local setting
            ushort buildingId = offer.GetBuilding();
            if (buildingId != 0)
            {
                BuildingType eBuildingType = offer.GetBuildingType();
                if (bIncoming)
                {
                    if (BuildingRuleSets.HasIncomingDistrictRules(eBuildingType, material))
                    {
                        RestrictionSettings settings = BuildingSettingsStorage.GetRestrictions(buildingId, offer.GetBuildingType(), material);
                        ePreferLocalDistrict = (RestrictionSettings.PreferLocal)Math.Max((int)settings.m_iPreferLocalDistrictsIncoming, (int)ePreferLocalDistrict);
                    }
                }
                else
                {
                    if (BuildingRuleSets.HasOutgoingDistrictRules(eBuildingType, material))
                    {
                        RestrictionSettings settings = BuildingSettingsStorage.GetRestrictions(buildingId, offer.GetBuildingType(), material);
                        ePreferLocalDistrict = (RestrictionSettings.PreferLocal)Math.Max((int)settings.m_iPreferLocalDistrictsOutgoing, (int)ePreferLocalDistrict);
                    }
                }
            }

            return ePreferLocalDistrict;
        }
    }
}