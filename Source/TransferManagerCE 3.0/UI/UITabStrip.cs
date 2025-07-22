using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.ComponentModel;
using ColossalFramework.DataBinding;
using System.Security.Policy;
using static MessageInfo;
using System;
using SleepyCommon;

namespace TransferManagerCE
{
    public class UITabStrip : UIPanel
    {
        public enum TabStyle
        {
            Generic,
            SubBar,
            ToolbarIcon,
        }

        struct TabData
        {
            public UIButton? button = null;
            public UIPanel? panel = null;
            public bool visible = true;
            public int m_id = 0;
            public float m_fWidth = 100f;
            public bool m_bCompact = false;

            public TabData()
            {

            }
        }

        public delegate void OnTabChanged(int index);

        private UIPanel? m_tabButtonPanel = null;
        private UIPanel? m_tabUnderline = null;
        private UIPanel? m_tabSeparator = null;
        private List<TabData> m_tabs = new List<TabData>();
        private int m_iSelectedIndex = -1;
        private OnTabChanged? m_tabChangedEvent = null;

        // Tab style
        private string m_buttonNormalSprite = "GenericTab";
        private string m_buttonDisabledSprite = "GenericTabDisabled";
        private string m_buttonFocusedSprite = "GenericTabFocused";
        private string m_buttonHoveredSprite = "GenericTabHovered";
        private string m_buttonPressedSprite = "GenericTabFocused";
        private string m_SepratorSprite = "ListItemHighlight";

        public static UITabStrip? Create(TabStyle style, UIComponent parent, float width, float height, OnTabChanged eventTabChanged)
        {
            UITabStrip? tabStrip = parent.AddUIComponent<UITabStrip>();
            if (tabStrip is not null)
            {
                tabStrip.SetTabStyle(style);
                tabStrip.name = "TransferManagerCE.tabStrip";
                tabStrip.width = width;
                tabStrip.height = height;
                tabStrip.autoLayout = true;
                tabStrip.autoLayoutDirection = LayoutDirection.Vertical;
                
                tabStrip.m_tabButtonPanel = tabStrip.AddUIComponent<UIPanel>();
                tabStrip.m_tabButtonPanel.name = "TransferManagerCE.m_tabButtonPanel";
                tabStrip.m_tabButtonPanel.width = width;
                tabStrip.m_tabButtonPanel.height = 25;
                tabStrip.m_tabButtonPanel.autoLayout = true;
                tabStrip.m_tabButtonPanel.autoLayoutDirection = LayoutDirection.Horizontal;

                tabStrip.m_tabUnderline = tabStrip.AddUIComponent<UIPanel>();
                tabStrip.m_tabUnderline.width = width;
                tabStrip.m_tabUnderline.height = 4;
                tabStrip.m_tabUnderline.backgroundSprite = tabStrip.m_SepratorSprite;

                tabStrip.m_tabSeparator = tabStrip.AddUIComponent<UIPanel>();
                tabStrip.m_tabSeparator.width = width;
                tabStrip.m_tabSeparator.height = 6;

                // Set this last
                tabStrip.m_tabChangedEvent = eventTabChanged;
                tabStrip.eventVisibilityChanged += OnTabVisibilityChanged;
            }

            return tabStrip;
        }

        public OnTabChanged? eventTabChanged
        {
            get
            {
                return m_tabChangedEvent;
            }
            set
            {
                m_tabChangedEvent = value;
            }
        }

        public UIPanel? AddTabIcon(string sIcon, string sText, UITextureAtlas atlas, string sTooltip, float fWidth = 100f)
        {
            if (m_tabs is not null && m_tabButtonPanel is not null)
            {
                TabData tab = new TabData();
                tab.m_fWidth = fWidth;

                // Create button
                tab.button = CreateTabButton(m_tabButtonPanel);
                tab.button.tooltip = sTooltip;
                tab.button.width = fWidth;
                tab.button.eventMouseDown += OnTabSelected;

                // Create a panel so we can add an icon and a label
                UIPanel buttonPanel = tab.button.AddUIComponent<UIPanel>();
                buttonPanel.name = "TabButtonPanel";
                buttonPanel.width = tab.button.width;
                buttonPanel.height = tab.button.height;
                buttonPanel.CenterToParent();
                buttonPanel.autoLayout = true;
                buttonPanel.autoLayoutDirection = LayoutDirection.Horizontal;
                buttonPanel.autoLayoutPadding = new RectOffset(4, 0, 0, 0);
                //buttonPanel.backgroundSprite = "InfoviewPanel";
                //buttonPanel.color = Color.red;

                UISprite sprite = buttonPanel.AddUIComponent<UISprite>();
                sprite.name = "TabButtonSprite";
                sprite.atlas = atlas; 
                sprite.spriteName = sIcon;
                sprite.width = buttonPanel.height - 2;
                sprite.height = buttonPanel.height - 2;

                UILabel label = buttonPanel.AddUIComponent<UILabel>();
                label.name = "TabButtonLabel";
                label.autoSize = false;
                label.text = sText;
                label.verticalAlignment = UIVerticalAlignment.Middle;
                label.textAlignment = UIHorizontalAlignment.Center;
                label.tooltip = sTooltip;
                label.height = buttonPanel.height;
                label.width = fWidth - sprite.width - 16;
                //label.backgroundSprite = "InfoviewPanel";
                //label.color = Color.green;

                tab.panel = AddUIComponent<UIPanel>();
                tab.panel.name = "TabPanel";
                tab.panel.width = width;
                tab.panel.height = height - m_tabButtonPanel.height - m_tabUnderline.height - m_tabSeparator.height - 4;
                tab.panel.autoLayout = true;
                tab.panel.autoLayoutDirection = LayoutDirection.Vertical;
                //tab.panel.backgroundSprite = "InfoviewPanel";
                //tab.panel.color = Color.green;

                m_tabs.Add(tab);

                return tab.panel;
            }
            return null;
        }

