using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSchool : StatusDataBuilding
    {
        public StatusDataSchool(TransferReason material, BuildingType eBuildingType, ushort BuildingId) : 
            base(material, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                SchoolAI? buildingAI = building.Info?.m_buildingAI as SchoolAI;
                if (buildingAI is not null)
                {
                    buildingAI.GetStudentCount(m_buildingId, ref building, out int count, out int capacity, out int global);
                    WarnText(true, true, count, capacity);
                    return $"{count} / {capacity}";
                }
                
            }

            return "";
        }
    }
}