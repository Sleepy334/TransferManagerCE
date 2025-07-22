using AlgernonCommons.UI;
using ColossalFramework.UI;
using SleepyCommon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManagerCE.NetworkModeHelper;

namespace TransferManagerCE.UI
{
    public class PathDistancePanel : UIMainPanel<PathDistancePanel>
    {
        const int iMARGIN = 8;
        const float fTEXT_SCALE = 0.9f;
        const int iButtonHeight = 28;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 150;

        private UITitleBar? m_title = null;
        private UILabel? m_lblStartBuilding = null;
        private UIPanel? m_pnlSelectBuildings = null;
        private UILabel? m_lblSelectBuildings = null;
        private UIToggleButton? m_btnSelectBuildings = null;
        private UIButton? m_btnClearBuildings = null;
        private UIButton? m_btnCalculate = null;
        // Results
        private UILabel? m_lblChosenCandidate = null;
        private UILabel? m_lblTravelTime = null;
        private UILabel? m_lblNodesExamined = null;
        private UILabel? m_lblTime = null;

        private UIComponent? m_tooltipComponent = null;

        // Algorithm settings
        public ushort m_buildingId = 0;
        private HashSet<ushort> m_candidates = new HashSet<ushort>();
        private NetworkMode m_algorithm = NetworkMode.Goods;
        private int m_direction = 0; // Forward = 0, Backward = 1
        private bool m_bShowConnectionGraph = false;

        // Path distance helper
        private PathDistanceTest m_pathDistanceTest = new PathDistanceTest();

        private UIInfoLabel? m_infoLabel = null;

        // ----------------------------------------------------------------------------------------
        public PathDistancePanel() : 
            base()
        {
            PathDistanceRenderer.RegisterRenderer();
        }

        public HashSet<ushort> candidates
        {
            get 
            { 
                return m_candidates; 
            }
        }

        public PathDistanceTest Test
        {
            get
            {
                return m_pathDistanceTest;
            }
        }

        public NetworkMode Algorithm
        {
            get
            {
                return m_algorithm;
            }
        }

        public int Direction
        {
            get
            {
                return m_direction;
            }
        }

        public bool ShowConnectionGraph
        {
            get
            {
                return m_bShowConnectionGraph;
            }
        }

        public ushort Building
        {
            get
            {
                return m_buildingId;
            }
            set
            {
                SetBuilding(value);
            }
        }

        public override void Start()
        {
            base.Start();
            name = "PathDistancePanel";
            width = 450;
            height = 474;
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            backgroundSprite = "SubcategoriesPanel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventVisibilityChanged += OnVisibilityChanged;
            eventTooltipEnter += new MouseEventHandler(OnTooltipEnter);
            eventTooltipLeave += new MouseEventHandler(OnTooltipLeave);
            CenterTo(parent);

            // Title Bar
            m_title = UITitleBar.Create(this, "Path Distance", "Transfer", TransferManagerMod.Instance.LoadResources(), OnCloseClick);
            if (m_title != null)
            {
                m_title.AddButton("btnSelectionTool", atlas, "LineDetailButton", "Activate Selection Tool", OnSelectionToolClick);
                m_title.SetupButtons();
            }

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height - m_title.height;
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;

            // ----------------------------------------------------------------
            // Label
            m_lblStartBuilding = mainPanel.AddUIComponent<UILabel>();
            m_lblStartBuilding.text = Localization.Get("txtSelectStartBuilding");
            m_lblStartBuilding.font = UIFonts.Regular;
            m_lblStartBuilding.textScale = fTEXT_SCALE;
            m_lblStartBuilding.autoSize = false;
            m_lblStartBuilding.height = 30;
            m_lblStartBuilding.width = mainPanel.width;
            m_lblStartBuilding.verticalAlignment = UIVerticalAlignment.Middle;
            m_lblStartBuilding.textAlignment = UIHorizontalAlignment.Center;
            m_lblStartBuilding.eventMouseEnter += (c, e) =>
            {
                m_lblStartBuilding.textColor = new Color32(13, 183, 255, 255);
            };
            m_lblStartBuilding.eventMouseLeave += (c, e) =>
            {
                m_lblStartBuilding.textColor = Color.white;
            };
            m_lblStartBuilding.eventClick += (c, e) =>
            {
                SelectionTool.Instance.Enable(SelectionTool.SelectionToolMode.PathDistance);

                if (m_buildingId != 0)
                {
                    InstanceHelper.ShowInstance(new InstanceID { Building = m_buildingId }); 
                }
            };

            AddSpacer(mainPanel);

            // ----------------------------------------------------------------
            // Select buildings
            m_pnlSelectBuildings = mainPanel.AddUIComponent<UIPanel>();
            m_pnlSelectBuildings.width = mainPanel.width;
            m_pnlSelectBuildings.height = 35;
            m_pnlSelectBuildings.autoLayout = true;
            m_pnlSelectBuildings.autoLayoutDirection = LayoutDirection.Horizontal;
            m_pnlSelectBuildings.autoLayoutPadding = new RectOffset(4, 4, 4, 4);
            m_pnlSelectBuildings.padding = new RectOffset(20, 0, 0, 0);

            // Label
            m_lblSelectBuildings = m_pnlSelectBuildings.AddUIComponent<UILabel>();
            m_lblSelectBuildings.text = Localization.Get("txtCandidates");
            m_lblSelectBuildings.font = UIFonts.Regular;
            m_lblSelectBuildings.textScale = fTEXT_SCALE;
            m_lblSelectBuildings.autoSize = false;
            m_lblSelectBuildings.height = 30;
            m_lblSelectBuildings.width = 150;
            m_lblSelectBuildings.verticalAlignment = UIVerticalAlignment.Middle;

            // Buttons
            m_btnSelectBuildings = UIMyUtils.AddToggleButton(UIMyUtils.ButtonStyle.DropDown, m_pnlSelectBuildings, Localization.Get("btnBuildingRestrictions"), "", 200, iButtonHeight, OnBuildingSelection);
            m_btnSelectBuildings.onColor = KnownColor.lightBlue;
            m_btnSelectBuildings.offColor = KnownColor.white;

            // Clear button
            m_btnClearBuildings = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, m_pnlSelectBuildings, "Niet", iButtonHeight, iButtonHeight, (c, e) =>
            {
                m_candidates.Clear();
                m_pathDistanceTest.Clear();
                UpdatePanel();
            });
            m_btnClearBuildings.tooltip = Localization.Get("btnClear");

