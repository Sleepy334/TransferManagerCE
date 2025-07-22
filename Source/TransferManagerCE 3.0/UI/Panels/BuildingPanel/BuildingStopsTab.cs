using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;
using UnifiedUI.Helpers;
using static TransferManagerCE.UI.BuildingPanel;

namespace TransferManagerCE.UI
{
    public class BuildingStopsTab : BuildingTab
    {
        public  StopHelper m_stopHelper = new StopHelper();
        private ListView? m_listStatus = null;

        // ----------------------------------------------------------------------------------------
        public override void SetupInternal()
        {
            UIPanel? tabStatus = m_tabStrip.AddTabIcon("InfoIconPublicTransport", Localization.Get("tabBuildingPanelStops"), "", 150f);
            if (tabStatus is not null)
            {
                tabStatus.autoLayout = true;
                tabStatus.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listStatus = ListView.Create<UIStatusRow>(tabStatus, "ScrollbarTrack", 0.8f, tabStatus.width, tabStatus.height - 10);
                if (m_listStatus is not null)
                {
                    string sTimerTooltip = "S = Sick\r\nD = Dead\r\nI = Incoming\r\nO = Outgoing\r\nW = Waiting\r\nB = Blocked";

                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", UIStatusRow.ColumnWidths[0], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", UIStatusRow.ColumnWidths[1], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), sTimerTooltip, UIStatusRow.ColumnWidths[2], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Distance (km)", UIStatusRow.ColumnWidths[3], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", UIStatusRow.ColumnWidths[4], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", UIStatusRow.ColumnWidths[5], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.Header.ResizeLastColumn();
                }
            }
        }

        public override bool ShowTab()
        {
            if (m_buildingId != 0)
            {
                switch (m_eBuildingType)
                {
                    case BuildingTypeHelper.BuildingType.DisasterShelter:
                    case BuildingTypeHelper.BuildingType.CableCarStation:
                    case BuildingTypeHelper.BuildingType.TransportStation:
                        {
                            return true;
                        }
                }
            }

            return false;
        }

        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(bActive))
            {
                return false;
            }

            // Update status tab count
            if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_STOPS))
            {
                int iStatusCount;
                List<StatusData>? statusList = m_stopHelper.GetStatusList(m_buildingId, out iStatusCount);

                string sMessage = Localization.Get("tabBuildingPanelStops");

                if (m_buildingId != 0)
                {
                    if (iStatusCount > 0)
                    {
                        sMessage += " (" + iStatusCount + ")";
                    }
                }

                m_tabStrip.SetTabText((int)TabIndex.TAB_STOPS, sMessage);

                if (bActive)
                {
                    // Update entries
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
            else
            {
                Clear();
            }

            return true;
        }

        public override void Clear()
        {
            if (m_listStatus is not null)
            {
                m_listStatus.Clear();
            }
            base.Clear();
        }

        public override void Destroy()
        {
            if (m_listStatus is not null)
            {
                m_listStatus.Destroy();
                m_listStatus = null;
            }
            base.Destroy();
        }
    }
}