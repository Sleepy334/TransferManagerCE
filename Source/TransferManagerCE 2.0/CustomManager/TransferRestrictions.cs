using UnityEngine;
using TransferManagerCE.Util;
using static TransferManager;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using static TransferManagerCE.BuildingTypeHelper;

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
            WarehouseFirst,
            WarehouseLowPriority,
            WarehouseStorageLevels,
            NotConnected,
            LowPriority,
        };

        private bool m_bDistrictRestrictionsSupported = false;
        private bool m_bDistanceRestrictionsSupported = false;

        public void SetMaterial(TransferReason material)
        {
            m_bDistrictRestrictionsSupported = DistrictRestrictions.IsDistrictRestrictionsSupported(material);
            m_bDistanceRestrictionsSupported = DistrictRestrictions.IsBuildingDistrictRestrictionsSupported(material);
        }

        public ExclusionReason CanTransfer(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer, bool bWarehouseOnly)
        {
            ExclusionReason reason = CanTransferFastChecksOnly(material, ref incomingOffer, ref outgoingOffer, bWarehouseOnly);
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
            if (m_bDistanceRestrictionsSupported && !DistanceCanTransfer(incomingOffer, outgoingOffer, material))
            {
                return ExclusionReason.DistanceRestriction;
            }

            // District restrictions
            if (m_bDistrictRestrictionsSupported && !DistrictRestrictions.CanTransfer(incomingOffer, outgoingOffer, material))
            {
                return ExclusionReason.DistrictRestriction;
            }

            return ExclusionReason.None;
        }

        public ExclusionReason CanTransferFastChecksOnly(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer, bool bWarehouseOnly)
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

            // Don't allow matching if combined priority less than 2, ei 2/0, 0/2 or 1/1 or higher
            if (incomingOffer.Priority + outgoingOffer.Priority < 2)
            {
                return ExclusionReason.LowPriority;
            }

            // In warehouse first mode, one of the offers needs to be a warehouse
            if (bWarehouseOnly && !(incomingOffer.IsWarehouse() || outgoingOffer.IsWarehouse()))
            {
                return ExclusionReason.WarehouseFirst;
            }

            // Don't allow matching if same building
            if (incomingOffer.GetBuilding() != 0 && incomingOffer.GetBuilding() == outgoingOffer.GetBuilding())
            {
                return ExclusionReason.SameBuilding;
            } 

            return ExclusionReason.None;
        }

        [MethodImpl(512)] //=[MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool PathfindExclusion(TransferReason material, ref CustomTransferOffer incomingOffer, ref CustomTransferOffer outgoingOffer)
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
                    fDistanceLimit = incomingOffer.GetDistanceRestrictionSquaredMeters(true, material);
                }                
            }
            else if (outgoingOffer.Active)
            {
                ushort buildingId = outgoingOffer.GetBuilding();
                if (buildingId != 0)
                {
                    fDistanceLimit = outgoingOffer.GetDistanceRestrictionSquaredMeters(false, material);
                }
            }
            
            if (fDistanceLimit > 0.0f)
            {
                float squaredDistance = Vector3.SqrMagnitude(outgoingOffer.Position - incomingOffer.Position);
                return squaredDistance <= fDistanceLimit;
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
        private ExclusionReason WarehouseCanTransfer(CustomTransferOffer incomingOffer, CustomTransferOffer outgoingOffer, TransferReason material)
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
                        double dInPercent = incomingOffer.GetWarehouseIncomingStoragePercent();
                        double dOutPercent = outgoingOffer.GetWarehouseOutgoingStoragePercent();
                        
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