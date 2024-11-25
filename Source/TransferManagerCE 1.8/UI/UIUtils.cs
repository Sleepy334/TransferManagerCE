using UnityEngine;
using ColossalFramework.UI;
using ICities;

namespace TransferManagerCE
{
    public class UIUtils
    {
        public static UIMyDropDown? AddDropDown(UIHelper helper, string text, float fTextScale, string[] items, PropertyChangedEventHandler<int> eventHandler, int selectedIndex = 0, float width = 350)
        {
            UIPanel panelHelper = (UIPanel)helper.self;
            if (panelHelper != null)
            {
                return UIMyDropDown.Create(panelHelper, text, fTextScale, items, eventHandler, selectedIndex, width);     
            }
            return null;
        }

        public static UICheckBox? AddCheckbox(UIHelper parent, string text, float fTextScale, bool defaultValue, OnCheckChanged eventCallback)
        {
            return AddCheckbox((UIComponent)parent.self, text, fTextScale, defaultValue, eventCallback);
        }

        public static UICheckBox? AddCheckbox(UIComponent parent, string text, float fTextScale, bool defaultValue, OnCheckChanged eventCallback)
        {
            UICheckBox? uICheckBox = null;
            if (eventCallback != null && !string.IsNullOrEmpty(text))
            {
                uICheckBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;
                if (uICheckBox != null)
                {
                    uICheckBox.text = text;
                    uICheckBox.label.textScale = fTextScale;
                    uICheckBox.isChecked = defaultValue;
                    uICheckBox.eventCheckChanged += delegate (UIComponent c, bool isChecked)
                    {
                        eventCallback(isChecked);
                    };
                }
            }

            return uICheckBox;
        }

        public static UIPanel? AddGroup(UIComponent parent, string text, float fTextScale, float width, float height)
        {
            UIPanel? uiPanel = null;
            if (!string.IsNullOrEmpty(text))
            {
                uiPanel = parent.AddUIComponent<UIPanel>();
                if (uiPanel != null)
                {
                    uiPanel.autoLayout = true;
                    uiPanel.autoLayoutDirection = LayoutDirection.Vertical;
                    uiPanel.autoLayoutPadding = new RectOffset(4, 4, 2, 2);
                    uiPanel.autoSize = false;
                    uiPanel.width = width;
                    uiPanel.height = height;

                    UILabel label = uiPanel.AddUIComponent<UILabel>();
                    label.text = text;
                    label.textScale = fTextScale;
                }
            }

            return uiPanel;
        }

        public static UIButton? AddButton(UIComponent parent, string sText, float width, float height)
        {
            UIButton? button = parent.AddUIComponent<UIButton>();
            if (button != null)
            {
                button.text = sText;
                button.autoSize = false;
                button.width = width;
                button.height = height;
                button.normalBgSprite = "OptionsDropboxListbox";
                button.disabledBgSprite = "ButtonMenuDisabled";
                button.hoveredBgSprite = "OptionsDropboxListboxHovered";
                button.focusedBgSprite = "OptionsDropboxListbox";
                button.pressedBgSprite = "OptionsDropboxListboxPressed";
            }
            
            return button;
        }
            
    }
}