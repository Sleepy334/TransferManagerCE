using UnityEngine;
using TransferManagerCE.Util;
using static TransferManager;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace TransferManagerCE.CustomManager
{
    public class TransferRestrictions
    {
        public enum ExclusionReason
        {
            None,
            PathFinding,
            WarehouseReserveTrucks,
            Import,
            Export,
            ActivePassive,
            DistanceRestriction,
            DistrictRestriction,
            SameObject,
            SameBuilding,
            DifferentParks,
            WarehouseLowPriority,
            WarehouseMode,
            WarehouseStorageLevels,
        };

        public static ExclusionReason CanTransfer(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer, int prio_lower_limit, float fPatrollingVehicleDistanceLimit)
        {
            ExclusionReason reason = CanTransferFastChecksOnly(material, ref incomingOffer, ref outgoingOffer, prio_lower_limit);
            if (reason != ExclusionReason.None)
            {
                return reason;
            }

            //temporary exclusion due to pathfinding issues?
            if (PathfindExclusion(material, ref incomingOffer, ref outgoingOffer))
            {
                return ExclusionReason.PathFinding;
            }

            // Check if Import/Export are disbled
            ExclusionReason eOutsideReason = OutsideConnectionCanTransfer(incomingOffer, outgoingOffer, material);
            if (eOutsideReason != ExclusionReason.None)
            {
                return eOutsideReason;
            }

            // This checks warehouse transfers
            ExclusionReason eWarehouseReason = WarehouseCanTransfer(incomingOffer, outgoingOffer, material);
            if (eWarehouseReason != ExclusionReason.None)
            {
                return eWarehouseReason;
            }

            // Check distance restrictions
            if (!DistanceCanTransfer(incomingOffer, outgoingOffer, material, fPatrollingVehicleDistanceLimit))
            {
                return ExclusionReason.DistanceRestriction;
            }

            // District restrictions
            if (!DistrictCanTransfer(incomingOffer, outgoingOffer, material))
            {
                return ExclusionReason.DistrictRestriction;
            }

            return ExclusionReason.None;
        }

        public static ExclusionReason CanTransferFastChecksOnly(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer, int prio_lower_limit)
        {
            // guards: out=in same? exclude offer
            if (outgoingOffer.m_object == incomingOffer.m_object)
            {
                return ExclusionReason.SameObject;
            }

            // New Parks and Plazas logic, don't match if local parks arent the same
            if (outgoingOffer.m_offer.m_isLocalPark != incomingOffer.m_offer.m_isLocalPark)
            {
                return ExclusionReason.DifferentParks;
            }

            // Don't allow matching if same building
            if (incomingOffer.GetBuilding() != 0 && incomingOffer.GetBuilding() == outgoingOffer.GetBuilding())
            {
                return ExclusionReason.SameBuilding;
            }

            // Dont allow Passive/Passive transfers for Sick as you end up with lots of sick citizens not getting picked up
            if (!ActivePassiveCanTransfer(incomingOffer, outgoingOffer, material))
            {
                return ExclusionReason.ActivePassive;
            }

            return ExclusionReason.None;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool DistrictCanTransfer(CustomTransferOffer offerIn, CustomTransferOffer offerOut, TransferReason material)
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
            BuildingSettings.PreferLocal eInBuildingLocalDistrict = GetPreferLocal(offerIn.GetBuilding(), true, material);
            BuildingSettings.PreferLocal eOutBuildingLocalDistrict = GetPreferLocal(offerOut.GetBuilding(), false, material);

            // Check max priority of both buildings
            BuildingSettings.PreferLocal ePreferLocalDistrict = (BuildingSettings.PreferLocal)Math.Max((int)eInBuildingLocalDistrict, (int)eOutBuildingLocalDistrict);
            switch (ePreferLocalDistrict)
            {
                case BuildingSettings.PreferLocal.ALL_DISTRICTS:
                    {
                        return true;// Any match is fine
                    }
                case BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                    {
                        // Combined priority needs to be equal or greater than PREFER_LOCAL_DISTRICT_THRESHOLD
                        int priority = offerIn.Priority + offerOut.Priority;
                        if (priority >= PREFER_LOCAL_DISTRICT_THRESHOLD)
                        {
                            return true; // Priority is high enough to allow match
                        }
                        break;
                    }
                case BuildingSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case BuildingSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
            }

            // get respective districts
            HashSet<DistrictData> incomingActualDistricts = offerIn.GetActualDistrictList();
            HashSet<DistrictData> outgoingActualDistricts = offerOut.GetActualDistrictList();

            if (SaveGameSettings.GetSettings().PreferLocalService && ePreferLocalDistrict == BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT)
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
                case BuildingSettings.PreferLocal.ALL_DISTRICTS:
                    {
                        bInIsValid = true;
                        break;
                    }
                case BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                case BuildingSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        HashSet<DistrictData> incomingAllowedDistricts = offerIn.GetAllowedIncomingDistrictList();
                        bInIsValid = DistrictData.Intersect(incomingAllowedDistricts, outgoingActualDistricts);
                        break;
                    }
                case BuildingSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        HashSet<DistrictData> incomingBannedDistricts = offerIn.GetAllowedIncomingDistrictList();
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
                    case BuildingSettings.PreferLocal.ALL_DISTRICTS:
                        {
                            bOutIsValid = true;
                            break;
                        }
                    case BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                    case BuildingSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                        {
                            HashSet<DistrictData> outgoingAllowedDistricts = offerOut.GetAllowedOutgoingDistrictList();
                            bOutIsValid = DistrictData.Intersect(outgoingAllowedDistricts, incomingActualDistricts);
                            break;
                        }
                    case BuildingSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                        {
                            HashSet<DistrictData> outgoingBannedDistricts = offerOut.GetAllowedOutgoingDistrictList();
                            bOutIsValid = !DistrictData.Intersect(outgoingBannedDistricts, incomingActualDistricts); // Valid if they DON'T intersect
                            break;
                        }
                }
            }

            return (bInIsValid && bOutIsValid);
        }

        // Determine current local district setting by combining building and global settings
        private static BuildingSettings.PreferLocal GetPreferLocal(ushort buildingId, bool bIncoming, TransferReason material)
        {
            BuildingSettings.PreferLocal ePreferLocalDistrict = BuildingSettings.PreferLocal.ALL_DISTRICTS;

            // Global setting is only applied to certain services as it is too powerful otherwise.
            if (bIncoming && SaveGameSettings.GetSettings().PreferLocalService && TransferManagerModes.IsGlobalPreferLocalSupported(material))
            {
                ePreferLocalDistrict = BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT;
            }

            // Local setting
            if (buildingId != 0 && TransferManagerModes.IsBuildingPreferLocalSupported(material))
            {
                if (bIncoming)
                {
                    ePreferLocalDistrict = (BuildingSettings.PreferLocal) Math.Max((int)BuildingSettings.PreferLocalDistrictServicesIncoming(buildingId), (int)ePreferLocalDistrict);
                }
                else
                {
                    bool bApplyAllowServices = true;

                    // Don't apply "Allow Services" to Taxi Depot when material is Taxi as we are overriding the actual restriction
                    if (material == TransferReason.Taxi)
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                        if (building.m_flags != 0 && building.Info != null && building.Info.GetSubService() == ItemClass.SubService.PublicTransportTaxi)
                        {
                            bApplyAllowServices = false;
                        }
                    }
                    
                    if (bApplyAllowServices && TransferManagerModes.IsServiceReason(material) && BuildingSettings.GetDistrictAllowServices(buildingId)) 
                    {
                        // Allow services option overrides district restrictions when its a service material.
                        ePreferLocalDistrict = BuildingSettings.PreferLocal.ALL_DISTRICTS;
                    }
                    else
                    {
                        ePreferLocalDistrict = (BuildingSettings.PreferLocal) Math.Max((int)BuildingSettings.PreferLocalDistrictServicesOutgoing(buildingId), (int)ePreferLocalDistrict);
                    }
                }
            }

            return ePreferLocalDistrict;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static bool PathfindExclusion(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer)
        {
            bool result = false;

            //check failed outside connection pair
            if (incomingOffer.IsOutside() || outgoingOffer.IsOutside())
            {
                if (incomingOffer.Active)
                {
                    result = PathFindFailure.FindOutsideConnectionPair(incomingOffer.m_object, outgoingOffer.m_object);
                }
                else if (outgoingOffer.Active)
                {
                    result = PathFindFailure.FindOutsideConnectionPair(outgoingOffer.m_object, incomingOffer.m_object);
                }
#if DEBUG
                if (result)
                {
                    //DebugLog.LogOnly(DebugLog.REASON_PATHFIND, $"       ** Pathfindfailure: Excluded outsideconnection: material:{material}, {TransferManagerUtils.DebugOffer(incomingOffer)}, {TransferManagerUtils.DebugOffer(outgoingOffer)}");
                }
#endif
            }
            else
            {
                if (incomingOffer.Active)
                {
                    result = PathFindFailure.FindPathPair(incomingOffer.m_object, outgoingOffer.m_object);
                }
                else if (outgoingOffer.Active)
                {
                    result = PathFindFailure.FindPathPair(outgoingOffer.m_object, incomingOffer.m_object);
                }

                if (result)
                {
#if DEBUG
                    //Debug.Log($"       ** Pathfindfailure: Excluded: material:{material}, in:{TransferManagerUtils.DebugOffer(incomingOffer)} out:{TransferManagerUtils.DebugOffer(outgoingOffer)}");
#endif
                    return result;
                }
            }

            return result;
        }

        private static bool DistanceCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material, float fPatrollingVehicleDistanceLimit)
        {
            // Don't limit import/export, that gets restricted elsewhere.
            if (incomingOffer.IsOutside() || outgoingOffer.IsOutside())
            {
                return true;
            }

            float fDistanceLimit = 0.0f;

            // Only apply distance restriction to active side.
            if (incomingOffer.Active)
            {
                fDistanceLimit = incomingOffer.GetDistanceRestrictionSquaredMeters(material);
            }
            else if (outgoingOffer.Active)
            {
                fDistanceLimit = outgoingOffer.GetDistanceRestrictionSquaredMeters(material);
            }

            // Apply patrolling vehicle limit if set
            if (fPatrollingVehicleDistanceLimit > 0.0f)
            {
                fDistanceLimit = Math.Min(fDistanceLimit, fPatrollingVehicleDistanceLimit);
            }
            
            if (fDistanceLimit > 0.0f)
            {
                float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position);
                return squaredDistance <= fDistanceLimit;
            }

            return true;
        }

        private static bool ActivePassiveCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            // Dont allow Passive/Passive or Active/Active transfers for the following types
            // TODO: Investigate PartnerAdult, PartnerYoung
            switch (material)
            {
                case TransferReason.Sick:
                case TransferReason.ElderCare:
                case TransferReason.ChildCare:
                    {
                        // Allow ACTIVE/PASSIVE or ACTIVE/ACTIVE but not PASSIVE/PASSIVE as neither travels to make the match
                        return (incomingOffer.Active != outgoingOffer.Active) || (incomingOffer.Active && outgoingOffer.Active);
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        public static bool IsExportRestrictionsSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:
                case TransferReason.Goods:
                case TransferReason.Food:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Lumber:
                case TransferReason.AnimalProducts:
                case TransferReason.Flours:
                case TransferReason.Paper:
                case TransferReason.PlanedTimber:
                case TransferReason.Petroleum:
                case TransferReason.Plastics:
                case TransferReason.Glass:
                case TransferReason.Metals:
                case TransferReason.LuxuryProducts:
                case TransferReason.Fish:
                case TransferReason.OutgoingMail:
                case TransferReason.UnsortedMail:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsImportRestrictionsSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Oil:
                case TransferReason.Ore:
                case TransferReason.Logs:
                case TransferReason.Grain:
                case TransferReason.Goods:
                case TransferReason.Food:
                case TransferReason.Coal:
                case TransferReason.Petrol:
                case TransferReason.Lumber:
                case TransferReason.IncomingMail:
                case TransferReason.SortedMail:
                    return true;

                default:
                    return false;
            }
        }

        private static ExclusionReason OutsideConnectionCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            if (incomingOffer.IsOutside())
            {
                if (IsExportRestrictionsSupported(material))
                {
                    if (BuildingSettings.IsExportDisabled(outgoingOffer.GetBuilding()))
                    {
                        return ExclusionReason.Export; // Attempting to Export is disabled
                    }
                    else if (BuildingSettings.IsExportDisabled(incomingOffer.GetBuilding()))
                    {
                        return ExclusionReason.Export; // The outside connection has export disabled
                    }
                }
            } 
            else if (outgoingOffer.IsOutside())
            {
                if (IsImportRestrictionsSupported(material))
                {
                    if (BuildingSettings.IsImportDisabled(incomingOffer.GetBuilding()))
                    {
                        return ExclusionReason.Import; // The incoming connection has import disabled
                    }
                    else if (BuildingSettings.IsImportDisabled(outgoingOffer.GetBuilding()))
                    {
                        return ExclusionReason.Import; // The outside connection has import disabled
                    }
                    else if (incomingOffer.IsWarehouse())
                    {
                        if (SaveGameSettings.GetSettings().IsWarehouseImportRestricted(material))
                        {
                            return ExclusionReason.Import;
                        }
                    }
                    else if (SaveGameSettings.GetSettings().IsImportRestricted(material))
                    {
                        return ExclusionReason.Import;
                    }
                }
            }

            return ExclusionReason.None;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static ExclusionReason WarehouseCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            const double dWAREHOUSE_LOW_PRIORITY_BUFFER = 0.50; // 50% - When priority = 2
            const double dWAREHOUSE_HIGH_PRIORITY_BUFFER = 0.25; // 25% - When priority > 2

            // Is it an inter-warehouse transfer
            if (incomingOffer.IsWarehouse() && outgoingOffer.IsWarehouse())
            {
                if (SaveGameSettings.GetSettings().NewInterWarehouseTransfer)
                {
                    // Check priority is high enough 1/1 or 2/0 and higher
                    if (incomingOffer.Priority + outgoingOffer.Priority < 2)
                    {
                        return ExclusionReason.WarehouseLowPriority;
                    }
                    else
                    {
                        // Only allow if the IN warehouse has substantially lower storage % to help balance warehouses out
                        double dInPercent = incomingOffer.GetWarehouseStoragePercent();
                        double dOutPercent = outgoingOffer.GetWarehouseStoragePercent();
                        
                        // If we fail to get storage levels, fall back on vanilla match logic
                        if (dInPercent < 0.0 || dOutPercent < 0.0)
                        {
                            // Vanilla: Check priority is high enough 2/1 or 3/0 and higher
                            if (incomingOffer.Priority + outgoingOffer.Priority < 3)
                            {
                                return ExclusionReason.WarehouseLowPriority;
                            }
                        } 
                        else
                        {
                            double dWarehouseLevel;
                            if ((incomingOffer.Priority + outgoingOffer.Priority == 2) && dInPercent > 0.0)
                            {
                                dWarehouseLevel = dWAREHOUSE_LOW_PRIORITY_BUFFER;
                            }
                            else
                            {
                                dWarehouseLevel = dWAREHOUSE_HIGH_PRIORITY_BUFFER;
                            }
                            if ((dInPercent + dWarehouseLevel) >= dOutPercent)
                            {
                                return ExclusionReason.WarehouseStorageLevels;
                            }
                        }
                    }
                }
                else
                {
                    // Vanilla: Check priority is high enough 2/1 or 3/0 and higher
                    if (incomingOffer.Priority + outgoingOffer.Priority < 3)
                    {
                        return ExclusionReason.WarehouseLowPriority;
                    }
                }

                // All OK
                return ExclusionReason.None;
            }
            else if (outgoingOffer.IsWarehouse() && outgoingOffer.Active && incomingOffer.IsOutside())
            {
                // It's an export, check WarehouseReserveTrucks
                if (BuildingSettings.ReserveCargoTrucksPercent(outgoingOffer.GetBuilding()) > 0f)
                {
                    // is outgoing a warehouse with active delivery, and is counterpart incoming an outside connection?
                    if (outgoingOffer.IsReservedTrucksOk(material))
                    {
                        return ExclusionReason.None;
                    }
                    else
                    {
                        return ExclusionReason.WarehouseReserveTrucks;
                    }
                }
            }

            // all good -> allow transfer
            return ExclusionReason.None;
        }
    }
}