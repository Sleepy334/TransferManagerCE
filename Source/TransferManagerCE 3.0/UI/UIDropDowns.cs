// <copyright file="UIDropDowns.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// UI dropdown menus.
    /// </summary>
    public static class UIDropDowns
    {
        /// <summary>
        /// Creates a dropdown menu with an attached text label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Text label.</param>
        /// <param name="width">Dropdown menu width, excluding label (default 220f).</param>
        /// <param name="height">Dropdown button height (default 25f).</param>
        /// <param name="labelTextScale">Label text scaling (default 0.8f).</param>
        /// <param name="itemTextScale">Menu item text scaling (default 0.7f).</param>
        /// <param name="itemHeight">Dropdown menu item height (default 20).</param>
        /// <param name="vertPadding">Dropdown menu selected item vertical text padding (default 8).</param>
        /// <param name="itemVertPadding">Dropdown menu item vertical text padding (default 8).</param>
        /// <param name="accomodateLabel">True (default) to move menu to accomodate text label width, false otherwise.</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        /// <returns>New dropdown menu with an attached text label and enclosing panel.</returns>
        public static UIDropDown AddLabelledDropDown(UIComponent parent, float xPos, float yPos, string text, float width = 220f, float height = 25f, float labelTextScale = 0.8f, float itemTextScale = 0.7f, int itemHeight = 20, int vertPadding = 8, int itemVertPadding = 8, bool accomodateLabel = true, string tooltip = null)
        {
            // Create dropdown.
            UIDropDown dropDown = AddDropDown(parent, xPos, yPos, width, height, itemTextScale, itemHeight, vertPadding, itemVertPadding, tooltip);

            // Add label.
            UILabel label = dropDown.AddUIComponent<UILabel>();
            label.textScale = labelTextScale;
            label.text = text;

            // Get width and position.
            float labelWidth = label.width + 10f;

            label.relativePosition = new Vector2(-labelWidth, (height - label.height) / 2f);

            // Move dropdown to accomodate label if that setting is set.
            if (accomodateLabel)
            {
                dropDown.relativePosition += new Vector3(labelWidth, 0f);
            }

            return dropDown;
        }

        /// <summary>
        /// Creates a dropdown menu without text label or enclosing panel.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="width">Dropdown menu width, excluding label (default 220f).</param>
        /// <param name="height">Dropdown button height (default 25f).</param>
        /// <param name="itemTextScale">Text scaling (default 0.7f).</param>
        /// <param name="itemHeight">Dropdown menu item height (default 20).</param>
        /// <param name="vertPadding">Dropdown menu selected item vertical text padding (default 8).</param>
        /// <param name="itemVertPadding">Dropdown menu item vertical text padding (default 8).</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        /// <returns>New dropdown menu *without* an attached text label or enclosing panel.</returns>
        public static UIDropDown AddDropDown(UIComponent parent, float xPos, float yPos, float width = 220f, float height = 25f, float itemTextScale = 0.7f, int itemHeight = 20, int vertPadding = 8, int itemVertPadding = 8, string tooltip = null)
        {
            // Create dropdown menu.
            UIDropDown dropDown = parent.AddUIComponent<UIDropDown>();
            //dropDown.atlas = UITextures.InGameAtlas;
            dropDown.normalBgSprite = "TextFieldPanel";
            dropDown.disabledBgSprite = "TextFieldPanelDisabled";
            dropDown.hoveredBgSprite = "TextFieldPanelHovered";
            dropDown.focusedBgSprite = "TextFieldPanelHovered";
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.listBackground = "TextFieldPanel";
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.color = Color.white;
            dropDown.popupColor = Color.white;
            dropDown.textColor = Color.white; //Color.black;
            dropDown.popupTextColor = Color.black;
            dropDown.disabledColor = Color.black;
            dropDown.font = UIFonts.SemiBold;
            dropDown.zOrder = 1;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.textFieldPadding = new RectOffset(8, 0, vertPadding, 0);
            dropDown.itemPadding = new RectOffset(14, 0, itemVertPadding, 0);

            dropDown.relativePosition = new Vector2(xPos, yPos);

            // Dropdown size parameters.
            dropDown.size = new Vector2(width, height);
            dropDown.listWidth = (int)width;
            dropDown.listHeight = 500;
            dropDown.itemHeight = itemHeight;
            dropDown.textScale = itemTextScale;

            // Create dropdown button.
            UIButton button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.size = dropDown.size;
            button.text = string.Empty;
            button.relativePosition = new Vector2(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.spritePadding = new RectOffset(3, 3, 3, 3);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;

            // Add tooltip.
            if (tooltip != null)
            {
                dropDown.tooltip = tooltip;
            }

            return dropDown;
        }

        /// <summary>
        /// Creates a plain dropdown using the game's option panel dropdown template.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative x position.</param>
        /// <param name="yPos">Relative y position.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <param name="items">Dropdown menu item list.</param>
        /// <param name="selectedIndex">Initially selected index (default 0).</param>
        /// <param name="width">Width of dropdown (default 270f).</param>
        /// <returns>New dropdown menu using game's option panel template.</returns>
        public static UIDropDown AddPlainDropDown(UIComponent parent, float xPos, float yPos, string text, string[] items, int selectedIndex = 0, float width = 270f)
        {
            UIPanel panel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject("OptionsDropdownTemplate")) as UIPanel;
            UIDropDown dropDown = panel.Find<UIDropDown>("Dropdown");

            // Set text.
            panel.Find<UILabel>("Label").text = text;

            // Slightly increase width.
            dropDown.autoSize = false;
            dropDown.width = width;

            // Add items.
            dropDown.items = items;
            dropDown.selectedIndex = selectedIndex;

            // Set position.
            dropDown.parent.relativePosition = new Vector2(xPos, yPos);

            return dropDown;
        }
    }
}