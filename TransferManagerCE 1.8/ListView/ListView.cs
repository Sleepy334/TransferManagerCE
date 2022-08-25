using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TransferManagerCE;
using UnityEngine;

namespace SleepyCommon
{
    public class ListView : UIPanel
    {
        public const int iSCROLL_BAR_WIDTH = 20;

        private ListViewHeader? m_header = null;
        public UIPanel? m_HorizontalPanel = null;
        public ListViewMainPanel? m_listPanel;
        public UIPanel? m_scrollbarPanel;
        public UIScrollbar? m_scrollbar;
        private string m_backgroundSprite;
        private List<ListViewRow> m_rows = null;
        private float m_fTextScale = 1.0f;

        public ListView() : base() {
            m_scrollbar = null;
            m_listPanel = null;
            m_scrollbarPanel = null;
            m_rows = new List<ListViewRow>();
        }

        public static ListView? Create(UIComponent oParent, string txtBackgroundSprite, float fTextScale, float fWidth, float fHeight)
        {
            try
            {
                ListView listView = oParent.AddUIComponent<ListView>();
                if (listView != null)
                {
                    //listView.m_backgroundSprite = txtBackgroundSprite;
                    //listView.backgroundSprite = txtBackgroundSprite;
                    listView.backgroundSprite = "InfoviewPanel";
                    listView.color = new Color32(81, 87, 89, 225);
                    listView.autoLayoutDirection = LayoutDirection.Vertical;
                    listView.autoLayoutStart = LayoutStart.TopLeft;
                    listView.autoLayout = true;
                    listView.m_fTextScale = fTextScale;
                    listView.width = fWidth;
                    listView.height = fHeight;
                    listView.Setup();
                }
                return listView;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex);
            }

            return null;
        }

        public void Setup()
        {
            m_header = ListViewHeader.Create(this, width, null);

            // Horizontal panel
            m_HorizontalPanel = AddUIComponent<UIPanel>();
            if (m_HorizontalPanel != null)
            {
                m_HorizontalPanel.backgroundSprite = m_backgroundSprite;
                m_HorizontalPanel.width = m_header.width;
                m_HorizontalPanel.height = height - m_header.height;
                m_HorizontalPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                m_HorizontalPanel.autoLayout = true;
            }

            m_listPanel = m_HorizontalPanel.AddUIComponent<ListViewMainPanel>();
            if (m_listPanel != null)
            {
                m_listPanel.Setup(m_backgroundSprite, width - iSCROLL_BAR_WIDTH, height - m_header.height);
            } else
            {
                TransferManagerCE.Debug.Log("m_listPanel is null");
                return;
            }

            m_scrollbarPanel = m_HorizontalPanel.AddUIComponent<UIPanel>();
            if (m_scrollbarPanel != null)
            {
                m_scrollbarPanel.width = iSCROLL_BAR_WIDTH;
                m_scrollbarPanel.height = m_HorizontalPanel.height;
                m_scrollbarPanel.relativePosition = new Vector3(m_HorizontalPanel.width - iSCROLL_BAR_WIDTH, 0.0f);
                m_scrollbar = SetUpScrollbar();
            }
            else
            {
                TransferManagerCE.Debug.Log("m_scrollbarPanel is null");
                return;
            }

            if (m_scrollbar != null)
            {
                m_listPanel.verticalScrollbar = m_scrollbar;
                m_listPanel.eventMouseWheel += (MouseEventHandler)((component, param) => this.m_listPanel.scrollPosition += new Vector2(0.0f, Mathf.Sign(param.wheelDelta) * -1f * m_scrollbar.incrementAmount));
            }
        }

        public UIScrollbar? SetUpScrollbar()
        {
            if (m_scrollbarPanel != null) 
            {
                m_scrollbar = m_scrollbarPanel.AddUIComponent<UIScrollbar>();
                m_scrollbar.name = "Scrollbar";
                m_scrollbar.width = iSCROLL_BAR_WIDTH;
                m_scrollbar.height = m_scrollbarPanel.height;// BuildingPanel.iLISTVIEW_OFFERS_HEIGHT - m_header.height; // Seem to have to hard code this, not sure why yet...
                m_scrollbar.orientation = UIOrientation.Vertical;
                m_scrollbar.pivot = UIPivotPoint.BottomLeft;
                m_scrollbar.relativePosition = Vector2.zero;
                m_scrollbar.minValue = 0;
                m_scrollbar.value = 0;
                m_scrollbar.incrementAmount = 50;

                UISlicedSprite tracSprite = m_scrollbar.AddUIComponent<UISlicedSprite>();
                tracSprite.relativePosition = Vector2.zero;
                tracSprite.autoSize = true;
                tracSprite.size = tracSprite.parent.size;
                tracSprite.fillDirection = UIFillDirection.Vertical;
                tracSprite.spriteName = "ScrollbarTrack";
                tracSprite.name = "Track";
                m_scrollbar.trackObject = tracSprite;
                m_scrollbar.trackObject.height = m_scrollbar.height;

                UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
                thumbSprite.relativePosition = Vector2.zero;
                thumbSprite.fillDirection = UIFillDirection.Vertical;
                thumbSprite.autoSize = true;
                thumbSprite.width = thumbSprite.parent.width - 8;
                thumbSprite.spriteName = "ScrollbarThumb";
                thumbSprite.name = "Thumb";

                m_scrollbar.thumbObject = thumbSprite;
                m_scrollbar.isVisible = true;
                m_scrollbar.isEnabled = true;

                return m_scrollbar;
            }
            else
            {
                return null;
            }
            
        }

