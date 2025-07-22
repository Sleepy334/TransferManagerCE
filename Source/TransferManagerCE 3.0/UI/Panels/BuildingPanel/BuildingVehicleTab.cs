using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Data;
using TransferManagerCE.Util;
using UnifiedUI.Helpers;
using static TransferManagerCE.UI.BuildingPanel;

namespace TransferManagerCE.UI
{
    public class BuildingVehicleTab : BuildingTab 
    {
        private ListView? m_listVehicles = null;

        public override void SetupInternal()
        {
            UIPanel? tabVehicles = m_tabStrip.AddTabIcon("InfoIconTrafficCongestion", Localization.Get("tabBuildingPanelVehicles"), "", 230);
            if (tabVehicles is not null)
            {
                tabVehicles.autoLayout = true;
                tabVehicles.autoLayoutDirection = LayoutDirection.Vertical;

                // Vehicles list
                m_listVehicles = ListView.Create<UIVehicleRow>(tabVehicles, "ScrollbarTrack", 0.8f, tabVehicles.width, tabVehicles.height - 10);
                if (m_listVehicles is not null)
                {
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", UIVehicleRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("txtLoad"), "Vehicle Load", UIVehicleRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_TIMER, Localization.Get("listBuildingPanelStatusColumn5"), "W = Waiting\r\nB = Blocked", UIVehicleRow.ColumnWidths[2], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Distance (km)", UIVehicleRow.ColumnWidths[3], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", UIVehicleRow.ColumnWidths[4], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.AddColumn(ListViewRowComparer.Columns.COLUMN_VEHICLE_TARGET, Localization.Get("listBuildingPanelVehicleTarget"), "Target", UIVehicleRow.ColumnWidths[5], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listVehicles.Header.ResizeLastColumn();
                    m_listVehicles.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }
            }
        }

        public override bool ShowTab()
        {
            if (m_buildingId == 0)
            {
                return false;
            }

            if (BuildingVehicleCount.GetVehicleTypeCount(m_eBuildingType, m_buildingId) > 0)
            {
                return true;
            }

            // Check the actual vehicle array as well
            Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
            return building.m_ownVehicles != 0;
        }

        public void UpdateTabWidth(UITabStrip tabStrip, ushort buildingId)
        {
            // Vehicle tab
            int iVehicleTypes = BuildingVehicleCount.GetVehicleTypeCount(m_eBuildingType, buildingId);
            if (iVehicleTypes > 0)
            {
                tabStrip.SetTabVisible((int)TabIndex.TAB_VEHICLES, true);

                // Set tab button width based on vehicle types
                if (iVehicleTypes > 1)
                {
                    tabStrip.SetTabWidth((int)TabIndex.TAB_VEHICLES, 230);
                }
                else
                {
                    tabStrip.SetTabWidth((int)TabIndex.TAB_VEHICLES, 175f);
                }
            }

            tabStrip.PerformLayout();
        }

        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(bActive))
            {
                return false;
            }

            // Update vehicle tab count
            if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_VEHICLES))
            {
                UpdateTabWidth(m_tabStrip, m_buildingId);

                string strTab = Localization.Get("tabBuildingPanelVehicles");

                Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_buildingId];
                if (building.m_flags != 0)
                {
                    strTab = BuildingVehicleCount.GetVehicleTabText(m_eBuildingType, m_buildingId, building);
                }

                m_tabStrip.SetTabText((int)TabIndex.TAB_VEHICLES, strTab);
            }

            if (bActive)
            {
                // Update vehicle list
                if (m_listVehicles is not null)
                {
                    List<VehicleData> vehicles = GetVehicles();

                    m_listVehicles.GetList().rowsData = new FastList<object>
                    {
                        m_buffer = vehicles.ToArray(),
                        m_size = vehicles.Count,
                    };
                }
            }
            else
            {
                Clear();
            }

            return true;
        }

        public List<VehicleData> GetVehicles()
        {
            return new BuildingOwnVehicles().GetVehicles(m_buildingId);
        }

        public override void Clear()
        {
            if (m_listVehicles is not null)
            {
                m_listVehicles.Clear();
            }

            base.Clear();
        }

        public override void Destroy()
        {
            if (m_listVehicles is not null)
            {
                m_listVehicles.Destroy();
                m_listVehicles = null;
            }

            base.Destroy();
        }
    }
}