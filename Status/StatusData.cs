using System;
using static TransferManager;

namespace TransferManagerCE.Data
{
    public abstract class StatusData
    {
        public TransferReason m_material;
        public ushort m_buildingId;
        public ushort m_responderBuilding;
        public ushort m_targetVehicle;

        public StatusData(TransferReason reason, ushort buildingId, ushort responderBuilding, ushort targetVehicle)
        {
            m_material = reason;
            m_buildingId = buildingId;
            m_responderBuilding = responderBuilding;
            m_targetVehicle = targetVehicle;
        }

        public virtual string GetMaterialDescription()
        {
            return m_material.ToString();
        }

        public virtual string GetValue()
        {
            Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[m_targetVehicle];
            return Math.Round(vehicle.m_transferSize * 0.001).ToString();
        }

        public virtual string GetTimer()
        {
            return "";
        }

        public virtual void Update() { }

        public virtual string GetResponder()
        {
            if (m_responderBuilding != 0)
            {
                return CitiesUtils.GetBuildingName(m_responderBuilding);
            }

            return "None";
        }

        public virtual string GetTarget()
        {
            if (m_targetVehicle != 0)
            {
                return CitiesUtils.GetVehicleName(m_targetVehicle);
            }

            return "None";
        }
    }
}
