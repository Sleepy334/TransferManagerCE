using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using TransferManagerCE.Util;
using UnifiedUI.Helpers;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.UI.BuildingPanel;

namespace TransferManagerCE.UI
{
    public class BuildingPathingTab : BuildingTab
    {
        // Pathing tab
        private ListView? m_listPathing = null;
        private UIButton? m_btnReset = null;
        private bool m_bHideTab = true;


        // ----------------------------------------------------------------------------------------
        public override void SetupInternal()
        {
            UIPanel? tabPathing = m_tabStrip.AddTabIcon("ToolbarIconRoads", Localization.Get("tabTransferIssuesPathing"), "", 140f);
            if (tabPathing is not null)
            {
                tabPathing.autoLayout = true;
                tabPathing.autoLayoutDirection = LayoutDirection.Vertical;

                const int iButtonHeight = 30;
                // Issue list
                m_listPathing = ListView.Create<UIPathRow>(tabPathing, "ScrollbarTrack", 0.8f, tabPathing.width, tabPathing.height - iButtonHeight - 10);
                if (m_listPathing is not null)
                {
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listPathingColumn1"), "", TransferIssuePanel.iCOLUMN_WIDTH_TIME, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("columnLocation"), "", TransferIssuePanel.iCOLUMN_WIDTH_LOCATION, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listPathingColumn2"), "Source location for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_SOURCE_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listPathingColumn3"), "Target destination for path", BuildingPanel.iCOLUMN_WIDTH_PATHING_BUILDING, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listPathing.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET_FAIL_COUNT, "#", "Path fail count", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                }

                m_btnReset = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, tabPathing, Localization.Get("btnResetPathingStatistics"), "", 200, iButtonHeight, OnReset);
                if (m_btnReset is not null)
                {
                    m_btnReset.tooltip = "Reset pathing for this building.";
                }
            }
        }

        public override void SetTabBuilding(ushort buildingId, BuildingType buildingType, List<ushort> subBuildingIds)
        {
            m_bHideTab = true;
            base.SetTabBuilding(buildingId, buildingType, subBuildingIds);
        }

        public override bool ShowTab()
        {
            if (m_buildingId == 0)
            {
                return false;
            }

            // Show pathing tab if there are any pathing uissues, do not hide pathing tab once it is shown for this building.
            bool bShowTab = false;
            if (GetPathingIssues().Count > 0)
            {
                bShowTab = true;
            }
            else if (m_buildingId == 0 || m_bHideTab)
            {
                bShowTab = false;
            }
            if (bShowTab)
            {
                if (!m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, true);
                }
            }
            else
            {
                m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, false);
            }

            // Don't hide tab once shown for this building
            m_bHideTab = false;

            return bShowTab;
        }

        public List<PathingContainer> GetPathingIssues()
        {
            List<PathingContainer> list = new List<PathingContainer>();

            if (m_buildingId != 0) 
            {
                HashSet<InstanceID> ids = new HashSet<InstanceID>();

                // Add main building
                ids.Add(new InstanceID { Building = m_buildingId });

                // Add sub buildings
                foreach (ushort subBuildingId in BuildingPanel.Instance.GetSubBuildingIds())
                {
                    ids.Add(new InstanceID { Building = subBuildingId });
                }

                // Add park if service point
                if (BuildingTypeHelper.GetBuildingType(m_buildingId) == BuildingTypeHelper.BuildingType.ServicePoint)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                    if ((building.Info.GetAI() as ServicePointAI).GetPark(m_buildingId, ref building, out byte park))
                    {
                        ids.Add(new InstanceID { Park = park });
                    }
                }

                // Local path failes
                Dictionary<Util.PATHFINDPAIR, long> failures = Util.PathFindFailure.GetPathFailsCopy();
                foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in failures)
                {
                    if (ids.Contains(kvp.Key.m_source))
                    {
                        list.Add(new PathingContainer(PathingContainer.LocationType.Local, kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Source, false));
                    }
                    else if (ids.Contains(kvp.Key.m_target))
                    {
                        list.Add(new PathingContainer(PathingContainer.LocationType.Local, kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Target, false));
                    }
                }

                // Outside path fails
                Dictionary<Util.PATHFINDPAIR, long> outside = Util.PathFindFailure.GetOutsideFailsCopy();
                foreach (KeyValuePair<Util.PATHFINDPAIR, long> kvp in outside)
                {
                    if (ids.Contains(kvp.Key.m_source))
                    {
                        list.Add(new PathingContainer(PathingContainer.LocationType.Outside, kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Source, false));
                    }
                    else if (ids.Contains(kvp.Key.m_target))
                    {
                        list.Add(new PathingContainer(PathingContainer.LocationType.Outside, kvp.Value, kvp.Key.m_source, kvp.Key.m_target, PathingContainer.SourceOrTarget.Target, false));
                    }
                }
            }

            return list;
        }

        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(bActive))
            {
                return false;
            }

            List<PathingContainer> listPathing = GetPathingIssues();

            // Show pathing tab if there are any pathing uissues, do not hide pathing tab once it is shown for this building.
            bool bShowTab = false;
            if (listPathing.Count > 0)
            {
                bShowTab = true;
            }
            else if (m_buildingId == 0 || m_bHideTab)
            {
                bShowTab = false;
            }
            if (bShowTab)
            {
                if (!m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                {
                    m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, true);
                }
            }
            else
            {
                m_tabStrip.SetTabVisible((int)TabIndex.TAB_PATHING, false);
            }

            // Don't hide tab once shown for this building
            m_bHideTab = false;

            // Update pathing tab count
            if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
            {
                m_tabStrip.SetTabText((int)TabIndex.TAB_PATHING, Localization.Get("tabTransferIssuesPathing") + "(" + listPathing.Count + ")");
            }

            if (bActive)
            {
                // Update entries
                if (m_listPathing is not null && m_tabStrip.IsTabVisible((int)TabIndex.TAB_PATHING))
                {
                    listPathing.Sort(PathingContainer.CompareToTime);
                    m_listPathing.GetList().rowsData = new FastList<object>
                    {
                        m_buffer = listPathing.ToArray(),
                        m_size = listPathing.Count,
                    };
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
            PathFindFailure.ResetPathingStatistics(m_buildingId);

            List<ushort> subBuildings = BuildingPanel.Instance.GetSubBuildingIds();
            if (subBuildings.Count > 0)
            {
                foreach (ushort subBuildingId in subBuildings)
                {
                    PathFindFailure.ResetPathingStatistics(subBuildingId);
                }
            }

            BuildingPanel.Instance.InvalidatePanel();
        }

        public override void Clear()
        {
            if (m_listPathing is not null)
            {
                m_listPathing.Clear();
            }

            base.Clear();
        }

        public override void Destroy()
        {
            if (m_listPathing is not null)
            {
                m_listPathing.Destroy();
                m_listPathing = null;
            }

            base.Destroy();
        }
    }
}