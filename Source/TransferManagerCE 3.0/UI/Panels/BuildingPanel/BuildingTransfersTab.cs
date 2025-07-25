﻿using ColossalFramework.UI;
using SleepyCommon;
using System.Collections.Generic;
using UnifiedUI.Helpers;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;
using static TransferManagerCE.UI.BuildingPanel;

namespace TransferManagerCE.UI
{
    public class BuildingTransfersTab : BuildingTab
    {
        public const int iLISTVIEW_OFFERS_HEIGHT = 196;
        public const int iLISTVIEW_MATCHES_HEIGHT = 284;

        private UILabel? m_lblOffers = null;
        private ListView? m_listOffers = null;

        private UILabel? m_lblMatches = null;
        private UITextField? m_txtSearch = null;
        private ListView? m_listMatches = null;

        private BuildingOffers m_buildingOffers = new BuildingOffers();
        private BuildingMatches m_matches = new BuildingMatches();

        // ----------------------------------------------------------------------------------------
        public override void SetupInternal()
        {
            UIPanel? tabTransfers = m_tabStrip.AddTabIcon("Transfer", Localization.Get("tabBuildingPanelTransfers"), TransferManagerMod.Instance.LoadResources(), "", 120f);
            if (tabTransfers is not null)
            {
                tabTransfers.autoLayout = true;
                tabTransfers.autoLayoutDirection = LayoutDirection.Vertical;
                //tabTransfers.backgroundSprite = "InfoviewPanel";
                //tabTransfers.color = new Color32(150, 0, 0, 255);

                // Object label
                m_lblOffers = tabTransfers.AddUIComponent<UILabel>();
                m_lblOffers.width = tabTransfers.width;
                m_lblOffers.height = 20;
                m_lblOffers.padding = new RectOffset(4, 4, 4, 4);
                m_lblOffers.text = Localization.Get("labelBuildingPanelOffers");

                // Offer list
                m_listOffers = ListView.Create<UIOfferRow>(tabTransfers, "ScrollbarTrack", 0.8f, tabTransfers.width, iLISTVIEW_OFFERS_HEIGHT);
                if (m_listOffers is not null)
                {
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelOffersColumn2"), "Reason for transfer request", UIOfferRow.ColumnWidths[0], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "Transfer Direction", UIOfferRow.ColumnWidths[1], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listBuildingPanelOffersColumn3"), "Transfer offer priority", UIOfferRow.ColumnWidths[2], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelOffersColumn4"), "Transfer offer Active/Passive", UIOfferRow.ColumnWidths[3], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelOffersColumn5"), "Transfer Offer Amount", UIOfferRow.ColumnWidths[4], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PARK, Localization.Get("listBuildingPanelOffersPark"), "Offer Park #", UIOfferRow.ColumnWidths[5], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelOffersColumn6"), "Offer description", UIOfferRow.ColumnWidths[6], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listOffers.Header.ResizeLastColumn();
                    m_listOffers.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }

                UIPanel panelMatches = tabTransfers.AddUIComponent<UIPanel>();
                panelMatches.width = tabTransfers.width;
                panelMatches.height = 30;
                panelMatches.autoLayout = true;
                panelMatches.autoLayoutDirection = LayoutDirection.Horizontal;

                // Matches label
                m_lblMatches = panelMatches.AddUIComponent<UILabel>();
                m_lblMatches.width = 574;
                m_lblMatches.height = 25;
                m_lblMatches.autoSize = false;
                m_lblMatches.padding = new RectOffset(4, 4, 4, 4);
                m_lblMatches.text = Localization.Get("labelBuildingPanelMatchOffers");

                // Search button
                UIMyUtils.AddSpriteButton(UIMyUtils.ButtonStyle.None, panelMatches, "LineDetailButton", 25, 25);

                // Search field
                m_txtSearch = UIMyUtils.CreateTextField(UIMyUtils.ButtonStyle.TextField, panelMatches, "txtSearch", 0.8f, 200f, 25f);
                m_txtSearch.eventTextChanged += OnTextChanged;
                m_txtSearch.eventMouseLeave += (sender, e) =>
                {
                    if (m_txtSearch.hasFocus)
                    {
                        m_txtSearch.Unfocus();
                    }
                };

