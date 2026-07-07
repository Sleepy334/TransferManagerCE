using SleepyCommon;
using UnityEngine;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataHeader : StatusData
    {
        string m_heading = string.Empty;

        // ----------------------------------------------------------------------------------------
        public StatusDataHeader(string heading) :
            base(CustomTransferReason.Reason.None, BuildingType.None, 0)
        {
            m_heading = heading;
        }

        public override bool IsSeparator()
        {
            return true;
        }

        public override bool IsHeader()
        {
            return true;
        }

        public override bool IsBuildingData()
        {
            return false;
        }

        public override bool IsVehicleData()
        {
            return false;
        }

        public override string GetMaterialDisplay()
        {
            return m_heading;
        }

        protected override string CalculateValue(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateTimer(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateDescription1(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = ""; 
            return "";
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
        }

        public override Color GetTextColor()
        {
            return KnownColor.cyan;
        }
    }
}
