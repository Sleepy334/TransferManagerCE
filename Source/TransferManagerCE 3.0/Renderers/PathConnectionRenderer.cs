using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;
using static RenderManager;
using System;
using TransferManagerCE.UI;
using static TransferManagerCE.NetworkModeHelper;

namespace TransferManagerCE
{
    public class PathConnectionRenderer : SimulationManagerBase<PathConnectionRenderer, MonoBehaviour>, IRenderableManager
    {
        private static bool s_rendererRegistered = false;
        private Color[]? m_color = null;
        
        public static void RegisterRenderer()
        {
            // Used to draw path connection graph, only add this once
            if (!s_rendererRegistered)
            {
                SimulationManager.RegisterManager(PathConnectionRenderer.instance);
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

        private void HighlightNodes(CameraInfo cameraInfo)
        {
            // Do nothing if panel not visible
            if (!PathDistancePanel.IsVisible())
            {
                return;
            }

            if (PathDistancePanel.Instance.ShowConnectionGraph && PathDistancePanel.Instance.Algorithm != NetworkModeHelper.NetworkMode.None)
            {
                // Show node connection colors
                ConnectedStorage? connectionNodes = null;
                switch (PathDistancePanel.Instance.Algorithm)
                {
                    case NetworkMode.Goods:
                        {
                            connectionNodes = PathConnectedCache.GetGoodsBufferCopy();

                            break;
                        }
                    case NetworkMode.PedestrianZone:
                        {
                            connectionNodes = PathConnectedCache.GetPedestrianZoneServicesBufferCopy();
                            break;
                        }
                    case NetworkMode.OtherServices:
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
                                RendererUtils.HighlightNode(cameraInfo, oNode, m_color[iColorIndex]);
                            }
                        }
                    }
                }
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
    }
}
