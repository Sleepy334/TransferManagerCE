using ColossalFramework.UI;
using SleepyCommon;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class OutsideConnectionPanel : UIMainPanel<OutsideConnectionPanel>
    {
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;

        private UITitleBar? m_title = null;
        private ListView? m_listConnections = null;

        public OutsideConnectionPanel() : base()
        {
        }

        public override void Start()
        {
            base.Start();
            name = "OutsideConnectionPanel";
            width = 780;
            height = 540;
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "SubcategoriesPanel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventVisibilityChanged += OnVisibilityChanged;
            CenterTo(parent);

            // Title Bar
            m_title = UITitleBar.Create(this, Localization.Get("titleOutsideConnectionPanel"), "Transfer", TransferManagerMod.Instance.LoadResources(), OnCloseClick);
            if (m_title != null)
            {
                m_title.SetupButtons();
            }
            
            // Issue list
            m_listConnections = ListView.Create<UIOutsideRow>(this, "ScrollbarTrack", 0.8f, width - 20f, height - m_title.height - 10);
            if (m_listConnections is not null)
            {
                m_listConnections.padding = new RectOffset(iMARGIN, iMARGIN, 4, iMARGIN);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_NAME, Localization.Get("listConnectionName"), "Name", UIOutsideRow.ColumnWidths[0], iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_TYPE, Localization.Get("listConnectionType"), "Type", UIOutsideRow.ColumnWidths[1], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listConnectionPriority"), "Match Priority", UIOutsideRow.ColumnWidths[2], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_USAGE, Localization.Get("listConnectionUsage"), "% of busiest connection", UIOutsideRow.ColumnWidths[3], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_OWN, Localization.Get("listConnectionOwn"), "Own Vehicles", UIOutsideRow.ColumnWidths[4], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_GUEST, Localization.Get("listConnectionGuest"), "Guest Vehicles", UIOutsideRow.ColumnWidths[5], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_STUCK, Localization.Get("listConnectionStuck"), "Stuck Vehicles", UIOutsideRow.ColumnWidths[6], iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.Header.ResizeLastColumn();
            }

            isVisible = true;
            UpdatePanel();
        }

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                UpdatePanel();
            }
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        public static List<OutsideContainer> GetOutsideConnections()
        {
            List<OutsideContainer> result = new List<OutsideContainer>();

            int maxCount = 0;

            FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
            foreach (ushort connection in connections)
            {
                int iOwnCount = BuildingUtils.GetOwnParentVehiclesForBuilding(connection, out int iOwnStuck).Count;
                int iGuestCount = BuildingUtils.GetGuestParentVehiclesForBuilding(connection, out int iGuestStuck).Count;

                maxCount = Mathf.Max(maxCount, iOwnCount + iGuestCount);

                result.Add(new OutsideContainer(connection, iOwnCount, iGuestCount, iOwnStuck + iGuestStuck, 0));
            }

            // Update max count now we know it
            foreach (OutsideContainer oc in result)
            {
                oc.m_maxConnectionCount = maxCount;
            }

            return result;
        }

        protected override void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            if (m_listConnections is not null)
            {
                List<OutsideContainer> connections = GetOutsideConnections();
                connections.Sort();

                m_listConnections.GetList().rowsData = new FastList<object>
                {
                    m_buffer = connections.ToArray(),
                    m_size = connections.Count,
                };
            }
        }

        public override void OnDestroy()
        {
            if (m_listConnections is not null)
            {
                Destroy(m_listConnections.gameObject);
                m_listConnections = null;
            }
        }
    }
}