                // Match list
                m_listMatches = ListView.Create<UIMatchRow>(tabTransfers, "ScrollbarTrack", 0.7f, panelMatches.width, iLISTVIEW_MATCHES_HEIGHT);
                if (m_listMatches is not null)
                {
                    m_listMatches.height = tabTransfers.height - m_lblOffers.height - m_listOffers.height - m_lblMatches.height - 6;
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listBuildingPanelMatchesColumn1"), "Time of match", UIMatchRow.ColumnWidths[0], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelMatchesColumn2"), "Reason for transfer request", UIMatchRow.ColumnWidths[1], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "", UIMatchRow.ColumnWidths[2], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelMatchesColumn3"), "Active or Passive for this match", UIMatchRow.ColumnWidths[3], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelMatchesColumn4"), "Transfer match amount", UIMatchRow.ColumnWidths[4], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Transfer distance", UIMatchRow.ColumnWidths[5], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, "P", "In priority / Out priority", UIMatchRow.ColumnWidths[6], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PARK, Localization.Get("listBuildingPanelOffersPark"), "Offer Park #", UIMatchRow.ColumnWidths[7], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelMatchesColumn6"), "Match description", UIMatchRow.ColumnWidths[8], BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listMatches.Header.ResizeLastColumn();
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }
        }

        public override void SetTabBuilding(ushort buildingId, BuildingType buildingType, List<ushort> subBuildingIds)
        {
            base.SetTabBuilding(buildingId, buildingType, subBuildingIds);

            if (m_matches is not null)
            {
                m_matches.SetBuildingIds(buildingId, subBuildingIds);
            }

            // Match logging
            if (MatchLogging.Instance is not null)
            {
                MatchLogging.Instance.SetBuildingId(buildingId, subBuildingIds);
            }

            Clear();
        }

        public override bool ShowTab()
        {
            return true; // Always show tab
        }

        public BuildingMatches GetBuildingMatches()
        {
            return m_matches;
        }

        public void OnTextChanged(UIComponent component, string value)
        {
            UpdateMatches();
        }

        public override bool UpdateTab(bool bActive)
        {
            if (!base.UpdateTab(bActive))
            {
                return false;
            }

            if (m_tabStrip.IsTabVisible((int)TabIndex.TAB_STATUS))
            {
                // Make "Transfers" tab compact if lots of tabs are visible
                m_tabStrip.SetCompactMode((int)TabIndex.TAB_TRANSFERS, m_tabStrip.GetVisibleTabCount() > 5);
                m_tabStrip.PerformLayout();
            }

            if (bActive)
            {
                UpdateOffers();
                UpdateMatches();
            }
            else
            {
                Clear();
            }

            return true;
        }

        private void UpdateOffers()
        {
            if (m_listOffers is not null)
            {
                ushort buildingId = BuildingPanel.Instance.Building;

                List<OfferData> offers;
                if (buildingId == 0)
                {
                    offers = new List<OfferData>();
                }
                else
                {
                    offers = m_buildingOffers.GetOffersForBuilding(buildingId, BuildingPanel.Instance.GetSubBuildingIds());
                    offers.Sort();
                }

                m_listOffers.GetList().rowsData = new FastList<object>
                {
                    m_buffer = offers.ToArray(),
                    m_size = offers.Count,
                };
            }
        }

        private void UpdateMatches()
        {
            if (m_listMatches is not null && MatchLogging.Instance is not null)
            {
                ushort buildingId = BuildingPanel.Instance.Building;

                List<BuildingMatchData>? listMatches;
                if (buildingId == 0)
                {
                    listMatches = new List<BuildingMatchData>();
                }
                else
                {
                    if (m_txtSearch.text.Length > 0)
                    {
                        string sSearch = m_txtSearch.text.ToUpper();

                        List<BuildingMatchData> listAllMatches = GetBuildingMatches().GetSortedBuildingMatches();

                        // Filter matches
                        listMatches = new List<BuildingMatchData>();
                        foreach (BuildingMatchData match in listAllMatches)
                        {
                            if (match.Contains(sSearch))
                            {
                                listMatches.Add(match);
                            }
                        }
                    }
                    else
                    {
                        listMatches = GetBuildingMatches().GetSortedBuildingMatches();
                    }
                }

                if (listMatches is not null && m_listMatches.GetList() is not null)
                {
                    m_listMatches.GetList().rowsData = new FastList<object>
                    {
                        m_buffer = listMatches.ToArray(),
                        m_size = listMatches.Count,
                    };

                    m_lblMatches.text = $"{Localization.Get("labelBuildingPanelMatchOffers")} ({listMatches.Count})";
                }
            }
        }

        public override void Clear()
        {
            if (m_listOffers is not null)
            {
                m_listOffers.Clear();
            }
            if (m_listMatches is not null)
            {
                m_listMatches.Clear();
            }

            base.Clear();
        }

        public override void Destroy()
        {
            if (m_listOffers is not null)
            {
                m_listOffers.Destroy();
                m_listOffers = null;
            }
            if (m_listMatches is not null)
            {
                m_listMatches.Destroy();
                m_listMatches = null;
            }

            base.Destroy();
        }
    }
}
