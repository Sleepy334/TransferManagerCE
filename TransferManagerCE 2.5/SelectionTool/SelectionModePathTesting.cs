using System.Linq;
using UnityEngine;
using static ToolBase;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    public class SelectionModePathTesting : SelectionModeNormalDebug
    {
        private HighlightBuildings m_highlightBuildings = new HighlightBuildings();

        public SelectionModePathTesting(SelectionTool tool) :
           base(tool)
        {
        }

        public override void Highlight(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            base.Highlight(toolManager, cameraInfo);

            HighlightPathDistanceNodes(cameraInfo);
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();
        }

        public override void OnToolGUI(Event e)
        {
            base.OnToolGUI(e);

            DisplayPathDistanceInformation();
        }

        private void HighlightPathDistanceNodes(RenderManager.CameraInfo cameraInfo)
        {
            foreach (QueueData nodeData in PathDistanceTest.s_nodesExamined)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeData.Node()];

                Color color;
                if (nodeData.Node() == PathDistanceTest.s_nodesExamined.Last<QueueData>().Node())
                {
                    color = Color.blue;
                }
                else
                {
                    color = Color.green;
                }
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

        private void DisplayPathDistanceInformation()
        {
            if (GetHoverInstance().NetNode != 0)
            {
                bool bFound = false;
                foreach (QueueData nodeData in PathDistanceTest.s_nodesExamined)
                {
                    if (nodeData.Node() == GetHoverInstance().NetNode)
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeData.Node()];
                        var text = $"Node {nodeData.Node()}\nTravelTime: {nodeData.TravelTime().ToString("F")}\nHeuristic:{nodeData.Heuristic().ToString("F")}\nPriority: {nodeData.Priority}";
                        var screenPoint = SelectionTool.MousePosition;
                        screenPoint.y = screenPoint.y - 40f;
                        var color = GUI.color;
                        GUI.color = Color.white;
                        DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
                        GUI.color = color;
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    var text = $"Node {GetHoverInstance().NetNode} not found";
                    var screenPoint = SelectionTool.MousePosition;
                    screenPoint.y = screenPoint.y - 40f;
                    var color = GUI.color;
                    GUI.color = Color.white;
                    DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
                    GUI.color = color;
                }
            }
        }
    }
}
