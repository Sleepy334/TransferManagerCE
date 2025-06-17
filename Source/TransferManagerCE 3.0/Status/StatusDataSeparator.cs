using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSeparator : StatusData
    {
        public StatusDataSeparator() :
            base(CustomTransferReason.Reason.None, BuildingType.None, 0)
        {
        }

        public override bool IsSeparator()
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
            return "";
        }

        public override string GetMaterialDescription()
        {
            return "";
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
    }
}
