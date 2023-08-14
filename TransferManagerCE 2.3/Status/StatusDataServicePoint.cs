using System.Collections.Generic;
using TransferManagerCE.Util;
using UnityEngine;
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

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.ServicePoint:
                    {
                        ServicePointUtils.GetServicePointInValues(m_buildingId, m_material, out int iCount, out int iBuffer);
                        return $"{iCount} | {ServicePointUtils.DisplayBuffer(iBuffer)}";
                    }
            }

            return "0";
        }

        public override string GetValueTooltip()
        {
            return $"<Building Count> | <Buffer>";
        }
    }
}