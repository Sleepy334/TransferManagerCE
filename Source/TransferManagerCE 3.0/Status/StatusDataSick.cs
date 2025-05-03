using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSick : StatusData
    {
        public StatusDataSick(TransferReason reason, BuildingType eBuildingType, ushort BuildingId, ushort responder, ushort target) : 
            base(reason, eBuildingType, BuildingId, responder, target)
        {
        }

        protected override string CalculateValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_flags != 0 && building.Info is not null && building.Info.GetService() == ItemClass.Service.HealthCare)
            {
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
                    return iSickCount + "/" + iPatientCapacity;
                }
            }

            // Default handling
            return BuildingUtils.GetSickCount(m_buildingId, building).ToString();
        }
        
        protected override string CalculateTimer()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if (building.m_healthProblemTimer > 0)
            {
                return $"S:{building.m_healthProblemTimer} {base.CalculateTimer()}";
            }
            else
            {
                return base.CalculateTimer();
            }
        }

        public override string GetTooltip()
        {
            // Display name of citizen the ambulance is coming to pick up
            ushort vehicleId = GetTargetId();
            if (vehicleId != 0)
            {
                string strCitizenName = "";

                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                if (vehicle.m_flags != 0)
                {
                    CitizenUtils.EnumerateCitizens(new InstanceID { Vehicle = vehicleId }, vehicle.m_citizenUnits, (citizendId, citizen) =>
                    {
                        if (citizen.Sick)
                        {
                            strCitizenName = $"#{citizendId}:{CitiesUtils.GetCitizenName(citizendId)}";
                            return false; // Break loop
                        }
                        return true; // continue loop
                    });
                }

                if (strCitizenName.Length == 0) 
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                    if (building.m_flags != 0)
                    {
                        CitizenUtils.EnumerateCitizens(new InstanceID { Building = m_buildingId }, building.m_citizenUnits, (citizendId, citizen) =>
                        {
                            if (citizen.m_vehicle == vehicleId)
                            {
                                strCitizenName = $"#{citizendId}:{CitiesUtils.GetCitizenName(citizendId)}";
                                return false; // Break loop
                            }
                            return true; // continue loop
                        });
                    }
                }

                if (strCitizenName.Length > 0)
                {
                    return $"{strCitizenName} | #{GetTargetId()}:{GetTarget()} | #{GetResponderId()}:{GetResponder()}";
                }
                else
                {
                    return $"#{GetTargetId()}:{GetTarget()} | #{GetResponderId()}:{GetResponder()}";
                }
            }

            return "";
        }
    }
}