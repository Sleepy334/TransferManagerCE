using UnityEngine;
using ColossalFramework.UI;
using ICities;

namespace TransferManagerCE
{
    public class UIUtils
    {
        public enum ButtonStyle
        {
            None,
            DropDown,
            BigRound,
            GenericLight,
            ButtonMenu,
            TextField,
            ButtonWhite,
            ToolbarIcon,
        }

        public static UIMyDropDown? AddDropDown(UIHelper helper, string text, float fTextScale, string[] items, PropertyChangedEventHandler<int> eventHandler, int selectedIndex = 0, float width = 350)
        {
            UIPanel panelHelper = (UIPanel)helper.self;
            if (panelHelper is not null)
            {
                return UIMyDropDown.Create(panelHelper, text, fTextScale, items, eventHandler, selectedIndex, width);     
            }
            return null;
        }

        public static UICheckBox? AddCheckbox(UIHelper parent, string text, UIFont font, float fTextScale, bool defaultValue, OnCheckChanged eventCallback)
        {
            return AddCheckbox((UIComponent)parent.self, text, font, fTextScale, defaultValue, eventCallback);
        }

        public static UICheckBox? AddCheckbox(UIComponent parent, string text, UIFont font, float fTextScale, bool defaultValue, OnCheckChanged eventCallback)
        {
            UICheckBox? uICheckBox = null;
            if (eventCallback is not null && !string.IsNullOrEmpty(text))
            {
                uICheckBox = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsCheckBoxTemplate")) as UICheckBox;
                if (uICheckBox is not null)
                {
                    uICheckBox.text = text;
                    uICheckBox.label.textScale = fTextScale;
                    uICheckBox.label.font = font;
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
                if (uiPanel is not null)
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

        public static UIButton? AddButton(ButtonStyle style, UIComponent parent, string sText, string sTooltip, float width, float height, MouseEventHandler? eventClick = null)
        {
            UIButton? button = parent.AddUIComponent<UIButton>();
            if (button is not null)
            {
                button.text = sText;
                button.tooltip = sTooltip;
                button.autoSize = false;
                button.width = width;
                button.height = height;
                SetButtonStyle(style, button);

                if (eventClick is not null)
                {
                    button.eventClick += eventClick;
                }
            }
            
            return button;
        }

        public static UIButton? AddSpriteButton(ButtonStyle style, UIComponent parent, string sSprite, float width, float height, MouseEventHandler? eventClick = null)
        {
            UIButton? button = parent.AddUIComponent<UIButton>();
            if (button is not null)
            {
                button.text = "";
                button.autoSize = false;
                button.width = width;
                button.height = height;
                SetButtonStyle(style, button);
                
                if (eventClick is not null)
                {
                    button.eventClick += eventClick;
                }

                UISprite sprite = button.AddUIComponent<UISprite>();
                if (sprite is not null)
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

        public static UIButton? AddSpriteButton(ButtonStyle style, UIComponent parent, string sSprite, string sTooltip, UITextureAtlas atlas, float width, float height, MouseEventHandler? eventClick = null)
        {
            UIButton? button = parent.AddUIComponent<UIButton>();
            if (button is not null)
            {
                button.text = "";
                button.tooltip = sTooltip;
                button.autoSize = false;
                button.width = width;
                button.height = height;
                SetButtonStyle(style, button);

                if (eventClick is not null)
                {
                    button.eventClick += eventClick;
                }

                UISprite sprite = button.AddUIComponent<UISprite>();
                if (sprite is not null)
                {
                    sprite.spriteName = sSprite;
                    sprite.atlas = atlas;
                    sprite.autoSize = false;
                    sprite.width = button.width - 4;
                    sprite.height = button.height - 4;
                    sprite.CenterToParent();
                }
            }

            return button;
        }

        public static UIToggleButton? AddSpriteToggleButton(bool bInitalValue, ButtonStyle style, UIComponent parent, string sSprite, string sTooltip, UITextureAtlas atlas, float width, float height, MouseEventHandler? eventClick = null)
        {
            UIToggleButton? button = parent.AddUIComponent<UIToggleButton>();
            if (button is not null)
            {
                
                button.text = "";
                button.tooltip = sTooltip;
                button.autoSize = false;
                button.width = width;
                button.height = height;
                button.ToggleState = bInitalValue;
                SetButtonStyle(style, button);

                if (eventClick is not null)
                {
                    button.eventClick += eventClick;
                }

                UISprite sprite = button.AddUIComponent<UISprite>();
                if (sprite is not null)
                {
                    sprite.spriteName = sSprite;
                    sprite.atlas = atlas;
                    sprite.autoSize = false;
                    sprite.width = button.width - 4;
                    sprite.height = button.height - 4;
                    sprite.CenterToParent();
                }
            }

            return button;
        }

        public static UITextField CreateTextField(ButtonStyle style, UIComponent parent, string fieldName, float fTextScale, float width, float height)
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
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.textColor = Color.white;
            textField.textScale = fTextScale;
            textField.selectOnFocus = true;
            SetTextFieldStyle(style, textField);

            return textField;
        }

        private static void SetTextFieldStyle(ButtonStyle style, UITextField textField)
        {
            switch (style)
            {
                case ButtonStyle.DropDown:
                    {
                        textField.normalBgSprite = "LevelBarBackground";
                        textField.hoveredBgSprite = "LevelBarBackground";
                        textField.disabledBgSprite = "LevelBarBackground";
                        textField.focusedBgSprite = "LevelBarBackground";
                        break;
                    }
                case ButtonStyle.TextField:
                    {
                        textField.normalBgSprite = "TextFieldPanelHovered";
                        textField.disabledBgSprite = "TextFieldPanel";
                        textField.selectionSprite = "EmptySprite";
                        break;
                    }
            }
        }

        private static void SetButtonStyle(ButtonStyle style, UIButton button)
        {
            switch (style)
            {
                case ButtonStyle.DropDown:
                    {
                        button.normalBgSprite = "OptionsDropboxListbox";
                        button.disabledBgSprite = "ButtonMenuDisabled";
                        button.hoveredBgSprite = "OptionsDropboxListboxHovered";
                        button.focusedBgSprite = "OptionsDropboxListbox";
                        button.pressedBgSprite = "OptionsDropboxListboxPressed";
                        break;
                    }
                case ButtonStyle.BigRound:
                    {
                        button.normalBgSprite = "RoundBackBig";
                        button.focusedBgSprite = "RoundBackBigFocused";
                        button.hoveredBgSprite = "RoundBackBigHovered";
                        button.pressedBgSprite = "RoundBackBigPressed";
                        button.disabledBgSprite = "RoundBackBigDisabled";
                        break;
                    }
                case ButtonStyle.GenericLight:
                    {
                        button.normalBgSprite = "GenericPanelLight";
                        button.focusedBgSprite = "GenericPanelLight";
                        button.hoveredBgSprite = "GenericPanelWhite";
                        button.pressedBgSprite = "GenericPanelLight";
                        button.disabledBgSprite = "ButtonMenuDisabled";
                        break;
                    }
                case ButtonStyle.ButtonMenu:
                    {
                        button.normalBgSprite = "ButtonMenu";
                        button.focusedBgSprite = "ButtonMenuFocused";
                        button.disabledBgSprite = "ButtonMenuDisabled";
                        button.hoveredBgSprite = "ButtonMenuHovered";
                        button.pressedBgSprite = "ButtonMenuPressed";
                        break;
                    }
                case ButtonStyle.TextField:
                    {
                        button.normalBgSprite = "TextFieldPanel";
                        button.focusedBgSprite = "TextFieldPanelFocused";
                        button.disabledBgSprite = "TextFieldPanelDisabled";
                        button.hoveredBgSprite = "TextFieldPanelHovered";
                        button.pressedBgSprite = "TextFieldPanelPressed";
                        break;
                    }
                case ButtonStyle.ButtonWhite:
                    {
                        button.normalBgSprite = "ButtonWhite";
                        button.hoveredBgSprite = "ButtonWhite";
                        button.focusedBgSprite = "ButtonWhite";
                        button.pressedBgSprite = "ButtonWhitePressed";
                        button.disabledBgSprite = "ButtonWhiteDisabled";
                        break;
                    }
                case ButtonStyle.ToolbarIcon:
                    {
                        //panelButton.normalFgSprite = "IconPolicyBigBusiness";
                        button.normalBgSprite = "ToolbarIconGroup6Normal";
                        button.hoveredBgSprite = "ToolbarIconGroup6Hovered";
                        button.focusedBgSprite = "ToolbarIconGroup6Focused";
                        button.pressedBgSprite = "ToolbarIconGroup6Pressed";
                        button.disabledBgSprite = "ToolbarIconGroup6Disabled";
                        break;
                    }
                case ButtonStyle.None:
                    {
                        break;
                    }
            }
        }
    }
}