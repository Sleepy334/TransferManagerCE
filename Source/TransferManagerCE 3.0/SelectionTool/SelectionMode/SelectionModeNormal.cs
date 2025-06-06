using ColossalFramework;
using SleepyCommon;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    public class SelectionModeNormal : SelectionModeBase
    {
        private HighlightBuildings m_highlightBuildings = new HighlightBuildings();

        // ----------------------------------------------------------------------------------------
        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.All;
        }
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.All;

        // ----------------------------------------------------------------------------------------
        public SelectionModeNormal(SelectionTool tool) :
           base(tool)
        {
        }

        public override void RenderOverlay(CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);

            // Highlight matched buildings or buildings with issues depending on mode.
            m_highlightBuildings.Highlight(Singleton<ToolManager>.instance, BuildingManager.instance.m_buildings.m_buffer, cameraInfo);
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            switch (HoverInstance.Type)
            {
                case InstanceType.Building:
                    {
                        m_tool.OnSelectBuilding(HoverInstance.Building);
                        break;
                    }
                case InstanceType.NetNode:
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[HoverInstance.NetNode];
                        if (oNode.m_building != 0)
                        {
                            Building building = BuildingManager.instance.m_buildings.m_buffer[oNode.m_building];
                            if (building.Info?.GetAI() is OutsideConnectionAI)
                            {
                                m_tool.OnSelectBuilding(oNode.m_building);
                            }
                        }
                        break;
                    }
            }
        }

        public override void OnSelectBuilding(ushort buildingId)
        {
            // Open building panel
            BuildingPanel.Instance.ShowPanel(buildingId);
        }

        public override void UpdateSelection()
        {
            if (BuildingPanel.IsVisible() && BuildingPanel.Instance.GetBuildingId() != 0)
            {
                m_highlightBuildings.LoadMatches();
            }
        }

        public override string GetTooltipText()
        {
            string sTooltip = "";

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (HoverInstance.Index != 0)
                {
                    sTooltip += InstanceHelper.DescribeInstance(HoverInstance, InstanceID.Empty, true);

                    if (HoverInstance.Building != 0)
                    {
                        Building building = BuildingManager.instance.m_buildings.m_buffer[HoverInstance.Building];
                        sTooltip += $"\nType: {BuildingTypeHelper.GetBuildingType(building)}";
                        if (building.Info is not null)
                        {
                            sTooltip += $"\nService: {building.Info.GetService()}";
                            sTooltip += $"\nAI: {building.Info.GetAI()}";
                        }
                        
                    }
                }
            }

            return sTooltip;
        }
    }
}
