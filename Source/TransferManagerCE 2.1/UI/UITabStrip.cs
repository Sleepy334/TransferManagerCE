using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class UITabStrip : UIPanel
    {
        struct TabData
        {
            public UIButton? button = null;
            public UIPanel? panel = null;
            public bool visible = true;
            public int m_id = 0;

            public TabData()
            {

            }
        }

        public UIPanel? m_tabButtonPanel = null;
        private List<TabData> m_tabs = new List<TabData>();
        private int m_iSelectedIndex = -1;
        private OnTabChanged? m_tabChangedEvent = null;

        public delegate void OnTabChanged(int index);

        public string buttonNormalSprite { get; set; } = "GenericTab";
        public string buttonDisabledSprite { get; set; } = "GenericTabDisabled";
        public string buttonFocusedSprite { get; set; } = "GenericTabFocused";
        public string buttonHoveredSprite { get; set; } = "GenericTabHovered";
        public string buttonPressedSprite { get; set; } = "GenericTabFocused";

        public static UITabStrip Create(UIComponent parent, float width, float height, OnTabChanged eventTabChanged)
        {
            UITabStrip tabStrip = parent.AddUIComponent<UITabStrip>();
            if (tabStrip != null)
            {
                tabStrip.name = "TransferManagerCE.tabStrip";
                tabStrip.width = width;
                tabStrip.height = height;
                tabStrip.autoLayout = true;
                tabStrip.autoLayoutDirection = LayoutDirection.Vertical;
                tabStrip.eventVisibilityChanged += OnTabVisibilityChanged;

                tabStrip.m_tabButtonPanel = tabStrip.AddUIComponent<UIPanel>();
                tabStrip.m_tabButtonPanel.name = "TransferManagerCE.m_tabButtonPanel";
                tabStrip.m_tabButtonPanel.width = width;
                tabStrip.m_tabButtonPanel.height = 25;
                tabStrip.m_tabButtonPanel.autoLayout = true;
                tabStrip.m_tabButtonPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                tabStrip.m_tabChangedEvent = eventTabChanged;
            }

            return tabStrip;
        }

        public UIPanel? AddTabIcon(string sIcon, UITextureAtlas atlas, string sTooltip)
        {
            if (m_tabs != null && m_tabButtonPanel != null)
            {
                TabData tab = new TabData();
                tab.button = CreateTabButton(m_tabButtonPanel);
                tab.button.normalFgSprite = sIcon;
                tab.button.atlas = atlas;
                tab.button.tooltip = sTooltip;
                tab.button.width = 30f;
                tab.button.eventMouseDown += OnTabSelected;

                tab.panel = AddUIComponent<UIPanel>();
                tab.panel.name = "TransferManagerCE.TabPanel";
                tab.panel.width = width;
                tab.panel.height = height - m_tabButtonPanel.height;
                tab.panel.autoLayout = true;
                tab.panel.autoLayoutDirection = LayoutDirection.Vertical;
                m_tabs.Add(tab);

                return tab.panel;
            }
            return null;
        }

        public UIPanel? AddTabIcon(string sIcon, string sTooltip)
        {
            if (m_tabs != null && m_tabButtonPanel != null)
            {
                return AddTabIcon(sIcon, m_tabButtonPanel.atlas, sTooltip);
            }
            return null;
        }

        public UIPanel? AddTab(string sText, float fWidth = 100f)
        {
            if (m_tabs != null && m_tabButtonPanel != null)
            {
                TabData tab = new TabData();
                tab.button = CreateTabButton(m_tabButtonPanel);
                tab.button.text = sText;
                tab.button.width = fWidth;
                tab.button.eventMouseDown += OnTabSelected;

                tab.panel = AddUIComponent<UIPanel>();
                tab.panel.name = "TransferManagerCE.TabPanel";
                tab.panel.width = width;
                tab.panel.height = height - m_tabButtonPanel.height;
                tab.panel.autoLayout = true;
                tab.panel.autoLayoutDirection = LayoutDirection.Vertical;
                m_tabs.Add(tab);

                return tab.panel;
            }
            return null;
        }

        public static void OnTabVisibilityChanged(UIComponent component, bool bVisible)
        {
            UITabStrip tabStrip = component as UITabStrip;
            if (tabStrip != null)
            {
                tabStrip.OnTabVisibilityChanged(bVisible);
            }
        }

        public void OnTabVisibilityChanged(bool bVisible)
        {
            if (bVisible)
            {
                if (m_iSelectedIndex < 0 && m_iSelectedIndex >= m_tabs.Count)
                {
                    SelectTabIndex(GetFirstVisibleTab());
                }
                if (m_iSelectedIndex >= 0 && m_iSelectedIndex < m_tabs.Count)
                {
                    ShowTab(m_iSelectedIndex);
                }
            }
        }

        private int GetFirstVisibleTab()
        {
            for (int i = 0; i < m_tabs.Count; ++i)
            {
                if (m_tabs[i].visible)
                {
                    return i;
                }
            }
            return -1;
        }

        public void OnTabSelected(UIComponent component, UIMouseEventParameter args)
        {
            for (int i = 0; i < m_tabs.Count; ++i)
            {
                if (component == m_tabs[i].button)
                {
                    SelectTabIndex(i);
                    break;
                }
            }
        }

        public int Count
        {
            get
            {
                if (m_tabs != null)
                {
                    return m_tabs.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static void OnSelectedTabIndexChanged(UIComponent component, int value)
        {
            UITabStrip? parent = (UITabStrip?)component.parent;
            if (parent != null)
            {
                parent.OnSelectTabChanged(value);
            }
        }

        public void OnSelectTabChanged(int value)
        {
            // Show coorect tab
            for (int i = 0; i < m_tabs.Count; i++)
            {
                if (i == value)
                {
                    ShowTab(i);
                    break;
                }
            }
        }

        public void SelectTabIndex(int iIndex)
        {
            if (m_tabs != null)
            {
                m_iSelectedIndex = iIndex;
                ShowTab(iIndex);
                if (m_tabChangedEvent != null)
                {
                    m_tabChangedEvent(iIndex);
                }
            }
        }

        private void ShowTab(int iIndex)
        {
            if (m_tabs != null)
            {
                if (iIndex >= 0 && iIndex < m_tabs.Count)
                {
                    for (int i = 0; i < m_tabs.Count; ++i)
                    {
                        if (i == iIndex)
                        {
                            m_tabs[i].panel.Show();
                            m_tabs[i].button.normalBgSprite = buttonPressedSprite;// "GenericTabFocused"; //"GenericTabHovered";
                            m_tabs[i].button.hoveredBgSprite = buttonPressedSprite;// "GenericTabFocused"; //"GenericTabHovered";
                        }
                        else
                        {
                            m_tabs[i].panel.Hide();
                            m_tabs[i].button.normalBgSprite = buttonNormalSprite;// "GenericTab";
                            m_tabs[i].button.hoveredBgSprite = buttonHoveredSprite;// "GenericTabHovered";
                        }

                    }
                    if (iIndex == m_iSelectedIndex)
                    {
                        m_tabs[iIndex].panel.Focus();
                    }
                    else
                    {
                        m_tabs[iIndex].panel.Unfocus();
                    }
                }
            }
        }

        public int GetSelectTabIndex()
        {
            return m_iSelectedIndex;
        }

        public void SetTabVisible(int iIndex, bool bVisible)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                TabData tab = m_tabs[iIndex];
                if (tab.visible != bVisible)
                {
                    tab.visible = bVisible;
                    m_tabs[iIndex] = tab;

                    if (bVisible)
                    {
                        //m_tabs[iIndex].panel.Show();
                        m_tabs[iIndex].button.Show();
                    }
                    else
                    {
                        m_tabs[iIndex].panel.Hide();
                        m_tabs[iIndex].button.Hide();
                    }
                    if (m_iSelectedIndex == iIndex)
                    {
                        SelectTabIndex(GetFirstVisibleTab());
                    }
                }
            }
        }

        public void SetTabText(int iIndex, string sTab)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                m_tabs[iIndex].button.text = sTab;
            }
        }

        public void SetTabWidth(int iIndex, float fWidth)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                m_tabs[iIndex].button.width = fWidth;
            }
        }
        

        public void SetTabToolTip(int iIndex, string sTooltip)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                m_tabs[iIndex].button.tooltip = sTooltip;
            }
        }

        public void SetTabId(int iIndex, int id)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                TabData data = m_tabs[iIndex];
                data.m_id = id;
                m_tabs[iIndex] = data;
            }
        }

        public int GetTabId(int iIndex)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                return m_tabs[iIndex].m_id;
            }
            return -1;
        }

        public bool IsTabVisible(int iIndex)
        {
            if (m_tabs != null && iIndex < m_tabs.Count)
            {
                return m_tabs[iIndex].visible;
            }
            return false;
        }

        private UIButton CreateTabButton(UIComponent parent)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = "TabButton";

            button.height = 26f;
            button.width = 120f;

            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;

            button.normalBgSprite = buttonNormalSprite;
            button.disabledBgSprite = buttonDisabledSprite;
            button.focusedBgSprite = buttonFocusedSprite;
            button.hoveredBgSprite = buttonHoveredSprite;
            button.pressedBgSprite = buttonPressedSprite;

            /*
            button.hoveredFgSprite = buttonHoveredSprite;
            button.normalFgSprite = buttonNormalSprite;
            button.disabledFgSprite = buttonDisabledSprite;
            button.focusedFgSprite = buttonFocusedSprite;
            button.hoveredFgSprite = buttonHoveredSprite;
            button.pressedFgSprite = buttonPressedSprite;
            */
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(111, 111, 111, 255);
            button.focusedTextColor = new Color32(16, 16, 16, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(255, 255, 255, 255);

            return button;
        }
    }
}