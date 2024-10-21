using ColossalFramework;
using TransferManagerCE.UI;
using UnityEngine;
using static DistrictPolicies;
using static RenderManager;
using static ToolBase;
using static TransferManagerCE.SelectionTool;

namespace TransferManagerCE
{
    public class SelectionModeDistrictRestrictions : SelectionModeBase
    {
        public SelectionModeDistrictRestrictions(SelectionTool tool) :
           base(tool)
        {
        }

        public override void Enable(SelectionToolMode mode) 
        {
            base.Enable(mode);
            Singleton<DistrictManager>.instance.DistrictsVisible = true;
            Singleton<DistrictManager>.instance.ParksVisible = true;
            DistrictSelectionPatches.s_DistrictSelectionToolActive = true;

            if (DistrictSelectionPanel.Instance is not null && !DistrictSelectionPanel.Instance.isVisible)
            {
                DistrictSelectionPanel.Instance.Show();
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

            if (DistrictSelectionPanel.Instance is not null && DistrictSelectionPanel.Instance.isVisible)
            {
                DistrictSelectionPanel.Instance.Hide();
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
                CheckDistrict(DistrictData.DistrictType.Park, (byte) DistrictManager.instance.HighlightPark);
            }
            else if (DistrictManager.instance.HighlightDistrict != -1)
            {
                CheckDistrict(DistrictData.DistrictType.District, (byte)DistrictManager.instance.HighlightDistrict);
            }
            else
            {
                byte parkId = Singleton<DistrictManager>.instance.GetPark(m_tool.GetMousePosition());
                if (parkId != 0)
                {
                    CheckDistrict(DistrictData.DistrictType.Park, parkId);
                }
                else
                {
                    byte districtId = Singleton<DistrictManager>.instance.GetDistrict(m_tool.GetMousePosition());
                    if (districtId != 0)
                    {
                        CheckDistrict(DistrictData.DistrictType.District, districtId);
                    }
                }
            }
        }

        private void CheckDistrict(DistrictData.DistrictType eType, byte district)
        {
            ushort buildingId = DistrictSelectionPanel.Instance.m_buildingId;
            int iRestrictionId = DistrictSelectionPanel.Instance.m_iRestrictionId;

            BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
            RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(iRestrictionId);

            bool bIncoming = SelectionTool.Instance.m_mode == SelectionToolMode.DistrictRestrictionIncoming;
            if (bIncoming)
            {
                restrictions.m_incomingDistrictSettings.ToggleDistrictAllowed(buildingId, eType, district);
            }
            else
            {
                restrictions.m_outgoingDistrictSettings.ToggleDistrictAllowed(buildingId, eType, district);
            }

            settings.SetRestrictions(iRestrictionId, restrictions);
            BuildingSettingsStorage.SetSettings(buildingId, settings);

            // Refresh overlay
            if (eType == DistrictData.DistrictType.District)
            {
                Singleton<DistrictManager>.instance.AreaModified(0, 0, 511, 511, fullUpdate: false);
            }
            else
            {
                Singleton<DistrictManager>.instance.ParksAreaModified(0, 0, 511, 511, fullUpdate: false);
            }

            DistrictSelectionPanel.Instance.UpdatePanel();
        }

        public override void OnToolUpdate()
        {
            m_tool.ShowToolInfo(Localization.Get("titleDistrictPanel"));
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
