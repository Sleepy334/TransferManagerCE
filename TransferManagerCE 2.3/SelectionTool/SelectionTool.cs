using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using TransferManagerCE.Common;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Settings;
using UnifiedUI.Helpers;
using UnityEngine;
using static TransferManagerCE.PathQueue;

namespace TransferManagerCE
{
    internal class SelectionTool : DefaultTool
    {
        public enum SelectionToolMode
        {
            Normal,
            BuildingRestrictionIncoming,
            BuildingRestrictionOutgoing,
        }
        
        public static SelectionTool? Instance = null;
        public SelectionToolMode m_mode = SelectionToolMode.Normal;

        private static bool s_bLoadingTool = false;
        private UIComponent? m_button = null;
        private Color[]? m_color = null;
        private bool m_processedClick = false;
        private HighlightBuildings m_highlightBuildings = new HighlightBuildings();

        public static bool HasUnifiedUIButtonBeenAdded()
        {
            return (Instance is not null && Instance.m_button is not null);
        }

        public static void AddSelectionTool()
        {
            if (TransferManagerLoader.IsLoaded())
            {
                if (Instance is null)
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
            if (Instance is not null)
            {
                if (Instance.m_button is not null)
                {
                    UUIHelpers.Destroy(Instance.m_button);
                    Instance.m_button = null;
                }
                Instance.Destroy();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (DependencyUtils.IsUnifiedUIRunning())
            {
                Texture2D? icon = TextureResources.LoadDllResource("Transfer.png", 32, 32);
                if (icon is null)
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
                    hotkeys: new UUIHotKeys { ActivationKey = ModSettings.GetSettings().SelectionToolHotkey });
            }
        }

        public static void Release() 
        {
            Destroy(FindObjectOfType<SelectionTool>());
        }

        public void SetMode(SelectionToolMode mode)
        {
            m_mode = mode;

            // Update the building panel to the changed state
            if (BuildingPanel.Instance is not null && BuildingPanel.Instance.isVisible)
            {
                BuildingPanel.Instance.UpdateTabs();
            }
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

        public void Enable()
        {
            if (Instance is not null && !Instance.enabled)
            {
                OnEnable();
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            
            // Ensure we are in normal mode.
            m_mode = SelectionToolMode.Normal;

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
            // Ensure we are in normal mode.
            m_mode = SelectionToolMode.Normal;

            ToolBase oCurrentTool = ToolsModifierControl.toolController.CurrentTool;
            if (oCurrentTool is not null && oCurrentTool == Instance && oCurrentTool.enabled)
            {
                OnDisable();
            }

            m_color = null;
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

            HighlightSelectedBuildingAndMatches(toolManager, cameraInfo);
            HighlightNodes(cameraInfo);
#if DEBUG
            HighlightPathDistanceNodes(cameraInfo);
#endif
        }

        private void HighlightSelectedBuildingAndMatches(ToolManager toolManager, RenderManager.CameraInfo cameraInfo)
        {
            // Highlight selected building and all matches
            if (BuildingPanel.Instance is not null && BuildingPanel.Instance.GetBuildingId() != 0)
            {
                Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

                ushort usSourceBuildingId = BuildingPanel.Instance.GetBuildingId();
                Building building = BuildingBuffer[usSourceBuildingId];
                if (building.m_flags != 0)
                {
                    // Highlight currently selected building
                    HighlightBuilding(toolManager, BuildingBuffer, usSourceBuildingId, cameraInfo, Color.red);

                    if (m_mode == SelectionToolMode.Normal)
                    {
                        // Now highlight buildings
                        m_highlightBuildings.Highlight(toolManager, BuildingBuffer, cameraInfo);
                    }
                    else
                    {
                        // Building restriction mode.
                        int restrictionId = BuildingPanel.Instance.GetRestrictionId();
                        if (restrictionId != -1)
                        {
                            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(usSourceBuildingId);
                            if (settings is not null)
                            {
                                RestrictionSettings? restrictions = settings.GetRestrictions(restrictionId);
                                if (restrictions is not null)
                                {
                                    // Select appropriate building restrictions
                                    HashSet<ushort> buildingRestrictions;
                                    if (m_mode == SelectionToolMode.BuildingRestrictionIncoming)
                                    {
                                        buildingRestrictions = restrictions.GetIncomingBuildingRestrictionsCopy();
                                    }
                                    else
                                    {
                                        buildingRestrictions = restrictions.GetOutgoingBuildingRestrictionsCopy();
                                    }

                                    // Now highlight buildings
                                    foreach (ushort buildingId in buildingRestrictions)
                                    {
                                        HighlightBuilding(toolManager, BuildingBuffer, buildingId, cameraInfo, Color.green);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

#if DEBUG
        private void HighlightPathDistanceNodes(RenderManager.CameraInfo cameraInfo)
        {
            if (PathDistanceTest.PATH_TESTING_ENABLED)
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
        }
#endif

        private void HighlightNodes(RenderManager.CameraInfo cameraInfo)
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
                        NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;
                        foreach (KeyValuePair<ushort, int> kvp in connectionNodes)
                        {
                            NetNode oNode = Nodes[kvp.Key];

                            // Color is 1 baseed.
                            int iColorIndex = kvp.Value - 1;
                            if (iColorIndex >= 0 && iColorIndex < m_color.Length)
                            {
                                RenderManager.instance.OverlayEffect.DrawCircle(
                                            cameraInfo,
                                            m_color[iColorIndex],
                                            oNode.m_position,
                                            oNode.m_bounds.size.magnitude,
                                            oNode.m_position.y - 1f,
                                            oNode.m_position.y + 1f,
                                            true,
                                            true);
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

        public void UpdateSelection()
        {
            if (BuildingPanel.Instance is not null && BuildingPanel.Instance.GetBuildingId() != 0)
            {
                m_highlightBuildings.LoadMatches();
            }
        }

        public static void HighlightBuilding(ToolManager toolManager, Building[] BuildingBuffer, ushort usBuildingId, RenderManager.CameraInfo cameraInfo, Color color)
        {
            ref Building building = ref BuildingBuffer[usBuildingId];
            if (building.m_flags != 0)
            {
                // Highlight building
                BuildingTool.RenderOverlay(cameraInfo, ref building, color, color);
                
                // Also highlight any sub buildings
                float m_angle = building.m_angle * 57.29578f;
                BuildingInfo info3 = building.Info;
                if (info3.m_subBuildings is not null && info3.m_subBuildings.Length != 0)
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
            if (m_mode != SelectionToolMode.Normal)
            {
                DrawLabel();  
            } 
            else if (PathDistanceTest.PATH_TESTING_ENABLED)
            {
#if DEBUG
                DisplayPathDistanceInformation();
#endif
            }

            if (m_toolController.IsInsideUI)
            {
                base.OnToolGUI(e);
                return;
            }

            if (e.type == EventType.MouseDown && Input.GetMouseButtonDown(0))
            {
                // cancel if the key input was already processed in a previous frame
                if (!m_processedClick)
                {
                    HandleLeftClick();
                    m_processedClick = true;
                }
            }
            else
            {
                m_processedClick = false;
            }
        }

#if DEBUG
        private void DisplayPathDistanceInformation()
        {
            if (m_hoverInstance.NetNode != 0)
            {
                bool bFound = false;
                foreach (QueueData nodeData in PathDistanceTest.s_nodesExamined)
                {
                    if (nodeData.Node() == m_hoverInstance.NetNode)
                    {
                        NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeData.Node()];
                        var text = $"Node {nodeData.Node()}\nTravelTime: {nodeData.TravelTime().ToString("F")}\nHeuristic:{nodeData.Heuristic().ToString("F")}\nPriority: {nodeData.Priority}";
                        var screenPoint = MousePosition;
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
                    var text = $"Node {m_hoverInstance.NetNode} not found";
                    var screenPoint = MousePosition;
                    screenPoint.y = screenPoint.y - 40f;
                    var color = GUI.color;
                    GUI.color = Color.white;
                    DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
                    GUI.color = color;
                }
            }  
        }
#endif

        private void HandleLeftClick()
        { 
            switch (m_hoverInstance.Type)
            {
                case InstanceType.Building:
                    {
                        if (m_mode == SelectionToolMode.Normal)
                        {
                            SelectBuilding(m_hoverInstance.Building);
                        }
                        else
                        {
                            // Building restriction mode.
                            if (BuildingPanel.Instance is not null)
                            {
                                ushort buildingId = BuildingPanel.Instance.GetBuildingId();
                                if (buildingId != 0 && buildingId != m_hoverInstance.Building)
                                {
                                    int restrictionId = BuildingPanel.Instance.GetRestrictionId();
                                    if (restrictionId != -1)
                                    {
                                        BuildingSettings settings = BuildingSettingsStorage.GetSettingsOrDefault(buildingId);
                                        RestrictionSettings restrictions = settings.GetRestrictionsOrDefault(restrictionId);

                                        // Get correct array
                                        HashSet<ushort> allowedBuildings;
                                        if (m_mode == SelectionToolMode.BuildingRestrictionIncoming)
                                        {
                                            allowedBuildings = restrictions.GetIncomingBuildingRestrictionsCopy();
                                        }
                                        else
                                        {
                                            allowedBuildings = restrictions.GetOutgoingBuildingRestrictionsCopy();
                                        }

                                        // Add or remove building
                                        if (allowedBuildings.Contains(m_hoverInstance.Building))
                                        {
                                            allowedBuildings.Remove(m_hoverInstance.Building);
                                        }
                                        else
                                        {
                                            allowedBuildings.Add(m_hoverInstance.Building);
                                        }

                                        // Update settings
                                        if (m_mode == SelectionToolMode.BuildingRestrictionIncoming)
                                        {
                                            restrictions.SetIncomingBuildingRestrictions(allowedBuildings);
                                        }
                                        else
                                        {
                                            restrictions.SetOutgoingBuildingRestrictions(allowedBuildings);
                                        }

                                        // Now update settings
                                        settings.SetRestrictions(restrictionId, restrictions);
                                        BuildingSettingsStorage.SetSettings(buildingId, settings);

                                        // Update tab to reflect selected building
                                        if (BuildingPanel.Instance is not null && BuildingPanel.Instance.isVisible)
                                        {
                                            BuildingPanel.Instance.UpdateTabs();
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case InstanceType.NetNode:
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
                        break;
                    }
            }
        }

        private void SelectBuilding(ushort buildingId)
        {
            if (!s_bLoadingTool)
            {
                // Open building panel
                BuildingPanel.Instance?.ShowPanel(buildingId);
            }
        }

        private void DrawLabel()
        {
            var text = Localization.Get("btnBuildingRestrictionsSelected");
            var screenPoint = MousePosition;
            var color = GUI.color;
            GUI.color = Color.white;
            DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
            GUI.color = color;
        }

        public static Vector2 MousePosition
        {
            get
            {
                var mouse = Input.mousePosition;
                mouse.y = Screen.height - mouse.y - 20f;
                return mouse;
            }
        }

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

        protected override bool CheckNode(ushort node, ref ToolErrors errors) => true;
        protected override bool CheckSegment(ushort segment, ref ToolErrors errors) => true;
        protected override bool CheckBuilding(ushort building, ref ToolErrors errors) => true;
        protected override bool CheckProp(ushort prop, ref ToolErrors errors) => true;
        protected override bool CheckTree(uint tree, ref ToolErrors errors) => true;
        protected override bool CheckVehicle(ushort vehicle, ref ToolErrors errors) => true;
        protected override bool CheckParkedVehicle(ushort parkedVehicle, ref ToolErrors errors) => true;
        protected override bool CheckCitizen(ushort citizenInstance, ref ToolErrors errors) => true;
        protected override bool CheckDisaster(ushort disaster, ref ToolErrors errors) => true;
    }
}