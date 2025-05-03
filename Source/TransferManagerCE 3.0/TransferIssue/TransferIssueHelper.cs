using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferIssueContainer;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferIssueHelper
    {
        const int GARBAGE_BUFFER_MIN_LEVEL = 3000;

        public static long s_lastUpdateTicks = 0;
        public static long s_maxUpdateTicks = 0; 
        public static long s_totalUpdateTicks = 0;
        public static int s_totalUpdates = 0;

        private List<TransferIssueContainer> m_buildingIssues = new List<TransferIssueContainer>();
        private List<RoadAccessData> m_buildingRoadAccess = new List<RoadAccessData>();
        private int m_iScanBuildingIndex = 0;

        public TransferIssueHelper()
        {
        }

        public List<TransferIssueContainer> GetAllIssues()
        {
            return m_buildingIssues;
        }

        public List<TransferIssueContainer> GetFilteredIssues()
        {
            ModSettings settings = ModSettings.GetSettings();

            bool bAllOn = settings.ShowSickIssues &&
                          settings.ShowDeadIssues &&
                          settings.ShowGarbageIssues &&
                          settings.ShowCrimeIssues &&
                          settings.ShowMailIssues &&
                          settings.ShowFireIssues &&
                          settings.ShowIncomingIssues &&
                          settings.ShowOutgoingIssues &&
                          settings.ShowWorkerIssues &&
                          settings.ShowServiceIssues &&
                          settings.ShowWithVehiclesOnRouteIssues;
            if (bAllOn)
            {
                return m_buildingIssues;
            }

            // Filter list
            List<TransferIssueContainer> listFiltered = new List<TransferIssueContainer>();

            foreach (TransferIssueContainer issue in m_buildingIssues)
            {
                // Check for vehicle
                if (!settings.ShowWithVehiclesOnRouteIssues && issue.HasVehicle())
                {
                    continue;
                }

                bool bAddIssue = true;
                switch (issue.GetIssue())
                {
                    case IssueType.Sick:
                        {
                            bAddIssue = settings.ShowSickIssues;
                            break;
                        }
                    case IssueType.Dead:
                        {
                            bAddIssue = settings.ShowDeadIssues;
                            break;
                        }
                    case IssueType.Garbage:
                        {
                            bAddIssue = settings.ShowGarbageIssues;
                            break;
                        }
                    case IssueType.Crime:
                        {
                            bAddIssue = settings.ShowCrimeIssues;
                            break;
                        }
                    case IssueType.Incoming:
                        {
                            bAddIssue = settings.ShowIncomingIssues;
                            break;
                        }
                    case IssueType.Outgoing:
                        {
                            bAddIssue = settings.ShowOutgoingIssues;
                            break;
                        }
                    case IssueType.Worker:
                        {
                            bAddIssue = settings.ShowWorkerIssues;
                            break;
                        }
                    case IssueType.Services:
                        {
                            bAddIssue = settings.ShowServiceIssues;
                            break;
                        }
                    case IssueType.Mail:
                        {
                            bAddIssue = settings.ShowMailIssues;
                            break;
                        }
                    case IssueType.Fire:
                        {
                            bAddIssue = settings.ShowFireIssues;
                            break;
                        }
                }

                if (bAddIssue)
                {
                    listFiltered.Add(issue);
                }
            }

            return listFiltered;
        }

        public List<RoadAccessData> GetRoadAccess()
        {
            return m_buildingRoadAccess;
        }

        public void Clear()
        {
            m_buildingRoadAccess.Clear();
            m_buildingIssues.Clear();
        }

        public void UpdateIssues()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            long startTicks = stopwatch.ElapsedTicks;

            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
            ModSettings settings = ModSettings.GetSettings();

            // Check existing issues first
            HashSet<ushort> currentIssueBuildings = GetCurrentIssueIds();

            // Clear current issues
            Clear();

            // Check for issues from old arrays.
            foreach (ushort buildingId in currentIssueBuildings)
            {
                Building building = BuildingBuffer[buildingId];
                CheckBuilding(buildingId, building);
            }

            // Get new issues, a chunk at a time so we dont add too much lag.
            int iCount = BuildingBuffer.Length;
            int iChunk = iCount / 8;
            int iLoopCount = 0;
            while (iLoopCount < iChunk)
            {
                ushort buildingId = (ushort) m_iScanBuildingIndex;

                if (!currentIssueBuildings.Contains(buildingId))
                {
                    Building building = BuildingBuffer[buildingId];
                    CheckBuilding(buildingId, building);
                }

                m_iScanBuildingIndex++;
                m_iScanBuildingIndex = m_iScanBuildingIndex % iCount;
                iLoopCount++;
            }

            long stopTicks = stopwatch.ElapsedTicks;
            s_lastUpdateTicks = stopTicks - startTicks;
            s_maxUpdateTicks = (long) Mathf.Max(s_lastUpdateTicks, s_maxUpdateTicks);
            s_totalUpdateTicks += s_lastUpdateTicks;
            s_totalUpdates += 1;
        }

        private HashSet<ushort> GetCurrentIssueIds()
        {
            HashSet<ushort> currentIssueBuildings = new HashSet<ushort>();
            foreach (var issue in m_buildingIssues)
            {
                currentIssueBuildings.Add(issue.GetBuildingId());
            }
            foreach (var issue in m_buildingRoadAccess)
            {
                if (issue.m_source.Building != 0)
                {
                    currentIssueBuildings.Add(issue.m_source.Building);
                }
            }
            return currentIssueBuildings;
        }

        private void CheckBuilding(ushort buildingId, Building building)
        {
            if ((building.m_flags & Building.Flags.Created) != 0 &&
                (building.m_flags & Building.Flags.Abandoned) == 0)
            {
                // Check before Active check as they stop being Active when on fire.
                CheckFireIssue(buildingId, building);

                if ((building.m_flags & Building.Flags.Active) != 0)
                {
                    // Issues
                    CheckDeadIssue(buildingId, building);
                    CheckSickIssue(buildingId, building);
                    CheckGarbageIssue(buildingId, building);
                    CheckCrimeIssue(buildingId, building);
                    CheckMailIssue(buildingId, building);
                    CheckIncomingIssue(buildingId, building);
                    CheckOutgoingIssue(buildingId, building);
                    CheckWorkerIssue(buildingId, building);
                    CheckServiceIssue(buildingId, building);

                    // Road access
                    CheckRoadAccess(buildingId, building);
                }
            }
        }

        public void CheckSickIssue(ushort buildingId, Building building)
        {
            // Health issues
            if (building.m_healthProblemTimer > ModSettings.GetSettings().SickTimerValue ||
                (building.m_problems & (Notification.Problem1.DirtyWater | Notification.Problem1.Pollution | Notification.Problem1.Noise)).IsNotNone)
            {
                int iSickCount = BuildingUtils.GetSickCount(buildingId, building);
                if (iSickCount > 0)
                {
                    bool bFoundVehicle = false;

                    // Enumerate vehicles on route.
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if ((TransferReason)vehicle.m_transferType == TransferReason.Sick)
                        {
                            TransferIssueContainer issue = new TransferIssueContainer(IssueType.Sick, SickHandler.GetPriority(building.m_healthProblemTimer), iSickCount.ToString(), building.m_healthProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                            m_buildingIssues.Add(issue);
                            bFoundVehicle = true;
                        }
                    });

                    // Add issue without vehicle if no vehicles found
                    if (!bFoundVehicle)
                    {
                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Sick, SickHandler.GetPriority(building.m_healthProblemTimer), iSickCount.ToString(), building.m_healthProblemTimer, buildingId, 0, 0));
                    }
                }
            }
        }

        public void CheckDeadIssue(ushort buildingId, Building building)
        {
            // Dead issues
            if (building.m_deathProblemTimer > ModSettings.GetSettings().DeadTimerValue || (building.m_problems & Notification.Problem1.Death).IsNotNone)
            {                
                int iDeadCount = BuildingUtils.GetDeadCount(buildingId, building);
                if (iDeadCount > 0)
                {
                    bool bFoundVehicle = false;

                    // Enumerate vehicles on route.
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if ((TransferReason) vehicle.m_transferType == TransferReason.Dead)
                        {
                            TransferIssueContainer issue = new TransferIssueContainer(IssueType.Dead, building.m_deathProblemTimer * 7 / 128, iDeadCount.ToString(), building.m_deathProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                            m_buildingIssues.Add(issue);
                            bFoundVehicle = true;
                        }
                    });

                    // Add issue without vehicle if no vehicles found
                    if (!bFoundVehicle)
                    {
                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Dead, building.m_deathProblemTimer * 7 / 128, iDeadCount.ToString(), building.m_deathProblemTimer, buildingId, 0, 0));
                    }
                }
            }
        }

        public void CheckGarbageIssue(ushort buildingId, Building building)
        {
            // Goods out issues
            if ((building.m_garbageBuffer >= GARBAGE_BUFFER_MIN_LEVEL || (building.m_problems & Notification.Problem1.Garbage).IsNotNone) &&
                building.Info is not null &&
                building.Info.GetService() != ItemClass.Service.Garbage)
            {
                bool bFoundVehicle = false;

                // Enumerate vehicles on route.
                BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                {
                    if ((TransferReason)vehicle.m_transferType == TransferReason.Garbage)
                    {
                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Garbage, building.m_garbageBuffer / 1000, building.m_garbageBuffer.ToString(), 0, buildingId, vehicle.m_sourceBuilding, vehicleId));
                        bFoundVehicle = true;
                    }
                });

                // Add issue without vehicle if no vehicles found
                if (!bFoundVehicle)
                {
                    m_buildingIssues.Add(new TransferIssueContainer(IssueType.Garbage, building.m_garbageBuffer / 1000, building.m_garbageBuffer.ToString(), 0, buildingId, 0, 0));
                }

            }
        }

        public void CheckIncomingIssue(ushort buildingId, Building building)
        {
            // Health issues
            if (building.m_incomingProblemTimer > ModSettings.GetSettings().GoodsTimerValue || (building.m_problems & Notification.Problem1.NoInputProducts).IsNotNone)
            {
                BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                if (eType != BuildingTypeHelper.BuildingType.SpaceElevator && eType != BuildingTypeHelper.BuildingType.OutsideConnection)
                {
                    bool bFoundVehicle = false;

                    // Enumerate vehicles on route.
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if ((vehicle.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.TransferToTarget &&
                            (vehicle.Info is not null && vehicle.Info.m_vehicleAI is CargoTruckAI) &&
                            vehicle.m_sourceBuilding != 0) // Quite often importing vehicles have no target till they get to a cargo staion etc...
                        {
                            TransferIssueContainer issue = new TransferIssueContainer(IssueType.Incoming, building.m_incomingProblemTimer * 7 / 64, $"{Math.Round(building.m_customBuffer2 * 0.001)}", building.m_incomingProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                            m_buildingIssues.Add(issue);
                            bFoundVehicle = true;
                        }
                    });

                    // Add issue without vehicle if no vehicles found
                    if (!bFoundVehicle)
                    {
                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Incoming, building.m_incomingProblemTimer * 7 / 64, $"{Math.Round(building.m_customBuffer2 * 0.001)}", building.m_incomingProblemTimer, buildingId, 0, 0));
                    }
                }
            }
        }

        public void CheckOutgoingIssue(ushort buildingId, Building building)
        {
            // Goods out issues
            if (!BuildingTypeHelper.IsOutsideConnection(buildingId) && building.m_outgoingProblemTimer > ModSettings.GetSettings().GoodsTimerValue)
            {
                m_buildingIssues.Add(new TransferIssueContainer(IssueType.Outgoing, building.m_outgoingProblemTimer * 7 / 64, $"{Math.Round(building.m_customBuffer2 * 0.001)}", building.m_outgoingProblemTimer, buildingId, 0, 0));
            }
        }

        public void CheckWorkerIssue(ushort buildingId, Building building)
        {
            // Goods out issues
            if (building.m_workerProblemTimer > 0 || (building.m_problems & (Notification.Problem1.NoWorkers | Notification.Problem1.NoEducatedWorkers)).IsNotNone)
            {
                // Check it actually has workers
                int iWorkPlaces = BuildingUtils.GetTotalWorkerPlaces(buildingId, building, out int workPlaces0, out int workPlaces1, out int workPlaces2, out int workPlaces3);
                if (iWorkPlaces > 0)
                {
                    int iWorkerCount = BuildingUtils.GetCurrentWorkerCount(buildingId, building, out int worker0, out int worker1, out int worker2, out int worker3);
                    string sValue = $"{iWorkerCount} / {iWorkPlaces}";
                    m_buildingIssues.Add(new TransferIssueContainer(IssueType.Worker, building.m_workerProblemTimer * 8 / 128, sValue, building.m_workerProblemTimer, buildingId, 0, 0));
                }
            }
        }

        public void CheckServiceIssue(ushort buildingId, Building building)
        {
            // Goods out issues
            if ((building.m_serviceProblemTimer > 1 || (building.m_problems & Notification.Problem1.TooFewServices).IsNotNone) &&
                !BuildingTypeHelper.IsOutsideConnection(buildingId))
            {
                m_buildingIssues.Add(new TransferIssueContainer(IssueType.Services, building.m_serviceProblemTimer * 7 / 4, "", building.m_serviceProblemTimer, buildingId, 0, 0));
            }
        }

        public void CheckCrimeIssue(ushort buildingId, Building building)
        {
            if (building.m_crimeBuffer > 0 &&
                (building.m_problems & Notification.Problem1.Crime).IsNotNone &&
                building.Info is not null && 
                building.Info.GetService() != ItemClass.Service.PoliceDepartment)
            {
                // Cached citizen count
                int iMaxRate = StatusDataCrime.iMAJOR_CRIME_RATE;
                int iCitizenCount = CrimeCitizenCountStorage.GetCitizenCount(buildingId, building); 
                if (iCitizenCount > 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case MainIndustryBuildingAI:
                        case MainCampusBuildingAI:
                            {
                                // Main buildings use a slightly different algorithm to determine priority.
                                iCitizenCount += 100;
                                iMaxRate = StatusDataCrime.iMAIN_BUILDING_MAJOR_CRIME_RATE;
 
                                goto default;
                            }
                        default:
                            {
                                int iCrimeRate = building.m_crimeBuffer / iCitizenCount;
                                bool bFoundVehicle = false;

                                // Enumerate vehicles on route.
                                BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                                {
                                    if ((CustomTransferReason.Reason)vehicle.m_transferType == CustomTransferReason.Reason.Crime ||
                                        (CustomTransferReason.Reason)vehicle.m_transferType == CustomTransferReason.Reason.Crime2)
                                    {
                                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Crime, building.m_crimeBuffer / Mathf.Max(1, iCitizenCount * 10), Utils.MakePercent(iCrimeRate, iMaxRate), 0, buildingId, vehicle.m_sourceBuilding, vehicleId));
                                        bFoundVehicle = true;
                                    }
                                });

                                // Add issue without vehicle if no vehicles found
                                if (!bFoundVehicle)
                                {
                                    m_buildingIssues.Add(new TransferIssueContainer(IssueType.Crime, building.m_crimeBuffer / Mathf.Max(1, iCitizenCount * 10), Utils.MakePercent(iCrimeRate, iMaxRate), 0, buildingId, 0, 0));
                                }
                                break;
                            }
                    }
                }
            }
        }

        public void CheckMailIssue(ushort buildingId, Building building)
        {
            // Cached citizen count
            if (building.m_mailBuffer > 0)
            {
                switch (building.Info.GetAI())
                {
                    case PostOfficeAI:
                        {
                            break;
                        }
                    default:
                        {
                            int iMaxMail = StatusDataBuildingMail.GetMaxMail(buildingId, building);
                            if (iMaxMail > 0)
                            {
                                int iPriority = building.m_mailBuffer * 8 / iMaxMail;
                                if (iPriority >= 5)
                                {
                                    bool bFoundVehicle = false;

                                    // Enumerate vehicles on route.
                                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                                    {
                                        if ((CustomTransferReason.Reason)vehicle.m_transferType == CustomTransferReason.Reason.Mail)
                                        {
                                            m_buildingIssues.Add(new TransferIssueContainer(IssueType.Mail, iPriority, Utils.MakePercent(building.m_mailBuffer, iMaxMail), 0, buildingId, vehicle.m_sourceBuilding, vehicleId));
                                            bFoundVehicle = true;
                                        }
                                    });

                                    // Add issue without vehicle if no vehicles found
                                    if (!bFoundVehicle)
                                    {
                                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Mail, iPriority, Utils.MakePercent(building.m_mailBuffer, iMaxMail), 0, buildingId, 0, 0));
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }

        public void CheckFireIssue(ushort buildingId, Building building)
        {
            // Cached citizen count
            if (building.m_fireIntensity > 0)
            {
                bool bFoundVehicle = false;

                // Enumerate vehicles on route.
                BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                {
                    if ((CustomTransferReason.Reason)vehicle.m_transferType == CustomTransferReason.Reason.Fire ||
                        (CustomTransferReason.Reason)vehicle.m_transferType == CustomTransferReason.Reason.Fire2)
                    {
                        m_buildingIssues.Add(new TransferIssueContainer(IssueType.Fire, 7, building.m_fireIntensity.ToString(), 0, buildingId, vehicle.m_sourceBuilding, vehicleId));
                        bFoundVehicle = true;
                    }
                });

                // Add issue without vehicle if no vehicles found
                if (!bFoundVehicle)
                {
                    m_buildingIssues.Add(new TransferIssueContainer(IssueType.Fire, 7, building.m_fireIntensity.ToString(), 0, buildingId, 0, 0));
                }
            }
        }

        public void CheckRoadAccess(ushort buildingId, Building building)
        {
            if (building.m_accessSegment == 0 &&
                building.m_parentBuilding == 0 &&
                building.Info is not null &&
                building.Info.m_placementMode == BuildingInfo.PlacementMode.Roadside &&
                building.Info.GetAI() is CommonBuildingAI && // Make sure its an actual building with needs
                building.Info.GetAI() is not ParkAI) // We exclude parks due to all the workshop parking lots having issues.
            {
                InstanceID buildingInstance = new InstanceID { Building = buildingId };
                m_buildingRoadAccess.Add(new RoadAccessData(buildingInstance, RoadAccessStorage.GetInstanceCount(buildingInstance)));
            }
        }
    }
}