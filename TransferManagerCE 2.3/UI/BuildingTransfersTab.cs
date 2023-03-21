using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransferManagerCE.Common;
using UnifiedUI.Helpers;
using UnityEngine;
using static TransferManagerCE.UIUtils;

namespace TransferManagerCE.UI
{
    internal class BuildingTransfersTab
    {
        public const int iLISTVIEW_OFFERS_HEIGHT = 196;
        public const int iLISTVIEW_MATCHES_HEIGHT = 288;

        private UILabel? m_lblOffers = null;
        private ListView? m_listOffers = null;

        private UILabel? m_lblMatches = null;
        private UITextField? m_txtSearch = null;
        private ListView? m_listMatches = null;

        private BuildingOffers m_buildingOffers = new BuildingOffers();
        private BuildingMatches m_matches = new BuildingMatches();

        public void Setup(UITabStrip tabStrip)
        {
            UIPanel? tabTransfers = tabStrip.AddTabIcon("Transfer", Localization.Get("tabBuildingPanelTransfers"), TransferManagerLoader.LoadResources(), "", 120f);
            if (tabTransfers is not null)
            {
                tabTransfers.autoLayout = true;
                tabTransfers.autoLayoutDirection = LayoutDirection.Vertical;

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
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelOffersColumn2"), "Reason for transfer request", BuildingPanel.iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "Transfer Direction", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listBuildingPanelOffersColumn3"), "Transfer offer priority", BuildingPanel.iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelOffersColumn4"), "Transfer offer Active/Passive", BuildingPanel.iCOLUMN_WIDTH_NORMAL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelOffersColumn5"), "Transfer Offer Amount", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PARK, Localization.Get("listBuildingPanelOffersPark"), "Offer Park #", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelOffersColumn6"), "Offer description", BuildingPanel.iCOLUMN_WIDTH_300, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
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
                AddSpriteButton(ButtonStyle.None, panelMatches, "LineDetailButton", 25, 25);

                // Search field
                m_txtSearch = UIUtils.CreateTextField(ButtonStyle.TextField, panelMatches, "txtSearch", 0.8f, 200f, 25f);
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
                    //m_listMatches.width = width - 20;
                    //m_listMatches.height = iLISTVIEW_MATCHES_HEIGHT;
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listBuildingPanelMatchesColumn1"), "Time of match", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelMatchesColumn2"), "Reason for transfer request", BuildingPanel.iCOLUMN_WIDTH_LARGE, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_ACTIVE, Localization.Get("listBuildingPanelMatchesColumn3"), "Active or Passive for this match", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelMatchesColumn4"), "Transfer match amount", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DISTANCE, "d", "Transfer distance", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, "P", "In priority / Out priority", BuildingPanel.iCOLUMN_WIDTH_TINY, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_PARK, Localization.Get("listBuildingPanelOffersPark"), "Offer Park #", BuildingPanel.iCOLUMN_WIDTH_SMALL, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelMatchesColumn6"), "Match description", BuildingPanel.iCOLUMN_WIDTH_250, BuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }
        }

        public void SetTabBuilding(ushort buildingId)
        {
            if (m_matches is not null)
            {
                m_matches.SetBuildingId(buildingId);
            }
            Clear();
        }

        public BuildingMatches GetBuildingMatches()
        {
            return m_matches;
        }

        public void OnTextChanged(UIComponent component, string value)
        {
            UpdateMatches();
        }

        public void Clear()
        {
            if (m_listOffers is not null)
            {
                m_listOffers.Clear();
            }
            if (m_listMatches is not null)
            {
                m_listMatches.Clear();
            }
        }

        public void UpdateTab()
        {
            UpdateOffers();
            UpdateMatches();
        }

        private void UpdateOffers()
        {
            if (m_listOffers is not null)
            {
                ushort buildingId = BuildingPanel.Instance.GetBuildingId();

                List<OfferData> offers;
                if (buildingId == 0)
                {
                    offers = new List<OfferData>();
                }
                else
                {
                    offers = m_buildingOffers.GetOffersForBuilding(buildingId);
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
                ushort buildingId = BuildingPanel.Instance.GetBuildingId();

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
                }
            }
        }

        public void Destroy()
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
        }
    }
}
