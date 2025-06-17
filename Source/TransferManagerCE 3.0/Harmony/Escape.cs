using HarmonyLib;
using TransferManagerCE.UI;

namespace TransferManagerCE.Patch
{
    /// <summary>
    /// Harmony patch to implement escape key handling.
    /// </summary>
    [HarmonyPatch]
    public static class EscapePatch
    {
        /// <summary>
        /// Harmony prefix patch to cancel the zoning tool when it's active and the escape key is pressed.
        /// </summary>
        /// <returns>True (continue on to game method) if the zoning tool isn't already active, false (pre-empt game method) otherwise</returns>
        [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
        [HarmonyPrefix]
        public static bool Prefix()
        {
            // Handle "Escape" in order of importance
            if (SelectionTool.Active && SelectionTool.Instance.GetCurrentMode() != SelectionTool.SelectionToolMode.Normal)
            {
                SelectionTool.Instance.SelectNormalTool();
                return false;
            }

            // Check panels
            if (DistrictSelectionPanel.IsVisible() && DistrictSelectionPanel.Instance.HandleEscape())
            {
                return false;
            }
            if (OutsideConnectionSelectionPanel.IsVisible() && OutsideConnectionSelectionPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (BuildingPanel.IsVisible() && BuildingPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (TransferIssuePanel.IsVisible() && TransferIssuePanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (StatsPanel.IsVisible() && StatsPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (SettingsPanel.IsVisible() && SettingsPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (OutsideConnectionPanel.IsVisible() && OutsideConnectionPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (PathDistancePanel.IsVisible() && PathDistancePanel.Instance.HandleEscape())
            {
                return false;
            }

            // Nothing was showing, pass on.
            return true;
        }
    }
}