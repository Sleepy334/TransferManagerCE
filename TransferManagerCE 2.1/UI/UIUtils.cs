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

        public static UIButton? AddButton(UIComponent parent, string sText, float width, float height, MouseEventHandler? eventClick = null)
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
                if (eventClick != null)
                {
                    button.eventClick += eventClick;
                }
            }
            
            return button;
        }

        public static UIButton? AddSpriteButton(UIComponent parent, string sSprite, float width, float height, MouseEventHandler? eventClick = null)
        {
            UIButton? button = parent.AddUIComponent<UIButton>();
            if (button != null)
            {
                button.text = "";
                button.autoSize = false;
                button.width = width;
                button.height = height;
                button.normalBgSprite = "OptionsDropboxListbox";
                button.disabledBgSprite = "ButtonMenuDisabled";
                button.hoveredBgSprite = "OptionsDropboxListboxHovered";
                button.focusedBgSprite = "OptionsDropboxListbox";
                button.pressedBgSprite = "OptionsDropboxListboxPressed";
                if (eventClick != null)
                {
                    button.eventClick += eventClick;
                }

                UISprite sprite = button.AddUIComponent<UISprite>();
                if (sprite != null)
                {
                    sprite.spriteName = sSprite;
                    sprite.autoSize = false;
                    sprite.width = button.width - 4;
                    sprite.height = button.height - 4;
                    sprite.CenterToParent();
                }
            }

            return button;
        }

        public static UITextField CreateTextField(UIComponent parent, string fieldName, float fTextScale, float width, float height)
        {
            var textField = parent.AddUIComponent<UITextField>();

            textField.name = fieldName;
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            textField.width = width;
            textField.height = height;
            textField.padding = new RectOffset(6, 6, 6, 6);
            textField.normalBgSprite = "LevelBarBackground";
            textField.hoveredBgSprite = "LevelBarBackground";
            textField.disabledBgSprite = "LevelBarBackground";
            textField.focusedBgSprite = "LevelBarBackground";
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.textColor = Color.white;
            textField.textScale = fTextScale;
            textField.selectOnFocus = true;

            return textField;
        }

    }
}