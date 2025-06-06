using ColossalFramework;
using SleepyCommon;
using TransferManagerCE.UI;
using UnityEngine;
using static RenderManager;
using static ToolBase;

namespace TransferManagerCE
{
    public class SelectionModeDistrictRestrictions : SelectionModeBase
    {
        public SelectionModeDistrictRestrictions(SelectionTool tool) :
           base(tool)
        {
        }

        // ----------------------------------------------------------------------------------------
        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.All;
        }
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.All;
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.All;

        // ----------------------------------------------------------------------------------------
        public override void Enable() 
        {
            base.Enable();

            Singleton<DistrictManager>.instance.DistrictsVisible = true;
            Singleton<DistrictManager>.instance.ParksVisible = true;
            DistrictSelectionPatches.s_DistrictSelectionToolActive = true;

            if (DistrictSelectionPanel.IsVisible())
            {
                DistrictSelectionPanel.Instance.Show();
            }

            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.InvalidatePanel();
            }
        }

        public override void Disable() 
        {
            base.Disable();

            // Turn off highlighting.
            Singleton<DistrictManager>.instance.DistrictsVisible = false;
            Singleton<DistrictManager>.instance.ParksVisible = false;
            DistrictSelectionPatches.s_DistrictSelectionToolActive = false;

            // Force full update
            Singleton<DistrictManager>.instance.AreaModified(0, 0, 511, 511, fullUpdate: true);
            Singleton<DistrictManager>.instance.ParksAreaModified(0, 0, 511, 511, fullUpdate: true);

            if (DistrictSelectionPanel.IsVisible())
            {
                DistrictSelectionPanel.Instance.Hide();
            }

            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.InvalidatePanel();
            }
        }

        public override RaycastInput GetRayCastInput(Ray ray, float rayCastLength)
        {
            // We change the ray cast input so we can highlight nodes and segments.
            // But it makes selecting buildings harder when the segments overlap the building.
            RaycastInput input = new RaycastInput(ray, rayCastLength);
            input.m_netService = new RaycastService(ItemClass.Service.Road, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            return input;
        }

        public override void RenderOverlay(CameraInfo cameraInfo)
        {
            // Do nothing, rendering is handled by DistrictManager patch.
        }

        public override bool RenderBuildingSelection()
        {
            return false;
        }

        public override void HandleLeftClick()
        {
            base.HandleLeftClick();

            // Check park first
            if (DistrictManager.instance.HighlightPark != -1)
            {
                ToggleDistrict(DistrictData.DistrictType.Park, (byte) DistrictManager.instance.HighlightPark);
            }
            else if (DistrictManager.instance.HighlightDistrict != -1)
            {
                ToggleDistrict(DistrictData.DistrictType.District, (byte)DistrictManager.instance.HighlightDistrict);
            }
            else
            {
                byte parkId = Singleton<DistrictManager>.instance.GetPark(m_tool.GetMousePosition());
                if (parkId != 0)
                {
                    ToggleDistrict(DistrictData.DistrictType.Park, parkId);
                }
                else
                {
                    byte districtId = Singleton<DistrictManager>.instance.GetDistrict(m_tool.GetMousePosition());
                    if (districtId != 0)
                    {
                        ToggleDistrict(DistrictData.DistrictType.District, districtId);
                    }
                }
            }
        }

        private void ToggleDistrict(DistrictData.DistrictType eType, byte district)
        {
            DistrictSelectionPanel.Instance.ToggleDistrictAllowed(eType, district);
            
            // Refresh overlay
            if (eType == DistrictData.DistrictType.District)
            {
                Singleton<DistrictManager>.instance.AreaModified(0, 0, 511, 511, fullUpdate: false);
            }
            else
            {
                Singleton<DistrictManager>.instance.ParksAreaModified(0, 0, 511, 511, fullUpdate: false);
            }
        }

        public override string GetTooltipText()
        {
            return Localization.Get("titleDistrictPanel");
        }

        public override void OnToolLateUpdate()
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;

            // Highlight the district/park under the mouse
            if (ToolController.IsInsideUI || !Cursor.visible)
            {
                instance.HighlightDistrict = -1;
                instance.HighlightPark = -1;
            }
            else
            {
                // We highlight parks in favour of districts
                byte park = instance.SamplePark(Tool.GetMousePosition());
                if (park != 0)
                {
                    instance.HighlightPark = park;
                }
                else
                {
                    instance.HighlightPark = -1;

                    byte district = instance.SampleDistrict(Tool.GetMousePosition());
                    if (district != 0)
                    {
                        instance.HighlightDistrict = district;
                    }
                    else
                    {
                        instance.HighlightDistrict = -1;
                    }
                }
            }
        }
    }
}
