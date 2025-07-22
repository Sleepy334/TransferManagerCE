using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using TransferManagerCE.TransferRules;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.CustomManager.TransferManagerModes;

namespace TransferManagerCE.CustomManager
{
    public class DistrictRestrictions
    {
        // Services subject to global prefer local services:
        public static bool IsGlobalDistrictRestrictionsSupported(CustomTransferReason.Reason material)
        {
            switch (material)
            {
                case CustomTransferReason.Reason.Garbage:
                case CustomTransferReason.Reason.Crime:
                case CustomTransferReason.Reason.Crime2:
                case CustomTransferReason.Reason.Cash:
                case CustomTransferReason.Reason.Fire:
                case CustomTransferReason.Reason.Fire2:
                case CustomTransferReason.Reason.ForestFire:
                case CustomTransferReason.Reason.Sick:
                case CustomTransferReason.Reason.Sick2:
                case CustomTransferReason.Reason.Collapsed:
                // case CustomTransferReason.Reason.Collapsed2: Collapsed2 is artificially limited to P:1 so never gets high enough.
                case CustomTransferReason.Reason.ParkMaintenance:
                case CustomTransferReason.Reason.Mail:
                case CustomTransferReason.Reason.Mail2:
                case CustomTransferReason.Reason.Taxi:
                case CustomTransferReason.Reason.Dead:
                    return true;

                // Goods subject to prefer local:
                // -none- it is too powerful, city will fall apart

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        public static bool CanTransferGlobalPreferLocal(CustomTransferOffer offerIn, CustomTransferOffer offerOut, CustomTransferReason.Reason material, TransferMode mode)
        {
            if (SaveGameSettings.GetSettings().PreferLocalService && IsGlobalDistrictRestrictionsSupported(material))
            {
                // Check if the offers pass the priority test
                if (IsMatchAllowed(DistrictRestrictionSettings.PreferLocal.PreferLocalDistrict, offerIn, offerOut, material, mode))
                {
                    return true;
                }

                // When it's the global setting we allow matching service buildings that are outside any district or if the active building has local district settings
                if (offerIn.Active && ((offerIn.GetArea() == 0 && offerIn.GetDistrict() == 0) || offerIn.GetDistrictRestriction(material) != DistrictRestrictionSettings.PreferLocal.AllDistricts))
                {
                    // Active side is outside any district so allow it
                    return true;
                }
                else if (offerOut.Active && ((offerOut.GetArea() == 0 && offerOut.GetDistrict() == 0) || offerOut.GetDistrictRestriction(material) != DistrictRestrictionSettings.PreferLocal.AllDistricts))
                {
                    // Active side is outside any district so allow it
                    return true;
                }

                // Otherwise check districts are the same
                if (offerIn.GetDistrict() != 0 && offerIn.GetDistrict() == offerOut.GetDistrict())
                {
                    return true;
                }
                if (offerIn.GetArea() != 0 && offerIn.GetArea() == offerOut.GetArea())
                {
                    return true;
                }

                // Match restricted
                return false;
            }

            // Match allowed
            return true;
        }

        public static bool CanTransfer(CustomTransferOffer offerIn, CustomTransferOffer offerOut, CustomTransferReason.Reason material, TransferMode mode)
        {
            // Check if it is an Import/Export
            if (offerIn.IsOutside() || offerOut.IsOutside())
            {
                // Don't restrict Import/Export with district restrictions
                return true;
            }

            // Find the maximum setting from both buildings
            DistrictRestrictionSettings.PreferLocal eInBuildingLocalDistrict = offerIn.GetDistrictRestriction(material);
            DistrictRestrictionSettings.PreferLocal eOutBuildingLocalDistrict = offerOut.GetDistrictRestriction(material);

            // Check max priority of both buildings
            DistrictRestrictionSettings.PreferLocal eMaxDistrictRestriction = (DistrictRestrictionSettings.PreferLocal)Math.Max((int)eInBuildingLocalDistrict, (int)eOutBuildingLocalDistrict);
            if (IsMatchAllowed(eMaxDistrictRestriction, offerIn, offerOut, material, mode))
            {
                return true;
            } 

            // Now we check allowed districts against actual districts for both sides
            bool bInIsValid = false;
            switch (eInBuildingLocalDistrict)
            {
                case DistrictRestrictionSettings.PreferLocal.AllDistricts:
                    {
                        bInIsValid = true;
                        break;
                    }
                case DistrictRestrictionSettings.PreferLocal.PreferLocalDistrict:
                case DistrictRestrictionSettings.PreferLocal.RestrictLocalDistrict:
                case DistrictRestrictionSettings.PreferLocal.AllDistrictsExceptFor:
                    {
                        // Do the district arrays intersect
                        if (offerOut.GetActualDistrictList().Count > 0 && offerIn.GetAllowedDistrictList(material).Count > 0)
                        {
                            bInIsValid = DistrictData.Intersect(offerIn.GetAllowedDistrictList(material), offerOut.GetActualDistrictList());
                        }
                        else
                        {
                            bInIsValid = false;
                        }

                        // Valid if they DON'T intersect
                        if (eInBuildingLocalDistrict == DistrictRestrictionSettings.PreferLocal.AllDistrictsExceptFor)
                        {
                            bInIsValid = !bInIsValid; 
                        }

                        break;
                    }
            }

            bool bOutIsValid = false;
            if (bInIsValid)
            {
                // Finally check outgoing district restrictions are fine
                switch (eOutBuildingLocalDistrict)
                {
                    case DistrictRestrictionSettings.PreferLocal.AllDistricts:
                        {
                            bOutIsValid = true;
                            break;
                        }
                    case DistrictRestrictionSettings.PreferLocal.PreferLocalDistrict:
                    case DistrictRestrictionSettings.PreferLocal.RestrictLocalDistrict:
                    case DistrictRestrictionSettings.PreferLocal.AllDistrictsExceptFor:
                        {
                            // Do the district arrays intersect
                            if (offerIn.GetActualDistrictList().Count > 0 && offerOut.GetAllowedDistrictList(material).Count > 0)
                            {
                                bOutIsValid = DistrictData.Intersect(offerOut.GetAllowedDistrictList(material), offerIn.GetActualDistrictList());
                            }
                            else
                            {
                                bOutIsValid = false;
                            }

                            // Valid if they DON'T intersect
                            if (eOutBuildingLocalDistrict == DistrictRestrictionSettings.PreferLocal.AllDistrictsExceptFor)
                            {
                                bOutIsValid = !bOutIsValid; 
                            }

                            break;
                        }
                }
            }

            return (bInIsValid && bOutIsValid);
        }

        private static bool IsMatchAllowed(DistrictRestrictionSettings.PreferLocal restriction, CustomTransferOffer offerIn, CustomTransferOffer offerOut, CustomTransferReason.Reason material, TransferMode mode)
        {
            // We only allow transfers outside district when priority climbs to this value
            const int PREFER_LOCAL_DISTRICT_THRESHOLD = 4;

            switch (restriction)
            {
                case DistrictRestrictionSettings.PreferLocal.AllDistricts:
                    {
                        return true;// Any match is fine
                    }
                case DistrictRestrictionSettings.PreferLocal.PreferLocalDistrict:
                    {
                        // We use match mode to determine which side needs to be high priority
                        switch (mode)
                        {
                            case TransferMode.OutgoingFirst:
                                {
                                    // We require the OUT priority to be really high before allowing
                                    if (offerOut.Priority >= PREFER_LOCAL_DISTRICT_THRESHOLD)
                                    {
                                        return true; // Priority is high enough to allow match
                                    }
                                    break;
                                }
                            case TransferMode.IncomingFirst:
                                {
                                    // We require the IN priority to be really high before allowing
                                    if (offerIn.Priority >= PREFER_LOCAL_DISTRICT_THRESHOLD)
                                    {
                                        return true; // Priority is high enough to allow match
                                    }
                                    break;
                                }
                            case TransferMode.Balanced:
                            case TransferMode.Priority:
                            default:
                                {
                                    // In balance mode we allow match if either is high enough
                                    if (offerIn.IsWarehouse() && offerOut.IsWarehouse())
                                    {
                                        // If it's a warehouse match we allow it at 2/2 as this is as high as normal warehouse matches can get
                                        if (offerIn.Priority >= 2 && offerOut.Priority >= 2)
                                        {
                                            return true;
                                        }
                                    }
                                    else if (Math.Max(offerIn.Priority, offerOut.Priority) >= PREFER_LOCAL_DISTRICT_THRESHOLD)
                                    {
                                        // One of the priorities is high enough, allow match outside district
                                        return true;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case DistrictRestrictionSettings.PreferLocal.RestrictLocalDistrict:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case DistrictRestrictionSettings.PreferLocal.AllDistrictsExceptFor:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case DistrictRestrictionSettings.PreferLocal.Unknown:
                    {
                        CDebug.Log("Error district restriction unknown");
                        return true;
                    }
            }

            return false;
        }
    }
}