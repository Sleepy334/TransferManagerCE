using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
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

        public override bool IsVehicleData()
        {
            return false;
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
    }
}
