using System.Collections.Generic;
using TransferManagerCore.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataServicePoint : StatusDataBuilding
    {
        public StatusDataServicePoint(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort buildingId)
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
                        ServicePointUtils.GetServicePointInValues(m_buildingId, (TransferReason) m_material, out int iCount, out int iBuffer);

                        tooltip = $"Buildings with {m_material}: {iCount}\n{MakeTooltip(iBuffer)}";
                        return $"{iCount} | {DisplayBuffer(iBuffer)}";
                    }
            }

            tooltip = "";
            return "0";
        }
    }
}