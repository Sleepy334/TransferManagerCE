using System;
using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using TransferManagerCE.UI;
using UnityEngine;
using static ColossalFramework.IO.EncodedArray;
using static RenderManager;

namespace TransferManagerCE
{
    public class SelectionTool : DefaultTool
    {
        public enum SelectionToolMode
        {
            Normal,
            DistrictRestriction,
            BuildingRestrictionIncoming,
            BuildingRestrictionOutgoing,
            PathDistance,
            PathDistanceCandidates,
        }
        
        public static SelectionTool? m_instance = null;
        private static bool s_bLoadingTool = false;

        public SelectionToolMode m_currentMode = SelectionToolMode.Normal;
        public SelectionToolMode m_newMode = SelectionToolMode.Normal; 
        private SelectionModeBase? m_selectionMode = null;

        public ushort m_nodeId = 0;
        public ushort m_segmentId = 0;

        private bool m_processedClick = false;

        // ----------------------------------------------------------------------------------------
        public static SelectionTool Instance
        {
            get
            {
                if (m_instance is null)
                {
                    AddSelectionTool();
                }
                return m_instance;
            }
        }

        public static bool Exists
        {
            get
            {
                return m_instance != null;
            }
        }

        public static bool Active
        {
            get
            {
                return m_instance is not null &&
                        ToolsModifierControl.toolController.CurrentTool == m_instance;
            }
        }

        public ToolController ToolController
        {
            get
            {
                return GetToolController();
            }
        }

        public SelectionTool selectionMode
        {
            get
            {
                if (m_selectionMode is null)
                {
                    CreateSelectionMode(m_newMode);
                }
                return selectionMode;
            }
        }

