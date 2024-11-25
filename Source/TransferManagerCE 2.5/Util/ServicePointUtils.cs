using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Util
{
    public class ServicePointUtils
    {
        public static HashSet<TransferReason> GetServicePointMaterials(ushort buildingId)
        {
            HashSet<TransferReason> serviceMaterials = new HashSet<TransferReason>();

            BuildingType buildingType = GetBuildingType(buildingId);
            if (buildingType == BuildingType.ServicePoint)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    // Print out parks request and suggestion arrays
                    byte parkId = DistrictManager.instance.GetPark(building.m_position);
                    if (parkId != 0)
                    {
                        DistrictPark park = DistrictManager.instance.m_parks.m_buffer[parkId];
                        if (park.m_flags != 0 && park.IsPedestrianZone)
                        {
                            for (int i = 0; i < DistrictPark.kPedestrianZoneTransferReasons.Length; ++i)
                            {
                                DistrictPark.PedestrianZoneTransferReason reason = DistrictPark.kPedestrianZoneTransferReasons[i];
                                int iMaterialCount = park.m_materialRequest[i].Count + park.m_materialSuggestion[i].Count;
                                if (iMaterialCount > 0)
                                {
                                    serviceMaterials.Add(reason.m_material);
                                }
                            }
                        }
                    }
                }
            }

            return serviceMaterials;
        }

        public static void GetServicePointInValues(ushort buildingId, TransferReason material, out int iCount, out int iBuffer)
        {
            iCount = 0;
            iBuffer = 0;

            BuildingType buildingType = GetBuildingType(buildingId);
            if (buildingType == BuildingType.ServicePoint)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    // Print out parks request and suggestion arrays
                    byte parkId = DistrictManager.instance.GetPark(building.m_position);
                    if (parkId != 0)
                    {
                        Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                        DistrictPark[] Parks = Singleton<DistrictManager>.instance.m_parks.m_buffer;
                        Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                        for (int i = 0; i < DistrictPark.pedestrianReasonsCount; i++)
                        {
                            if (DistrictPark.kPedestrianZoneTransferReasons[i].m_material == material)
                            {
                                int inCount = Parks[parkId].m_materialRequest[i].Count;
                                if (inCount > 0)
                                {
                                    int bufferIn = 0;
                                    foreach (ushort item2 in Parks[parkId].m_materialRequest[i])
                                    {
                                        int amount2 = 0;
                                        int max2 = 0;
                                        Buildings[item2].Info.m_buildingAI.GetMaterialAmount(item2, ref Buildings[item2], material, out amount2, out max2);
                                        bufferIn += max2 - amount2;
                                    }

                                    int bufferIncoming = 0;
                                    for (int j = 0; j < Parks[parkId].m_finalGateCount; j++)
                                    {
                                        ushort num4 = Parks[parkId].m_finalServicePointList[j];
                                        if ((Buildings[num4].m_problems & Notification.Problem1.TurnedOff).IsNotNone)
                                        {
                                            continue;
                                        }

                                        ushort num5 = Buildings[num4].m_guestVehicles;
                                        int num6 = 0;
                                        while (num5 != 0)
                                        {
                                            if ((TransferManager.TransferReason)Vehicles[num5].m_transferType == material)
                                            {
                                                bufferIncoming++;
                                            }

                                            num5 = Vehicles[num5].m_nextGuestVehicle;
                                            if (++num6 > 16384)
                                            {
                                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                                break;
                                            }
                                        }
                                    }

                                    iCount = inCount;
                                    iBuffer = bufferIn + bufferIncoming;
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void GetServicePointOutValues(ushort buildingId, TransferReason material, out int iCount, out int iBuffer)
        {
            iCount = 0;
            iBuffer = 0;

            BuildingType buildingType = GetBuildingType(buildingId);
            if (buildingType == BuildingType.ServicePoint)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.m_flags != 0)
                {
                    // Print out parks request and suggestion arrays
                    byte parkId = DistrictManager.instance.GetPark(building.m_position);
                    if (parkId != 0)
                    {
                        Building[] Buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                        DistrictPark[] Parks = Singleton<DistrictManager>.instance.m_parks.m_buffer;
                        Vehicle[] Vehicles = Singleton<VehicleManager>.instance.m_vehicles.m_buffer;

                        for (int i = 0; i < DistrictPark.pedestrianReasonsCount; i++)
                        {
                            if (DistrictPark.kPedestrianZoneTransferReasons[i].m_material == material)
                            {
                                    
                                iCount = Parks[parkId].m_materialSuggestion[i].Count;
                                iBuffer = 0;
                                foreach (ushort item in Parks[parkId].m_materialSuggestion[i])
                                {
                                    int amount = 0;
                                    int max = 0;
                                    Buildings[item].Info.m_buildingAI.GetMaterialAmount(item, ref Buildings[item], material, out amount, out max);
                                    iBuffer += amount;
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        public static string DisplayBuffer(int iBuffer)
        {
            if (iBuffer > 10000)
            {
                return $"{(int)(iBuffer * 0.001)}k";
            }
            else
            {
                return $"{iBuffer}";
            }
        }
    }
}
