using UnityEngine;
using TransferManagerCE.Util;
using static TransferManager;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using static TransferManagerCE.BuildingTypeHelper;
using TransferManagerCE.TransferRules;
using static TransferManagerCE.CustomManager.CustomTransferOffer;
using static PathUnit;
using static RenderManager;

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
            BuildingRestriction,
            SameObject,
            SameBuilding,
            DifferentParks,
            WarehouseLowPriority,
            WarehouseStorageModes,
            WarehouseStorageLevels,
            NotConnected,
            NoStartNode,
            LowPriority,
            CloseByOnly,
            ExportVehicleLimit,
        };


        private bool m_bDistrictRestrictionsSupported = false;
        private bool m_bBuildingRestrictionsSupported = false;
        private bool m_bDistanceRestrictionsSupported = false;

        public void SetMaterial(TransferReason material)
        {
            m_bDistrictRestrictionsSupported = DistrictRestrictions.IsGlobalDistrictRestrictionsSupported(material) || BuildingRuleSets.IsDistrictRestrictionsSupported(material);
            m_bBuildingRestrictionsSupported = BuildingRuleSets.IsBuildingRestrictionsSupported(material);
            m_bDistanceRestrictionsSupported = BuildingRuleSets.IsDistanceRestrictionsSupported(material);
        }

        public ExclusionReason CanTransfer(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer, bool bCloseByOnly)
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

            // Dont allow Passive/Passive transfers for Sick as you end up with lots of sick citizens not getting picked up
            if (!ActivePassiveCanTransfer(incomingOffer, outgoingOffer, material))
            {
                return ExclusionReason.ActivePassive;
            }

            // Don't allow matching if combined priority less than 2, ie 2/0, 0/2 or 1/1 or higher
            // as it is more efficient just to process things as the priority rises.
            if (incomingOffer.Priority + outgoingOffer.Priority < 2)
            {
                return ExclusionReason.LowPriority;
            }

            // Don't allow matching if same building
            if (incomingOffer.GetBuilding() != 0 && incomingOffer.GetBuilding() == outgoingOffer.GetBuilding())
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

            if (!TransferManagerModes.IsFastChecksOnly(material))
            {
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

                ExclusionReason eExportLimitReason = ExportVehicleLimitCanTransfer(incomingOffer, outgoingOffer, material);
                if (eExportLimitReason != ExclusionReason.None)
                {
                    return eExportLimitReason;
                }

                // Check distance restrictions
                if (m_bDistanceRestrictionsSupported && !DistanceCanTransfer(incomingOffer, outgoingOffer, material))
                {
                    return ExclusionReason.DistanceRestriction;
                }

                // Check building restrictions
                if (m_bBuildingRestrictionsSupported && !BuildingCanTransfer(incomingOffer, outgoingOffer, material))
                {
                    return ExclusionReason.BuildingRestriction;
                }

                // District restrictions - this one is last as it is the slowest
                if (m_bDistrictRestrictionsSupported && !DistrictRestrictions.CanTransfer(incomingOffer, outgoingOffer, material))
                {
                    return ExclusionReason.DistrictRestriction;
                }
            }

            return ExclusionReason.None;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool PathfindExclusion(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer)
        {
            // Exclude helicopter transfer reasons from path finding exclusions
            switch (material)
            {
                case TransferReason.Sick2:
                case TransferReason.SickMove:
                case TransferReason.Collapsed2:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
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

        private bool DistanceCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            if (!m_bDistanceRestrictionsSupported)
            {
                return true;
            }

            // Don't limit import/export, that gets restricted elsewhere.
            if (incomingOffer.IsOutside() || outgoingOffer.IsOutside())
            {
                return true;
            }

            float fDistanceLimit = 0.0f;

            // Only apply distance restriction to active side.
            if (incomingOffer.Active)
            {
                ushort buildingId = incomingOffer.GetBuilding();
                if (buildingId != 0)
                {
                    fDistanceLimit = incomingOffer.GetDistanceRestrictionSquaredMeters(material);
                }                
            }
            else if (outgoingOffer.Active)
            {
                ushort buildingId = outgoingOffer.GetBuilding();
                if (buildingId != 0)
                {
                    fDistanceLimit = outgoingOffer.GetDistanceRestrictionSquaredMeters(material);
                }
            }
            
            if (fDistanceLimit > 0.0f)
            {
                float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position);
                return squaredDistance <= fDistanceLimit;
            }

            return true;
        }

        private bool BuildingCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            // Don't limit import/export, that gets restricted elsewhere.
            if (incomingOffer.IsOutside() || outgoingOffer.IsOutside())
            {
                return true;
            }

            // Check incoming list
            HashSet<ushort> incomingList = incomingOffer.GetAllowedIncomingBuildingList(material);
            if (incomingList.Count > 0 && !incomingList.Contains(outgoingOffer.GetBuilding()))
            {
                return false;
            }

            // Check outgoing list
            HashSet<ushort> outgoingList = outgoingOffer.GetAllowedOutgoingBuildingList(material);
            if (outgoingList.Count > 0 && !outgoingList.Contains(incomingOffer.GetBuilding()))
            {
                return false;
            }

            return true;
        }

        private bool ActivePassiveCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
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

        private ExclusionReason OutsideConnectionCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            if (incomingOffer.IsOutside())
            {
                if (IsExportRestrictionsSupported(material))
                {
                    if (!outgoingOffer.IsExportAllowed(material))
                    {
                        return ExclusionReason.Export; // Attempting to Export is disabled
                    }
                    else if (!incomingOffer.IsExportAllowed(material))
                    {
                        return ExclusionReason.Export; // The outside connection has export disabled
                    }                    
                }
            } 
            else if (outgoingOffer.IsOutside())
            {
                if (IsImportRestrictionsSupported(material))
                {
                    if (!incomingOffer.IsImportAllowed(material))
                    {
                        return ExclusionReason.Import; // The incoming connection has import disabled
                    }
                    else if (!outgoingOffer.IsImportAllowed(material))
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
        private ExclusionReason WarehouseCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            // Is it an inter-warehouse transfer
            if (incomingOffer.IsWarehouse() && outgoingOffer.IsWarehouse())
            {
                // We have to also use the new Inter-warehouse logic when ImprovedWarehouseMatching is ON as the
                // vanilla logic only works when warehouse priorities are restricted to 0, 1 and 2
                if (SaveGameSettings.GetSettings().NewInterWarehouseTransfer || 
                    BuildingSettingsStorage.GetSettings(incomingOffer.GetBuilding()).IsImprovedWarehouseMatching() ||
                    BuildingSettingsStorage.GetSettings(outgoingOffer.GetBuilding()).IsImprovedWarehouseMatching())
                {
                    // Add some logic based on warehouse mode
                    WarehouseMode inMode = CitiesUtils.GetWarehouseMode(incomingOffer.GetBuilding());
                    WarehouseMode outMode = CitiesUtils.GetWarehouseMode(outgoingOffer.GetBuilding());

                    if (inMode == WarehouseMode.Unknown || outMode == WarehouseMode.Unknown)
                    {
                        // We couldn't determine mode for some reason so block it.
                        return ExclusionReason.WarehouseStorageModes;
                    }
                    
                    double dWarehouseLevel;
                    if ((int)inMode < (int)outMode)
                    {
                        // IN is in a lower "Fill" mode than OUT so only allow if IN is a lot more empty than out
                        if (outMode == WarehouseMode.Fill)
                        {
                            // Don't allow warehouse transfer when OUT is in Fill mode and IN isn't also in fill mode
                            return ExclusionReason.WarehouseStorageModes;
                        }

                        // Only allow transfer if OUT is nearly full
                        dWarehouseLevel = 0.90; 
                    }
                    else if ((int)inMode > (int)outMode)
                    {
                        // IN is in a higher "Fill" mode than OUT so allow this if IN is emptier
                        dWarehouseLevel = 0.0;
                    }
                    else
                    {
                        // inMode == outMode
                        dWarehouseLevel = 0.33; // Allow transfer if IN is 1/3 emptier than OUT
                    }

                    // We use actual storage percentages to allow/deny transfer rather than relying on priority
                    double dInPercent = incomingOffer.GetWarehouseIncomingStoragePercent();
                    double dOutPercent = outgoingOffer.GetWarehouseOutgoingStoragePercent();
                        
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

        private ExclusionReason ExportVehicleLimitCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
        {
            if (TransferManagerModes.IsWarehouseMaterial(material) && 
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
    }
}