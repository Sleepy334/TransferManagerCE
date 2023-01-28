using HarmonyLib;
using TransferManagerCE.UI;

namespace TransferManagerCE.Patch
{
    /// <summary>
    /// Harmony patch to implement escape key handling.
    /// </summary>
    [HarmonyPatch(typeof(GameKeyShortcuts), "Escape")]
    public static class EscapePatch
    {
        /// <summary>
        /// Harmony prefix patch to cancel the zoning tool when it's active and the escape key is pressed.
        /// </summary>
        /// <returns>True (continue on to game method) if the zoning tool isn't already active, false (pre-empt game method) otherwise</returns>
        public static bool Prefix()
        {
            // Is a panel showing
            if (SelectionTool.Instance != null && SelectionTool.Instance.m_mode != SelectionTool.SelectionToolMode.Normal)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                return false;
            }
            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.Hide();
                return false;
            }
            else if (TransferIssueThreadExtension.HandleEscape())
            {
                return false;
            }
            else if(StatisticsThreadExtension.HandleEscape())
            {
                return false;
            }
            else if (BuildingPanelThreadExtension.HandleEscape())
            {
                return false;
            }
            else if (OutsideConnectionPanelThreadExtension.HandleEscape())
            {
                return false;
            }

            // Nothing was showing, pass on.
            return true;
        }
    }
}