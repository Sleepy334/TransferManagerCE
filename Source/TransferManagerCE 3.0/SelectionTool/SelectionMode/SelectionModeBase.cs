using ColossalFramework;
using System;
using UnityEngine;
using TransferManagerCE.UI;
using static RenderManager;
using static ToolBase;
using System.Collections.Generic;
using TransferManagerCE.TransferRules;
using static TransferManagerCE.BuildingTypeHelper;
using SleepyCommon;
using System.Data;
using TransferManagerCE.CustomManager;
using System.Linq;

namespace TransferManagerCE
{
    public abstract class SelectionModeBase
    {
        protected SelectionTool m_tool;

        // ----------------------------------------------------------------------------------------
        public abstract NetNode.Flags GetNodeIgnoreFlags();
        public abstract NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly);
        public abstract Building.Flags GetBuildingIgnoreFlags();
        public abstract TransportLine.Flags GetTransportIgnoreFlags();

        // ----------------------------------------------------------------------------------------
        public SelectionModeBase(SelectionTool tool)
        {
            m_tool = tool;
        }

        public SelectionTool Tool
        {
            get
            {
                return m_tool;
            }
        }

        public ToolController ToolController
        {
            get
            {
                return m_tool.GetToolController();
            }
        }

        protected InstanceID HoverInstance
        {
            get
            {
                return Tool.GetHoverInstance();
            }
        }

        protected ushort NodeId
        {
            get
            {
                return Tool.m_nodeId;
            }
        }

        protected ushort SegmentId
        {
            get
            {
                return Tool.m_segmentId;
            }
        }
        

        public virtual void Enable() { }
        public virtual void Disable() { }

        public virtual void RenderOverlay(CameraInfo cameraInfo)
        {
            HighlightSelectedBuilding(Singleton<ToolManager>.instance, cameraInfo);
        }

        public virtual bool RenderBuildingSelection()
        {
            return true;
        }

        public virtual RaycastInput GetRayCastInput(Ray ray, float rayCastLength)
        {
            RaycastInput input = new RaycastInput(ray, rayCastLength);
            input.m_buildingService = input.m_netService;
            input.m_propService = input.m_netService;
            input.m_treeService = input.m_netService;
            input.m_districtNameOnly = Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Districts;
            input.m_ignoreNodeFlags = GetNodeIgnoreFlags();
            input.m_ignoreSegmentFlags = GetSegmentIgnoreFlags(out input.m_segmentNameOnly);
            input.m_ignoreBuildingFlags = GetBuildingIgnoreFlags();
            input.m_ignoreTransportFlags = GetTransportIgnoreFlags();
            return input;
        }

        public abstract string GetTooltipText();

        public virtual void OnToolGUI(Event e)
        {

        }

        public virtual void OnToolLateUpdate()
        {

        }

        public virtual void UpdateSelection()
        {
        }

        public virtual void HandleLeftClick() { }

        public virtual void OnSelectBuilding(ushort buildingId) { }

        protected void HighlightSelectedBuilding(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            // Highlight selected building
            if (BuildingPanel.IsVisible() && BuildingPanel.Instance.Building != 0)
            {
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
                Vehicle[] VehicleBuffer = VehicleManager.instance.m_vehicles.m_buffer;

                ushort usSourceBuildingId = BuildingPanel.Instance.Building;
                if (usSourceBuildingId != 0)
                {
                    Building building = BuildingBuffer[usSourceBuildingId];
                    if (building.m_flags != 0)
                    {
                        // Highlight accessSegment first so it is underneath
                        if (building.m_accessSegment != 0)
                        {
                            ref NetSegment segment = ref NetManager.instance.m_segments.m_buffer[building.m_accessSegment];
                            if (segment.m_flags != 0)
                            {
                                NetTool.RenderOverlay(cameraInfo, ref segment, KnownColor.grey, KnownColor.grey);
                            }
                        }

                        BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                        HashSet<CustomTransferReason.Reason> localReasons = new HashSet<CustomTransferReason.Reason>();

                        // Highlight currently selected building
                        if (BuildingSettingsStorage.HasSettings(usSourceBuildingId))
                        {
                            RendererUtils.HighlightBuilding(BuildingBuffer, usSourceBuildingId, cameraInfo, Color.red);

                            // If building has distance restriction then draw the distance as a circle
                            DrawLocalDistanceCircle(cameraInfo, eType, usSourceBuildingId, building, ref localReasons);
                        }
                        else
                        {
                            RendererUtils.HighlightBuilding(BuildingBuffer, usSourceBuildingId, cameraInfo, Color.white);
                        }

                        // Draw global distance setting if any
                        DrawGlobalDistanceCircle(cameraInfo, eType, usSourceBuildingId, building, localReasons);

                        // Highlight own vehicles
                        BuildingUtils.EnumerateOwnVehicles(building, (vehicleId, vehicle) =>
                        {
                            RendererUtils.HighlightVehicle(VehicleBuffer, cameraInfo, vehicleId, Color.magenta);
                            return true;
                        });

                        // Highlight guest vehicles
                        BuildingUtils.EnumerateGuestVehicles(building, (vehicleId, vehicle) =>
                        {
                            RendererUtils.HighlightVehicle(VehicleBuffer, cameraInfo, vehicleId, Color.green);
                            return true;
                        });
                    }
                }
            }
        }

