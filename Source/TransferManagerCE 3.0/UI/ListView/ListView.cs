﻿using ColossalFramework.UI;
using SleepyCommon;
using System;

namespace TransferManagerCE.UI
{
    public class ListView : UIPanel
    {
        public const int iSCROLL_BAR_WIDTH = 16;//20;
        public const int iROW_HEIGHT = 22;

        private ListViewHeader? m_header = null;
        private UIFastList? m_listPanel;
        private float m_fTextScale = 1.0f;

        // ----------------------------------------------------------------------------------------
        public ListViewHeader? Header
        {
            get
            {
                return m_header;
            }
        }

        public ListView() : base()
        {
            m_listPanel = null;
        }

        public static ListView? Create<T>(UIComponent oParent, string txtBackgroundSprite, float fTextScale, float fWidth, float fHeight)
            where T : UIPanel, IUIFastListRow
        {
            try
            {
                ListView listView = oParent.AddUIComponent<ListView>();
                if (listView is not null)
                {
                    listView.autoLayoutDirection = LayoutDirection.Vertical;
                    listView.autoLayoutStart = LayoutStart.TopLeft;
                    listView.autoLayout = true;
                    listView.m_fTextScale = fTextScale;
                    listView.width = fWidth;
                    listView.height = fHeight;

                    listView.m_header = ListViewHeader.Create(listView, fWidth, null);

                    listView.m_listPanel = UIFastList.Create<T>(listView);
                    listView.m_listPanel.width = fWidth;
                    listView.m_listPanel.height = fHeight - listView.m_header.height;
                    listView.m_listPanel.rowHeight = iROW_HEIGHT;
                }
                return listView;
            }
            catch (Exception ex)
            {
                CDebug.Log(ex);
            }

            return null;
        }

        public int Count
        {
            get
            {
                if (m_listPanel is not null)
                {
                    return m_listPanel.rowsData.m_size;
                }
                else
                {
                    return 0;
                }
            }
        }

        public UIFastList? GetList()
        {
            return m_listPanel;
        }

        public void SetItems(FastList<object> data)
        {
            if (m_listPanel is not null)
            {
                m_listPanel.rowsData = data;
            }
        }

        public void AddColumn(ListViewRowComparer.Columns eColumn, string sText, string sTooltip, float fWidth, float fHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, ListViewHeaderColumnBase.OnListViewColumnClick eventCallback)
        {
            if (m_header is not null)
            {
                m_header.AddColumn(eColumn, sText, sTooltip, m_fTextScale, fWidth, fHeight, oTextAlignment, oAncor, eventCallback);
            }
        }

        public void HandleSort(ListViewRowComparer.Columns eColumn)
        {
            if (m_header is not null)
            {
                m_header.HandleSort(eColumn);
            }
        }

        public void Clear()
        {
            if (m_listPanel is not null)
            {
                m_listPanel.Clear();
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_header is not null)
            {
                m_header.width = width;

                if (m_listPanel is not null)
                {
                    m_listPanel.width = width;
                    m_listPanel.height = height - m_header.height;
                }
            }
        }

        public override void OnDestroy()
        {
            if (m_header is not null)
            {
                Destroy(m_header.gameObject);
                m_listPanel = null;
            }
            if (m_listPanel is not null)
            {
                Destroy(m_listPanel.gameObject);
                m_listPanel = null;
            }
            base.OnDestroy();
        }

        public void Refresh()
        {
            if (m_listPanel is not null)
            {
                m_listPanel.Refresh();
            }
        }
    }
}
