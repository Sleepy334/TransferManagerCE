using ColossalFramework;
using System;
using System.Text;
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

        public static string DebugOffer(CustomTransferReason.Reason material, CustomTransferOffer offer, bool bAlign, bool bNode, bool bDistrict)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((offer.IsIncoming() ? "IN  | " : "OUT | ") + DebugOffer(offer.m_offer, bAlign));

            Building building = BuildingManager.instance.m_buildings.m_buffer[offer.GetBuilding()];

            // Add building specific information
            if (offer.GetBuilding() != 0)
            {
                stringBuilder.Append($" | Building:{offer.GetBuilding().ToString("00000")}");
            }
            else if (bAlign)
            {
                stringBuilder.Append($" | Building:     ");
            }

            if (TransferManagerModes.IsWarehouseMaterial(material))
            {
                // Incoming timer
                if (offer.IsIncoming())
                {
                    if (offer.GetBuilding() != 0 && building.m_flags != 0 && building.m_incomingProblemTimer > 0)
                    {
                        stringBuilder.Append($" | IT:{building.m_incomingProblemTimer.ToString("000")}");
                    }
                    else if (bAlign)
                    {
                        stringBuilder.Append($" | IT:   ");
                    }
                }
                else
                {
                    if (offer.GetBuilding() != 0 && building.m_flags != 0 && building.m_outgoingProblemTimer > 0)
                    {
                        stringBuilder.Append($" | OT:{building.m_outgoingProblemTimer.ToString("000")}");
                    }
                    else if (bAlign)
                    {
                        stringBuilder.Append($" | OT:   ");
                    }
                }
            }
            else
            {
                switch (material)
                {
                    case CustomTransferReason.Reason.Sick:
                    case CustomTransferReason.Reason.Sick2:
                    case CustomTransferReason.Reason.SickMove:
                        {
                            if (offer.IsOutgoing())
                            {
                                // Add sick timer
                                if (offer.GetBuilding() != 0 && building.m_flags != 0)
                                {
                                    stringBuilder.Append($" | ST:{building.m_healthProblemTimer.ToString("000")}");
                                }
                                else
                                {
                                    stringBuilder.Append($" | ST:   ");
                                }

                                // Add citizen health
                                if (offer.Citizen != 0)
                                {
                                    int iHealth = Singleton<CitizenManager>.instance.m_citizens.m_buffer[offer.Citizen].m_health;
                                    stringBuilder.Append($" | Health:{iHealth.ToString("000")}");
                                }
                                else
                                {
                                    stringBuilder.Append($" | Health:   ");
                                }
                            }
                            
                            break;
                        }
                    case CustomTransferReason.Reason.Dead:
                    case CustomTransferReason.Reason.DeadMove:
                        {
                            if (building.m_flags != 0)
                            {
                                stringBuilder.Append($" | DT:{building.m_deathProblemTimer.ToString("000")}");
                            }
                            else
                            {
                                stringBuilder.Append($" | DT:   ");
                            }
                            break;
                        }
                    case CustomTransferReason.Reason.ChildCare:
                    case CustomTransferReason.Reason.ElderCare:
                        {
                            if (offer.IsIncoming())
                            {
                                if (offer.Citizen != 0)
                                {
                                    int iHealth = Singleton<CitizenManager>.instance.m_citizens.m_buffer[offer.Citizen].m_health;
                                    stringBuilder.Append($" | Health:{iHealth.ToString("000")}");
                                }
                                else
                                {
                                    stringBuilder.Append($" | Health:   ");
                                }
                            }
                            break;
                        }
                    case CustomTransferReason.Reason.Garbage:
                        {
                            if (offer.GetBuilding() != 0 && building.m_flags != 0 && !offer.IsIncoming())
                            {
                                stringBuilder.Append($" | Garbage:{building.m_garbageBuffer.ToString("0000")}"); 
                            }
                            else
                            {
                                stringBuilder.Append($" | Garbage:    ");
                            }
                            break;
                        }
                    case CustomTransferReason.Reason.Worker0:
                    case CustomTransferReason.Reason.Worker1:
                    case CustomTransferReason.Reason.Worker2:
                    case CustomTransferReason.Reason.Worker3:
                        {
                            if (offer.IsIncoming())
                            {
                                if (offer.GetBuilding() != 0 && building.m_flags != 0)
                                {
                                    stringBuilder.Append($" | WT:{building.m_workerProblemTimer.ToString("000")}");
                                }
                                else
                                {
                                    stringBuilder.Append($" | WT:   ");
                                }

                                if (offer.GetBuilding() != 0)
                                {
                                    int iWorkers = BuildingUtils.GetCurrentWorkerCount(offer.GetBuilding(), building, out int worker0, out int worker1, out int worker2, out int worker3);
                                    int iPlaces = BuildingUtils.GetTotalWorkerCount(offer.GetBuilding(), building, out int workPlaces0, out int workPlaces1, out int workPlaces2, out int workPlaces3);
                                    float fPercent = ((float)iWorkers / (float)iPlaces) * 100.0f;

                                    // Workers
                                    stringBuilder.Append(SleepyCommon.Utils.PadToWidth($" | Workers:{iWorkers}/{iPlaces} ({fPercent.ToString("00")}%)", 30));

                                    // Worker Levels
                                    stringBuilder.Append(SleepyCommon.Utils.PadToWidth($"| W0:{worker0}/{workPlaces0} W1:{worker1}/{workPlaces1} W2:{worker2}/{workPlaces2} W3:{worker3}/{workPlaces3}", 36));
                                }
                            } 
                            else if (offer.IsOutgoing())                     
                            {
                                if (offer.Citizen != 0)
                                {
                                    Citizen citizen = Singleton<CitizenManager>.instance.m_citizens.m_buffer[offer.Citizen];
                                    stringBuilder.Append(SleepyCommon.Utils.PadToWidth($" | Education: {citizen.EducationLevel}", 26));
                                }
                            }

                            break;
                        }
                }
            }
            
            // Is it an outside connection
            if (offer.IsOutside())
            {
                stringBuilder.Append(" | Outside ");
            }
            else
            {
                stringBuilder.Append(" | Internal");
            }

            // Force calculation when requested
            if (bNode)
            {
                stringBuilder.Append($" | Node:{offer.GetNearestNode(material).ToString("00000")}");
            }

            // Only add if requested
            if (bDistrict)
            {
                stringBuilder.Append($" | District:{offer.GetDistrict().ToString("000")} | Area:{offer.GetArea().ToString("000")}");

                if (bAlign)
                {
                    // Pad district setting so it aligns
                    stringBuilder.Append($" | DistrictR:{SleepyCommon.Utils.PadToWidth(offer.GetDistrictRestriction(material).ToString(), 24, false)}");
                }
                else
                {
                    stringBuilder.Append($" | DistrictR:{offer.GetDistrictRestriction(material)}");
                }

                // Also add building restrictions
                stringBuilder.Append($" | BuildingR:{offer.GetAllowedBuildingList(material).Count.ToString("00")}");
            }

            // Is it a warehouse
            if (offer.IsWarehouse())
            {
                stringBuilder.Append($" | WarehouseMode: {offer.GetWarehouseMode()}");
                stringBuilder.Append($" | Storage: {(offer.GetWarehouseStoragePercent() * 100.0).ToString("00")}%");
            }

            if (offer.IsOutside())
            {
                stringBuilder.Append($" | Multiplier: {offer.GetEffectiveOutsideModifier()}");
            }

            return stringBuilder.ToString();
        }

        public static string DebugOffer(TransferOffer offer, bool bAlign = false)
        {
            string sMessage = $"{InstanceHelper.DescribeInstance(offer.m_object)} [{offer.m_object.Type}:{offer.m_object.Index}]";
            if (bAlign)
            {
                sMessage = SleepyCommon.Utils.PadToWidth(sMessage, 60, false);
            }

            StringBuilder stringBuilder = new StringBuilder(sMessage);
            stringBuilder.Append($" | Priority:{offer.Priority}");
            stringBuilder.Append(offer.Active ? " | Active " : " | Passive");
            stringBuilder.Append($" | Amount:{offer.Amount.ToString("000")}");
            stringBuilder.Append(offer.Unlimited ? "*" : " ");
            stringBuilder.Append($" | Park:{offer.m_isLocalPark.ToString("000")}");

            return stringBuilder.ToString();
        }
    }
}

