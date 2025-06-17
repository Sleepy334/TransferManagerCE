using SleepyCommon;
using System;
using TransferManagerCE.UI;

namespace TransferManagerCE
{
    public class OutsideContainer : IComparable
    {
        public ushort m_buildingId;
        public BuildingTypeHelper.OutsideType m_eType;
        public int m_ownCount = 0;
        public int m_guestCount = 0;
        public int m_stuckCount = 0;
        public int m_maxConnectionCount = 0;

        public OutsideContainer(ushort buildingId, int ownCount, int guestCount, int stuckCount, int maxConnectionCount)
        {
            m_buildingId = buildingId;
            m_eType = BuildingTypeHelper.GetOutsideConnectionType(m_buildingId);
            m_ownCount = ownCount;
            m_guestCount = guestCount;
            m_stuckCount = stuckCount;
            m_maxConnectionCount = maxConnectionCount;
        }

        public int CompareTo(object second)
        {
            if (second is null)
            {
                return 1;
            }
            OutsideContainer oSecond = (OutsideContainer)second;
            return m_eType.CompareTo(oSecond.m_eType);
        }

        public string GetName()
        {
            InstanceID caller = new InstanceID { Building = m_buildingId };
            return CitiesUtils.GetBuildingName(m_buildingId, caller);
        }

        public string GetUsage()
        {
            return Utils.MakePercent(GetTotal(), m_maxConnectionCount);
        }

        public void Show()
        {
            if (OutsideConnectionSelectionPanel.IsVisible())
            {
                InstanceHelper.ShowInstance(new InstanceID { Building = m_buildingId });
            }
            else
            {
                BuildingUtils.ShowInstanceSetBuildingPanel(new InstanceID { Building = m_buildingId });
            }   
        }

        public int GetTotal()
        {
            return m_ownCount + m_guestCount;
        }
    }
}