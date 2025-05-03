// <copyright file="UIScrollbars.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>
using ColossalFramework.UI;
using UnityEngine;

namespace TransferManagerCE
{
    /// <summary>
    /// UI scrollbars.
    /// </summary>
    public static class UIScrollbars
    {
        /// <summary>
        /// Creates a vertical scrollbar.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <returns>New vertical scrollbar.</returns>
        public static UIScrollbar AddScrollbar(UIComponent parent)
        {
            // Basic setup.
            UIScrollbar newScrollbar = parent.AddUIComponent<UIScrollbar>();
            newScrollbar.orientation = UIOrientation.Vertical;
            newScrollbar.pivot = UIPivotPoint.TopLeft;
            newScrollbar.minValue = 0;
            newScrollbar.value = 0;
            newScrollbar.incrementAmount = 50f;
            newScrollbar.autoHide = true;

            // Location and size.
            newScrollbar.width = 10f;

            // Tracking sprite.
            UISlicedSprite trackSprite = newScrollbar.AddUIComponent<UISlicedSprite>();
            trackSprite.relativePosition = Vector2.zero;
            trackSprite.autoSize = true;
            trackSprite.anchor = UIAnchorStyle.All;
            trackSprite.size = trackSprite.parent.size;
            trackSprite.fillDirection = UIFillDirection.Vertical;
            trackSprite.spriteName = "ScrollbarTrack";
            newScrollbar.trackObject = trackSprite;

            // Thumb sprite.
            UISlicedSprite thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width;
            thumbSprite.spriteName = "ScrollbarThumb";
            newScrollbar.thumbObject = thumbSprite;

            return newScrollbar;
        }

        /// <summary>
        /// Creates a vertical scrollbar linked to the specified scrollable panel.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="scrollPanel">Panel to scroll.</param>
        /// <returns>New vertical scrollbar linked to the specified scrollable panel, immediately to the right.</returns>
        public static UIScrollbar AddScrollbar(UIComponent parent, UIScrollablePanel scrollPanel)
        {
            // Create new scrollbar and set size and relative position.
            UIScrollbar newScrollbar = AddScrollbar(parent);
            newScrollbar.relativePosition = new Vector2(scrollPanel.relativePosition.x + scrollPanel.width, scrollPanel.relativePosition.y);
            newScrollbar.height = scrollPanel.height;

            // Event handler to handle resize of scroll panel.
            scrollPanel.eventSizeChanged += (c, newSize) =>
            {
                newScrollbar.relativePosition = new Vector2(scrollPanel.relativePosition.x + scrollPanel.width, scrollPanel.relativePosition.y);
                newScrollbar.height = scrollPanel.height;
            };

            // Attach to scroll panel.
            scrollPanel.verticalScrollbar = newScrollbar;

            return newScrollbar;
        }
    }
}