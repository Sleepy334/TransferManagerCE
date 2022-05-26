using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Patch;
using TransferManagerCE.Settings;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class StatsPanel : UIPanel
    {
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;
        public const int iLISTVIEW_STATS_HEIGHT = 300;

        public const int iCOLUMN_MATERIAL_WIDTH = 120;
        public const int iCOLUMN_WIDTH = 70;
        public const int iCOLUMN_BIGGER_WIDTH = 95;

        private UITitleBar? m_title = null;
        private ListView m_listStats = null;


        public StatsPanel() : base()
        {
        }

        public override void Start()
        {
            base.Start();
            name = "TransferBuildingPanel";
            width = 800;
            height = 600;
            padding = new RectOffset(iMARGIN, iMARGIN, 4, 4);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "UnlockingPanel2";
            canFocus = true;
            isInteractive = true;
            isVisible = false;
            playAudioEvents = true;
            m_ClipChildren = true;
            eventPositionChanged += (sender, e) =>
            {
                ModSettings settings = ModSettings.GetSettings();
                settings.TransferIssueLocationSaved = true;
                settings.TransferIssueLocation = absolutePosition;
                settings.Save();
            };

            if (ModSettings.GetSettings().TransferBuildingLocationSaved)
            {
                absolutePosition = ModSettings.GetSettings().TransferBuildingLocation;
                FitToScreen();
            }
            else
            {
                CenterToParent();
            }

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.SetOnclickHandler(OnCloseClick);
            m_title.title = "Transfer Manager Stats";

            // Offer list
            m_listStats = ListView.Create(this, "ScrollbarTrack", 0.7f, width - 20f, height - m_title.height - 12);
            if (m_listStats != null)
            {
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, "Material", "Material", iCOLUMN_MATERIAL_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_COUNT, "OUT Count", "Transfer offer priority", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_AMOUNT, "OUT Amount", "Transfer Offer Amount", iCOLUMN_BIGGER_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_COUNT, "IN Count", "IN Count", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_AMOUNT, "IN Amount", "Reason for transfer request", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_COUNT, "Matches", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_MATCH_AMOUNT, "Match Amount", "Offer description", iCOLUMN_BIGGER_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_OUT_PERCENT, "OUT%", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                m_listStats.AddColumn(ListViewRowComparer.Columns.COLUMN_IN_PERCENT, "IN%", "Offer description", iCOLUMN_WIDTH, iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
            }
           
            isVisible = true;
            UpdatePanel();
        }

        private void FitToScreen()
        {
            Vector2 oScreenVector = UIView.GetAView().GetScreenResolution();
            float fX = Math.Max(0.0f, Math.Min(absolutePosition.x, oScreenVector.x - width));
            float fY = Math.Max(0.0f, Math.Min(absolutePosition.y, oScreenVector.y - height));
            Vector3 oFitPosition = new Vector3(fX, fY, absolutePosition.z);
            absolutePosition = oFitPosition;
        }

        new public void Show()
        {
            UpdatePanel();
            base.Show();
            UpdatePanel();
        }

        new public void Hide()
        {
            base.Hide();
        }

        public void OnCloseClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_listStats != null)
            {
                m_listStats.Clear();
            }
            Hide();
        }

        public void UpdatePanel()
        {
            if (m_listStats != null)
            {
                // Currently only reason up to Biofuel bus are used.
                StatsContainer[] statsContainers = new StatsContainer[((int) TransferReason.BiofuelBus) + 1];
                statsContainers[0] = TransferManagerStats.s_Stats[255]; // Totals first
                for (int i = 0; i < (int)TransferReason.BiofuelBus; i++)
                {
                    statsContainers[i + 1] = TransferManagerStats.s_Stats[i];
                }
                m_listStats.SetItems(statsContainers);
            }
        }
    }
}