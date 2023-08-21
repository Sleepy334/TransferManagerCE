using ColossalFramework.UI;
using SleepyCommon;
using System;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using static TransferManager;

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
#if DEBUG
            return $"{CitiesUtils.GetBuildingName(m_buildingId)} ({PathNodeCache.FindCachedOutsideNode(m_buildingId)})";
#else
            return CitiesUtils.GetBuildingName(m_buildingId);
#endif

        }

        public void Show()
        {
            InstanceHelper.ShowInstance(new InstanceID { Building = m_buildingId });
            
            BuildingPanel.Init(); 
            if (BuildingPanel.Instance is not null)
            {    
                BuildingPanel.Instance.ShowPanel(m_buildingId);
            }
        }
    }
}