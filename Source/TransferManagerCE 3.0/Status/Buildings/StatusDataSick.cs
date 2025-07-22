using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSick : StatusDataBuilding
    {
        public StatusDataSick(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "";

            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if ((building.m_flags & Building.Flags.Active) == 0)
            {
                return "";
            }

            if (building.Info is not null && building.Info.GetService() == ItemClass.Service.HealthCare)
            {
                // Some sort of healthcare facility
                int iPatientCapacity = 0;
                int iSickCount = 0;

                switch (m_eBuildingType)
                {
                    case BuildingType.Hospital:
                    case BuildingType.UniversityHospital:
                        {
                            iSickCount = BuildingUtils.GetSickCount(m_buildingId, building);
                            break;
                        }
                    case BuildingType.Childcare:
                        {
                            iSickCount = BuildingUtils.GetChildCount(m_buildingId, building);
                            break;
                        }
                    case BuildingType.Eldercare:
                        {
                            iSickCount = BuildingUtils.GetSeniorCount(m_buildingId, building);
                            break;
                        }
                }

                // Access the PatientCapacity property
                PrefabAI buildingAI = building.Info.GetAI();
                if (buildingAI is not null)
                {
                    PropertyInfo? property = buildingAI.GetType().GetProperty("PatientCapacity");
                    if (property != null)
                    {
                        iPatientCapacity = (int)property.GetValue(buildingAI, new object[] { });
                    }
                }

                if (iPatientCapacity > 0)
                {
                    WarnText(false, true, iSickCount, iPatientCapacity);
                    tooltip = MakeTooltip(iSickCount, iPatientCapacity);
                    return iSickCount + "/" + iPatientCapacity;
                }
            }

            // Default handling
            WarnText(false, true, building.m_healthProblemTimer, 1);
            return BuildingUtils.GetSickCount(m_buildingId, building).ToString();
        }

        protected override string CalculateTimer(out string tooltip)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_healthProblemTimer > 0)
            {
                tooltip = $"Sick Timer: {building.m_healthProblemTimer}";
                return $"S:{building.m_healthProblemTimer}";
            }

            tooltip = "";
            return "";
        }
    }
}