using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataServicePoint : StatusData
    {
        public StatusDataServicePoint(TransferReason material, BuildingType eBuildingType, ushort buildingId, ushort responderBuilding, ushort targetVehicle)
            : base(material, eBuildingType, buildingId, responderBuilding, targetVehicle)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.ServicePoint:
                    {
                        Dictionary<TransferReason, int> serviceValues = StatusHelper.GetServicePointValues(m_buildingId);
                        if (serviceValues.ContainsKey(m_material))
                        {
                            return $"{serviceValues[m_material]}";
                        }
                        break;
                    }
            }

            return "0";
        }

        public override string GetValueTooltip()
        {
            return $"# of buildings requesting \"{m_material}\"";
        }
    }
}