using SleepyCommon;
using System.Collections.Generic;
using UnityEngine;
using static RenderManager;

namespace TransferManagerCE
{
    public abstract class SelectionModeSelectBuildings : SelectionModeBase
    {
        protected HashSet<ushort> m_buildings = new HashSet<ushort>();

        // ----------------------------------------------------------------------------------------
        public SelectionModeSelectBuildings(SelectionTool tool) : 
            base(tool) 
        {
        }

        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.All;
        }
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.All;

        public override void RenderOverlay(CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);

            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            // Now highlight buildings
            foreach (ushort buildingId in m_buildings)
            {
                RendererUtils.HighlightBuilding(BuildingBuffer, buildingId, cameraInfo, GetColor());
            }
        }

        protected abstract Color GetColor();

        public override string GetTooltipText()
        {
            return string.Empty;
        }

        public virtual string GetTooltipText2()
        {
            string sText = "";

            // Now describe buildings
            foreach (ushort buildingId in m_buildings)
            {
                sText += $"{CitiesUtils.GetBuildingName(buildingId, InstanceID.Empty, true)}\n";
            }

            return sText;
        }
    }
}
