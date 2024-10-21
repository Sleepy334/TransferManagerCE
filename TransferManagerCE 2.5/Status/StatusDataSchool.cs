using ColossalFramework;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSchool : StatusData
    {
        public StatusDataSchool(TransferReason material, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(material, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                SchoolAI? buildingAI = building.Info?.m_buildingAI as SchoolAI;
                if (buildingAI is not null)
                {
                    buildingAI.GetStudentCount(m_buildingId, ref building, out int count, out int capacity, out int global);
                    return $"{count}/{capacity}";
                }
                
            }
            return "";
        }

        public override string GetTarget()
        {
            return "";
        }

        public override string GetResponder()
        {
            return "";
        }
    }
}