using ICities;
using SleepyCommon;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusDataBuilding : StatusData
    {
        public StatusDataBuilding(CustomTransferReason.Reason reason, BuildingType eBuildingType, ushort BuildingId) :
            base(reason, eBuildingType, BuildingId)
        {
        }

        public override bool IsBuildingData()
        {
            return true;
        }

        public override bool IsNodeData()
        {
            return false;
        }

        public override string GetMaterialDisplay()
        {
            return GetMaterialDescription();
        }

        public override bool HasVehicle()
        {
            return false;
        }

        public override ushort GetVehicleId()
        {
            return 0;
        }

        protected override string CalculateTimer(out string tooltip)
        {
            // Timers are material specific for buildings
            tooltip = "";
            return "";
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
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

        protected string DisplayValueAsPercent(int iBuffer, int iMaxValue)
        {
            return Utils.MakePercent(iBuffer, iMaxValue);
        }

        protected string MakeTooltip(int iBuffer)
        {
            // We also store the tooltip here
            return $"{GetMaterialDescription()}: {DisplayBufferLong(iBuffer)}";
        }

        protected string MakeTooltip(int iBuffer, int iMaxValue)
        {
            return $"{GetMaterialDescription()}: {DisplayBufferLong(iBuffer)}/{DisplayBufferLong(iMaxValue)}";
        }

        protected void WarnText()
        {
            m_color = KnownColor.orange;
        }

        protected void WarnText(bool bMin, bool bMax, int iBuffer, int iMaxValue)
        {
            if (bMin && iBuffer == 0)
            {
                m_color = KnownColor.orange;
            }

            if (bMax && iBuffer >= iMaxValue)
            {
                m_color = KnownColor.orange;
            }
        }
    }
}