using ColossalFramework;
using HarmonyLib;
using System.Collections.Generic;
using TransferManagerCE.UI;
using UnityEngine;

namespace TransferManagerCE
{
    [HarmonyPatch]
    public class DistrictSelectionPatches
    {
        public enum DistrictType
        {
            None,
            District,
            Park,
        }

        public static bool s_DistrictSelectionToolActive = false;
        public static HashSet<byte> s_highlightDistricts = new HashSet<byte>();
        public static HashSet<byte> s_highlightParks = new HashSet<byte>();
        private static DistrictType s_UpdateType = DistrictType.None;

        [HarmonyPatch(typeof(DistrictManager), "UpdateParkTexture")]
        [HarmonyPrefix]
        public static void UpdateParkTexturePrefix()
        {
            s_UpdateType = DistrictType.Park;
        }

        [HarmonyPatch(typeof(DistrictManager), "UpdateParkTexture")]
        [HarmonyPostfix]
        public static void UpdateParkTexturePostfix()
        {
            s_UpdateType = DistrictType.None;
        }

        [HarmonyPatch(typeof(DistrictManager), "UpdateDistrictTexture")]
        [HarmonyPrefix]
        public static void UpdateDistrictTexturePrefix()
        {
            s_UpdateType = DistrictType.District;
        }

        [HarmonyPatch(typeof(DistrictManager), "UpdateDistrictTexture")]
        [HarmonyPostfix]
        public static void UpdateDistrictTexturePostfix()
        {
            s_UpdateType = DistrictType.None;
        }

        // Performance critical, called thousands of times when rendering districts.
        [HarmonyPatch(typeof(DistrictManager), "AddDistrictColor2")]
        [HarmonyPostfix]
        public static void AddDistrictColor2Postfix(byte district, DistrictPolicies.Policies policy, byte alpha, bool inArea, ref Color32 color2)
        {
            if (s_DistrictSelectionToolActive && district > 0)
            {
                switch (s_UpdateType)
                {
                    case DistrictType.District:
                        {
                            if (s_highlightDistricts.Contains(district))
                            {
                                // Color the district
                                color2.a = (byte)Mathf.Max(color2.a, alpha);
                            }
                            break;
                        }
                    case DistrictType.Park:
                        {
                            if (s_highlightParks.Contains(district))
                            {
                                // Color the district
                                color2.a = (byte)Mathf.Max(color2.a, alpha);
                            }
                            break;
                        }
                }
            }
        }

        public static void UpdateDistricts()
        {
            s_highlightDistricts.Clear();
            s_highlightParks.Clear();

            DistrictSelectionPanel? panel = DistrictSelectionPanel.Instance;
            if (panel is not null && panel.isVisible)
            {
                ushort buildingId = panel.m_buildingId;
                int iRestrictionId = panel.m_iRestrictionId;

                BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
                if (settings is not null)
                {
                    RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                    if (restrictions is not null)
                    {
                        HashSet<DistrictData> allowedDistricts;
                        if (SelectionTool.Instance.m_mode == SelectionTool.SelectionToolMode.DistrictRestrictionIncoming)
                        {
                            allowedDistricts = restrictions.m_incomingDistrictSettings.GetAllowedDistricts(buildingId, null, null);
                        }
                        else
                        {
                            allowedDistricts = restrictions.m_outgoingDistrictSettings.GetAllowedDistricts(buildingId, null, null);
                        }

                        // Load into hash sets.
                        foreach (DistrictData district in allowedDistricts)
                        {
                            switch (district.m_eType)
                            {
                                case DistrictData.DistrictType.District:
                                    {
                                        s_highlightDistricts.Add((byte) district.m_iDistrictId);
                                        break;
                                    }
                                case DistrictData.DistrictType.Park:
                                    {
                                        s_highlightParks.Add((byte)district.m_iDistrictId);
                                        break;
                                    }
                            }
                        }
                    }
                }

                // Update overlay
                Singleton<DistrictManager>.instance.AreaModified(0, 0, 511, 511, fullUpdate: false);
                Singleton<DistrictManager>.instance.ParksAreaModified(0, 0, 511, 511, fullUpdate: false);
            }
        }
    }
}
