using System;
using UnityEngine;
using TransferManagerCE.Util;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using TransferManagerCE.Settings;
using static TransferManagerCE.WarehouseUtils;
using SleepyCommon;
using ColossalFramework.Math;
using System.Threading;

namespace TransferManagerCE.CustomManager
{
    public class TransferRestrictions
    {
        public enum ExclusionReason
        {
            None,
            NotValid,
            PathFinding,
            WarehouseReserveTrucks,
            Import,
            ImportGlobal,
            ImportGlobalWarehouse,
            Export,
            OutsideConnectionExcluded,
            OutsideConnectionPriority,
            ActivePassive,
            DistanceRestrictionLocal,
            DistanceRestrictionGlobal,
            DistrictRestriction,
            BuildingRestriction,
            SameObject,
            SameBuilding,
            DifferentParks,
            NotConnected,
            NoStartNode,
            DuplicateNode,
            LowPriority,
            CloseByOnly,
            ExportVehicleLimit,
            GlobalPreferLocal,
            WarehouseLowPriority,
            WarehouseStorageMode,
            WarehouseStorageLevels,
            WarehouseStationType,
            TransportType,
        };

        private bool m_bDistrictRestrictionsSupported = false;
        private bool m_bBuildingRestrictionsSupported = false;
        private bool m_bLocalDistanceRestrictionsSupported = false;
        private bool m_bEnablePathFailExclusion = false;
        private bool m_bIsImportRestrictionsSupported = false;
        private bool m_bIsExportRestrictionsSupported = false;
        private bool m_bIsWarehouseMaterial = false;
        private bool m_bIsHelicopterReason = false;
        private bool m_bIsFactoryFirst = false;
        private float m_fGlobalDistanceRestriction = 0;
        private Randomizer m_random = new Randomizer(Thread.CurrentThread.ManagedThreadId);


        public void SetMaterial(CustomTransferReason.Reason material)
        {
            // Cache these for better performance
            m_bDistrictRestrictionsSupported = DistrictRestrictions.IsGlobalDistrictRestrictionsSupported(material) || BuildingRuleSets.IsDistrictRestrictionsSupported(material);
            m_bBuildingRestrictionsSupported = BuildingRuleSets.IsBuildingRestrictionsSupported(material);
            m_bEnablePathFailExclusion = SaveGameSettings.GetSettings().EnablePathFailExclusion;
            m_bIsImportRestrictionsSupported = TransferManagerModes.IsImportRestrictionsSupported(material);
            m_bIsExportRestrictionsSupported = TransferManagerModes.IsExportRestrictionsSupported(material);
            m_bIsWarehouseMaterial = TransferManagerModes.IsWarehouseMaterial(material);
            m_bIsHelicopterReason = TransferManagerModes.IsHelicopterReason(material);
            
            // Distance restrictions
            m_bLocalDistanceRestrictionsSupported = BuildingRuleSets.IsLocalDistanceRestrictionsSupported(material);
            m_fGlobalDistanceRestriction = SaveGameSettings.GetSettings().GetActiveDistanceRestrictionSquaredMeters(material);
        }

        public void SetFactoryFirst(bool bFactoryFirst)
        {
            m_bIsFactoryFirst = bFactoryFirst;
        }

