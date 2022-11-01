using ColossalFramework.UI;
using SleepyCommon;
using System;
using TransferManagerCE.Settings;

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
            OutsideConnectionSettings settings = OutsideConnectionSettings.GetSettings(m_buildingId);
            return settings.GetName(m_buildingId);
        }
        
        public string GetImportSupported()
        {
            // Note Outgoing here is actually importing from city
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if ((building.m_flags & Building.Flags.Outgoing) != 0) 
            {
                return BuildingSettings.IsImportDisabled(m_buildingId) ? "No" : "Yes";
            }
            else
            {
                return "-";
            } 
        }

        public string GetExportSupported()
        {
            // Note Incoming here is actually exporting from city
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            if ((building.m_flags & Building.Flags.Incoming) != 0)
            {
                return BuildingSettings.IsExportDisabled(m_buildingId) ? "No" : "Yes";
            }
            else
            {
                return "-";
            }
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