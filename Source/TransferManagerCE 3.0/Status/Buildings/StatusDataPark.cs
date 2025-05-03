using ColossalFramework;
using System.Reflection;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataPark : StatusDataBuilding
    {
        public StatusDataPark(TransferReason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0)
            {
                switch (m_material)
                {
                    case TransferReason.ParkMaintenance:
                        {
                            int current = 0;
                            int max = 0;

                            switch (building.Info.GetAI())
                            {
                                case ParkAI parkAI:
                                    {
                                        GetMaintenanceLevel(parkAI, m_buildingId, ref building, out current, out max);
                                        break;
                                    }
                                case ParkBuildingAI:
                                case ParkGateAI:
                                    {
                                        DistrictManager instance = Singleton<DistrictManager>.instance;
                                        byte park = instance.GetPark(building.m_position);
                                        if (park != 0)
                                        {
                                            instance.m_parks.m_buffer[park].GetMaintenanceLevel(out current, out max);
                                        }
                                        break;
                                    }
                            }

                            if (max > 0)
                            {
                                WarnText(true, false, current, max);
                                tooltip = MakeTooltip(false, current, max);
                                return $"{SleepyCommon.Utils.MakePercent(current, max)}";
                            }
                            else
                            {
                                tooltip = "";
                                return $"{current}";
                            }
                        }
                }              
            }

            tooltip = "";
            return $"";
        }
        
        public static void GetMaintenanceLevel(ParkAI buildingAI, ushort buildingId, ref Building building, out int current, out int max)
        {
            current = 0;
            max = 0;

            if (buildingAI is not null)
            {
                MethodInfo? methodGetMaintenanceLevel = buildingAI.GetType().GetMethod("GetMaintenanceLevel", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodGetMaintenanceLevel != null)
                {
                    int iCurrent = 0;
                    int iMax = 0;
                    object[] args = new object[] { buildingId, building, iCurrent, iMax };
                    methodGetMaintenanceLevel.Invoke(buildingAI, args);
                    current = (int) args[2];
                    max = (int)args[3];
                }
            }
        }
    }
}