using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TransferManagerCE;
using UnityEngine;

namespace SleepyCommon
{
    public class CheckListView : UIPanel
    {
        public const int iSCROLL_BAR_WIDTH = 20;

        public UIPanel? m_HorizontalPanel = null;
        public UIScrollablePanel? m_listPanel;
        public UIPanel? m_scrollbarPanel;
        public UIScrollbar? m_scrollbar;
        private string m_backgroundSprite;
        private List<CheckListRow> m_rows = null;
        private float m_fTextScale = 1.0f;

        public CheckListView() : base() {
            m_scrollbar = null;
            m_listPanel = null;
            m_scrollbarPanel = null;
            m_rows = new List<CheckListRow>();
        }

        public static CheckListView? Create(UIComponent oParent, string txtBackgroundSprite, float fTextScale, float fWidth, float fHeight)
        {
            try
            {
                CheckListView listView = oParent.AddUIComponent<CheckListView>();
                if (listView != null)
                {
                    listView.m_backgroundSprite = txtBackgroundSprite;
                    listView.backgroundSprite = txtBackgroundSprite;
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
            // Horizontal panel
            m_HorizontalPanel = AddUIComponent<UIPanel>();
            if (m_HorizontalPanel != null)
            {
                m_HorizontalPanel.backgroundSprite = m_backgroundSprite;
                m_HorizontalPanel.width = width;
                m_HorizontalPanel.height = height;
                m_HorizontalPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                m_HorizontalPanel.autoLayout = true;

                m_listPanel = m_HorizontalPanel.AddUIComponent<UIScrollablePanel>();
                if (m_listPanel != null)
                {
                    m_listPanel.backgroundSprite = m_backgroundSprite;// "GenericPanelWhite";
                    m_listPanel.autoLayoutDirection = LayoutDirection.Vertical;
                    m_listPanel.autoLayoutStart = LayoutStart.TopLeft;
                    m_listPanel.autoLayoutPadding = new RectOffset(2, 2, 2, 2);
                    m_listPanel.autoLayout = true;
                    m_listPanel.clipChildren = true;
                    m_listPanel.width = width;
                    m_listPanel.height = height;
                }
                else
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
        }

        public UIScrollbar? SetUpScrollbar()
        {
            if (m_scrollbarPanel != null) 
            {
                m_scrollbar = m_scrollbarPanel.AddUIComponent<UIScrollbar>();
                m_scrollbar.name = "Scrollbar";
                m_scrollbar.width = iSCROLL_BAR_WIDTH;
                m_scrollbar.height = m_scrollbarPanel.height;
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

        public CheckListRow? AddItem(CheckListData data)
        {
            m_rows.Add(CheckListRow.Create(m_listPanel, data));
            return m_rows[m_rows.Count - 1];
        }

        public void SetItems(CheckListData[] items)
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
                            CheckListRow? row = AddItem(items[i]);
                            if (row != null)
                            {
                                row.UpdateData();
                            }
                        }
                    }
                }

                // Remove items if we have too many
                while (m_rows.Count > items.Length)
                {
                    CheckListRow row = m_rows[m_rows.Count - 1];
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

        public void Clear()
        {
            if (m_rows != null)
            {
                foreach (CheckListRow row in m_rows)
                {
                    UnityEngine.Object.Destroy((UnityEngine.Object)row.gameObject);
                }
                m_rows.Clear();
            }
        }

        public void UpdateData()
        {
            if (m_listPanel != null)
            {
                foreach (CheckListRow oRow in m_listPanel.components)
                {
                    oRow.UpdateData();
                }
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
