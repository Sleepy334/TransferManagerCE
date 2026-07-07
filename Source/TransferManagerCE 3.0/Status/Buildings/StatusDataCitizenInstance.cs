using System;
using ColossalFramework;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataCitizenInstance : StatusDataBuilding
    {
        public StatusDataCitizenInstance(CustomTransferReason.Reason material, BuildingType eBuildingType, ushort BuildingId) :
            base(material, eBuildingType, BuildingId)
        {
        }

        public override string GetMaterialDescription()
        {
            return "Citizen Instances";
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "Source citizen instances | Target citizen instances";

            if (m_buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    return $"{GetSourceCitizens(building)} | {GetTargetCitizens(building)}";
                }
            }

            return "0";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = "";
            return ""; // No vehicles
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = "";
            return ""; // No vehicles
        }

        public static TransferReason GetOutgoingTransferReason(Building building)
        {
            return TransferReason.None;
        }

        private int GetSourceCitizens(Building building)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;

            int iCitizenCount = 0;

            ushort num = building.m_sourceCitizens;
            int num2 = 0;
            while (num != 0)
            {
                iCitizenCount++;

                ushort nextSourceInstance = instance.m_instances.m_buffer[num].m_nextSourceInstance;
                CitizenInfo info = instance.m_instances.m_buffer[num].Info;
                num = nextSourceInstance;
                if (++num2 > 65536)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iCitizenCount;
        }

        private int GetTargetCitizens(Building building)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;

            int iCitizenCount = 0;

            ushort num = building.m_targetCitizens;
            int num2 = 0;
            while (num != 0)
            {
                iCitizenCount++;

                ushort nextSourceInstance = instance.m_instances.m_buffer[num].m_nextTargetInstance;
                CitizenInfo info = instance.m_instances.m_buffer[num].Info;
                num = nextSourceInstance;
                if (++num2 > 65536)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return iCitizenCount;
        }
    }
}