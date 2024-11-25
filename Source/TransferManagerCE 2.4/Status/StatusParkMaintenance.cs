using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusParkMaintenance : StatusData
    {
        public StatusParkMaintenance(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) :
            base(TransferReason.ParkMaintenance, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
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