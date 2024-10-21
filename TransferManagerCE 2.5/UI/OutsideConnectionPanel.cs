using ColossalFramework.UI;
using System.Collections;
using System.Collections.Generic;
using TransferManagerCE.Common;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE.UI
{
    public class OutsideConnectionPanel : UIPanel
    {
        const int iMARGIN = 8;

        public const int iHEADER_HEIGHT = 20;
        public const int iCOLUMN_WIDTH_XS = 20;
        public const int iCOLUMN_WIDTH_SMALL = 60;
        public const int iCOLUMN_WIDTH_NORMAL = 80;
        public const int iCOLUMN_WIDTH_LARGE = 100;
        public const int iCOLUMN_WIDTH_XLARGE = 200;

        public static OutsideConnectionPanel? Instance = null;

        private UITitleBar? m_title = null;
        private ListView? m_listConnections = null;
        private Coroutine? m_coroutine = null;

        public OutsideConnectionPanel() : base()
        {
            m_coroutine = StartCoroutine(UpdatePanelCoroutine(4));
        }

        public static void Init()
        {
            if (Instance is null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(OutsideConnectionPanel)) as OutsideConnectionPanel;
                if (Instance is null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Outside Connection Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "OutsideConnectionPanel";
            width = 700;
            height = 540;
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "SubcategoriesPanel";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventVisibilityChanged += OnVisibilityChanged;
            CenterTo(parent);


            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = Localization.Get("titleOutsideConnectionPanel");
            
            // Issue list
            m_listConnections = ListView.Create<UIOutsideRow>(this, "ScrollbarTrack", 0.8f, width - 20f, height - m_title.height - 10);
            if (m_listConnections is not null)
            {
                m_listConnections.padding = new RectOffset(iMARGIN, iMARGIN, 4, iMARGIN);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_NAME, Localization.Get("listConnectionName"), "Name", iCOLUMN_WIDTH_XLARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_TYPE, Localization.Get("listConnectionType"), "Type", iCOLUMN_WIDTH_SMALL, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_MULTIPLIER, Localization.Get("listConnectionMultiplier"), "Distance Multiplier", iCOLUMN_WIDTH_NORMAL, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_OWN, Localization.Get("listConnectionOwn"), "Own Vehicles", iCOLUMN_WIDTH_LARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_GUEST, Localization.Get("listConnectionGuest"), "Guest Vehicles", iCOLUMN_WIDTH_LARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_STUCK, Localization.Get("listConnectionStuck"), "Stuck Vehicles", iCOLUMN_WIDTH_LARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
            }

            isVisible = true;
            UpdatePanel();
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

        public static List<OutsideContainer> GetOutsideConnections()
        {
            List<OutsideContainer> result = new List<OutsideContainer>();

            FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
            foreach (ushort connection in connections)
            {
                result.Add(new OutsideContainer(connection));
            }

            return result;
        }

        IEnumerator UpdatePanelCoroutine(int seconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(seconds);
                UpdatePanel();
            }
        }

        public void UpdatePanel()
        {
            if (!isVisible)
            {
                return;
            }

            if (m_listConnections is not null)
            {
                List<OutsideContainer> connections = GetOutsideConnections();
                connections.Sort();

                m_listConnections.GetList().rowsData = new FastList<object>
                {
                    m_buffer = connections.ToArray(),
                    m_size = connections.Count,
                };
            }
        }

        public override void OnDestroy()
        {
            if (m_coroutine is not null)
            {
                StopCoroutine(m_coroutine);
            }
            if (m_listConnections is not null)
            {
                Destroy(m_listConnections.gameObject);
                m_listConnections = null;
            }
            if (Instance is not null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}