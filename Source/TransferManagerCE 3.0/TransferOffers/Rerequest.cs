using ColossalFramework;
using System;
using static TransferManager;
using UnityEngine;
using System.Collections.Generic;
using SleepyCommon;

namespace TransferManagerCE.TransferOffers
{
    public class Rerequest
    {
        public const int iREREQUEST_TIMER_LEVEL1 = 10; // Start re-requesting 
        public const int iREREQUEST_TIMER_LEVEL2 = 32; // Level 2 - Vehicles need to be closer in to count
        public const int iREREQUEST_DISTANCE_LEVEL1 = 16000000; // 4km
        public const int iREREQUEST_DISTANCE_LEVEL2 = 4000000; // 2km

        public enum ProblemLevel
        {
            Level0,
            Level1,
            Level2,
        }

        public static ProblemLevel GetLevelIncomingTimer(int iIncomingTimer)
        {
            if (iIncomingTimer > iREREQUEST_TIMER_LEVEL2)
            {
                return ProblemLevel.Level2;
            }
            else if (iIncomingTimer > iREREQUEST_TIMER_LEVEL1)
            {
                return ProblemLevel.Level1;
            }
            else
            {
                return ProblemLevel.Level0;
            }
        }

        // We ignore far away trucks based on the problem level
        public static int GetNearbyGuestVehiclesTransferSize(Building building, ProblemLevel level, TransferReason material1, TransferReason material2, out int iTotalTrucks)
        {
            Vehicle[] Vehicles = VehicleManager.instance.m_vehicles.m_buffer;

            int iTransferSize = 0;
            int iTempTotalTrucks = 0;

            BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
            {
                // We only include this material if the truck is close by depending on incoming timer value
                if (vehicle.m_flags != 0 && ((TransferReason)vehicle.m_transferType == material1 || (material2 != TransferReason.None && (TransferReason)vehicle.m_transferType == material2)))
                {
                    iTempTotalTrucks++;
                    bool bVehicleValid = true;

                    // Check parent is also valid
                    if (vehicle.m_cargoParent != 0)
                    {
                        bVehicleValid = Vehicles[vehicle.m_cargoParent].m_flags != 0;
                    }

                    if (bVehicleValid)
                    {
                        switch (level)
                        {
                            case ProblemLevel.Level2:
                                {
                                    // We exclude vehicles who are loading or who's parent is loading
                                    if (!IsVehicleOrParentWaiting(Vehicles, vehicle))
                                    {
                                        // We need it to be really close, 2km as we are about to be abandoned
                                        double dDistanceSquared = Vector3.SqrMagnitude(vehicle.GetLastFramePosition() - building.m_position);
                                        if (dDistanceSquared < iREREQUEST_DISTANCE_LEVEL2)
                                        {
                                            iTransferSize += vehicle.m_transferSize;
                                        }
                                    }
                                    break;
                                }
                            case ProblemLevel.Level1:
                                {
                                    // We exclude vehicles who are loading or who's parent is loading
                                    if (!IsVehicleOrParentWaiting(Vehicles, vehicle))
                                    {
                                        // Include truck if closer than 4km as we still have a little time for them to arrive
                                        double dDistanceSquared = Vector3.SqrMagnitude(vehicle.GetLastFramePosition() - building.m_position);
                                        if (dDistanceSquared < iREREQUEST_DISTANCE_LEVEL1)
                                        {
                                            iTransferSize += vehicle.m_transferSize;
                                        }
                                    }

                                    break;
                                }
                            case ProblemLevel.Level0:
                                {
                                    // Any distance is fine
                                    iTransferSize += vehicle.m_transferSize;
                                    break;
                                }
                        }
                    }
                }
                return true;
            });

            iTotalTrucks = iTempTotalTrucks;
            return iTransferSize;
        }

        public static void RerequestMaterial(CustomTransferReason primary, CustomTransferReason secondary, ushort buildingID, Building buildingData)
        {
            const int iMAX_RETRIES = 10;

            // Add a random delay to give trucks time to respond before we ask again
            if (buildingData.m_fireIntensity == 0 && primary != TransferReason.None && Singleton<SimulationManager>.instance.m_randomizer.UInt32(3U) == 0)
            {
                // Check if we are running out of time to get material
                ProblemLevel level = GetLevelIncomingTimer(buildingData.m_incomingProblemTimer);
                if (level != ProblemLevel.Level0)
                {
                    // We use the incoming timer value to determine how close a truck needs to be
                    int iNearbyTransferSize = GetNearbyGuestVehiclesTransferSize(buildingData, level, primary, secondary, out int iTotalTrucks);
                    if (iNearbyTransferSize == 0 && iTotalTrucks < iMAX_RETRIES)
                    {
                        TransferOffer offer = default;
                        offer.Priority = 7;
                        offer.Building = buildingID;
                        offer.Position = buildingData.m_position;
                        offer.Amount = 1;
                        offer.Active = false;

                        // Remove any vanilla offers in case there are any
                        Singleton<TransferManager>.instance.RemoveIncomingOffer(primary, offer);
                        if (secondary != TransferReason.None)
                        {
                            Singleton<TransferManager>.instance.RemoveIncomingOffer(secondary, offer);
                        }

                        // Add re-request offer
                        Singleton<TransferManager>.instance.AddIncomingOffer(primary, offer);
#if DEBUG
                        CDebug.Log($"Building: {buildingID} Re-requesting material: {primary}");
#endif
                    }
                }
            }
        }

        private static bool IsVehicleOrParentWaiting(Vehicle[] Vehicles, Vehicle vehicle)
        {
            if (vehicle.m_waitCounter > 0)
            {
                return true;
            }
            else if (vehicle.m_cargoParent != 0)
            {
                return Vehicles[vehicle.m_cargoParent].m_waitCounter > 0;
            }

            return false;
        }
    }
}
