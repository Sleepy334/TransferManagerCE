using System;
using System.Collections.Generic;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public class StatusDataProcessingFacility : StatusData
    {
        public StatusDataProcessingFacility(TransferReason reason, ushort BuildingId, ushort responder, ushort target) :
            base(reason, BuildingId, responder, target)
        {
        }

        public override string GetValue()
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            ProcessingFacilityAI? buildingAI = building.Info?.m_buildingAI as ProcessingFacilityAI;
            if (buildingAI != null)
            {
                double dValue = 0;
                double dBufferSize = 0;
                if (buildingAI.m_inputResource1 == m_material)
                {
                    dValue = (building.m_customBuffer2 * 0.001);
                    dBufferSize = buildingAI.GetInputBufferSize1(m_buildingId, ref building) * 0.001;
                } 
                else if (buildingAI.m_inputResource2 == m_material)
                {
                    dValue = ((building.m_teens << 8) | building.m_youngs) * 0.001;
                    dBufferSize = buildingAI.GetInputBufferSize2(m_buildingId, ref building) * 0.001;
                }
                else if (buildingAI.m_inputResource3 == m_material)
                {
                    dValue = ((building.m_adults << 8) | building.m_seniors) * 0.001;
                    dBufferSize = buildingAI.GetInputBufferSize3(m_buildingId, ref building) * 0.001;
                }
                else if (buildingAI.m_inputResource4 == m_material)
                {
                    dValue = ((building.m_education1 << 8) | building.m_education2) * 0.001;
                    dBufferSize = buildingAI.GetInputBufferSize4(m_buildingId, ref building) * 0.001;
                }
                return Math.Round(dValue).ToString("N0") + "/" + Math.Round(dBufferSize).ToString("N0");
            }
            return 0.ToString();
        }

        public override string GetTarget()
        {
            if (m_targetVehicle != 0)
            {
                Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
                return CitiesUtils.GetVehicleName(m_targetVehicle) + " (" + vehicle.m_transferSize * 0.001 + ")";
            }

            return "None";
        }
    }
}