using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;
using TransferManagerCE.Data;
using UnifiedUI.Helpers;
using static TransferManagerCE.UI.BuildingPanel;

namespace TransferManagerCE.UI
{
    public class BuildingStatusTab
    {
        public  StatusHelper m_statusHelper = new StatusHelper();
        private ListView? m_listStatus = null;
        private ushort m_buildingId = 0;

        public void Setup(UITabStrip tabStrip)
        {
            UIPanel? tabStatus = tabStrip.AddTabIcon("Information", Localization.Get("tabBuildingPanelStatus"), TransferManagerLoader.LoadResources(), "", 150f);
            if (tabStatus is not null)
            {
                tabStatus.autoLayout = true;
                tabStatus.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listStatus = ListView.Create<UIStatusRow>(tabStatus, "ScrollbarTrack", 0.8f, tabStatus.width, tabStatus.height - 10);
                if (m_listStatus is not null)
                {
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "S = Sick\r\nD = Dead\r\nI = Incoming\r\nW = Waiting\r\nB = Blocked", iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Distance (km)", iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_WIDTH_XLARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", iCOLUMN_WIDTH_XLARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                }
            }
        }

        public void SetTabBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
        }

        public void UpdateTab(UITabStrip tabStrip)
        {
            // Update status tab count
            if (tabStrip.IsTabVisible((int)TabIndex.TAB_STATUS))
            {
                string sMessage = Localization.Get("tabBuildingPanelStatus");

                if (m_buildingId != 0)
                {
                    int iVehicleCount = 0;

                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingId];
                    if (building.m_flags != 0)
                    {
                        // Add vehicle count if there are any guest vehicles
                        List<ushort> vehicles = BuildingUtils.GetGuestParentVehiclesForBuilding(building);
                        iVehicleCount = vehicles.Count;
                    }

                    if (iVehicleCount > 0)
                    {
                        sMessage += " (" + iVehicleCount + ")";
                    }
                }

                tabStrip.SetTabText((int)TabIndex.TAB_STATUS, sMessage);
            }

            bool bActive = (TabIndex)tabStrip.GetSelectTabIndex() == TabIndex.TAB_STATUS;
            if (bActive)
            {
                // Update entries
                int iStatusCount;
                List<StatusData>? statusList = m_statusHelper.GetStatusList(m_buildingId, out iStatusCount);
                if (m_listStatus is not null && statusList is not null)
                {
                    // Services
                    m_listStatus.GetList().rowsData = new FastList<object>
                    {
                        m_buffer = statusList.ToArray(),
                        m_size = statusList.Count,
                    };
                }
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            if (m_listStatus is not null)
            {
                m_listStatus.Clear();
            }
        }

        public void Destroy()
        {
            if (m_listStatus is not null)
            {
                m_listStatus.Destroy();
                m_listStatus = null;
            }
        }
    }
}