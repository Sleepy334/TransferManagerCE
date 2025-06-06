using System.Collections.Generic;
using static TransferManagerCE.SelectionTool;
using TransferManagerCE.UI;
using UnityEngine;
using UnityEngine.Networking.Types;
using SleepyCommon;

namespace TransferManagerCE
{
    public class SelectionModeSelectCandidates : SelectionModeSelectBuildings
    {
        public SelectionModeSelectCandidates(SelectionTool tool) :
            base(tool)
        {
        }

        // ----------------------------------------------------------------------------------------
        public override void Enable()
        {
            base.Enable();
            m_buildings = PathDistancePanel.Instance.candidates;
            PathDistancePanel.Instance.InvalidatePanel();
        }

        public override void Disable()
        {
            base.Disable();

            if (PathDistancePanel.IsVisible())
            {
                PathDistancePanel.Instance.ShowInfo(string.Empty);
                PathDistancePanel.Instance.InvalidatePanel();
            }
        }

        public override void HandleLeftClick()
        {
            if (HoverInstance.Building != 0)
            {
                // Add or remove building
                if (m_buildings.Contains(HoverInstance.Building))
                {
                    m_buildings.Remove(HoverInstance.Building);
                }
                else
                {
                    m_buildings.Add(HoverInstance.Building);
                }

                // Update tab to reflect selected building
                PathDistancePanel.Instance.SetCandidates(m_buildings);
            }
        }

        protected override Color GetColor()
        {
            return PathDistanceRenderer.s_candidateColor;
        }

        public override string GetTooltipText2() 
        {
            string sText = string.Empty;

            sText += $"<color #FFFFFF>{Localization.Get("txtSelectBuildings")}</color>\n";
            sText += "\n";
            sText += $"<color #FFFFFF>{Localization.Get("txtCandidates")}: {m_buildings.Count}</color>\n";

            // Actual buildings
            sText += base.GetTooltipText2();

            return sText;
        }

        public override void OnToolLateUpdate()
        {
            if (PathDistancePanel.IsVisible())
            {
                PathDistancePanel.Instance.ShowInfo(GetTooltipText2());
            }
        }
    }
}
