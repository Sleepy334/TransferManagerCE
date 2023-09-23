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
            // Handle "Escape" in order of importance
            if (SelectionTool.Instance is not null && SelectionTool.Instance.m_mode != SelectionTool.SelectionToolMode.Normal)
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                return false;
            }
            if (DistrictSelectionPanel.Instance is not null && DistrictSelectionPanel.Instance.isVisible)
            {
                DistrictSelectionPanel.Instance.Hide();
                return false;
            }
            else if (TransferIssuePanel.Instance is not null && TransferIssuePanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (StatsPanel.Instance is not null && StatsPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (ToolsModifierControl.toolController.CurrentTool == SelectionTool.Instance) 
            {
                ToolsModifierControl.SetTool<DefaultTool>();
                return false;
            }
            else if (BuildingPanel.Instance is not null && BuildingPanel.Instance.HandleEscape())
            {
                return false;
            }
            else if (OutsideConnectionPanel.Instance is not null && OutsideConnectionPanel.Instance.HandleEscape())
            {
                return false;
            }

            // Nothing was showing, pass on.
            return true;
        }
    }
}