            // ----------------------------------------------------------------
            // Algorithm dropdown
            string[] itemsAlgorithm = {
                        Localization.Get("dropdownConnectionGraphGoods"),
                        Localization.Get("dropdownConnectionGraphPedestrianZoneServices"),
                        Localization.Get("dropdownConnectionGraphOtherServices"),
                    };

            const int iDropDownWidth = 235;

            UIPanel pnlAlgorithm = mainPanel.AddUIComponent<UIPanel>();
            pnlAlgorithm.width = mainPanel.width;
            pnlAlgorithm.height = 40;
            //pnlAlgorithm.backgroundSprite = "InfoviewPanel";
            //pnlAlgorithm.color = new Color32(150, 0, 0, 255);

            UIMyDropDown drpAlgorithm = UIMyDropDown.Create(pnlAlgorithm, $"{Localization.Get("txtAlgorithm")}:", fTEXT_SCALE, itemsAlgorithm, OnAlgorithm, (int) m_algorithm - 1, iDropDownWidth);
            if (drpAlgorithm is not null)
            {
                drpAlgorithm.Panel.relativePosition = new Vector3(0, 10);
                drpAlgorithm.Panel.padding = new RectOffset(24, 32, 0, 0);
                drpAlgorithm.Label.width = drpAlgorithm.Panel.width - drpAlgorithm.DropDown.width - drpAlgorithm.Panel.padding.left - drpAlgorithm.Panel.padding.right;
                drpAlgorithm.DropDown.textScale = 0.9f;
            }

            // ----------------------------------------------------------------
            // Algorithm dropdown
            string[] itemsDirection = {
                        Localization.Get("txtForward"),
                        Localization.Get("txtBackward"),
                    };

            UIPanel pnlDirection = mainPanel.AddUIComponent<UIPanel>();
            pnlDirection.width = mainPanel.width;
            pnlDirection.height = 40;
            //pnlAlgorithm.backgroundSprite = "InfoviewPanel";
            //pnlAlgorithm.color = new Color32(150, 0, 0, 255);

            UIMyDropDown drpDirection = UIMyDropDown.Create(pnlDirection, $"{Localization.Get("txtDirection")}:", fTEXT_SCALE, itemsDirection, OnDirection, m_direction, iDropDownWidth);
            if (drpDirection is not null)
            {
                drpDirection.Panel.relativePosition = new Vector3(0, 10);
                drpDirection.Panel.padding = new RectOffset(24, 32, 0, 0);
                drpDirection.Label.width = drpDirection.Panel.width - drpDirection.DropDown.width - drpDirection.Panel.padding.left - drpDirection.Panel.padding.right;
                drpDirection.DropDown.textScale = 0.9f;
            }

            AddSpacer(mainPanel);

            // ----------------------------------------------------------------
            UIPanel pnlConnectionGraph = mainPanel.AddUIComponent<UIPanel>();
            pnlConnectionGraph.width = mainPanel.width;
            pnlConnectionGraph.height = 26;
            pnlConnectionGraph.padding = new RectOffset(40, 0, 0, 0);
            pnlConnectionGraph.autoLayout = true;
            pnlConnectionGraph.autoLayoutDirection = LayoutDirection.Horizontal;

