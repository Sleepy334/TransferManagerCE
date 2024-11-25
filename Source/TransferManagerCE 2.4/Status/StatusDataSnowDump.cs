using System;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSnowDump : StatusData
    {
        public StatusDataSnowDump(BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(TransferReason.Snow, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
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
                            if (iAmount == 0 || iCapacity == 0)
                            {
                                return "0%";
                            }
                            else
                            {
                                return $"{Math.Round(((float)iAmount / (float)iCapacity * 100.0), 0)}%";
                            }
                        }
                        else
                        {
                            return "";
                        }
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        public override string GetResponder()
        {
            return "";
        }

        public override string GetTarget()
        {
            return "";
        }
    }
}