using ColossalFramework.UI;
using System.Collections.Generic;
using TransferManagerCE;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class ListViewHeader : UIPanel
    {
        protected List<ListViewHeaderColumnBase> m_columns;
        protected ListViewRowComparer.Columns m_eSortColumn = ListViewRowComparer.Columns.COLUMN_PRIORITY;
        public bool m_bSortDesc = false;
        private ListViewColumnClickEvent? m_eventOnListViewColumnClick = null;

        public delegate void ListViewColumnClickEvent(ListViewRowComparer.Columns eColumn, bool bSortDescending);

        public ListViewHeader()
        {
            m_columns = new List<ListViewHeaderColumnBase>();
        }

        public static ListViewHeader? Create(UIComponent parent, float iWidth, ListViewColumnClickEvent? m_eventOnListViewColumnClick)
        {
            ListViewHeader header = parent.AddUIComponent<ListViewHeader>();
            if (header is not null)
            {
                header.Setup(iWidth, m_eventOnListViewColumnClick);
            }
            return header;
        }

        public virtual void Setup(float fWidth, ListViewColumnClickEvent? eventOnListViewColumnClick)
        {
            m_eventOnListViewColumnClick = eventOnListViewColumnClick;
            width = fWidth;
            height = BuildingPanel.iHEADER_HEIGHT;
            backgroundSprite = "ListItemHighlight";
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopLeft;
            autoLayoutPadding = new RectOffset(2, 2, 2, 2);
            autoLayout = true;
        }

        public void AddColumn(ListViewRowComparer.Columns eColumn, string sText, string sTooltip, float fTextScale, int iWidth, int iHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, ListViewHeaderColumnBase.OnListViewColumnClick eventClickCallback)
        {
            if (m_columns is not null)
            {
                m_columns.Add(new ListViewHeaderColumnLabel(eColumn, this, sText, sTooltip, fTextScale, iWidth, iHeight, oTextAlignment, oAncor, eventClickCallback));
            }
        }

        public void OnListViewColumnClick(ListViewHeaderColumnBase oColumn)
        {
            if (oColumn is not null)
            {
                ListViewRowComparer.Columns eColumn = oColumn.GetColumn();
                HandleSort(eColumn);
            }
        }

        public void HandleSort(ListViewRowComparer.Columns eColumn)
        {
            if (m_eSortColumn == eColumn)
            {
                m_bSortDesc = !m_bSortDesc;
            }
            else
            {
                m_eSortColumn = eColumn;
            }

            // Update columns
            foreach (ListViewHeaderColumnBase column in m_columns)
            {
                column.Sort(m_eSortColumn, m_bSortDesc);
            }

            // Notify parent
            if (m_eventOnListViewColumnClick is not null)
            {
                m_eventOnListViewColumnClick(m_eSortColumn, m_bSortDesc);
            }
        }

        public ListViewRowComparer.Columns GetSortColumn()
        {
            return m_eSortColumn;
        }

        public bool GetSortDirection()
        {
            return m_bSortDesc;
        }

        public void Destroy()
        {
            foreach (ListViewHeaderColumnBase column in m_columns)
            {
                column.Destroy();
            }
        }
    }
}
