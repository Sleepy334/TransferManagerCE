using ColossalFramework;
using TransferManagerCE.UI;
using static RenderManager;

namespace TransferManagerCE
{
    public class SelectionModeNormal : SelectionModeBase
    {
        private HighlightBuildings m_highlightBuildings = new HighlightBuildings();

        public SelectionModeNormal(SelectionTool tool) :
           base(tool)
        {
        }

        public override void Highlight(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            // Highlight matched buildings or buildings with issues depending on mode.
            m_highlightBuildings.Highlight(toolManager, BuildingManager.instance.m_buildings.m_buffer, cameraInfo);
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            switch (GetHoverInstance().Type)
            {
                case InstanceType.Building:
                    {
                        m_tool.SelectBuilding(GetHoverInstance().Building);
                        break;
                    }
                case InstanceType.NetNode:
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[GetHoverInstance().NetNode];
                        if (oNode.m_building != 0)
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[oNode.m_building];
                            if (building.Info?.GetAI() is OutsideConnectionAI)
                            {
                                m_tool.SelectBuilding(oNode.m_building);
                            }
                        }
                        break;
                    }
            }
        }

        public override void UpdateSelection()
        {
            if (BuildingPanel.Instance is not null && BuildingPanel.Instance.GetBuildingId() != 0)
            {
                m_highlightBuildings.LoadMatches();
            }
        }
    }
}