        public int Count
        {
            get
            {
                return m_rows.Count;
            }
        }

        public void AddColumn(ListViewRowComparer.Columns eColumn, string sText, string sTooltip, int iWidth, int iHeight, UIHorizontalAlignment oTextAlignment, UIAlignAnchor oAncor, ListViewHeaderColumnBase.OnListViewColumnClick eventCallback)
        {
            if (m_header != null)
            {
                m_header.AddColumn(eColumn, sText, sTooltip, m_fTextScale, iWidth, iHeight, oTextAlignment, oAncor, eventCallback);
            }
        }

        public ListViewRow? AddItem(ListData data)
        {
            m_rows.Add(ListViewRow.Create(m_listPanel, m_backgroundSprite, m_fTextScale, data));
            return m_rows[m_rows.Count - 1];
        }

        public void SetItems(ListData[] items)
        {
            if (m_rows != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null)
                    {
                        if (i < m_rows.Count)
                        {
                            if (items[i] != null)
                            {
                                m_rows[i].SetData(items[i]);
                            }                            
                        }
                        else
                        {
                            AddItem(items[i]);
                        }
                    }
                }

                // Remove items if we have too many
                while (m_rows.Count > items.Length)
                {
                    ListViewRow row = m_rows[m_rows.Count - 1];
                    if (row != null)
                    {
                        UnityEngine.Object.Destroy(row.gameObject);
                    }
                    m_rows.RemoveAt(m_rows.Count - 1);
                }
            }
        }

        public void UpdateItems()
        {
            if (m_rows != null)
            {
                for (int i = 0; i < m_rows.Count; i++)
                {
                    if (i < m_rows.Count)
                    {
                        m_rows[i].UpdateData();
                    }
                }
            }
        }

        public void HandleSort(ListViewRowComparer.Columns eColumn) {
            if (m_header != null)
            {
                m_header.HandleSort(eColumn);
            }
        }

        public void Clear()
        {
            if (m_rows != null)
            {
                foreach (ListViewRow row in m_rows)
                {
                    UnityEngine.Object.Destroy((UnityEngine.Object)row.gameObject);
                }
                m_rows.Clear();
            }
        }

        protected override void OnSizeChanged()
        { 
            base.OnSizeChanged();

            if (m_header != null)
            {
                m_header.width = width;

                if (m_HorizontalPanel != null)
                {
                    m_HorizontalPanel.width = width;
                    m_HorizontalPanel.height = height - m_header.height;
                }
                
                if (m_listPanel != null)
                {
                    m_listPanel.width = m_HorizontalPanel.width - iSCROLL_BAR_WIDTH;
                    m_listPanel.height = m_HorizontalPanel.height;
                }
            }

            if (m_scrollbarPanel != null)
            {
                m_scrollbarPanel.height = m_scrollbarPanel.parent.height;
            }

            if (m_scrollbar != null)
            {
                m_scrollbar.height = m_scrollbar.parent.height;
                TransferManagerCE.Debug.Log("m_scrollbar.height" + m_scrollbar.height + " m_scrollbarPanel.height:" + m_scrollbarPanel.height);
            }
        }

        public void UpdateData(ListViewRowComparer.Columns eSortColumn, bool bDesc)
        {
            if (m_listPanel != null)
            {
                foreach (ListViewRow oRow in m_listPanel.components)
                {
                    oRow.UpdateData();
                }
            }

            Sort(eSortColumn, bDesc);
        }

        public void Sort(ListViewRowComparer.Columns eSortColumn, bool bDesc)
        {
            if (m_listPanel != null)
            {
                m_listPanel.Sort(eSortColumn, bDesc);
                m_listPanel.Invalidate();
            }
        }

        public override void OnDestroy()
        {
            if (m_listPanel != null)
            {
                UnityEngine.Object.Destroy(m_listPanel.gameObject);
                m_listPanel = null;
            }
            if (m_scrollbarPanel != null)
            {
                UnityEngine.Object.Destroy(m_scrollbarPanel.gameObject);
                m_scrollbarPanel = null;
            }
            if (m_scrollbar != null)
            {
                UnityEngine.Object.Destroy(m_scrollbar.gameObject);
                m_scrollbar = null;
            }
            base.OnDestroy();
        }
    }
}
