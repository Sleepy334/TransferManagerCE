using SleepyCommon;
using System;

namespace TransferManagerCE
{
    public class OutsideContainer : IComparable
    {
        public ushort m_buildingId;
        public BuildingTypeHelper.OutsideType m_eType;

        public OutsideContainer(ushort buildingId)
        {
            m_buildingId = buildingId;
            m_eType = BuildingTypeHelper.GetOutsideConnectionType(m_buildingId);
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
#if DEBUG
            return $"{CitiesUtils.GetBuildingName(m_buildingId, caller)} ({OutsideConnectionCache.FindCachedOutsideNode(m_buildingId)})";
#else

            return CitiesUtils.GetBuildingName(m_buildingId, caller);
#endif
        }

        public void Show()
        {
            BuildingUtils.ShowInstanceSetBuildingPanel(new InstanceID { Building = m_buildingId });
        }
    }
}