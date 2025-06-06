using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.Data
{
    public class VehicleDataMail : VehicleData
    {
        public VehicleDataMail(Vector3 buildingPos, ushort vehicleId) : 
            base(buildingPos, vehicleId)
        {
        }

        public override string GetValue()
        {
            if (m_value is null)
            {
                if ((CustomTransferReason)m_vehicle.m_transferType == CustomTransferReason.Reason.Mail)
                {
                    m_value = CitiesUtils.GetVehicleTransferValue(GetVehicleId(), out int current, out int max);
                    m_valueTooltip = $"SortedMail: {SleepyCommon.Utils.MakePercent(current, max, 1)}\nUnsortedMail: {SleepyCommon.Utils.MakePercent(max - current, max, 1)}\nBuffer: {StatusData.DisplayBufferLong(m_vehicle.m_transferSize)}";
                }
                else
                {
                    base.GetValue();
                }
            }

            return m_value;
        }
    }
}
