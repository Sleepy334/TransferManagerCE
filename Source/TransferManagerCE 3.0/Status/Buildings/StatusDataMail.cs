using ColossalFramework;
using ICities;
using SleepyCommon;
using System;
using System.Reflection;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataBuildingMail : StatusDataBuilding
    {
        public StatusDataBuildingMail(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) : 
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (m_eBuildingType)
                {
                    case BuildingType.ServicePoint:
                        {
                            ServicePointUtils.GetServicePointOutValues(m_buildingId, (TransferReason) m_material, out int iCount, out int iBuffer);
                            tooltip = $"Buildings with {m_material}: {iCount}\n{m_material}: {DisplayBufferLong(iBuffer)}";
                            return $"{iCount} | {DisplayBuffer(iBuffer)}";
                        }
                    case BuildingType.PostOffice:
                        {
                            // An actual post office, delivers SortedMail to city, returns with UnsortedMail which is sent to Post Sorting Facilities.
                            PostOfficeAI postOfficeAI = building.Info.GetAI() as PostOfficeAI;
                            postOfficeAI.GetMaterialAmount(m_buildingId, ref building, (TransferReason) m_material, out int amount, out int max);

                            // Tooltip and color handling
                            switch ((CustomTransferReason.Reason) GetMaterial())
                            {
                                case CustomTransferReason.Reason.SortedMail:
                                    {
                                        // If we run out of sorted mail, the post office stops working
                                        WarnText(true, false, amount, max);
                                        tooltip = MakeTooltip(amount, max);
                                        break;
                                    }
                                case CustomTransferReason.Reason.UnsortedMail:
                                    {
                                        // If the UnsortedMail buffer is full the post offcie stops working.
                                        WarnText(false, true, amount, max);
                                        tooltip = MakeTooltip(amount, max);
                                        break;
                                    }
                                default:
                                    {
                                        tooltip = "";
                                        break;
                                    }
                            }

                            return SleepyCommon.Utils.MakePercent(amount, max);
                        }
                    case BuildingType.PostSortingFacility:
                        {
                            PostOfficeAI postOfficeAI = building.Info.GetAI() as PostOfficeAI;
                            postOfficeAI.GetMaterialAmount(m_buildingId, ref building, (TransferReason) m_material, out int amount, out int max);
                            WarnText(true, true, amount, max); // We warn for both issues here
                            tooltip = MakeTooltip(amount, max);
                            return SleepyCommon.Utils.MakePercent(amount, max);
                        }
                    default:
                        {
                            // Show the mail buffer for the building
                            int maxMail = GetMaxMail(m_buildingId, building);
                            if (maxMail > 0)
                            {
                                WarnText(false, true, building.m_mailBuffer, maxMail);
                                tooltip = MakeTooltip(building.m_mailBuffer, maxMail);
                                return SleepyCommon.Utils.MakePercent(building.m_mailBuffer, maxMail, 1);
                            }
                            else
                            {
                                tooltip = MakeTooltip(building.m_mailBuffer);
                                return DisplayBuffer(building.m_mailBuffer);
                            }
                        }
                }
            }

            tooltip = "";
            return "";
        }

        public static int GetMaxMail(ushort buildingId, Building building)
        {
            int maxMail = 0;

            if (building.m_flags != 0 && building.Info is not null)
            {
                switch (building.Info.GetAI())
                {
                    case AirportEntranceAI buildingAI:
                        {
                            if (!IsMainGate(buildingId, building))
                            {
                                Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                                int aliveWorkerCount = 0;
                                int totalWorkerCount = 0;
                                int workPlaceCount = 0;
                                int aliveVisitorCount = 0;
                                int totalVisitorCount = 0;
                                int visitPlaceCount = 0;
                                BuildingUtils.HandleWorkAndVisitPlaces(buildingAI, buildingId, ref building, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount, ref workPlaceCount, ref aliveVisitorCount, ref totalVisitorCount, ref visitPlaceCount);

                                maxMail = workPlaceCount * 50 + visitPlaceCount * 5;
                            }

                            maxMail = Mathf.Max(maxMail, SaveGameSettings.GetSettings().MainBuildingMaxMail);

                            break;
                        }
                    case ParkGateAI:
                    case MainIndustryBuildingAI:
                    case MainCampusBuildingAI:
                        {
                            maxMail = SaveGameSettings.GetSettings().MainBuildingMaxMail;
                            break;
                        }
                    case ResidentialBuildingAI:
                        {
                            maxMail = CitiesUtils.GetHomeCount(building) * 50;
                            break;
                        }
                    case PrivateBuildingAI:
                        {
                            maxMail = CitiesUtils.GetWorkerCount(buildingId, building) * 50;
                            break;
                        }
                    case PlayerBuildingAI buildingAI:
                        {
                            Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
                            int aliveWorkerCount = 0;
                            int totalWorkerCount = 0;
                            int workPlaceCount = 0;
                            int aliveVisitorCount = 0;
                            int totalVisitorCount = 0;
                            int visitPlaceCount = 0;
                            BuildingUtils.HandleWorkAndVisitPlaces(buildingAI, buildingId, ref building, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount, ref workPlaceCount, ref aliveVisitorCount, ref totalVisitorCount, ref visitPlaceCount);

                            maxMail = workPlaceCount * 50 + visitPlaceCount * 5;

                            break;
                        }
                }
            }

            return maxMail;
        }

        public static bool IsMainGate(ushort buildingId, Building building)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte b = instance.GetPark(building.m_position);
            if (b != 0 && !instance.m_parks.m_buffer[b].IsAirport)
            {
                b = 0;
            }
            ushort num = 0;
            if (b != 0)
            {
                num = instance.m_parks.m_buffer[b].m_mainGate;
                return num == buildingId;
            }

            return false;
        }
    }
}