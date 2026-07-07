using ICities;
using SleepyCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
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
            if (building.m_flags != 0)
            { 
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

                tooltip = DescribeSick(m_buildingId, building, out int iCount);

                return iCount.ToString();
            }

            return "0";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            string sTimer = base.CalculateTimer(out tooltip);

            AddTimerText(TimerType.Sick, ref sTimer, ref tooltip);

            return sTimer;
        }

        private string DescribeSick(ushort buildingId, Building building, out int Count)
        {
            int iCount = 0;
            string sText = "";

            CitizenUtils.EnumerateCitizens(new InstanceID { Building = buildingId }, building.m_citizenUnits, (citizenId, citizen) =>
            {
                if (citizen.GetBuildingByLocation() == buildingId && (citizen.m_flags & Citizen.Flags.Sick) == Citizen.Flags.Sick)
                {
                    iCount++;
                    sText += $"{InstanceHelper.DescribeInstance(new InstanceID { Citizen = citizenId }, false, false)} | Age: {citizen.Age} | Health: {citizen.m_health}\r\n";
                }
                // continue loop
                return true;
            });

            Count = iCount;
            return sText;
        }
    }
}