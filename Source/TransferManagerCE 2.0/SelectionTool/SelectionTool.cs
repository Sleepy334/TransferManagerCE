using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using UnifiedUI.Helpers;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    internal class SelectionTool : DefaultTool
    {
        private UIComponent? m_button = null;
        public static SelectionTool? Instance = null;
        private static bool s_bLoadingTool = false;
#if DEBUG
        private UnconnectedGraph? m_unconnectedGraph = null;
        private Color[]? m_color = null;
#endif
        public static bool HasUnifiedUIButtonBeenAdded()
        {
            return (Instance != null && Instance.m_button != null);
        }

        public static void AddSelectionTool()
        {
            if (TransferManagerLoader.IsLoaded())
            {
                if (Instance == null)
                {
                    try
                    {
                        s_bLoadingTool = true;
                        Instance = ToolsModifierControl.toolController.gameObject.AddComponent<SelectionTool>();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Selection tool failed to load: ", e);
                    }
                    finally
                    {
                        s_bLoadingTool = false;
                    }
                }
            } 
            else
            {
                Debug.Log("Game not loaded");
            }
        }

        public static void RemoveUnifiedUITool()
        {
            if (Instance != null)
            {
                if (Instance.m_button != null)
                {
                    UUIHelpers.Destroy(Instance.m_button);
                }
                Instance.Destroy();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (DependencyUtilities.IsUnifiedUIRunning())
            {
                Texture2D? icon = TextureResources.LoadDllResource("Transfer.png", 32, 32);
                if (icon == null)
                {
                    Debug.Log("Failed to load icon from resources");
                    return;
                }

                m_button = UUIHelpers.RegisterToolButton(
                    name: "TransferManagerCE",
                    groupName: null,
                    tooltip: TransferManagerMain.Title,
                    tool: this,
                    icon: icon,
                    hotkeys: new UUIHotKeys { ActivationKey = ModSettings.SelectionToolHotkey });
            }
        }

        public static void Release() {
            Destroy(FindObjectOfType<SelectionTool>());
        }

        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.All;
        public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.All;
        public override District.Flags GetDistrictIgnoreFlags() => District.Flags.All;
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.None;
        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.All;
        public override TreeInstance.Flags GetTreeIgnoreFlags() => TreeInstance.Flags.All;
        public override PropInstance.Flags GetPropIgnoreFlags() => PropInstance.Flags.All;
        //public override Vehicle.Flags GetVehicleIgnoreFlags() => Vehicle.Flags.All;
        
        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.All;
        }

        public override void SimulationStep()
        {
            base.SimulationStep();

            if (RayCastSegmentAndNode(out var hoveredSegment, out var hoveredNode))
            {
                if (hoveredNode > 0)
                {
                    m_hoverInstance.NetNode = hoveredNode;
                }
            }
        }

        private static bool RayCastSegmentAndNode(out ushort netSegment, out ushort netNode)
        {
            if (RayCastSegmentAndNode(out var output))
            {
                netSegment = output.m_netSegment;
                netNode = output.m_netNode;
                return true;
            }

            netSegment = 0;
            netNode = 0;
            return false;
        }

        private static bool RayCastSegmentAndNode(out RaycastOutput output)
        {
            var input = new RaycastInput(Camera.main.ScreenPointToRay(Input.mousePosition), Camera.main.farClipPlane)
            {
                m_netService = { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels },
                m_ignoreSegmentFlags = NetSegment.Flags.None,
                m_ignoreNodeFlags = NetNode.Flags.None,
                m_ignoreTerrain = true,
            };

            return RayCast(input, out output);
        }

        protected override void OnDestroy() {
            if (m_button != null)
            {
                Destroy(m_button.gameObject);
                m_button = null;
            }

            base.OnDestroy();
        }

        public void Enable()
        {
            if (Instance != null && !Instance.enabled)
            {
                OnEnable();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();

            // We seem to get an eroneous OnEnable call when adding the tool.
            if (!s_bLoadingTool)
            {
                ToolsModifierControl.mainToolbar.CloseEverything();
                ToolsModifierControl.SetTool<SelectionTool>();
                BuildingPanel.Init();
                BuildingPanel.Instance?.ShowPanel();
            }
        }

        public void Disable()
        {
            ToolBase oCurrentTool = ToolsModifierControl.toolController.CurrentTool;
            if (oCurrentTool != null && oCurrentTool == Instance && oCurrentTool.enabled)
            {
                OnDisable();
            }
#if DEBUG
            m_color = null;
            m_unconnectedGraph = null;
#endif
        }

        protected override void OnDisable() {
            m_toolController ??= ToolsModifierControl.toolController; // workaround exception in base code.
            base.OnDisable();
            ToolsModifierControl.SetTool<DefaultTool>();
            BuildingPanel.Instance?.HidePanel();
        }

        public void ToogleSelectionTool()
        {
            if (isActiveAndEnabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }
        
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            ToolManager toolManager = Singleton<ToolManager>.instance;

            switch (m_hoverInstance.Type)
            {
                case InstanceType.Building:
                    {
                        base.RenderOverlay(cameraInfo);
                        break;
                    }
                case InstanceType.NetNode:
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode];
                        if (oNode.m_building != 0)
                        {
                            if (BuildingTypeHelper.IsOutsideConnection(oNode.m_building))
                            {
                                RenderManager.instance.OverlayEffect.DrawCircle(
                                    cameraInfo,
                                    GetToolColor(false, false),
                                    oNode.m_position,
                                    oNode.m_bounds.size.magnitude,
                                    oNode.m_position.y - 1f,
                                    oNode.m_position.y + 1f,
                                    true,
                                    true);
                                toolManager.m_drawCallData.m_overlayCalls++;
                            }
                        }
                        break;
                    }
            }

            // Highlight selected building and all matches
            if (BuildingPanel.Instance != null && BuildingPanel.Instance.m_buildingId != 0)
            {
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

                ushort usSourceBuildingId = BuildingPanel.Instance.m_buildingId;
                ref Building building = ref BuildingBuffer[usSourceBuildingId];
                if (building.m_flags != 0)
                {
                    HighlightBuilding(toolManager, BuildingBuffer, usSourceBuildingId, cameraInfo, Color.red);
                }

                if (ModSettings.GetSettings().HighlightMatches)
                {
                    // Limit the number of buildings to highlight
                    const int iMAX_BUILDINGS = 100;

                    List<BuildingMatchData>? listMatches = BuildingPanel.Instance.GetBuildingMatches().GetSortedBuildingMatches();
                    if (listMatches != null && listMatches.Count > 0)
                    {
                        // Use a hash set as we only want to highlight a building once.
                        HashSet<KeyValuePair<ushort, Color>> highlightBuildings = new HashSet<KeyValuePair<ushort, Color>>();
                        
                        int iCount = Math.Min(iMAX_BUILDINGS, listMatches.Count);
                        for (int i = 0; i < iCount; ++i)
                        {
                            BuildingMatchData matchData = listMatches[i];

                            // A match can now produce multiple buildings (Service point)
                            List<ushort> buildings;
                            if (matchData.m_outgoing.GetBuildings().Contains(usSourceBuildingId))
                            {
                                buildings = matchData.m_incoming.GetBuildings();
                            }
                            else
                            {
                                buildings = matchData.m_outgoing.GetBuildings();
                            }

                            foreach (ushort usBuildingId in buildings)
                            {
                                if (usBuildingId != 0 && usBuildingId != usSourceBuildingId)
                                {
                                    Color color = TransferManagerModes.GetTransferReasonColor(matchData.m_material);
                                    highlightBuildings.Add(new KeyValuePair<ushort, Color>(usBuildingId, color));
                                }
                            }
                        }

                        // Now highlight buildings
                        foreach (KeyValuePair<ushort, Color> kvp in highlightBuildings)
                        {
                            ref Building matchbuilding = ref BuildingBuffer[kvp.Key];
                            if (matchbuilding.m_flags != 0)
                            {
                                HighlightBuilding(toolManager, BuildingBuffer, kvp.Key, cameraInfo, kvp.Value);
                            }
                        }
                    }
                }
            }
#if DEBUG
            // DEBUGGING, Show node connection colors
            if (m_unconnectedGraph == null)
            {
                m_unconnectedGraph = new UnconnectedGraph();

                //NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(TransferReason.Dead);
                NetInfo.LaneType laneType = PathDistanceTypes.GetLaneTypes(TransferReason.Goods);
                m_unconnectedGraph.FloodFill(laneType);
            }
                
            if (m_color == null)
            {
                m_color = new Color[m_unconnectedGraph.Colors];
                for (int i = 0; i < m_color.Length; i++)
                {
                    m_color[i] = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                }
            }

            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
            foreach (KeyValuePair<ushort, int> kvp in m_unconnectedGraph.GetBuffer())
            {
                NetNode oNode = Nodes[kvp.Key];
                if (kvp.Value < m_color.Length)
                {
                    RenderManager.instance.OverlayEffect.DrawCircle(
                                cameraInfo,
                                m_color[kvp.Value - 1],
                                oNode.m_position,
                                oNode.m_bounds.size.magnitude,
                                oNode.m_position.y - 1f,
                                oNode.m_position.y + 1f,
                                true,
                                true);
                }
            } 
#endif
        }

        private void HighlightBuilding(ToolManager toolManager, Building[] BuildingBuffer, ushort usBuildingId, RenderManager.CameraInfo cameraInfo, Color color)
        {
            ref Building building = ref BuildingBuffer[usBuildingId];
            if (building.m_flags != 0)
            {
                // Highlight building
                BuildingTool.RenderOverlay(cameraInfo, ref building, color, color);
                
                // Also highlight any sub buildings
                float m_angle = building.m_angle * 57.29578f;
                BuildingInfo info3 = building.Info;
                if (info3.m_subBuildings != null && info3.m_subBuildings.Length != 0)
                {
                    Matrix4x4 matrix4x = default(Matrix4x4);
                    matrix4x.SetTRS(building.m_position, Quaternion.AngleAxis(m_angle, Vector3.down), Vector3.one);
                    for (int i = 0; i < info3.m_subBuildings.Length; i++)
                    {
                        BuildingInfo buildingInfo = info3.m_subBuildings[i].m_buildingInfo;
                        Vector3 position = matrix4x.MultiplyPoint(info3.m_subBuildings[i].m_position);
                        float angle = (info3.m_subBuildings[i].m_angle + m_angle) * ((float)Math.PI / 180f);
                        buildingInfo.m_buildingAI.RenderBuildOverlay(cameraInfo, color, position, angle, default(Segment3));
                        BuildingTool.RenderOverlay(cameraInfo, buildingInfo, 0, position, angle, color, radius: false);
                    }
                }
            }
        }

        protected override void OnToolGUI(Event e)
        {
            if (m_toolController.IsInsideUI || (e.type != EventType.MouseDown))
            {
                base.OnToolGUI(e);
                return;
            }

            if (m_hoverInstance.IsEmpty)
            {
                return;
            }

            if (m_hoverInstance.Building != 0 && Input.GetMouseButtonDown(0))
            {
                SelectBuilding(m_hoverInstance.Building);
            }
            else if (m_hoverInstance.Type == InstanceType.NetNode)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode];
                if (oNode.m_building != 0)
                {
                    Building building = BuildingManager.instance.m_buildings.m_buffer[oNode.m_building];
                    if (building.Info?.GetAI() is OutsideConnectionAI)
                    {
                        SelectBuilding(oNode.m_building);
                    }
                }
            }
        }

        private void SelectBuilding(ushort buildingId)
        {
            if (!s_bLoadingTool)
            {
                // Select building
                CitiesUtils.ShowBuilding(buildingId);

                // Open building panel
                BuildingPanel.Instance?.ShowPanel(buildingId);
            }
        }

        protected override bool CheckNode(ushort node, ref ToolErrors errors) => true;

        protected override bool CheckSegment(ushort segment, ref ToolErrors errors) => true;

        protected override bool CheckBuilding(ushort building, ref ToolErrors errors) => true;

        protected override bool CheckProp(ushort prop, ref ToolErrors errors) => true;

        protected override bool CheckTree(uint tree, ref ToolErrors errors) => true;

        protected override bool CheckVehicle(ushort vehicle, ref ToolErrors errors) => true;

        protected override bool CheckParkedVehicle(ushort parkedVehicle, ref ToolErrors errors) => true;

        protected override bool CheckCitizen(ushort citizenInstance, ref ToolErrors errors) => true;

        protected override bool CheckDisaster(ushort disaster, ref ToolErrors errors) => true;

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();
            if (UIView.library.Get("PauseMenu")?.isVisible == true)
            {
                UIView.library.Hide("PauseMenu");
                ToolsModifierControl.SetTool<DefaultTool>();
            }

            if (Input.GetMouseButtonDown(1))
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
    }
}