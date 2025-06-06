using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;
using UnifiedUI.Helpers;
using static TransferManagerCE.UI.BuildingPanel;

namespace TransferManagerCE.UI
{
    public class BuildingStatusTab : BuildingTab
    {
        public  StatusHelper m_statusHelper = new StatusHelper();
        private ListView? m_listStatus = null;

        // ----------------------------------------------------------------------------------------
        public override void SetupInternal()
        {
            UIPanel? tabStatus = m_tabStrip.AddTabIcon("Information", Localization.Get("tabBuildingPanelStatus"), TransferManagerMod.Instance.LoadResources(), "", 150f);
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

        public override bool ShowTab()
        {
            return true; // Always show tab
        }

        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(bActive))
            {
                return false;
            }

            // Update status tab count
            if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_STATUS))
            {
                int iStatusCount;
                List<StatusData>? statusList = m_statusHelper.GetStatusList(m_buildingId, out iStatusCount);

                string sMessage = Localization.Get("tabBuildingPanelStatus");

                if (m_buildingId != 0)
                {
                    if (iStatusCount > 0)
                    {
                        sMessage += " (" + iStatusCount + ")";
                    }
                }

                m_tabStrip.SetTabText((int)TabIndex.TAB_STATUS, sMessage);

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