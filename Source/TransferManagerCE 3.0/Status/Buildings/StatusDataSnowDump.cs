using SleepyCommon;
using System;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSnowDump : StatusDataBuilding
    {
        public StatusDataSnowDump(BuildingType eBuildingType, ushort BuildingId) : 
            base(TransferReason.Snow, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            switch (m_eBuildingType)
            {
                case BuildingType.SnowDump:
                    {
                        SnowDumpAI? buildingAI = building.Info.m_buildingAI as SnowDumpAI;
                        if (buildingAI is not null)
                        {
                            int iAmount = buildingAI.GetSnowAmount(m_buildingId, ref building);
                            int iCapacity = buildingAI.m_snowCapacity;

                            WarnText(false, true, iAmount, iCapacity);
                            tooltip = MakeTooltip(true, iAmount, iCapacity);
                            return Utils.MakePercent(iAmount, iCapacity);
                        }
                        break;
                    }
            }

            tooltip = "";
            return "";
        }
    }
}