        public static void AddSelectionTool()
        {
            if (TransferManagerMod.Instance.IsLoaded && !s_bLoadingTool)
            {
                if (m_instance is null)
                {
                    try
                    {
                        s_bLoadingTool = true;
                        m_instance = ToolsModifierControl.toolController.gameObject.AddComponent<SelectionTool>();
                    }
                    catch (Exception e)
                    {
                        CDebug.Log("Selection tool failed to load: ", e);
                    }
                    finally
                    {
                        s_bLoadingTool = false;
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // Start with normal tool mode.
            CreateSelectionMode(SelectionToolMode.Normal);
        }

        public void OnToggle(bool bToggle)
        {
            BuildingPanel.TogglePanel();
        }

        public static void Release() 
        {
            Destroy(FindObjectOfType<SelectionTool>());
        }

        private void CreateSelectionMode(SelectionToolMode mode)
        {
            m_currentMode = mode;
            m_newMode = mode;

            switch (mode)
            {
                case SelectionToolMode.Normal:
                    {
#if DEBUG
                        m_selectionMode = new SelectionModeNormalDebug(this);
#else
                        m_selectionMode = new SelectionModeNormal(this);
#endif
                        break;
                    }
                case SelectionToolMode.DistrictRestriction:
                    {
                        m_selectionMode = new SelectionModeDistrictRestrictions(this);
                        break;
                    }
                case SelectionToolMode.BuildingRestrictionIncoming:
                case SelectionToolMode.BuildingRestrictionOutgoing:
                    {
                        m_selectionMode = new SelectionModeBuildingRestrictions(this);
                        break;
                    }
                case SelectionToolMode.PathDistance:
                    {
                        m_selectionMode = new SelectionModePathDistance(this);
                        break;
                    }
                case SelectionToolMode.PathDistanceCandidates:
                    {
                        m_selectionMode = new SelectionModeSelectCandidates(this);
                        break;
                    }
            }
        }

        public void SetMode(SelectionToolMode mode)
        {
            if (m_selectionMode is null || mode != GetCurrentMode())
            {
                // We set this so it can be queried in Disable.
                m_newMode = mode;

                // Disable old tool
                if (m_selectionMode is not null)
                {
                    m_selectionMode.Disable();
                }

                CreateSelectionMode(mode);
                
                // Enable new tool
                m_selectionMode.Enable();
            }
        }

        public void SelectNormalTool()
        {
            if (BuildingPanel.IsVisible())
            {
                SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
            }
            else
            {
                SelectionTool.Instance.Disable();
            }
        }

        public SelectionToolMode GetCurrentMode()
        {
            return m_currentMode;
        }

        public SelectionToolMode GetNewMode()
        {
            return m_newMode;
        }

        public InstanceID GetHoverInstance()
        {
            return m_hoverInstance;
        }

        public Vector3 GetMousePosition()
        {
            return m_mousePosition;
        }

        public ToolController GetToolController()
        {
            return m_toolController;
        }

        public void ShowToolInfo(string sText)
        {
            base.ShowToolInfo(true, sText, m_mousePosition);
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (m_nodeId != 0 && CitiesUtils.IsOutsideConnectionNode(m_nodeId))
            {
                // Highlight outside connections
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[m_nodeId];
                if (oNode.m_flags != 0)
                {
                    HighlightNode(cameraInfo, oNode, GetToolColor(false, false));

                    if (m_hoverInstance.Index == 0 && oNode.m_building != 0)
                    {
                        m_hoverInstance.Building = oNode.m_building;
                    }
                }
            }

            if (m_selectionMode is not null && m_selectionMode.RenderBuildingSelection())
            {
                if (m_hoverInstance.Building != 0)
                {
                    base.RenderOverlay(cameraInfo);
                }
            }

            m_selectionMode.RenderOverlay(cameraInfo);
        }

        public override void SimulationStep()
        {
            // Check we arent still setting up tool when this is called as it can crash
            if (s_bLoadingTool)
            {
                return;
            }

            base.SimulationStep();

            // Grab the node and segment
            if (RayCastSegmentAndNode(out RaycastOutput output))
            {
                m_nodeId = output.m_netNode;
                m_segmentId = output.m_netSegment;

                // We try to find the node from the segment
                if (output.m_netNode == 0)
                {
                    if (output.m_netSegment != 0)
                    {
                        var segment = NetManager.instance.m_segments.m_buffer[output.m_netSegment];
                        var startNode = NetManager.instance.m_nodes.m_buffer[segment.m_startNode];
                        var endNode = NetManager.instance.m_nodes.m_buffer[segment.m_endNode];
                        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                        if (startNode.CountSegments() > 0)
                        {
                            var bounds = startNode.m_bounds;
                            if (bounds.IntersectRay(mouseRay))
                            {
                                m_nodeId = segment.m_startNode;
                            }
                        }

                        if (m_nodeId == 0 && endNode.CountSegments() > 0)
                        {
                            var bounds = endNode.m_bounds;
                            if (bounds.IntersectRay(mouseRay))
                            {
                                m_nodeId = segment.m_endNode;
                            }
                        }
                    }

                    // Try looking in node grid instead
                    if (m_nodeId == 0)
                    {
                        m_nodeId = FindNearestNode(output.m_hitPos);
                    }
                }
            }
            else
            {
                m_nodeId = 0;
                m_segmentId = 0;
            }

            // If the node is set and hover instance isnt the set hover instance
            if (m_nodeId != 0 && m_hoverInstance.Index == 0)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[m_nodeId];
                if (oNode.m_flags != 0)
                {
                    m_hoverInstance.Building = oNode.m_building;
                }
            }
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

        public static ushort FindNearestNode(Vector3 position)
        {
            int num = Mathf.Clamp((int)(position.x / 64f + 135f), 0, 269);
            int num2 = Mathf.Clamp((int)(position.z / 64f + 135f), 0, 269);
            int num3 = num2 * 270 + num;
            ushort nodeId = Singleton<NetManager>.instance.m_nodeGrid[num3];

            if (nodeId != 0)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[nodeId];
                if (oNode.m_flags != 0)
                {
                    if (Math.Abs(oNode.m_position.x - position.x) < oNode.m_bounds.extents.x &&
                        Math.Abs(oNode.m_position.z - position.z) < oNode.m_bounds.extents.z)
                    {
                        return nodeId;
                    }
                }
            }

            return 0;
        }

        public void Enable(SelectionToolMode mode)
        {
            if (Exists)
            {
                m_newMode = mode;

                Enable();
                SetMode(mode);
            }
        }

        public void Enable()
        {
            if (Exists && !Instance.enabled)
            {
                OnEnable();
            }
        }

        protected override void OnEnable() 
        {
            base.OnEnable();

            // We seem to get an eroneous OnEnable call when adding the tool.
            if (!s_bLoadingTool)
            {
                ToolsModifierControl.mainToolbar.CloseEverything();

                ToolsModifierControl.SetTool<SelectionTool>();
            }
        }

        public void Disable()
        {
            m_newMode = SelectionToolMode.Normal;

            m_selectionMode.Disable();

            // Ensure we are in normal mode.
            SetMode(SelectionToolMode.Normal);

            if (Active && Instance.enabled)
            {
                OnDisable();
            }
        }

        protected override void OnDisable() 
        {
            // workaround exception in base code.
            m_toolController ??= ToolsModifierControl.toolController; 

            base.OnDisable();

            ToolsModifierControl.SetTool<DefaultTool>();
            
            if (BuildingPanel.IsVisible())
            {
                BuildingPanel.Instance.Hide();
            }
        }

        public void Toggle()
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
        
        private static void HighlightNode(CameraInfo cameraInfo, NetNode oNode, Color color)
        {
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

        public void UpdateSelection()
        {
            m_selectionMode.UpdateSelection();
        }

        protected override void OnToolGUI(UnityEngine.Event e)
        {
            if (s_bLoadingTool || m_toolController.IsInsideUI)
            {
                base.OnToolGUI(e);
                return;
            }

            m_selectionMode.OnToolGUI(e);

            if (e.type == EventType.MouseDown)
            {
                if (!m_processedClick)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        HandleLeftClick();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        HandleRightClick();
                    }
                    m_processedClick = true;
                }
            }
            else
            {
                m_processedClick = false;
            }
        }

        public void HandleLeftClick()
        {
            m_selectionMode.HandleLeftClick();
        }

        public void HandleRightClick()
        {
            switch (GetCurrentMode())
            {
                case SelectionToolMode.Normal:
                    {
                        // Close building panel.
                        ToolsModifierControl.SetTool<DefaultTool>();
                        BuildingPanel.Instance.Hide();
                        break;
                    }
                default:
                    {
                        SetMode(SelectionToolMode.Normal);
                        break;
                    }
            }
        }

        public void OnSelectBuilding(ushort buildingId)
        {
            if (!s_bLoadingTool)
            {
                m_selectionMode.OnSelectBuilding(buildingId);
            }
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

            if (!ToolController.IsInsideUI)
            {
                ShowToolInfo(m_selectionMode.GetTooltipText());
            }

            if (UIView.library.Get("PauseMenu")?.isVisible == true)
            {
                UIView.library.Hide("PauseMenu");
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }

        protected override void OnToolLateUpdate()
        {
            base.OnToolLateUpdate();
            m_selectionMode.OnToolLateUpdate();
        }

        public override NetNode.Flags GetNodeIgnoreFlags() 
        { 
            return m_selectionMode.GetNodeIgnoreFlags();
        }

        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            return m_selectionMode.GetSegmentIgnoreFlags(out nameOnly);
        }

        public override Building.Flags GetBuildingIgnoreFlags()
        {
            return m_selectionMode.GetBuildingIgnoreFlags();
        }

        public override TransportLine.Flags GetTransportIgnoreFlags() 
        {
            return m_selectionMode.GetTransportIgnoreFlags();
        }

        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.All;
        public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.All;
        public override District.Flags GetDistrictIgnoreFlags() => District.Flags.All;
        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.All;
        public override TreeInstance.Flags GetTreeIgnoreFlags() => TreeInstance.Flags.All;
        public override PropInstance.Flags GetPropIgnoreFlags() => PropInstance.Flags.All;

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