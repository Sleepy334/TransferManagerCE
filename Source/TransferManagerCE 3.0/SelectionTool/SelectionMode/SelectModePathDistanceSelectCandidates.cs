using System.Collections.Generic;
using static TransferManagerCE.SelectionTool;
using TransferManagerCE.UI;
using UnityEngine;
using UnityEngine.Networking.Types;
using SleepyCommon;
using static RenderManager;

namespace TransferManagerCE
{
    public class SelectionModeSelectCandidates : SelectionModeSelectBuildings
    {
        // ----------------------------------------------------------------------------------------
        public SelectionModeSelectCandidates(SelectionTool tool) :
            base(tool)
        {
        }

        // ----------------------------------------------------------------------------------------
        public override void RenderOverlay(CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);

            // Now highlight nodes as well
            NetNode[] NodeBuffer = NetManager.instance.m_nodes.m_buffer;

            CustomTransferReason.Reason material = NetworkModeHelper.GetTransferReason(PathDistancePanel.Instance.Algorithm);
            bool bStartActive = (PathDistancePanel.Instance.Direction == 0);

            foreach (ushort buildingId in m_buildings)
            {
                ushort nodeId = PathNode.FindBuildingNode(material, buildingId, bStartActive);
                if (nodeId != 0)
                {
                    RendererUtils.HighlightNode(cameraInfo, NodeBuffer[nodeId], KnownColor.magenta);
                }
            }
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
                SelectBuilding(HoverInstance.Building);

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

            CustomTransferReason.Reason material = NetworkModeHelper.GetTransferReason(PathDistancePanel.Instance.Algorithm);
            bool bStartActive = (PathDistancePanel.Instance.Direction == 0);

            // Now describe buildings
            foreach (ushort buildingId in m_buildings)
            {
                sText += $"{CitiesUtils.GetBuildingName(buildingId, true, true)} | Node: {PathNode.FindBuildingNode(material, buildingId, bStartActive)}\n";
            }

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
