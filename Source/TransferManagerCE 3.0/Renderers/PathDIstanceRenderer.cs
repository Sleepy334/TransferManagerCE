using SleepyCommon;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    public class PathDistanceRenderer : SimulationManagerBase<PathDistanceRenderer, MonoBehaviour>, IRenderableManager
    {
        public static KnownColor s_candidateColor = KnownColor.magenta;
        private static bool s_rendererRegistered = false;
        // ----------------------------------------------------------------------------------------
        public static void RegisterRenderer()
        {
            // Used to draw path connection graph, only add this once
            if (!s_rendererRegistered)
            {
                SimulationManager.RegisterManager(PathDistanceRenderer.instance);
                s_rendererRegistered = true;
            }
        }

        protected override void BeginOverlayImpl(CameraInfo cameraInfo)
        {
            base.BeginOverlayImpl(cameraInfo);

            if (TransferManagerMod.Instance.IsLoaded)
            {
                HighlightNodes(cameraInfo);
            }
        }

        private void HighlightNodes(RenderManager.CameraInfo cameraInfo)
        {
            // Do nothing if panel not visible
            if (PathDistancePanel.IsVisible())
            {
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

                // Highlight start building
                if (PathDistancePanel.Instance.m_buildingId != 0)
                {
                    RendererUtils.HighlightBuilding(BuildingBuffer, PathDistancePanel.Instance.m_buildingId, cameraInfo, Color.red);
                }

                // Highlight candidates
                if (PathDistancePanel.Instance.candidates.Count > 0)
                {
                    foreach (ushort candidate in PathDistancePanel.Instance.candidates)
                    {
                        RendererUtils.HighlightBuilding(BuildingBuffer, candidate, cameraInfo, s_candidateColor);
                    }
                }

                // Highlight examined nodes
                Dictionary<ushort, PathData> examinedNodes = PathDistancePanel.Instance.Test.GetExaminedNodes();
                if (examinedNodes.Count > 0)
                {
                    // Get a list of nodes that make up the path
                    HashSet<ushort> chosenPath = PathDistancePanel.Instance.Test.GetChosenPathNodes();

                    // Highlight nodes and color based on status
                    PathData[] pathData = examinedNodes.Values.ToArray();
                    for (int i = 0; i < pathData.Length; i++)
                    {
                        PathData nodeData = pathData[i];
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeData.nodeId];

                        Color color;
                        if (chosenPath.Contains(nodeData.nodeId))
                        {
                            color = Color.blue;
                        }
                        else if (nodeData.visited)
                        {
                            color = Color.green;
                        }
                        else
                        {
                            color = Color.yellow;
                        }

                        RendererUtils.HighlightNode(cameraInfo, oNode, color);
                    }
                }
            }
        }
    }
}