        public UIPanel? AddTabIcon(string sIcon, string sText, string sTooltip, float fWidth = 100f)
        {
            if (m_tabs is not null && m_tabButtonPanel is not null)
            {
                return AddTabIcon(sIcon, sText, m_tabButtonPanel.atlas, sTooltip, fWidth);
            }
            return null;
        }

        public UIPanel? AddTab(string sText, float fWidth = 100f)
        {
            if (m_tabs is not null && m_tabButtonPanel is not null)
            {
                TabData tab = new TabData();
                tab.button = CreateTabButton(m_tabButtonPanel);
                tab.button.text = sText;
                tab.button.width = fWidth;
                tab.button.eventMouseDown += OnTabSelected;

                tab.panel = AddUIComponent<UIPanel>();
                tab.panel.name = "TabPanel";
                tab.panel.width = width;
                tab.panel.height = height - m_tabButtonPanel.height - m_tabUnderline.height - m_tabSeparator.height - 4;
                tab.panel.autoLayout = true;
                tab.panel.autoLayoutDirection = LayoutDirection.Vertical;

                m_tabs.Add(tab);

                return tab.panel;
            }
            return null;
        }

        public void SetCompactMode(int iTabIndex, bool bCompact)
        {
            if (iTabIndex < m_tabs.Count)
            {
                TabData tab = m_tabs[iTabIndex];
                if (tab.m_bCompact != bCompact)
                {
                    // Show or hide label
                    UILabel? buttonLabel = tab.button.Find<UILabel>("TabButtonLabel");
                    UIPanel? buttonPanel = tab.button.Find<UIPanel>("TabButtonPanel");
                    UISprite? buttonSprite = tab.button.Find<UISprite>("TabButtonSprite");

                    if (buttonLabel is not null && buttonPanel is not null && buttonSprite is not null)
                    {
                        // Set button width
                        tab.button.width = bCompact ? 60f : tab.m_fWidth;

                        // Make sure sub panel is the same width
                        buttonPanel.width = tab.button.width;
                        buttonPanel.autoLayout = !bCompact;

                        // Show or hide label
                        buttonLabel.isVisible = !bCompact;

                        if (bCompact)
                        {
                            tab.button.tooltip = buttonLabel.text;
                            buttonSprite.CenterToParent();
                        }
                        else
                        {
                            tab.button.tooltip = "";
                        }
                    }
                    else
                    {
                        CDebug.Log($"ERROR: Unable to compact tab: {iTabIndex} buttonLabel: {buttonLabel} buttonPanel: {buttonPanel} buttonSprite: {buttonSprite}");
                    }

                    // Update state
                    tab.m_bCompact = bCompact;
                    m_tabs[iTabIndex] = tab;
                }
            }
        }

