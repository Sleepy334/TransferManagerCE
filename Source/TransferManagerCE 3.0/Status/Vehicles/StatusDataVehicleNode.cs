using SleepyCommon;
using static TransferManagerCore.BuildingTypeHelper;
using static TransferManagerCore.StopHelper;

namespace TransferManagerCore.Data
{
    public class StatusDataVehicleNode : StatusDataVehicle
    {
        public StopType m_eStopType;

        // --------------------------------------------------------------------
        public StatusDataVehicleNode(StopType eStopType, BuildingType eBuildingType, ushort BuildingId, ushort vehicleId, ushort sourceBuildingId, InstanceID target) :
            base(CustomTransferReason.Reason.None, eBuildingType, BuildingId, vehicleId, sourceBuildingId, target)
        {
            m_eStopType = eStopType;
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
            tooltip = InstanceHelper.DescribeInstance(m_target, true, true);

            if (GetVehicleId() != 0)
            {
                string sState = DescribeVehicleState(GetVehicleId());
                if (!string.IsNullOrEmpty(sState))
                {
                    return sState;
                }

                if (m_target.NetNode != 0)
                {
                    ushort buildingId = FindBuilding(m_target.NetNode);
                    if (buildingId != 0)
                    {
                        return InstanceHelper.DescribeInstance(new InstanceID { Building = buildingId }, false, false);
                    }
                }

                return InstanceHelper.DescribeInstance(m_target, false, false);
            }

            return InstanceHelper.DescribeInstance(m_target, false, false);
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