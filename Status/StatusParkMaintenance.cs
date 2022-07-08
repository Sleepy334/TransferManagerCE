using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusParkMaintenance : StatusData
    {
        public StatusParkMaintenance(ushort BuildingId, ushort responder, ushort target) :
            base(TransferReason.ParkMaintenance, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];

            int current = 0;
            if (building.Info.GetAI() is ParkAI)
            {
                current = ((building.m_workerProblemTimer << 8) | building.m_taxProblemTimer);
            }
            else if (building.Info.GetAI() is ParkBuildingAI || building.Info.GetAI() is ParkGateAI)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte park = instance.GetPark(building.m_position);
                if (park != 0)
                {
                    int max;
                    instance.m_parks.m_buffer[park].GetMaintenanceLevel(out current, out max);
                }
            }

            return current.ToString();
        }
    }
}