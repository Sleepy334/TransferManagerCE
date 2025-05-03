using ColossalFramework.UI;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManagerCE.UIUtils;

namespace TransferManagerCE.UI
{
    public class SettingsPanel : UIPanel
    {
        const int iMARGIN = 8;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_ID = 80;
        public const int iCOLUMN_WIDTH_RESTRICTIONS = 90;
        public const int iCOLUMN_WIDTH_DESCRIPTION = 230;

        public static SettingsPanel? Instance = null;

        private UITitleBar? m_title = null;
        private UITextField? m_txtSearch = null;
        private ListView? m_listSettings = null;
        private bool m_bUpdatePanel = false;
        private Coroutine? m_coroutine = null;
        BuildingSettingsRenderer? m_buildingSettingsRenderer = null;

        public SettingsPanel() : base()
        {
            m_coroutine = StartCoroutine(UpdatePanelCoroutine(4));
        }

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(SettingsPanel)) as SettingsPanel;
                if (Instance is null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Settings Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "SettingsPanel";
            width = 700;
            height = 800;
            backgroundSprite = "SubcategoriesPanel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventVisibilityChanged += OnVisibilityChanged;

            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }

            // Panel position
            if (ModSettings.GetSettings().SettingsPanelPosX == float.MaxValue || 
                ModSettings.GetSettings().SettingsPanelPosY == float.MaxValue)
            {
                CenterTo(parent);
            }
            else
            {
                absolutePosition = new Vector3(ModSettings.GetSettings().SettingsPanelPosX, ModSettings.GetSettings().SettingsPanelPosY);
            }

            // Save new position
            eventPositionChanged += (component, pos) =>
            {
                ModSettings settings = ModSettings.GetSettings();
                settings.SettingsPanelPosX = absolutePosition.x;
                settings.SettingsPanelPosY = absolutePosition.y;
                settings.Save();
            };

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.SetHighlightHandler(OnHighlightBuildingsClick);
            m_title.title = Localization.Get("titleSettingsPanel");

            UIPanel mainPanel = AddUIComponent<UIPanel>();
            mainPanel.width = width;
            mainPanel.height = height - m_title.height;
            mainPanel.relativePosition = new Vector3(0f, m_title.height);
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            mainPanel.autoLayoutPadding = new RectOffset(0, 0, 4, 0);

            UIPanel panelFilter = mainPanel.AddUIComponent<UIPanel>();
            panelFilter.width = width;
            panelFilter.height = 24;
            panelFilter.autoLayout = true;
            panelFilter.autoLayoutDirection = LayoutDirection.Horizontal;
            panelFilter.padding = new RectOffset(462, 0, 0, 0);

            // Search button
            AddSpriteButton(ButtonStyle.None, panelFilter, "LineDetailButton", 25, 25);

            // Search field
            m_txtSearch = UIUtils.CreateTextField(ButtonStyle.TextField, panelFilter, "txtSearch", 0.8f, 200f, 25f);
            m_txtSearch.eventTextChanged += OnTextChanged;
            m_txtSearch.eventMouseLeave += (sender, e) =>
            {
                if (m_txtSearch.hasFocus)
                {
                    m_txtSearch.Unfocus();
                }
            };

            // Issue list
            m_listSettings = ListView.Create<UISettingsRow>(mainPanel, "ScrollbarTrack", 0.8f, width - 20f, mainPanel.height - panelFilter.height - 20);
            if (m_listSettings is not null)
            {
                m_listSettings.padding = new RectOffset(iMARGIN, iMARGIN, 4, iMARGIN);
                m_listSettings.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("columnId"), "", iCOLUMN_WIDTH_ID, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                m_listSettings.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("columnRestriction"), "", iCOLUMN_WIDTH_RESTRICTIONS, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listSettings.AddColumn(ListViewRowComparer.Columns.COLUMN_NAME, Localization.Get("listBuildingPanelMatchesColumn6"), "", iCOLUMN_WIDTH_DESCRIPTION, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                m_listSettings.AddColumn(ListViewRowComparer.Columns.COLUMN_NAME, Localization.Get("btnDistrict"), "", iCOLUMN_WIDTH_DESCRIPTION, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
            }

            isVisible = true;
            UpdateHighlightButtonIcon();
            UpdatePanel();
        }

        public void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            //Stopwatch stopwatch = Stopwatch.StartNew();
            //long startTicks = stopwatch.ElapsedTicks;

            if (m_listSettings is not null)
            {
                // Filter list based on search values
                List<SettingsData> settingsList = GetSettingsList();
                settingsList.Sort();

                m_listSettings.GetList().rowsData = new FastList<object>
                {
                    m_buffer = settingsList.ToArray(),
                    m_size = settingsList.Count,
                };

                m_title.title = $"{Localization.Get("titleSettingsPanel")} ({settingsList.Count} / {BuildingSettingsStorage.GetSettingsArray().Count})";
            }

            //long stopTicks = stopwatch.ElapsedTicks;
            //Debug.Log($"{((double)(stopTicks - startTicks) * 0.0001).ToString("F")}ms");
        }

