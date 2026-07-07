using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using System.Reflection;
using TransferManagerCore.Data;
using TransferManagerCore.Util;
using UnifiedUI.Helpers;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using static TransferManagerCore.UI.BuildingPanel;

namespace TransferManagerCore.UI
{
    public class BuildingStopsTab : BuildingTab
    {
        public  StopHelper m_stopHelper = new StopHelper();
        private ListView? m_listStatus = null;
        private UIButton? m_btnResetIntercityStops = null;
        private UIButton? m_btnClearIntercityStops = null;
        
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
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStopsSourceVehicle"), "Source / Vehicle", UIStatusRow.ColumnWidths[4], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelVehicleTarget"), "Target", UIStatusRow.ColumnWidths[5], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.Header.ResizeLastColumn();
                }

                UIPanel pnlButtons = tabStatus.AddUIComponent<UIPanel>();
                pnlButtons.width = tabStatus.width;
                pnlButtons.height = 30;
                pnlButtons.autoLayout = true;
                pnlButtons.autoLayoutDirection = LayoutDirection.Horizontal;
                pnlButtons.autoLayoutPadding = new RectOffset(6, 0, 6, 0);

                // Reset pathing button
                m_btnResetIntercityStops = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, pnlButtons, Localization.Get("btnResetIntercityStops"), "", 200, 30, OnReset);

                m_btnClearIntercityStops = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, pnlButtons, "Niet", m_btnResetIntercityStops.height, m_btnResetIntercityStops.height, OnClear);
                if (m_btnClearIntercityStops is not null)
                {
                    m_btnClearIntercityStops.tooltip = Localization.Get("btnClearIntercityStops");
                }

                // Adjust list height
                m_listStatus.height = tabStatus.height - pnlButtons.height - 12;
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

        private void OnReset(UIComponent component, UIMouseEventParameter eventParam)
        {
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.Info.m_buildingAI is TransportStationAI station)
                {
                    TransportStationAIReversePatches.ResetIntercityLines(station, m_buildingId, ref building);
                }
            });
        }

        private void OnClear(UIComponent component, UIMouseEventParameter eventParam)
        {
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                ref Building building = ref BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                if (building.Info.m_buildingAI is TransportStationAI station)
                {
                    TransportStationAIReversePatches.ClearIntercityLines(station, m_buildingId, ref building);
                }
            });
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