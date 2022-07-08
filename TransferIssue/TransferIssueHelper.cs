using System;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferIssueHelper
    { 
        public Dictionary<TransferReason, List<TransferIssueContainer>> m_listIssues = new Dictionary<TransferReason, List<TransferIssueContainer>>();

        public TransferIssueHelper()
        {
            m_listIssues = new Dictionary<TransferReason, List<TransferIssueContainer>>();
            m_listIssues[TransferReason.Sick] = new List<TransferIssueContainer>();
            m_listIssues[TransferReason.Dead] = new List<TransferIssueContainer>();
        }

        public List<TransferIssueContainer> GetIssues(TransferReason reason)
        {
            return m_listIssues[reason];
        }

        public void UpdateIssues()
        {
            Dictionary<TransferReason, List<TransferIssueContainer>> list = new Dictionary<TransferReason, List<TransferIssueContainer>>();
            m_listIssues[TransferReason.Sick] = new List<TransferIssueContainer>();
            m_listIssues[TransferReason.Dead] = new List<TransferIssueContainer>();
            m_listIssues[TransferReason.Goods] = new List<TransferIssueContainer>();

            // Get new issues
            for (int i = 0; i < BuildingManager.instance.m_buildings.m_buffer.Length; i++)
            {
                ushort sourceBuilding = (ushort)i;
                Building building = BuildingManager.instance.m_buildings.m_buffer[sourceBuilding];

                m_listIssues[TransferReason.Sick].AddRange(GetSickIssues((ushort)i, building));
                m_listIssues[TransferReason.Dead].AddRange(GetDeadIssues((ushort)i, building));
                m_listIssues[TransferReason.Goods].AddRange(GetGoodsIssues((ushort)i, building));
            }
        }

        public List<TransferIssueContainer> GetDeadIssues(ushort buildingId, Building building)
        {
            List<TransferIssueContainer> list = new List<TransferIssueContainer>();

            // Dead issues
            if (building.m_deathProblemTimer > 0)
            {
                List<uint> cimDead = CitiesUtils.GetDeadCitizens(buildingId, building);
                if (cimDead.Count > 0)
                {
                    List<ushort> vehicles = CitiesUtils.GetHearsesOnRoute(buildingId);
                    if (vehicles.Count > 0)
                    {
                        DateTime timestamp = GetTimestamptForIssue(TransferReason.Dead, buildingId);
                        if (ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute ||
                            (DateTime.Now - GetTimestamptForIssue(TransferReason.Dead, buildingId)).TotalSeconds < ModSettings.GetSettings().TransferIssueDeleteResolvedDelay)
                        {
                            foreach (ushort vehicleId in vehicles)
                            {
                                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                                TransferIssueContainer issue = new TransferIssueContainer(TransferReason.Dead, cimDead.Count, building.m_deathProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                                if (timestamp != DateTime.MinValue)
                                {
                                    issue.m_timeStamp = timestamp;
                                }
                                list.Add(issue);
                            }
                        }
                    }
                    else
                    {
                        list.Add(new TransferIssueContainer(TransferReason.Dead, cimDead.Count, building.m_deathProblemTimer, buildingId, 0, 0));
                    }
                }
            }

            return list;
        }

        public List<TransferIssueContainer> GetSickIssues(ushort buildingId, Building building)
        {
            List<TransferIssueContainer> list = new List<TransferIssueContainer>();

            // Health issues
            if (building.m_healthProblemTimer > 0)
            {
                List<uint> cimSick = CitiesUtils.GetSickCitizens(buildingId, building);
                if (cimSick.Count > 0)
                {
                    List<ushort> vehicles = CitiesUtils.GetAmbulancesOnRoute(buildingId);
                    if (vehicles.Count > 0)
                    {
                        DateTime timestamp = GetTimestamptForIssue(TransferReason.Sick, buildingId);
                        if (ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute ||
                            (DateTime.Now - timestamp).TotalSeconds < ModSettings.GetSettings().TransferIssueDeleteResolvedDelay)
                        {
                            foreach (ushort vehicleId in vehicles)
                            {
                                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                                TransferIssueContainer issue = new TransferIssueContainer(TransferReason.Sick, cimSick.Count, building.m_healthProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                                if (timestamp != DateTime.MinValue)
                                {
                                    issue.m_timeStamp = timestamp;
                                }
                                list.Add(issue);
                            }
                        }
                    }
                    else
                    {
                        list.Add(new TransferIssueContainer(TransferManager.TransferReason.Sick, cimSick.Count, building.m_healthProblemTimer, buildingId, 0, 0));
                    }

                }
            }

            return list;
        }

        public List<TransferIssueContainer> GetGoodsIssues(ushort buildingId, Building building)
        {
            List<TransferIssueContainer> list = new List<TransferIssueContainer>();

            // Health issues
            if (building.m_flags != Building.Flags.None && !BuildingTypeHelper.IsOutsideConnection(buildingId) && building.m_incomingProblemTimer > 0)
            {
                List<ushort> vehicles = CitiesUtils.GetGoodsTrucksOnRoute(buildingId);
                if (vehicles.Count > 0)
                {
                    DateTime timestamp = GetTimestamptForIssue(TransferReason.Goods, buildingId);
                    if (ModSettings.GetSettings().TransferIssueShowWithVehiclesOnRoute ||
                        (DateTime.Now - timestamp).TotalSeconds < ModSettings.GetSettings().TransferIssueDeleteResolvedDelay)
                    {
                        foreach (ushort vehicleId in vehicles)
                        {
                            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                            TransferIssueContainer issue = new TransferIssueContainer(TransferReason.Goods, (int)Math.Round(building.m_customBuffer2 * 0.001), building.m_incomingProblemTimer, buildingId, vehicle.m_sourceBuilding, vehicleId);
                            if (timestamp != DateTime.MinValue)
                            {
                                issue.m_timeStamp = timestamp;
                            }
                            list.Add(issue);
                        }
                    }
                }
                else
                {
                    list.Add(new TransferIssueContainer(TransferReason.Goods, (int)Math.Round(building.m_customBuffer2 * 0.001), building.m_incomingProblemTimer, buildingId, 0, 0));
                }
            }

            return list;
        }

        private DateTime GetTimestamptForIssue(TransferReason material, ushort buildingId)
        {
            List<TransferIssueContainer> list = m_listIssues[material];
            if (list != null)
            {
                foreach (TransferIssueContainer issue in list)
                {
                    if (issue.m_sourceBuildingId == buildingId)
                    {
                        return issue.m_timeStamp;
                    }
                }
            }
            return DateTime.MinValue;
        }
    }
}