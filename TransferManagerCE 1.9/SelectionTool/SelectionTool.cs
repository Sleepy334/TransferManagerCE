using System;
using ColossalFramework.UI;
using TransferManagerCE.Settings;
using UnifiedUI.Helpers;
using UnityEngine;

namespace TransferManagerCE
{
    internal class SelectionTool : DefaultTool
    {
        private UIComponent? m_button = null;
        public static SelectionTool? Instance = null;
        private static bool s_bLoadingTool = false;

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
            if (m_hoverInstance.Type == InstanceType.Building)
            {
                base.RenderOverlay(cameraInfo);
            }
            else if (m_hoverInstance.Type == InstanceType.NetNode)
            {
                NetNode oNode = NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode];
                if (oNode.m_building != 0)
                {
                    if (BuildingTypeHelper.IsOutsideConnection(oNode.m_building))
                    {
                        RenderManager.instance.OverlayEffect.DrawCircle(
                            cameraInfo,
                            GetToolColor(false, false),
                            NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position,
                            NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_bounds.size.magnitude,
                            NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position.y - 1f,
                            NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position.y + 1f,
                            true,
                            true);
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