        public static void OnTabVisibilityChanged(UIComponent component, bool bVisible)
        {
            UITabStrip tabStrip = component as UITabStrip;
            if (tabStrip is not null)
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
            else if (m_iSelectedIndex < 0 || 
                    (m_iSelectedIndex >= 0 && m_iSelectedIndex < m_tabs.Count && !m_tabs[m_iSelectedIndex].visible)) 
            {
                SelectTabIndex(GetFirstVisibleTab());
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
                if (m_tabs is not null)
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
            if (parent is not null)
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
            m_iSelectedIndex = iIndex;

            if (m_tabs is not null)
            {
                ShowTab(iIndex);
                if (m_tabChangedEvent is not null)
                {
                    m_tabChangedEvent(iIndex);
                }
            }
        }

        public void SelectFirstVisibleTab()
        {
            SelectTabIndex(GetFirstVisibleTab());
        }

        private void ShowTab(int iIndex)
        {
            if (m_tabs is not null)
            {
                if (iIndex >= 0 && iIndex < m_tabs.Count)
                {
                    for (int i = 0; i < m_tabs.Count; ++i)
                    {
                        if (i == iIndex)
                        {
                            m_tabs[i].panel.Show();
                            m_tabs[i].button.normalBgSprite = m_buttonPressedSprite;
                            m_tabs[i].button.hoveredBgSprite = m_buttonPressedSprite;
                        }
                        else
                        {
                            m_tabs[i].panel.Hide();
                            m_tabs[i].button.normalBgSprite = m_buttonNormalSprite;
                            m_tabs[i].button.hoveredBgSprite = m_buttonHoveredSprite;
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
            if (m_tabs is not null && iIndex < m_tabs.Count)
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
            else
            {
                throw new Exception($"Requested index is out of bounds {iIndex} of {m_tabs.Count}");
            }
        }

        public void SetTabText(int iIndex, string sTab)
        {
            if (m_tabs is not null && iIndex < m_tabs.Count)
            {
                UIButton button = m_tabs[iIndex].button;
                if (button is not null)
                {
                    // Try and update sub label first
                    bool bLabelFound = false;
                    if (button.components.Count > 0)
                    {
                        UIPanel? subPanel = button.components[0] as UIPanel;
                        if (subPanel is not null && subPanel.components.Count >= 2)
                        {
                            UILabel? label = subPanel.components[1] as UILabel;
                            if (label is not null)
                            {
                                bLabelFound = true;
                                label.text = sTab;
                            }
                        }
                    }

                    // Otherwise just update button
                    if (!bLabelFound)
                    {
                        button.text = sTab;
                    }
                }
            }
        }

        public void SetTabWidth(int iIndex, float fWidth)
        {
            if (m_tabs is not null && iIndex < m_tabs.Count)
            {
                UIButton button = m_tabs[iIndex].button;
                if (button is not null)
                {
                    m_tabs[iIndex].button.width = fWidth;

                    // Rearange icon button layout
                    UILabel label = button.Find<UILabel>("TabButtonLabel");
                    if (label is not null)
                    {
                        label.width = fWidth - (button.height - 2) - 16;
                    }
                }
            }
        }
        

        public void SetTabToolTip(int iIndex, string sTooltip)
        {
            if (m_tabs is not null && iIndex < m_tabs.Count)
            {
                m_tabs[iIndex].button.tooltip = sTooltip;
            }
        }

        public void SetTabId(int iIndex, int id)
        {
            if (m_tabs is not null && iIndex < m_tabs.Count)
            {
                TabData data = m_tabs[iIndex];
                data.m_id = id;
                m_tabs[iIndex] = data;
            }
        }

        public int GetTabId(int iIndex)
        {
            if (m_tabs is not null && iIndex >= 0 && iIndex < m_tabs.Count)
            {
                return m_tabs[iIndex].m_id;
            }
            return -1;
        }

        public bool IsTabVisible(int iIndex)
        {
            if (m_tabs is not null && iIndex < m_tabs.Count)
            {
                return m_tabs[iIndex].visible;
            }
            return false;
        }

        public int GetVisibleTabCount()
        {
            int iVisible = 0;
            foreach (TabData data in m_tabs)
            {
                if (data.visible)
                {
                    iVisible++;
                }
            }

            return iVisible;
        }

        public override void PerformLayout()
        {
            // Check curent tab is visible
            int index = GetSelectTabIndex();
            if (index == -1 || (GetSelectTabIndex() >= 0 && !IsTabVisible(GetSelectTabIndex())))
            {
                SelectFirstVisibleTab();
            }

            base.PerformLayout();
        }

        private UIButton CreateTabButton(UIComponent parent)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = "TabButton";
            button.height = 26f;
            button.width = 120f;

            // Text
            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(111, 111, 111, 255);
            button.focusedTextColor = new Color32(16, 16, 16, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(255, 255, 255, 255);
            
            // Set style
            button.normalBgSprite = m_buttonNormalSprite;
            button.disabledBgSprite = m_buttonDisabledSprite;
            button.focusedBgSprite = m_buttonFocusedSprite;
            button.hoveredBgSprite = m_buttonHoveredSprite;
            button.pressedBgSprite = m_buttonPressedSprite;

            return button;
        }

        private void SetTabStyle(TabStyle style)
        {
            switch (style)
            {
                case TabStyle.Generic:
                    {
                        m_buttonNormalSprite = "GenericTab";
                        m_buttonDisabledSprite = "GenericTabDisabled";
                        m_buttonFocusedSprite = "GenericTabFocused";
                        m_buttonHoveredSprite = "GenericTabHovered";
                        m_buttonPressedSprite = "GenericTabFocused";
                        m_SepratorSprite = "ListItemHighlight";
                        break;
                    }
                case TabStyle.SubBar:
                    {
                        m_buttonNormalSprite = "SubBarButtonBase";
                        m_buttonDisabledSprite = "SubBarButtonBaseDisabled";
                        m_buttonFocusedSprite = "SubBarButtonBasePressed";
                        m_buttonHoveredSprite = "SubBarButtonBaseHovered";
                        m_buttonPressedSprite = "SubBarButtonBasePressed";
                        m_SepratorSprite = "SubBarButtonBasePressed";
                        break;
                    }                    
            }
        }
    }
}