        public ExclusionReason CanTransfer(CustomTransferReason.Reason material, TransferManagerModes.TransferMode mode, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer, bool bCloseByOnly)
        {
            // guards: out=in same? exclude offer
            if (outgoingOffer.m_object == incomingOffer.m_object)
            {
                return ExclusionReason.SameObject;
            }

            // New Parks and Plazas logic, don't match if local parks arent the same
            if (outgoingOffer.LocalPark != incomingOffer.LocalPark)
            {
                return ExclusionReason.DifferentParks;
            }

            // Dont allow Passive/Passive transfers for Sick as you end up with lots of sick citizens not getting picked up
            if (!ActivePassiveCanTransfer(incomingOffer, outgoingOffer, material))
            {
                return ExclusionReason.ActivePassive;
            }

            // Don't allow matching if combined priority less than 2, ie 2/0, 0/2 or 1/1 or higher
            // as it is more efficient just to process things as the priority rises.
            // If in factory first mode we disable this check as we always want the closest match no matter the priority
            if (!m_bIsFactoryFirst && incomingOffer.Priority + outgoingOffer.Priority < 2)
            {
                return ExclusionReason.LowPriority;
            }

            // Don't allow matching if same building, unless it is a warehouse IN request in which case it will be the truck
            // wanting to return its material.
            if (incomingOffer.GetBuilding() != 0 && !incomingOffer.IsWarehouse() && incomingOffer.GetBuilding() == outgoingOffer.GetBuilding())
            {
                return ExclusionReason.SameBuilding;
            }

            if (bCloseByOnly)
            {
                // restrict secondary loops to 1km range so they don't travel half way across the city
                float fDistanceSquared = Vector3.SqrMagnitude(incomingOffer.Position - outgoingOffer.Position);
                if (fDistanceSquared > 1000000f)
                {
                    return ExclusionReason.CloseByOnly;
                }
            }

            // Check if Import/Export are disbled
            ExclusionReason eOutsideReason = OutsideConnectionCanTransfer(incomingOffer, outgoingOffer, material);
            if (eOutsideReason != ExclusionReason.None)
            {
                return eOutsideReason;
            }

            if (!TransferManagerModes.IsFastChecksOnly(material))
            {
                // Order calls by speed of execution!

                //temporary exclusion due to pathfinding issues?
                if (m_bEnablePathFailExclusion && PathfindExclusion(material, ref incomingOffer, ref outgoingOffer))
                {
                    return ExclusionReason.PathFinding;
                }

                // This checks warehouse transfers
                ExclusionReason eWarehouseReason = WarehouseCanTransfer(incomingOffer, outgoingOffer, material);
                if (eWarehouseReason != ExclusionReason.None)
                {
                    return eWarehouseReason;
                }

                ExclusionReason eExportLimitReason = ExportVehicleLimitCanTransfer(incomingOffer, outgoingOffer, material);
                if (eExportLimitReason != ExclusionReason.None)
                {
                    return eExportLimitReason;
                }

                if (!DistrictRestrictions.CanTransferGlobalPreferLocal(incomingOffer, outgoingOffer, material, mode))
                {
                    return ExclusionReason.GlobalPreferLocal;
                }

                // Check distance restrictions, only check global if local not supported as we overwrite global with local setting.
                ExclusionReason reason = DistanceCanTransfer(incomingOffer, outgoingOffer, material);
                if (reason != ExclusionReason.None)
                {
                    return reason;
                }

                // Check building restrictions
                if (m_bBuildingRestrictionsSupported && !BuildingCanTransfer(incomingOffer, outgoingOffer, material))
                {
                    return ExclusionReason.BuildingRestriction;
                }

                // District restrictions - this one is last as it is the slowest
                if (m_bDistrictRestrictionsSupported && !DistrictRestrictions.CanTransfer(incomingOffer, outgoingOffer, material, mode))
                {
                    return ExclusionReason.DistrictRestriction;
                }

                // Check Cargo Warehouse restrictions - Only allow matching with train outside connections
                ExclusionReason eWarehouseStationReason = CargoWarehouseCanTransfer(incomingOffer, outgoingOffer, material);
                if (eWarehouseStationReason != ExclusionReason.None)
                {
                    return eWarehouseStationReason;
                }
            }

            return ExclusionReason.None;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool PathfindExclusion(CustomTransferReason.Reason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer)
        {
            // Exclude helicopter transfer reasons from path finding exclusions
            if (m_bIsHelicopterReason)
            {
                return false;
            }

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
            }

            return result;
        }

        private ExclusionReason DistanceCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            // Don't limit import/export, that gets restricted elsewhere.
            if (incomingOffer.IsOutside() || outgoingOffer.IsOutside())
            {
                return ExclusionReason.None;
            }

