using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
    public class UIMyDropDown
    {
        public UIDropDown? m_dropdown = null;
        public UIPanel? m_panel = null;
        public UILabel? m_label = null;
        private PropertyChangedEventHandler<int>? m_eventHandler = null;

        public static UIMyDropDown? Create(UIComponent parent, string text, float fTextScale, string[] items, PropertyChangedEventHandler<int> eventHandler, int selectedIndex = 0, float width = 350, float height = 20)
        {
            UIMyDropDown? myDropDown = null;
            if (parent != null)
            {
                myDropDown = new UIMyDropDown();
                myDropDown.Setup(parent, text, fTextScale, items, eventHandler, selectedIndex, width, height);
            }

            return myDropDown;
        }

        public void Setup(UIComponent parent, string text, float fTextScale, string[] items, PropertyChangedEventHandler<int> eventHandler, int selectedIndex = 0, float width = 350, float height = 20)
        {
            m_panel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
            if (m_panel != null)
            {
                m_eventHandler = eventHandler;
                m_panel.autoLayout = true;
                m_panel.autoLayoutDirection = LayoutDirection.Horizontal;
                m_panel.width = parent.width;
                m_panel.height = 30;

                m_dropdown = m_panel.Find<UIDropDown>("Dropdown");
                m_dropdown.textScale = fTextScale;
                m_dropdown.size = new Vector2(width, height + 8);
                //dropDown.itemHeight = (int) height;
                m_dropdown.scaleFactor = fTextScale;
                m_dropdown.textFieldPadding = new RectOffset(8, 0, 8, 0);
                m_dropdown.itemPadding = new RectOffset(14, 0, 8, 0);
                m_dropdown.eventSelectedIndexChanged += OnSelectedIndexChanged;

                // Set text.
                m_label = m_panel.Find<UILabel>("Label");
                m_label.autoSize = false;
                m_label.text = text;
                m_label.textScale = fTextScale;
                m_label.width = parent.width - m_dropdown.width;
                m_label.height = m_dropdown.height;
                m_label.verticalAlignment = UIVerticalAlignment.Middle;

                // Slightly increase width.
                m_dropdown.autoSize = false;
                m_dropdown.width = width;
                m_dropdown.items = items;
                m_dropdown.selectedIndex = selectedIndex;
            }
        }

        public bool isVisible
        {
            get
            {
                if (m_panel != null)
                {
                    Debug.Log("m_panel.isVisible: " + m_panel.isVisible);
                    return m_panel.isVisible;
                }
                return false;
            }

            set
            {
                if (m_panel != null) 
                {
                    m_panel.isVisible = value;
                }
            }
        }

        public int selectedIndex
        {
            get 
            { 
                return m_dropdown.selectedIndex; 
            }

            set 
            { 
                m_dropdown.selectedIndex = value; 
            }
        }

        public string tooltip
        {
            get
            {
                if (m_dropdown != null)
                {
                    return m_dropdown.tooltip;
                }
                return "";
            }

            set
            {
                if (m_dropdown != null)
                {
                    m_dropdown.tooltip = value;
                }
            }
        }

        public void OnSelectedIndexChanged(UIComponent component, int Value)
        {
            if (m_eventHandler != null)
            {
                m_eventHandler(component, Value);
            }
        }

        public void SetPanelWidth(float width)
        {
            if (m_panel != null)
            {
                m_panel.width = width;

                if (m_label != null && m_dropdown != null)
                {
                    m_label.width = width - m_dropdown.width;
                }
            }
        }

        public void OnDestroy()
        {
            if (m_dropdown != null)
            {
                UnityEngine.Object.Destroy(m_dropdown.gameObject);
                m_dropdown = null;
            }
            if (m_panel != null)
            {
                UnityEngine.Object.Destroy(m_panel.gameObject);
                m_panel = null;
            }
            if (m_label != null)
            {
                UnityEngine.Object.Destroy(m_label.gameObject);
                m_label = null;
            }
            m_eventHandler = null;
        }
    }
}