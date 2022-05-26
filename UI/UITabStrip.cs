using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class UITabStrip : UIPanel
    {
        UITabstrip? m_tabStrip = null;
        UITabContainer? m_tabContainer = null;
        UIButton? m_buttonTemplate = null;
        List<UIButton> m_tabButtons = new List<UIButton>();
        OnTabChanged? m_tabChangedEvent = null;

        public delegate void OnTabChanged(int index);

        public static UITabStrip Create(UIComponent parent, float width, float height, OnTabChanged? eventHandler)
        {
            UITabStrip tabStrip = parent.AddUIComponent<UITabStrip>();
            if (tabStrip != null)
            {
                tabStrip.width = width;
                tabStrip.height = height;
                tabStrip.autoLayout = true;
                tabStrip.autoLayoutDirection = LayoutDirection.Vertical;

                tabStrip.m_tabStrip = tabStrip.AddUIComponent<UITabstrip>();
                tabStrip.m_tabStrip.name = "UITabstrip";
                tabStrip.m_tabStrip.width = width;
                tabStrip.m_tabStrip.height = 25;
                tabStrip.m_tabStrip.eventSelectedIndexChanged += OnSelectedTabIndexChanged;

                tabStrip.m_tabContainer = tabStrip.AddUIComponent<UITabContainer>();
                tabStrip.m_tabContainer.width = width;
                tabStrip.m_tabContainer.height = height - tabStrip.m_tabStrip.height;
                tabStrip.m_tabStrip.tabPages = tabStrip.m_tabContainer;

                tabStrip.m_buttonTemplate = CreateTabButton(parent);
                tabStrip.m_tabChangedEvent = eventHandler;
            }


            return tabStrip;
        }

        public UIPanel? AddTab(string sText)
        {
            if (m_tabStrip != null)
            {
                int iTabCount = m_tabStrip.tabCount;
                m_tabButtons.Add(m_tabStrip.AddTab(sText, m_buttonTemplate, true));

                // Don't notify parent while adding tab
                m_tabStrip.eventSelectedIndexChanged -= OnSelectedTabIndexChanged;
                m_tabStrip.selectedIndex = iTabCount;
                m_tabStrip.eventSelectedIndexChanged += OnSelectedTabIndexChanged;

                UIPanel panel = m_tabStrip.tabContainer.components[iTabCount] as UIPanel;
                if (panel != null)
                {
                    panel.isVisible = false;
                    return panel;
                }
            }
            return null;
        }

        public int Count {
            get { 
                if (m_tabButtons != null)
                {
                    return m_tabButtons.Count;
                }
                else
                {
                    return 0;
                }
            } 
        }

        public static void OnSelectedTabIndexChanged(UIComponent component, int value) 
        {
            UITabStrip? parent = (UITabStrip?) component.parent;
            if (parent != null && parent.m_tabChangedEvent != null)
            {
                parent.m_tabChangedEvent(value);
            }
        }

        public void SelectTabIndex(int iIndex)
        {
            if (m_tabStrip != null)
            {
                m_tabStrip.selectedIndex = iIndex;
                m_tabStrip.tabContainer.components[iIndex].isVisible = true;
            }
        }

        public void SetTabText(int iIndex, string sTab)
        {
            if (m_tabButtons != null)
            {
                m_tabButtons[iIndex].text = sTab;
            }
        }

        public int GetSelectTabIndex()
        {
            if (m_tabStrip != null)
            {
                return m_tabStrip.selectedIndex;
            }
            return -1;
        }

        private static UIButton CreateTabButton(UIComponent parent)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = "TabButton";

            button.height = 26f;
            button.width = 120f;

            button.textHorizontalAlignment = UIHorizontalAlignment.Center;
            button.textVerticalAlignment = UIVerticalAlignment.Middle;

            button.normalBgSprite = "GenericTab";
            button.disabledBgSprite = "GenericTabDisabled";
            button.focusedBgSprite = "GenericTabFocused";
            button.hoveredBgSprite = "GenericTabHovered";
            button.pressedBgSprite = "GenericTabPressed";

            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(111, 111, 111, 255);
            button.focusedTextColor = new Color32(16, 16, 16, 255);
            button.hoveredTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(255, 255, 255, 255);
            button.isVisible = false;

            return button;
        }
    }
}