            UICheckBox chkShowConnectionGraph = UIMyUtils.AddCheckbox(pnlConnectionGraph, Localization.Get("dropdownConnectionGraph"), UIFonts.Regular, fTEXT_SCALE, false, OnShowConnectionGraph);
            chkShowConnectionGraph.tooltip = UIMyUtils.GetWordWrapTooltipText(Localization.Get("txtShowConnectionGraph"));

            AddSpacer(mainPanel);

            // ----------------------------------------------------------------
            m_lblChosenCandidate = AddLabel(mainPanel, "Chosen Candidate");
            m_lblChosenCandidate.eventMouseEnter += (c, e) =>
            {
                m_lblChosenCandidate.textColor = new Color32(13, 183, 255, 255);
            };
            m_lblChosenCandidate.eventMouseLeave += (c, e) =>
            {
                m_lblChosenCandidate.textColor = Color.white;
            };
            m_lblChosenCandidate.eventClick += (s, e) =>
            {
                if (m_pathDistanceTest.ChosenBuildingId != 0)
                {
                    InstanceHelper.ShowInstance(new InstanceID { Building = (ushort)m_pathDistanceTest.ChosenBuildingId });
                }
            };
            m_lblTravelTime = AddLabel(mainPanel, "Travel Time");
            m_lblNodesExamined = AddLabel(mainPanel, "Nodes Examined");
            m_lblTime = AddLabel(mainPanel, "Time");

            AddSpacer(mainPanel);

            // ----------------------------------------------------------------
            // Calculate panel
            UIPanel pnlCalculate = mainPanel.AddUIComponent<UIPanel>();
            pnlCalculate.width = mainPanel.width;
            pnlCalculate.height = 50;
            pnlCalculate.autoLayout = false;

            m_btnCalculate = UIMyUtils.AddButton(UIMyUtils.ButtonStyle.DropDown, pnlCalculate, "Calculate", "", 200, iButtonHeight, (c, e) =>
            {
                bool bStartActive = (m_direction == 0);
                m_pathDistanceTest.FindNearestNeighbour(m_algorithm, bStartActive, m_buildingId, m_candidates.ToArray());
                UpdatePanel();
            });
            
            // Clear button
            UIButton btnClearResults = UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.DropDown, pnlCalculate, "Niet", iButtonHeight, iButtonHeight, (c, e) =>
            {
                m_pathDistanceTest.Clear();
                UpdatePanel();
            });
            btnClearResults.tooltip = Localization.Get("btnClear");

            float fLeftPadding = (pnlCalculate.width - (m_btnCalculate.width + btnClearResults.width + 4)) * 0.5f;
            m_btnCalculate.relativePosition = new Vector3(fLeftPadding, 10);
            btnClearResults.relativePosition = new Vector3(fLeftPadding + m_btnCalculate.width + 4, 10);

            // ----------------------------------------------------------------
            m_infoLabel = new UIInfoLabel(this);

