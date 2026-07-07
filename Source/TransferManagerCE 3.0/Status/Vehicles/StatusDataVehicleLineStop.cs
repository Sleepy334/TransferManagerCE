using SleepyCommon;
using static TransferManagerCore.BuildingTypeHelper;
using static TransferManagerCore.StopHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleLineStop : StatusDataVehicleNode
    {
        // --------------------------------------------------------------------
        public ushort m_lineId;
        private int m_stopNumber;

        // --------------------------------------------------------------------
        public StatusDataVehicleLineStop(StopType eStopType, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort lineId, ushort sourceBuildingId, InstanceID target) :
            base(eStopType, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
            m_lineId = lineId;
            m_stopNumber = TransportUtils.GetStopNumber(m_lineId, m_target.NetNode);
        }

        // --------------------------------------------------------------------
        public override string GetMaterialDisplay()
        {
            // We leave this column blank so they become sub-items for the node.
            return "";
        }

        // --------------------------------------------------------------------
        protected override string CalculateDescription2(out string tooltip)
        {
            // Tooltip
            tooltip = $"Stop: {m_stopNumber} | {InstanceHelper.DescribeInstance(m_target, true, true)}";
            return $"Stop: {m_stopNumber}";
        }

        // --------------------------------------------------------------------
        public override void OnClickDescription1()
        {
            InstanceHelper.ShowInstance(new InstanceID { Vehicle = GetVehicleId() });
        }

        // --------------------------------------------------------------------
        public override void OnClickDescription2()
        {
            InstanceHelper.ShowInstance(m_target);
        }
    }
}