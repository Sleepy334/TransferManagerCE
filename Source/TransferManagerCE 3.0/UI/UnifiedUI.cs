using SleepyCommon;
using System.Reflection;
using TransferManagerCore.Settings;
using TransferManagerCore.UI;
using UnifiedUI.Helpers;
using UnityEngine;

namespace TransferManagerCore
{
    public class UnifiedUIButton
    {
        private static UUICustomButton? s_button = null;

        // ----------------------------------------------------------------------------------------
        public static void Add()
        {
            if (s_button is null && ModSettings.GetSettings().AddUnifiedUIButton && DependencyUtils.IsUnifiedUIRunning())
            {
                Texture2D icon = TextureResources.LoadDllResource(Assembly.GetExecutingAssembly(), "Transfer.png", 32, 32);
                if (icon is null)
                {
                    Log.Info("Failed to load icon from resources");
                    return;
                }

                s_button = UUIHelpers.RegisterCustomButton("TransferManagerCE", null, TransferManagerMod.Instance.Name, icon, OnToggle, null, null);
            }
        }

        public static void Remove()
        {
            if (s_button is not null)
            {
                UUIHelpers.Destroy(s_button.Button);
                s_button = null;
            }
        }

        public static void OnToggle(bool bToggle)
        {
            BuildingPanel.TogglePanel();
        }
    }
}