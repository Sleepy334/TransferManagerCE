using System.Collections.Generic;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataServicePoint : StatusDataBuilding
    {
        public StatusDataServicePoint(TransferReason material, BuildingType eBuildingType, ushort buildingId)
            : base(material, eBuildingType, buildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.ServicePoint:
                    {
                        ServicePointUtils.GetServicePointInValues(m_buildingId, m_material, out int iCount, out int iBuffer);

                        tooltip = $"Buildings with {m_material}: {iCount}\n{MakeTooltip(iBuffer)}";
                        return $"{iCount} | {DisplayBuffer(iBuffer)}";
                    }
            }

            tooltip = "";
            return "0";
        }
    }
}