            // ----------------------------------------------------------------
            isVisible = true;
            UpdatePanel();
        }

        private UILabel AddLabel(UIComponent parent, string sText)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.text = sText;
            label.font = UIFonts.Regular;
            label.textScale = fTEXT_SCALE;
            label.autoSize = false;
            label.height = 24;
            label.width = parent.width;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.padding = new RectOffset(30, 0, 0, 0);
            return label;
        }

        public void OnBuildingSelection(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (SelectionTool.Instance.GetCurrentMode() == SelectionTool.SelectionToolMode.PathDistanceCandidates)
            {
                SelectionTool.Instance.SelectNormalTool();
            }
            else
            {
                SelectionTool.Instance.Enable(SelectionTool.SelectionToolMode.PathDistanceCandidates);
            }
            UpdatePanel();
        }

        public void OnAlgorithm(UIComponent component, int index)
        {
            m_algorithm = (NetworkMode)index + 1;
        }

        public void OnDirection(UIComponent component, int index)
        {
            m_direction = index;
        }

        public void OnShowConnectionGraph(bool bChecked)
        {
            m_bShowConnectionGraph = bChecked;
            PathConnectionRenderer.RegisterRenderer();
        }

        private void AddSpacer(UIComponent component)
        {
            UIPanel gap1 = component.AddUIComponent<UIPanel>();
            gap1.width = width;
            gap1.height = 10f;

            UIPanel spacerPanel = component.AddUIComponent<UIPanel>();
            spacerPanel.width = width;
            spacerPanel.height = 5f;
            spacerPanel.backgroundSprite = "ContentManagerItemBackground";

            UIPanel gap2 = component.AddUIComponent<UIPanel>();
            gap2.width = width;
            gap2.height = 10f;
        }

        public void SetBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
            UpdatePanel();

            if (isVisible)
            {
                InstanceHelper.ShowInstance(new InstanceID { Building = m_buildingId });
            }
        }

        public void SetCandidates(HashSet<ushort> candidates)
        {
            m_candidates = candidates;
            UpdatePanel();
        }

        public void ShowPanel(ushort buildingId)
        {
            Show();
            UpdatePanel();
            BringToFront();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        public void OnSelectionToolClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (SelectionTool.Active)
            {
                if (SelectionTool.Instance.GetCurrentMode() != SelectionTool.SelectionToolMode.PathDistance)
                {
                    SelectionTool.Instance.Enable(SelectionTool.SelectionToolMode.PathDistance);
                }
                else
                {
                    SelectionTool.Instance.SetMode(SelectionTool.SelectionToolMode.Normal);
                    
                    // Disable selection tool if building panel is not visible
                    if (!BuildingPanel.IsVisible())
                    {
                        SelectionTool.Instance.Disable();
                    }
                }
            }
            else
            {
                SelectionTool.Instance.Enable(SelectionTool.SelectionToolMode.PathDistance);
            }

            UpdatePanel();
        }

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                UpdatePanel();
            }
            else if (SelectionTool.Instance.GetCurrentMode() == SelectionTool.SelectionToolMode.PathDistance ||
                     SelectionTool.Instance.GetCurrentMode() == SelectionTool.SelectionToolMode.PathDistanceCandidates)
            {
                SelectionTool.Instance.SelectNormalTool();

                if (m_infoLabel is not null)
                {
                    m_infoLabel.Hide();
                }
            }
        }

        protected virtual void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (!enabled)
            {
                return;
            }

            m_tooltipComponent = component;
        }

        protected virtual void OnTooltipLeave(UIComponent component, UIMouseEventParameter eventParam)
        {
            m_tooltipComponent = null;
        }

        protected override void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            // Building label
            if (m_buildingId != 0)
            {
                m_lblStartBuilding.text = InstanceHelper.DescribeInstance(new InstanceID { Building = m_buildingId }, true, true);
            }
            else
            {
                m_lblStartBuilding.text = Localization.Get("txtSelectStartBuilding");
            }

            // Update selection tool icon
            string sTooltip = "Path Distance Tool: ";
            if (SelectionTool.Active && 
                SelectionTool.Instance.GetNewMode() == SelectionTool.SelectionToolMode.PathDistance)
            {
                m_title.Buttons[0].normalBgSprite = "LineDetailButtonFocused";
                m_title.Buttons[0].tooltip = sTooltip + "On";
            }
            else
            {
                m_title.Buttons[0].normalBgSprite = "LineDetailButton";
                m_title.Buttons[0].tooltip = sTooltip + "Off";
            }

            // Refresh tooltip if needed
            if (m_tooltipComponent == m_title.Buttons[0])
            {
                m_title.Buttons[0].RefreshTooltip();
            }

            // Update candidate text
            m_lblSelectBuildings.text = $"{Localization.Get("txtCandidates")} ({m_candidates.Count}):";

            
            if (SelectionTool.Active && SelectionTool.Instance.GetNewMode() == SelectionTool.SelectionToolMode.PathDistanceCandidates)
            {
                m_btnSelectBuildings.text = Localization.Get("txtSelectBuildings");
                m_btnSelectBuildings.StateOn = true;
            }
            else
            {
                m_btnSelectBuildings.text = Localization.Get("txtBuildings");
                m_btnSelectBuildings.StateOn = false;
            }
            m_btnClearBuildings.isEnabled = m_candidates.Count > 0;

            // Update results
            if (m_pathDistanceTest.ChosenBuildingId > 0)
            {
                m_lblChosenCandidate.text = $"Chosen Candidate: {InstanceHelper.DescribeInstance(new InstanceID { Building = (ushort) m_pathDistanceTest.ChosenBuildingId }, true, true)}";
            }
            else
            {
                m_lblChosenCandidate.text = "Chosen Candidate: ";
            }
            m_lblTravelTime.text = $"Travel Time: {m_pathDistanceTest.TravelTime}";
            m_lblNodesExamined.text = $"Nodes Examined: {m_pathDistanceTest.GetExaminedNodes().Count}";
            m_lblTime.text = $"Calculation Time: {Utils.DisplayTicks(m_pathDistanceTest.Ticks)}ms";
            
            if (m_buildingId != 0 && m_candidates.Count > 0)
            {
                m_btnCalculate.Enable();
            }
            else
            {
                m_btnCalculate.Disable();
            }
        }

        public void ShowInfo(string sText)
        {
            if (m_infoLabel is not null)
            {
                m_infoLabel.text = sText;
                m_infoLabel.Show();
            }
        }

        public void HideInfo()
        {
            if (m_infoLabel is not null)
            {
                m_infoLabel.Hide();
            }
        }
    }
}