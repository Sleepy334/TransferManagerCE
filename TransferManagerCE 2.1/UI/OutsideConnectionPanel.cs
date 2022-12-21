using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.Common;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
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

        public OutsideConnectionPanel() : base()
        {
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = UIView.GetAView().AddUIComponent(typeof(OutsideConnectionPanel)) as OutsideConnectionPanel;
                if (Instance == null)
                {
                    Prompt.Info("Transfer Manager CE", "Error creating Outside Connection Panel.");
                }
            }
        }

        public override void Start()
        {
            base.Start();
            name = "OutsideConnectionPanel";
            width = 600;
            height = 540;
            if (ModSettings.GetSettings().EnablePanelTransparency)
            {
                opacity = 0.95f;
            }
            padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            CenterToParent();

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = Localization.Get("titleOutsideConnectionPanel");
            
            // Issue list
            m_listConnections = ListView.Create<UIOutsideRow>(this, "ScrollbarTrack", 0.8f, width - 20f, height - m_title.height - 10);
            if (m_listConnections != null)
            {
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_NAME, Localization.Get("listConnectionName"), "Name", iCOLUMN_WIDTH_XLARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_TYPE, Localization.Get("listConnectionType"), "Type", iCOLUMN_WIDTH_SMALL, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_MULTIPLIER, Localization.Get("listConnectionMultiplier"), "Distance Multiplier", iCOLUMN_WIDTH_NORMAL, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_OWN, Localization.Get("listConnectionOwn"), "Own Vehicles", iCOLUMN_WIDTH_LARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listConnections.AddColumn(ListViewRowComparer.Columns.COLUMN_GUEST, Localization.Get("listConnectionGuest"), "Guest Vehicles", iCOLUMN_WIDTH_LARGE, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
            }
            
            isVisible = true;
            UpdatePanel();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }

        private void FitToScreen()
        {
            Vector2 oScreenVector = UIView.GetAView().GetScreenResolution();
            float fX = Math.Max(0.0f, Math.Min(absolutePosition.x, oScreenVector.x - width));
            float fY = Math.Max(0.0f, Math.Min(absolutePosition.y, oScreenVector.y - height));
            Vector3 oFitPosition = new Vector3(fX, fY, absolutePosition.z);
            absolutePosition = oFitPosition;
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

        public List<OutsideContainer> GetOutsideConnections()
        {
            List<OutsideContainer> result = new List<OutsideContainer>();

            FastList<ushort> connections = BuildingManager.instance.GetOutsideConnections();
            foreach (ushort connection in connections)
            {
                result.Add(new OutsideContainer(connection));
            }

            return result;
        }

        public void UpdatePanel()
        {
            if (m_listConnections != null)
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
            if (m_listConnections != null)
            {
                Destroy(m_listConnections.gameObject);
                m_listConnections = null;
            }
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }
    }
}