        public void DrawLocalDistanceCircle(CameraInfo cameraInfo, BuildingType eType, ushort buildingId, Building building, ref HashSet<CustomTransferReason.Reason> localReasons)
        {
            if (BuildingSettingsStorage.HasSettings(buildingId) && BuildingPanel.Instance.IsSettingsTabActive())
            {
                BuildingSettings settings = BuildingSettingsStorage.GetSettings(buildingId);
                if (settings is not null)
                {
                    int restrictionId = BuildingPanel.Instance.GetRestrictionId();
                    if (restrictionId != -1)
                    {
                        ReasonRule rule = BuildingRuleSets.GetRule(eType, buildingId, restrictionId);

                        // Check distance is allowed for this rule
                        if ((rule.m_incomingDistance || rule.m_outgoingDistance) && rule.m_reasons.Count > 0)
                        {
                            // See if this rule has non-zero distance setting
                            RestrictionSettings? restrictions = settings.GetRestrictions(rule.m_id);

                            if (restrictions is not null)
                            {
                                if (rule.m_incomingDistance && restrictions.m_incomingServiceDistanceMeters > 0)
                                {
                                    // Add reasons for restriction
                                    localReasons.UnionWith(rule.m_reasons);

                                    RenderManager.instance.OverlayEffect.DrawCircle(
                                            cameraInfo,
                                            GetKnownColor(true),
                                            building.m_position,
                                            restrictions.m_incomingServiceDistanceMeters * 2.0f, // Range of service matching, we need diameter not radius
                                            building.m_position.y - 1f,
                                            building.m_position.y + 1f,
                                            true,
                                            true);
                                }

                                if (rule.m_outgoingDistance && restrictions.m_outgoingServiceDistanceMeters > 0)
                                {
                                    // Add reasons for restriction
                                    localReasons.UnionWith(rule.m_reasons);

                                    RenderManager.instance.OverlayEffect.DrawCircle(
                                            cameraInfo,
                                            GetKnownColor(false),
                                            building.m_position,
                                            restrictions.m_outgoingServiceDistanceMeters * 2.0f, // Range of service matching, we need diameter not radius
                                            building.m_position.y - 1f,
                                            building.m_position.y + 1f,
                                            true,
                                            true);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DrawGlobalDistanceCircle(CameraInfo cameraInfo, BuildingType eType, ushort buildingId, Building building, HashSet<CustomTransferReason.Reason> localReasons)
        {
            // Draw global distance setting if any
            if (SaveGameSettings.GetSettings().GetDistanceRestrictionCount() > 0 && BuildingPanel.Instance.IsSettingsTabActive())
            {
                int restrictionId = BuildingPanel.Instance.GetRestrictionId();
                if (restrictionId != -1)
                {
                    ReasonRule rule = BuildingRuleSets.GetRule(eType, buildingId, restrictionId);

                    // Try and determine current reason
                    CustomTransferReason.Reason reason = BuildingPanel.Instance.GetSettingsTab().GetCurrentGlobalDistanceReason(eType, buildingId, rule);
                    if (reason != CustomTransferReason.Reason.None && TransferManagerModes.IsGlobalDistanceRestrictionsSupported(reason))
                    {
                        double dDistance = SaveGameSettings.GetSettings().GetActiveDistanceRestrictionSquaredMeters(reason);
                        if (dDistance > 0.0)
                        {
                            RenderManager.instance.OverlayEffect.DrawCircle(
                                    cameraInfo,
                                    Color.yellow,
                                    building.m_position,
                                    (float)Math.Sqrt(dDistance) * 2.0f, // Range of service matching, we need diameter not radius
                                    building.m_position.y - 1f,
                                    building.m_position.y + 1f,
                                    true,
                                    true);
                        }
                    }
                }
            }
        }

        public static KnownColor GetKnownColor(bool bIncoming)
        {
            if (bIncoming)
            {
                return KnownColor.green;
            }
            else
            {
                return KnownColor.blue;
            }
                
        }
    }
}