            if (m_bLocalDistanceRestrictionsSupported)
            {
                // Local distance restriction
                float fDistanceLimitIncoming = float.MaxValue;
                float fDistanceLimitOutgoing = float.MaxValue;

                if (incomingOffer.GetBuilding() != 0)
                {
                    fDistanceLimitIncoming = incomingOffer.GetDistanceRestrictionSquaredMeters(material);
                }

                if (outgoingOffer.GetBuilding() != 0)
                {
                    fDistanceLimitOutgoing = outgoingOffer.GetDistanceRestrictionSquaredMeters(material);
                }

                float fDistanceLimit = Math.Min(fDistanceLimitIncoming, fDistanceLimitOutgoing);
                if (fDistanceLimit != float.MaxValue)
                {
                    // Local setting found, use it instead of global value.
                    float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.GetBuildingPosition() - incomingOffer.GetBuildingPosition());
                    if (squaredDistance > fDistanceLimit)
                    {
                        return ExclusionReason.DistanceRestrictionLocal;
                    }
                }
                else if (m_fGlobalDistanceRestriction > 0.0f)
                {
                    // No local setting so check global setting instead.
                    float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.GetBuildingPosition() - incomingOffer.GetBuildingPosition());
                    if (squaredDistance > m_fGlobalDistanceRestriction)
                    {
                        return ExclusionReason.DistanceRestrictionGlobal;
                    }
                }
            }
            else if (m_fGlobalDistanceRestriction > 0.0f)
            {
                // Global distance restriction
                float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.GetBuildingPosition() - incomingOffer.GetBuildingPosition());
                if (squaredDistance > m_fGlobalDistanceRestriction)
                {
                    return ExclusionReason.DistanceRestrictionGlobal;
                }
            }

            return ExclusionReason.None;
        }

        private bool BuildingCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            // Don't limit import/export, that gets restricted elsewhere.
            if (incomingOffer.IsOutside() || outgoingOffer.IsOutside())
            {
                return true;
            }

            // Check we can get buildings or service points for both offers
            if ((incomingOffer.GetBuilding() == 0 && incomingOffer.Park == 0) || 
                (outgoingOffer.GetBuilding() == 0 && outgoingOffer.Park == 0))
            {
                return true;
            }

            // Check incoming list
            HashSet<ushort> incomingList = incomingOffer.GetAllowedBuildingList(material);
            if (incomingList.Count > 0)
            {
                if (outgoingOffer.Park > 0)
                {
                    // Get service points and check against allowed list
                    if (!incomingList.Overlaps(InstanceHelper.GetBuildings(outgoingOffer.m_object)))
                    {
                        return false;
                    }
                }
                else if (!outgoingOffer.Intersects(incomingList))
                {
                    return false;
                }
            }

            // Check outgoing list
            HashSet<ushort> outgoingList = outgoingOffer.GetAllowedBuildingList(material);
            if (outgoingList.Count > 0)
            {
                if (incomingOffer.Park > 0)
                {
                    // Get service points and check against allowed list
                    if (!outgoingList.Overlaps(InstanceHelper.GetBuildings(incomingOffer.m_object)))
                    {
                        return false;
                    }
                }
                else if (!incomingOffer.Intersects(outgoingList))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ActivePassiveCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            // Dont allow Passive/Passive or Active/Active transfers for the following types
            // TODO: Investigate PartnerAdult, PartnerYoung
            switch (material)
            {
                case CustomTransferReason.Reason.Sick:
                case CustomTransferReason.Reason.ElderCare:
                case CustomTransferReason.Reason.ChildCare:
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

        private ExclusionReason OutsideConnectionCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            if (incomingOffer.IsOutside())
            {
                // Apply priority restrictions to all transfer types
                if (m_bIsWarehouseMaterial)
                {
                    if (incomingOffer.GetEffectiveOutsideCargoPriorityFactor() != 1)
                    {
                        if (m_random.Int32(incomingOffer.GetEffectiveOutsideCargoPriorityFactor()) != 0)
                        {
                            return ExclusionReason.OutsideConnectionPriority;
                        }
                    }
                }
                else
                {
                    // Apply priority restrictions to all transfer types
                    if (incomingOffer.GetEffectiveOutsideCitizenPriorityFactor() != 1)
                    {
                        if (m_random.Int32(incomingOffer.GetEffectiveOutsideCitizenPriorityFactor()) != 0)
                        {
                            return ExclusionReason.OutsideConnectionPriority;
                        }
                    }
                }

                if (m_bIsExportRestrictionsSupported)
                {
                    if (!outgoingOffer.IsExportAllowed(material))
                    {
                        return ExclusionReason.Export; // Attempting to Export is disabled
                    }

                    if (!incomingOffer.IsExportAllowed(material))
                    {
                        return ExclusionReason.Export; // The outside connection has export disabled
                    }

                    if (outgoingOffer.IsOutsideConnectionExcluded(material, incomingOffer.GetBuilding()))
                    {
                        return ExclusionReason.OutsideConnectionExcluded;
                    }

                    if (outgoingOffer.IsWarehouse())
                    {
                        switch (outgoingOffer.GetWarehouseMode())
                        {
                            case WarehouseMode.Fill:
                                {
                                    // Fill mode warehouses should never export
                                    return ExclusionReason.WarehouseStorageMode;
                                }
                            case WarehouseMode.Balanced:
                                {
                                    if (SaveGameSettings.GetSettings().WarehouseSmartImportExport && outgoingOffer.GetWarehouseStoragePercent() < 0.66)
                                    {
                                        // Don't allow balanced to export when below 2/3 full
                                        return ExclusionReason.WarehouseStorageLevels;
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            else if (outgoingOffer.IsOutside())
            {
                // Apply priority restrictions to all transfer types
                if (m_bIsWarehouseMaterial)
                {
                    if (outgoingOffer.GetEffectiveOutsideCargoPriorityFactor() != 1)
                    {
                        if (m_random.Int32(outgoingOffer.GetEffectiveOutsideCargoPriorityFactor()) != 0)
                        {
                            return ExclusionReason.OutsideConnectionPriority;
                        }
                    }
                }
                else
                {
                    // Apply priority restrictions to all transfer types
                    if (outgoingOffer.GetEffectiveOutsideCitizenPriorityFactor() != 1)
                    {
                        if (m_random.Int32(outgoingOffer.GetEffectiveOutsideCitizenPriorityFactor()) != 0)
                        {
                            return ExclusionReason.OutsideConnectionPriority;
                        }
                    }
                }

                if (m_bIsImportRestrictionsSupported)
                {
                    if (!incomingOffer.IsImportAllowed(material))
                    {
                        return ExclusionReason.Import; // The incoming connection has import disabled
                    }

                    if (!outgoingOffer.IsImportAllowed(material))
                    {
                        return ExclusionReason.Import; // The outside connection has import disabled
                    }

                    if (incomingOffer.IsOutsideConnectionExcluded(material, outgoingOffer.GetBuilding()))
                    {
                        return ExclusionReason.OutsideConnectionExcluded;
                    }

                    if (incomingOffer.IsWarehouse())
                    {
                        // Check if restricted for warehouses
                        if (SaveGameSettings.GetSettings().IsWarehouseImportRestricted(material))
                        {
                            return ExclusionReason.ImportGlobalWarehouse;
                        }

                        switch (incomingOffer.GetWarehouseMode())
                        {
                            case WarehouseMode.Empty:
                                {
                                    // Empty mode warehouses should never import
                                    return ExclusionReason.WarehouseStorageMode;
                                }
                            case WarehouseMode.Balanced:
                                {
                                    if (SaveGameSettings.GetSettings().WarehouseSmartImportExport && incomingOffer.GetWarehouseStoragePercent() > 0.33)
                                    {
                                        // Don't allow balanced to import when more than 1 / 3 full
                                        return ExclusionReason.WarehouseStorageLevels;
                                    }
                                    break;
                                }
                        }
                    }
                    else
                    {
                        // Check for none warehouses
                        if (SaveGameSettings.GetSettings().IsImportRestricted(material))
                        {
                            return ExclusionReason.ImportGlobal;
                        }
                    }
                }
            }

            return ExclusionReason.None;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private ExclusionReason WarehouseCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            // Is it an inter-warehouse transfer
            if (incomingOffer.IsWarehouse() && outgoingOffer.IsWarehouse())
            {
                // We have to also use the Inter-warehouse logic when ImprovedWarehouseMatching is ON as the
                // vanilla logic only works when warehouse priorities are restricted to 0, 1 and 2
                if (SaveGameSettings.GetSettings().InterWarehouseTransfer || SaveGameSettings.GetSettings().ImprovedWarehouseMatching)
                {
                    // Add some logic based on warehouse mode
                    WarehouseMode inMode = incomingOffer.GetWarehouseMode();
                    WarehouseMode outMode = outgoingOffer.GetWarehouseMode();

                    if (inMode == WarehouseMode.None || outMode == WarehouseMode.None)
                    {
                        // We couldn't determine mode for some reason so block it.
                        return ExclusionReason.WarehouseStorageMode;
                    }
                    
                    double dWarehouseLevel;
                    if ((int)inMode < (int)outMode)
                    {
                        // Fill -> Balanced -> Empty - Never allowed
                        return ExclusionReason.WarehouseStorageMode;
                    }
                    else if ((int)inMode > (int)outMode)
                    {
                        // Empty -> Balanced -> Fill
                        switch (outMode)
                        {
                            case WarehouseMode.Empty:
                                {
                                    return ExclusionReason.None; // Always allowed
                                }
                            case WarehouseMode.Balanced:
                                {
                                    // IN warehouse: Fill
                                    // OUT warehouse: Balanced

                                    double dInPercent = incomingOffer.GetWarehouseStoragePercent();
                                    double dOutPercent = outgoingOffer.GetWarehouseStoragePercent();

                                    if (dInPercent < 0.0 || dOutPercent < 0.0)
                                    {
                                        // We don't know their storage levels so just say no.
                                        return ExclusionReason.WarehouseStorageLevels;
                                    }

                                    if (dOutPercent > dInPercent)
                                    {
                                        // Always allow when Fill warehouse is emptier
                                        return ExclusionReason.None;
                                    }

                                    if (dOutPercent < 0.33)
                                    {
                                        // Don't allow when balanced OUT warehouse is below 1/3rd full so it remains balanced
                                        return ExclusionReason.WarehouseStorageLevels;
                                    }
                                    else
                                    {
                                        // We occasionally allow transfer between warehouses, but use randomize to slow it down
                                        if (m_random.Int32(6U) == 0)
                                        {
                                            return ExclusionReason.None;
                                        }
                                        else
                                        {
                                            return ExclusionReason.WarehouseStorageLevels;
                                        }
                                    }
                                }
                        }

                        // Block anything else
                        return ExclusionReason.WarehouseStorageLevels;
                    }
                    else
                    {
                        // Same warehouse mode
                        dWarehouseLevel = 0.33; // Allow transfer if IN is 1/3 emptier than OUT

                        // We use actual storage percentages to allow/deny transfer rather than relying on priority
                        double dInPercent = incomingOffer.GetWarehouseStoragePercent();
                        double dOutPercent = outgoingOffer.GetWarehouseStoragePercent();

                        if (dInPercent < 0.0 || dOutPercent < 0.0)
                        {
                            // We don't know their storage levels so just say no.
                            return ExclusionReason.WarehouseStorageLevels;
                        }

                        // Only transfer if IN warehouse is dWarehouseLevel lower storage level
                        if ((dInPercent + dWarehouseLevel) >= dOutPercent)
                        {
                            return ExclusionReason.WarehouseStorageLevels;
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
            }

            // all good -> allow transfer
            return ExclusionReason.None;
        }

        private ExclusionReason ExportVehicleLimitCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            if (m_bIsWarehouseMaterial && 
                outgoingOffer.Active && 
                incomingOffer.IsOutside())
            {
                // It's an export, check ExportLimit
                if (!outgoingOffer.IsExportVehicleLimitOk(material))
                {
                    return ExclusionReason.ExportVehicleLimit;
                }
            }

            // all good -> allow transfer
            return ExclusionReason.None;
        }

        private ExclusionReason CargoWarehouseCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, CustomTransferReason.Reason material)
        {
            // Cargo Warehouse (Train) <----> Outside Connection
            // Cargo Warehouse (Train) <----> Cargo Warehouse (Train)
            // Cargo Warehouse (Train) <----> Cargo Station
            // Cargo Warehouse (Train) <----> Any train based offer

            // Cargo Warehouse (Road) <----> Normal Warehouse
            // Cargo Warehouse (Road) <----> Local (Road)
            // Cargo Warehouse (Road) <-X-> Cargo Warehouse (Road) - Not allowed
            // Cargo Warehouse (Road) <-X-> Outside Connection 
            // Cargo Warehouse (Road) <-X-> Any train offer - Not allowed

            if (m_bIsWarehouseMaterial && (incomingOffer.IsCargoWarehouse() || outgoingOffer.IsCargoWarehouse()))
            {
                if (incomingOffer.IsCargoWarehouse() && outgoingOffer.IsCargoWarehouse())
                {
                    if (incomingOffer.GetTransportType() != outgoingOffer.GetTransportType() || 
                        incomingOffer.GetTransportType() != TransportUtils.TransportType.Train)
                    {
                        // We want train to be the only method to transport between cargo warehouses
                        return ExclusionReason.WarehouseStationType;
                    }
                }
                else if (incomingOffer.IsCargoWarehouse())
                {
                    if (incomingOffer.GetTransportType() == TransportUtils.TransportType.Train) 
                    {
                        return CargoWarehouseTrainCanTransfer(outgoingOffer);
                    }
                    else
                    {
                        return CargoWarehouseRoadCanTransfer(outgoingOffer);
                    }
                }
                else if (outgoingOffer.IsCargoWarehouse())
                {
                    if (outgoingOffer.GetTransportType() == TransportUtils.TransportType.Train)
                    {
                        return CargoWarehouseTrainCanTransfer(incomingOffer);
                    }
                    else
                    {
                        return CargoWarehouseRoadCanTransfer(incomingOffer);
                    }
                }
            }

            return ExclusionReason.None;
        }

        private ExclusionReason CargoWarehouseRoadCanTransfer(CustomTransferOffer otherOffer)
        {
            // Cargo Warehouse (Road) <----> Normal Warehouse
            // Cargo Warehouse (Road) <----> Local (Road)
            // Cargo Warehouse (Road) <-X-> Cargo Warehouse (Road) - Not allowed
            // Cargo Warehouse (Road) <-X-> Outside Connection 
            // Cargo Warehouse (Road) <-X-> Any train offer - Not allowed

            if (otherOffer.IsOutside())
            {
                return ExclusionReason.WarehouseStationType; // Not allowed, force all OC through train connection
            }
            else if (BuildingTypeHelper.IsCargoStation(otherOffer.GetBuildingType()))
            {
                return ExclusionReason.WarehouseStationType; // Not allowed, force all cargo station material through train connection
            }
            else if (otherOffer.GetTransportType() == TransportUtils.TransportType.Train)
            {
                return ExclusionReason.WarehouseStationType; // Dont allow road side to connect with a train offer
            }

            return ExclusionReason.None;
        }

        private ExclusionReason CargoWarehouseTrainCanTransfer(CustomTransferOffer otherOffer)
        {
            // Cargo Warehouse (Train) <----> Outside Connection
            // Cargo Warehouse (Train) <----> Cargo Warehouse (Train)
            // Cargo Warehouse (Train) <----> Cargo Station
            // Cargo Warehouse (Train) <----> Any train based offer

            if (otherOffer.IsOutside())
            {
                return ExclusionReason.None; // Allowed
            }
            else if (BuildingTypeHelper.IsCargoStation(otherOffer.GetBuildingType()))
            {
                return ExclusionReason.None;
            }
            else if (otherOffer.GetTransportType() == TransportUtils.TransportType.Train)
            {
                return ExclusionReason.None;
            }

            return ExclusionReason.WarehouseStationType;
        }
    }
}