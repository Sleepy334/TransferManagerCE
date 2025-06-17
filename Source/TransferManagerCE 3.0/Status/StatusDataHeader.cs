using SleepyCommon;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
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

        public override bool IsNodeData()
        {
            return false;
        }

        public override bool HasVehicle()
        {
            return false;
        }

        public override ushort GetVehicleId()
        {
            return 0;
        }

        public override ushort GetResponderId()
        {
            return 0;
        }

        public override string GetMaterialDisplay()
        {
            return m_heading;
        }

        public override string GetMaterialDescription()
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

        protected override string CalculateVehicle(out string tooltip)
        {
            tooltip = "";
            return "";
        }

        protected override string CalculateResponder(out string tooltip)
        {
            tooltip = ""; 
            return "";
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
        }

        public override void OnClickResponder()
        {
        }

        public override void OnClickTarget()
        {
        }

        public override Color GetTextColor()
        {
            return KnownColor.cyan;
        }
    }
}
