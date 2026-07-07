using ColossalFramework;
using SleepyCommon;
using System;
using UnityEngine;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public abstract class StatusNodeStop : StatusData
    {
        public ushort m_nodeId;

        public StatusNodeStop(BuildingType eBuildingType, ushort m_buildingId, ushort nodeId) :
            base(CustomTransferReason.Reason.None, eBuildingType, m_buildingId)
        {
            m_nodeId = nodeId;
        }

        public override int CompareTo(object second)
        {
            if (second is StatusNodeStop oSecond)
            {
                // Wait timer (Descending)
                if (oSecond.GetWaitTimer() != GetWaitTimer())
                {
                    return oSecond.GetWaitTimer().CompareTo(GetWaitTimer());
                }

                // Finally sort by node so they dont skip around
                if (oSecond.m_nodeId != m_nodeId)
                {
                    return m_nodeId.CompareTo(oSecond.m_nodeId);
                }
            }

            return base.CompareTo(second);
        }

        public override bool IsBuildingData()
        {
            return true;
        }
        public override bool IsVehicleData()
        {
            return false;
        }

        public override string GetMaterialDisplay()
        {
            return GetTransportType().ToString();
        }

        protected abstract TransportInfo.TransportType GetTransportType();

        protected override string CalculateValue(out string tooltip)
        {
            int iCount = CitiesUtils.CalculatePassengerCount(m_nodeId, GetTransportType());
            tooltip = $"Waiting Passengers: {iCount}";
            return iCount.ToString();
        }

        protected override string CalculateTimer(out string tooltip)
        {
            int iWaitTimer = GetWaitTimer();

            tooltip = $"Wait Timer: {iWaitTimer}";

            if (iWaitTimer > 0)
            {
                return $"W:{iWaitTimer}";
            }
            else
            {
                return "";
            }
        }

        protected override double CalculateDistance()
        {
            return double.MaxValue;
        }

        protected override string CalculateDescription1(out string tooltip) 
        { 
            tooltip = "";
            return "";
        }

        protected override string CalculateDescription2(out string tooltip)
        {
            tooltip = InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_nodeId }, true, false);
            return InstanceHelper.DescribeInstance(new InstanceID { NetNode = m_nodeId }, false, false);
        }

        public override void OnClickDescription2()
        {
            if (m_nodeId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { NetNode = m_nodeId });
            }
        }

        protected int GetWaitTimer()
        {
            NetNode node = NetManager.instance.m_nodes.m_buffer[m_nodeId];
            if (node.m_flags != 0)
            {
                return node.m_maxWaitTime;
            }
            return 0;
        }
    }
}