using ColossalFramework;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;
using static RenderManager;
using System;

namespace TransferManagerCE
{
    public class PathConnectionRenderer : SimulationManagerBase<PathConnectionRenderer, MonoBehaviour>, IRenderableManager
    {
        private Color[]? m_color = null;

        protected override void BeginOverlayImpl(CameraInfo cameraInfo)
        {
            base.BeginOverlayImpl(cameraInfo);
            HighlightNodes(cameraInfo);
        }

        private void HighlightNodes(CameraInfo cameraInfo)
        {
            int iShowConnection = ModSettings.GetSettings().ShowConnectionGraph;
            if (iShowConnection > 0)
            {
                // DEBUGGING, Show node connection colors
                ConnectedStorage? connectionNodes = null;
                switch (iShowConnection)
                {
                    case 1:
                        {
                            connectionNodes = PathConnectedCache.GetGoodsBufferCopy();

                            break;
                        }
                    case 2:
                        {
                            connectionNodes = PathConnectedCache.GetPedestrianZoneServicesBufferCopy();
                            break;
                        }
                    case 3:
                        {
                            connectionNodes = PathConnectedCache.GetOtherServicesBufferCopy();
                            break;
                        }
                }

                if (connectionNodes is not null)
                {
                    GenerateColorArray(connectionNodes.Colors);
                    if (m_color is not null)
                    {
                        ToolManager toolManager = Singleton<ToolManager>.instance;
                        NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
                        foreach (KeyValuePair<ushort, int> kvp in connectionNodes)
                        {
                            NetNode oNode = Nodes[kvp.Key];

                            // Color is 1 baseed.
                            int iColorIndex = kvp.Value - 1;
                            if (iColorIndex >= 0 && iColorIndex < m_color.Length)
                            {
                                HighlightNode(cameraInfo, oNode, m_color[iColorIndex]);
                            }
                        }
                    }
                }
            }
            else
            {
                m_color = null;
            }
        }

        private void GenerateColorArray(int iColors)
        {
            if (m_color is null || m_color.Length < iColors)
            {
                m_color = new Color[Math.Max(iColors, 9)];

                m_color[0] = Color.green;
                m_color[1] = Color.blue;
                m_color[2] = Color.red;
                m_color[3] = Color.cyan;
                m_color[4] = Color.yellow;
                m_color[5] = Color.magenta;
                m_color[6] = Color.grey;
                m_color[7] = Color.white;
                m_color[8] = Color.black;

                // Fill rest with random
                for (int i = 9; i < m_color.Length; i++)
                {
                    m_color[i] = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                }
            }
        }

        private static void HighlightNode(CameraInfo cameraInfo, NetNode oNode, Color color)
        {
            RenderManager.instance.OverlayEffect.DrawCircle(
                                            cameraInfo,
                                            color,
                                            oNode.m_position,
                                            oNode.m_bounds.size.magnitude,
                                            oNode.m_position.y - 1f,
                                            oNode.m_position.y + 1f,
                                            true,
                                            true);
        }
    }
}
