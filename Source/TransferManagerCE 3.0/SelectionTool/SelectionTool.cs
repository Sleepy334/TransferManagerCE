using System;
using ColossalFramework;
using ColossalFramework.UI;
using TransferManagerCE.Common;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using UnifiedUI.Helpers;
using UnityEngine;
using static DistrictPolicies;
using static RenderManager;
using static TransferManagerCE.SelectionTool;

namespace TransferManagerCE
{
    public class SelectionTool : DefaultTool
    {
        public enum SelectionToolMode
        {
            Normal,
            DistrictRestrictionIncoming,
            DistrictRestrictionOutgoing,
            BuildingRestrictionIncoming,
            BuildingRestrictionOutgoing,
            PathTesting,
        }
        
        public static SelectionTool? Instance = null;

        public SelectionToolMode m_mode = SelectionToolMode.Normal;
        private SelectionModeBase? m_selectionMode = null;

        private static bool s_bLoadingTool = false;
        private UIComponent? m_button = null;
        private bool m_processedClick = false;
        private Vector3 m_mousePos = Vector3.zero;

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

            // Start with normal tool mode.
            CreateSelectionMode(SelectionToolMode.Normal);
        }

        public static void Release() 
        {
            Destroy(FindObjectOfType<SelectionTool>());
        }

        private void CreateSelectionMode(SelectionToolMode mode)
        {
            if (m_selectionMode is null || mode != m_mode)
            {
                if (PathDistanceTest.PATH_TESTING_ENABLED && mode == SelectionToolMode.Normal)
                {
                    mode = SelectionToolMode.PathTesting;
                }

                if (m_selectionMode is not null) 
                {
                    m_selectionMode.Disable();
                }

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
                    case SelectionToolMode.DistrictRestrictionIncoming:
                    case SelectionToolMode.DistrictRestrictionOutgoing:
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
                    case SelectionToolMode.PathTesting:
                        {
                            m_selectionMode = new SelectionModePathTesting(this);
                            break;
                        }
                }
            }
        }

        public void SetMode(SelectionToolMode mode)
        {
            CreateSelectionMode(mode);
            m_mode = mode;
            m_selectionMode.Enable(mode);

            // Update the building panel to the changed state
            if (BuildingPanel.Instance is not null && BuildingPanel.Instance.isVisible)
            {
                BuildingPanel.Instance.UpdateTabs();
            }
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
            base.ShowToolInfo(true, sText, m_mousePos);
        }

        public override void SimulationStep()
        {
            base.SimulationStep();

            // Check we arent still setting up tool when this is called as it can crash
            if (s_bLoadingTool)
            {
                return;
            }

            RaycastInput input = m_selectionMode.GetRayCastInput(m_mouseRay, m_mouseRayLength);
            if (RayCast(input, out RaycastOutput output))
            {
                if (output.m_netNode > 0)
                {
                    m_hoverInstance.NetNode = output.m_netNode;
                }
                else if (output.m_netSegment > 0)
                {
                    m_hoverInstance.NetSegment = output.m_netSegment;
                }
            }
            
            m_mousePos = output.m_hitPos;
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
            SetMode(SelectionToolMode.Normal);

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
            SetMode(SelectionToolMode.Normal);

            ToolBase oCurrentTool = ToolsModifierControl.toolController.CurrentTool;
            if (oCurrentTool is not null && oCurrentTool == Instance && oCurrentTool.enabled)
            {
                OnDisable();
            }
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
            
            if (m_selectionMode is not null && m_selectionMode.RenderBuildingSelection())
            {
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
                                    HighlightNode(Singleton<ToolManager>.instance, cameraInfo, oNode, GetToolColor(false, false));
                                }
                            }
                            break;
                        }
                }
            }
            
            m_selectionMode.RenderOverlay(cameraInfo);
        }

  
        private static void HighlightNode(ToolManager toolManager, CameraInfo cameraInfo, NetNode oNode, Color color)
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
            toolManager.m_drawCallData.m_overlayCalls++;
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
            switch (m_mode)
            {
                case SelectionToolMode.Normal:
                    {
                        // Close building panel.
                        ToolsModifierControl.SetTool<DefaultTool>();
                        break;
                    }
                default:
                    {
                        SetMode(SelectionToolMode.Normal);
                        break;
                    }
            }
        }
        

        public void SelectBuilding(ushort buildingId)
        {
            if (!s_bLoadingTool)
            {
                // Open building panel
                BuildingPanel.Instance?.ShowPanel(buildingId);
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

            m_selectionMode.OnToolUpdate();

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

        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.All;
        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.All;
        public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.All;
        public override District.Flags GetDistrictIgnoreFlags() => District.Flags.All;
        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.None;
        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.All;
        public override TreeInstance.Flags GetTreeIgnoreFlags() => TreeInstance.Flags.All;
        public override PropInstance.Flags GetPropIgnoreFlags() => PropInstance.Flags.All;

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