        public void OnHighlightBuildingsClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            int iHighlightMode = (int)ModSettings.GetSettings().HighlightSettingsState;
            ModSettings.GetSettings().HighlightSettingsState = ((iHighlightMode + 1) % 2);
            ModSettings.GetSettings().Save();

            UpdateHighlightButtonIcon();
        }

        public void UpdateHighlightButtonIcon()
        {
            if (m_title is not null && m_title.m_btnHighlight is not null)
            {
                string sIcon = "";
                string sTooltip = "";

                switch ((ModSettings.SettingsHighlightMode) ModSettings.GetSettings().HighlightSettingsState)
                {
                    case ModSettings.SettingsHighlightMode.None:
                        {
                            sIcon = "InfoIconLevelPressed";
                            sTooltip = Localization.Get("tooltipHighlightModeOff");
                            break;
                        }
                    case ModSettings.SettingsHighlightMode.Settings:
                        {
                            sIcon = "InfoIconLevel";
                            sTooltip = Localization.Get("tooltipHighlightModeSettings");

                            // Create renderer if needed
                            if (m_buildingSettingsRenderer is null)
                            {
                                m_buildingSettingsRenderer = new BuildingSettingsRenderer();
                            }

                            break;
                        }
                }

                m_title.m_btnHighlight.normalBgSprite = sIcon;
                m_title.m_btnHighlight.tooltip = sTooltip;
            }
        }

        public bool HandleEscape()
        {
            if (isVisible)
            {
                Hide();
                return true;
            }
            return false;
        }

        public void OnVisibilityChanged(UIComponent component, bool bVisible)
        {
            if (bVisible)
            {
                UpdatePanel();
            }
            else if (tooltipBox is not null)
            {
                tooltipBox.tooltip = "";
                tooltipBox.tooltipBox.Hide();
            }
        }

        public void OnTextChanged(UIComponent component, string value)
        {
            UpdatePanel();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        public void TogglePanel()
        {
            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public List<SettingsData> GetSettingsList()
        {
            List<SettingsData> listAllSettings = GetAllSettingsList();
            
            if (m_txtSearch.text.Length > 0)
            {
                List<SettingsData> result = new List<SettingsData>();

                // APply filters
                string sSearch = m_txtSearch.text.ToUpper();

                // Filter matches
                foreach (SettingsData setting in listAllSettings)
                {
                    if (sSearch.Length > 0 && !setting.Contains(sSearch))
                    {
                        // Filter by search text.
                        continue;
                    }

                    // Passed the filters
                    result.Add(setting);
                }

                return result;
            }
            else
            {
                return listAllSettings;
            }
        }

        private List<SettingsData> GetAllSettingsList()
        {
            List<SettingsData> result = new List<SettingsData>();

            foreach (KeyValuePair<ushort, BuildingSettings> kvp in BuildingSettingsStorage.GetSettingsArray())
            {
                result.Add(new SettingsData(kvp.Key));
            }

            return result;
        }

        public void InvalidatePanel()
        {
            m_bUpdatePanel = true;
        }

        public override void Update()
        {
            if (m_bUpdatePanel)
            {
                // just refresh the list
                if (m_listSettings is not null)
                {
                    m_listSettings.Refresh();
                }

                //UpdatePanel();
                m_bUpdatePanel = false;
            }
            base.Update();
        }

        IEnumerator UpdatePanelCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);
                UpdatePanel();
            }
        }

        public override void OnDestroy()
        {
            if (m_coroutine is not null)
            {
                StopCoroutine(m_coroutine);
            }
            if (m_listSettings is not null)
            {
                Destroy(m_listSettings.gameObject);
                m_listSettings = null;
            }
            if (m_buildingSettingsRenderer is not null)
            {
                Destroy(m_buildingSettingsRenderer.gameObject);
                m_buildingSettingsRenderer = null;
            }
            if (Instance is not null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}