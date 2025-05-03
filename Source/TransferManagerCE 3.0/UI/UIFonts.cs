// <copyright file="UIFonts.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.UI
{
    using System.Linq;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Font handling.
    /// </summary>
    public static class UIFonts
    {
        // Reference caches.
        private static UIFont s_regular;
        private static UIFont s_semiBold;

        /// <summary>
        /// Gets the regular sans-serif font.
        /// </summary>
        public static UIFont Regular
        {
            get
            {
                if (s_regular == null)
                {
                    s_regular = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Regular");
                }

                return s_regular;
            }
        }

        /// <summary>
        /// Gets the semi-bold sans-serif font.
        /// </summary>
        public static UIFont SemiBold
        {
            get
            {
                if (s_semiBold == null)
                {
                    s_semiBold = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");
                }

                return s_semiBold;
            }
        }
    }
}