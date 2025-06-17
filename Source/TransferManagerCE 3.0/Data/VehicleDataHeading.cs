using SleepyCommon;
using UnityEngine;

namespace TransferManagerCE.Data
{
    public class VehicleDataHeading : VehicleData
    {
        string m_heading = string.Empty;

        public VehicleDataHeading(string heading) :
            base(Vector3.zero, 0)
        {
            m_heading = heading;
        }

        public override bool IsHeading()
        {
            return true;
        }

        public override string GetMaterialDescription()
        {
            return m_heading;
        }

        public override string GetValue()
        {
            return "";
        }

        public override string GetTimer()
        {
            return "";
        }

        public override string GetTarget()
        {
            return "";
        }

        public override string GetVehicle()
        {
            return "";
        }

        public override Color GetTextColor()
        {
            return KnownColor.cyan;
        }
    }
}
