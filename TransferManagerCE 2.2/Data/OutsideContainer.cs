using ColossalFramework.UI;
using SleepyCommon;
using System;
using TransferManagerCE.Settings;
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
            if (second == null)
            {
                return 1;
            }
            OutsideContainer oSecond = (OutsideContainer)second;
            return m_eType.CompareTo(oSecond.m_eType);
        }

        public string GetName()
        {
            return CitiesUtils.GetBuildingName(m_buildingId);
        }

        public void Show()
        {
            CitiesUtils.ShowBuilding(m_buildingId, true); 
            
            BuildingPanel.Init(); 
            if (BuildingPanel.Instance != null)
            {    
                BuildingPanel.Instance.ShowPanel(m_buildingId);
            }
        }
    }
}