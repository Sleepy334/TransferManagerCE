using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferManagerUtils
    {
        public static string GetDistanceKm(CustomTransferOffer offer1, CustomTransferOffer offer2)
        {
            return (Math.Sqrt(Vector3.SqrMagnitude(offer1.Position - offer2.Position)) * 0.001).ToString("00.000");
        }

        public static string DebugOffer(TransferReason material, CustomTransferOffer offer, bool bAlign, bool bNode, bool bDistrict)
        {
            string sMessage = (offer.IsIncoming() ? "IN  | " : "OUT | ") + DebugOffer(offer.m_offer, bAlign);

            // Add building specific information
            if (offer.GetBuilding() != 0)
            {
                sMessage += $" | Building:{offer.GetBuilding().ToString("00000")}";

                Building building = BuildingManager.instance.m_buildings.m_buffer[offer.GetBuilding()];
                if (building.m_flags != 0)
                {
                    if (TransferManagerModes.IsWarehouseMaterial(material))
                    {
                        if (building.m_incomingProblemTimer > 0)
                        {
                            sMessage += $" | IT:{building.m_incomingProblemTimer.ToString("000")}";
                        }
                        else if (bAlign)
                        {
                            sMessage += $" | IT:   ";
                        }
                        if (building.m_outgoingProblemTimer > 0)
                        {
                            sMessage += $" | OT:{building.m_outgoingProblemTimer.ToString("000")}";
                        }
                        else if (bAlign)
                        {
                            sMessage += $" | OT:   ";
                        }
                    }
                    switch (material)
                    {
                        case TransferReason.Sick:
                        case TransferReason.Sick2:
                        case TransferReason.SickMove:
                            {
                                if (building.m_healthProblemTimer > 0)
                                {
                                    sMessage += $" | ST:{building.m_healthProblemTimer.ToString("000")}";
                                }
                                else if (bAlign)
                                {
                                    sMessage += $" | ST:   ";
                                }
                                break;
                            }
                        case TransferReason.Dead:
                        case TransferReason.DeadMove:
                            {
                                if (building.m_deathProblemTimer > 0)
                                {
                                    sMessage += $" | DT:{building.m_deathProblemTimer.ToString("000")}";
                                }
                                else if (bAlign)
                                {
                                    sMessage += $" | DT:   ";
                                }
                                break;
                            }
                        case TransferReason.Garbage:
                            {
                                if (offer.IsIncoming())
                                {
                                    sMessage += $" | Garbage:    ";
                                }
                                else
                                {
                                    sMessage += $" | Garbage:{building.m_garbageBuffer.ToString("0000")}";
                                }
                                break;
                            }
                    }
                }
            }
            else if (bAlign)
            {
                sMessage += $" | Building:     ";
            }

            // Is it an outside connection
            if (offer.IsOutside())
            {
                sMessage += " | Outside ";
            }
            else
            {
                sMessage += " | Internal";
            }

            // Only add if evaluated
            if (bNode && 
                (PathDistanceTypes.IsPathDistanceSupported(material) || PathDistanceTypes.IsConnectedLOSSupported(material)))
            {
                if (offer.m_nearestNode != ushort.MaxValue)
                {
                    sMessage += $" | Node:{offer.m_nearestNode.ToString("00000")}";
                }
                else if (bAlign)
                {
                    sMessage += $" | Node:     ";
                }
            }

            // Only add if requested
            if (bDistrict)
            {
                sMessage += $" | District:{offer.GetDistrict().ToString("000")} | Area:{offer.GetArea().ToString("000")}";

                if (bAlign)
                {
                    // Pad district setting so it aligns
                    sMessage += $" | DistrictR:{SleepyCommon.Utils.PadToWidth(offer.GetDistrictRestriction(material).ToString(), 24, false)}";
                }
                else
                {
                    sMessage += $" | DistrictR:{offer.GetDistrictRestriction(material)}";
                }

                // Also add building restrictions
                sMessage += $" | BuildingR:{offer.GetAllowedBuildingList(material).Count.ToString("00")}";
            }

            // Is it a warehouse
            if (offer.IsWarehouse())
            {
                sMessage += $" | WarehouseMode: {offer.GetWarehouseMode()}";
                sMessage += $" | Storage: {Math.Round(offer.GetWarehouseStoragePercent() * 100.0f, 2)}%";
            }

            return sMessage;
        }

        public static string DebugOffer(TransferOffer offer, bool bAlign = false)
        {
            string sMessage = $"{InstanceHelper.DescribeInstance(offer.m_object)} [{offer.m_object.Type}:{offer.m_object.Index}]";
            if (bAlign)
            {
                sMessage = SleepyCommon.Utils.PadToWidth(sMessage, 60, false);
            }

            sMessage += $" | Priority:{offer.Priority}";
            if (offer.Active)
            {
                sMessage += " | Active ";
            }
            else
            {
                sMessage += " | Passive";
            }

            sMessage += $" | Amount:{offer.Amount.ToString("000")}";
            sMessage += $" | Park:{offer.m_isLocalPark.ToString("000")}";

            return sMessage;
        }
    }
}

