using System;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferIssueHelper
    {
        public enum IssueType
        {
            Sick,
            Dead,
            GoodsIn,
            GoodsOut,
        }

        private List<TransferIssueContainer> m_sickIssues = new List<TransferIssueContainer>();
        private List<TransferIssueContainer> m_deadIssues = new List<TransferIssueContainer>();
        private List<TransferIssueContainer> m_goodsInIssues = new List<TransferIssueContainer>();
        private List<TransferIssueContainer> m_goodsOutIssues = new List<TransferIssueContainer>();

        public TransferIssueHelper()
        {
        }

        public List<TransferIssueContainer> GetIssues(IssueType type)
        {
            switch (type)
            {
                case IssueType.Sick: return m_sickIssues;
                case IssueType.Dead: return m_deadIssues;
                case IssueType.GoodsIn: return m_goodsInIssues;
                case IssueType.GoodsOut: return m_goodsOutIssues;
            }
            return m_sickIssues;
        }

        public void UpdateIssues()
        {
            m_sickIssues.Clear();
            m_deadIssues.Clear();
            m_goodsInIssues.Clear();
            m_goodsOutIssues.Clear();

            // Get new issues
            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; i++)
            {
                ushort sourceBuilding = (ushort)i;
                Building building = BuildingManager.instance.m_buildings.m_buffer[sourceBuilding];
                if (building.m_flags != 0)
                {
                    CheckSickIssue(sourceBuilding, building);
                    CheckDeadIssue(sourceBuilding, building);
                    CheckGoodsInIssue(sourceBuilding, building);
                    CheckGoodsOutIssue(sourceBuilding, building);
                }
            }
        }

        public void CheckSickIssue(ushort buildingId, Building building)
        {
            // Health issues
            if (building.m_healthProblemTimer > ModSettings.GetSettings().SickTimerValue ||
                (building.m_problems & Notification.Problem1.DirtyWater) == Notification.Problem1.DirtyWater ||
                (building.m_problems & Notification.Problem1.Pollution) == Notification.Problem1.Pollution)
            {
                int iSickCount = BuildingUtils.GetSickCount(buildingId, building);
                if (iSickCount > 0)
                {
                    bool bShowIssuesWithVehicles = ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute;
                    bool bFoundVehicle = false;

                    // Enumerate vehicles on route.
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if ((TransferReason)vehicle.m_transferType == TransferReason.Sick)
                        {
                            if (bShowIssuesWithVehicles)
                            {
                                TransferIssueContainer issue = new TransferIssueContainer(TransferReason.Sick, iSickCount, building.m_healthProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                                m_sickIssues.Add(issue);
                            }
                            bFoundVehicle = true;
                        }
                    });

                    // Add issue without vehicle if no vehicles found
                    if (!bFoundVehicle)
                    {
                        m_sickIssues.Add(new TransferIssueContainer(TransferManager.TransferReason.Sick, iSickCount, building.m_healthProblemTimer, buildingId, 0, 0));
                    }
                }
            }
        }

        public void CheckDeadIssue(ushort buildingId, Building building)
        {
            // Dead issues
            if (building.m_deathProblemTimer > ModSettings.GetSettings().DeadTimerValue || (building.m_problems & Notification.Problem1.Death) == Notification.Problem1.Death)
            {                
                int iDeadCount = BuildingUtils.GetDeadCount(buildingId, building);
                if (iDeadCount > 0)
                {
                    bool bShowIssuesWithVehicles = ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute;
                    bool bFoundVehicle = false;

                    // Enumerate vehicles on route.
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if ((TransferReason) vehicle.m_transferType == TransferReason.Dead)
                        {
                            if (bShowIssuesWithVehicles)
                            {
                                TransferIssueContainer issue = new TransferIssueContainer(TransferReason.Dead, iDeadCount, building.m_deathProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                                m_deadIssues.Add(issue);
                            }
                            bFoundVehicle = true;
                        }
                    });

                    // Add issue without vehicle if no vehicles found
                    if (!bFoundVehicle)
                    {
                        m_deadIssues.Add(new TransferIssueContainer(TransferReason.Dead, iDeadCount, building.m_deathProblemTimer, buildingId, 0, 0));
                    }
                }
            }
        }

        public void CheckGoodsInIssue(ushort buildingId, Building building)
        {
            // Health issues
            if (building.m_flags != Building.Flags.None && building.m_incomingProblemTimer > ModSettings.GetSettings().GoodsTimerValue)
            {
                BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                if (eType != BuildingTypeHelper.BuildingType.SpaceElevator && eType != BuildingTypeHelper.BuildingType.OutsideConnection)
                {
                    bool bShowIssuesWithVehicles = ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute;
                    bool bFoundVehicle = false;

                    // Enumerate vehicles on route.
                    BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                    {
                        if ((vehicle.m_flags & Vehicle.Flags.TransferToTarget) == Vehicle.Flags.TransferToTarget &&
                            (vehicle.Info != null && vehicle.Info.m_vehicleAI is CargoTruckAI) &&
                            vehicle.m_sourceBuilding != 0) // Quite often importing vehicles have no target till they get to a cargo staion etc...
                        {
                            if (bShowIssuesWithVehicles)
                            {
                                TransferIssueContainer issue = new TransferIssueContainer((TransferReason)vehicle.m_transferType, (int)Math.Round(building.m_customBuffer2 * 0.001), building.m_incomingProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                                m_goodsInIssues.Add(issue);
                            }
                            bFoundVehicle = true;
                        }
                    });

                    // Add issue without vehicle if no vehicles found
                    if (!bFoundVehicle)
                    {
                        m_goodsInIssues.Add(new TransferIssueContainer(TransferReason.Goods, (int)Math.Round(building.m_customBuffer2 * 0.001), building.m_incomingProblemTimer, buildingId, 0, 0));
                    }
                }
            }
        }

        public void CheckGoodsOutIssue(ushort buildingId, Building building)
        {
            // Goods out issues
            if (building.m_flags != Building.Flags.None && !BuildingTypeHelper.IsOutsideConnection(buildingId) && building.m_outgoingProblemTimer > ModSettings.GetSettings().GoodsTimerValue)
            {
                m_goodsOutIssues.Add(new TransferIssueContainer(TransferReason.Goods, (int)Math.Round(building.m_customBuffer2 * 0.001), building.m_outgoingProblemTimer, buildingId, 0, 0));
            }
        }
    }
}