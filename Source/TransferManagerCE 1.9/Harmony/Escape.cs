using HarmonyLib;

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
            if (DistrictPanel.Instance != null && DistrictPanel.Instance.isVisible)
            {
                DistrictPanel.Instance.Hide();
                return false;
            }
            else if (TransferIssueThread.HandleEscape())
            {
                return false;
            }
            else if(StatisticsThread.HandleEscape())
            {
                return false;
            }
            else if (BuildingPanelThread.HandleEscape())
            {
                return false;
            }
            else if (OutsideConnectionPanelThread.HandleEscape())
            {
                return false;
            }

            // Nothing was showing, pass on.
            return true;
        }
    }
}