using ColossalFramework;
using ColossalFramework.UI;
using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Patch;
using TransferManagerCE.Settings;
using TransferManagerCE.Util;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class TransferBuildingPanel : UIPanel
    {
        const int iMARGIN = 8;
        public const int iHEADER_HEIGHT = 20;

        public const int iLISTVIEW_MATCHES_HEIGHT = 200;
        public const int iLISTVIEW_OFFERS_HEIGHT = 300;

        public const int iCOLUMN_WIDTH = 80;
        public const int iCOLUMN_MATERIAL_WIDTH = 150;
        public const int iCOLUMN_VEHICLE_WIDTH = 200;
        public const int iCOLUMN_DESCRIPTION_WIDTH = 300;

        private UITitleBar? m_title = null;
        private UILabel? m_lblSource = null;
        private UILabel? m_lblOffers = null;
        private UILabel? m_lblMatches = null;
        private ListView m_listOffers = null;
        private ListView m_listMatches = null;
        private ListView m_listStatus = null;
        private UITabStrip? m_tabStrip = null;

        public ushort m_buildingId = 0;

        private List<OfferData> m_TransferOffers = null;
        private List<MatchData> m_Matches = null;

        public TransferBuildingPanel() : base()
        {
            m_TransferOffers = new List<OfferData>();
            m_Matches = new List<MatchData>();
        }

        public override void Start()
        {
            base.Start();
            name = "TransferBuildingPanel";
            width = 700;
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
            m_title.title = "Transfer Manager CE";

            // Object label
            m_lblSource = AddUIComponent<UILabel>();
            m_lblSource.width = width - (iMARGIN * 2);
            m_lblSource.height = 25;
            m_lblSource.padding = new RectOffset(4, 4, 4, 4);
            m_lblSource.text = "Select Building";
            m_lblSource.textAlignment = UIHorizontalAlignment.Center;
            m_lblSource.eventMouseEnter += (c, e) =>
            {
                m_lblSource.textColor = new Color32(13, 183, 255, 255);
            };
            m_lblSource.eventMouseLeave += (c, e) =>
            {
                m_lblSource.textColor = Color.white;
            };
            m_lblSource.eventClick += (c, e) =>
            {
                if (m_buildingId != 0)
                {
                    InstanceID oInstanceId = new InstanceID { Building = m_buildingId };
                    Building building = BuildingManager.instance.m_buildings.m_buffer[m_buildingId];
                    Vector3 oPosition = building.m_position;
                    ToolsModifierControl.cameraController.SetTarget(oInstanceId, oPosition, false);
                }
            };

            m_tabStrip = UITabStrip.Create(this, width - 20f, height - m_lblSource.height - m_title.height - 10, null);
            UIPanel? tabStatus = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelStatus"));
            if (tabStatus != null)
            {
                tabStatus.autoLayout = true;
                tabStatus.autoLayoutDirection = LayoutDirection.Vertical;

                // Issue list
                m_listStatus = ListView.Create(tabStatus, "ScrollbarTrack", 0.8f, tabStatus.width, tabStatus.height - 10);
                if (m_listStatus != null)
                {
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelStatusColumn1"), "Type of material", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_VALUE, Localization.Get("listBuildingPanelStatusColumn2"), "Current value", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_OWNER, Localization.Get("listBuildingPanelStatusColumn3"), "Responder", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_TARGET, Localization.Get("listBuildingPanelStatusColumn4"), "Vehicle", iCOLUMN_VEHICLE_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listStatus.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelStatusColumn5"), "Status description", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listStatus.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }
            }

            UIPanel? tabTransfers = m_tabStrip.AddTab(Localization.Get("tabBuildingPanelTransfers"));
            if (tabTransfers != null)
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
                m_listOffers = ListView.Create(tabTransfers, "ScrollbarTrack", 0.8f, tabTransfers.width, iLISTVIEW_OFFERS_HEIGHT);
                if (m_listOffers != null)
                {
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_INOUT, Localization.Get("listBuildingPanelOffersColumn1"), "Transfer Direction", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelOffersColumn2"), "Reason for transfer request", iCOLUMN_MATERIAL_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_PRIORITY, Localization.Get("listBuildingPanelOffersColumn3"), "Transfer offer priority", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    // Active goes here in next version
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelOffersColumn5"), "Transfer Offer Amount", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listOffers.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelOffersColumn6"), "Offer description", iCOLUMN_DESCRIPTION_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listOffers.HandleSort(ListViewRowComparer.Columns.COLUMN_PRIORITY);
                }


                m_lblMatches = tabTransfers.AddUIComponent<UILabel>();
                m_lblMatches.width = tabTransfers.width;
                m_lblMatches.height = 20;
                m_lblMatches.padding = new RectOffset(4, 4, 4, 4);
                m_lblMatches.text = "Match Offers";

                // Offer list
                m_listMatches = ListView.Create(tabTransfers, "ScrollbarTrack", 0.8f, tabTransfers.width, 144f);
                if (m_listMatches != null)
                {
                    //m_listMatches.width = width - 20;
                    //m_listMatches.height = iLISTVIEW_MATCHES_HEIGHT;
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_TIME, Localization.Get("listBuildingPanelMatchesColumn1"), "Time of match", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_MATERIAL, Localization.Get("listBuildingPanelMatchesColumn2"), "Reason for transfer request", iCOLUMN_MATERIAL_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopLeft, null);
                    // Active goes here next version
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_AMOUNT, Localization.Get("listBuildingPanelMatchesColumn4"), "Transfer match amount", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DELTAAMOUNT, Localization.Get("listBuildingPanelMatchesColumn5"), "Transfer match delta", iCOLUMN_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Center, UIAlignAnchor.TopRight, null);
                    m_listMatches.AddColumn(ListViewRowComparer.Columns.COLUMN_DESCRIPTION, Localization.Get("listBuildingPanelMatchesColumn6"), "Match description", iCOLUMN_DESCRIPTION_WIDTH, TransferBuildingPanel.iHEADER_HEIGHT, UIHorizontalAlignment.Left, UIAlignAnchor.TopRight, null);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                    m_listMatches.HandleSort(ListViewRowComparer.Columns.COLUMN_TIME);
                }
            }

            m_tabStrip.SelectTabIndex(0);

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

        public void SetPanelBuilding(ushort buildingId)
        {
            m_buildingId = buildingId;
            m_TransferOffers.Clear();
            m_Matches.Clear();
            UpdatePanel();
        }

        public void StartTransfer(TransferReason material, TransferOffer outgoingOffer, TransferOffer incomingOffer, int deltaamount)
        {
            if (m_buildingId != 0)
            {
                if (outgoingOffer.Building == m_buildingId)
                {
                    m_Matches.Insert(0, new MatchData(material, false, outgoingOffer, incomingOffer, deltaamount));
                }
                if (outgoingOffer.Citizen != 0)
                {
                    Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[outgoingOffer.Citizen];
                    if (oCitizen.GetBuildingByLocation() == m_buildingId)
                    {
                        m_Matches.Insert(0, new MatchData(material, false, outgoingOffer, incomingOffer, deltaamount));
                    }
                }

                if (incomingOffer.Building == m_buildingId)
                {
                    m_Matches.Insert(0, new MatchData(material, true, outgoingOffer, incomingOffer, deltaamount));
                }
                if (incomingOffer.Citizen != 0)
                {
                    Citizen oCitizen = CitizenManager.instance.m_citizens.m_buffer[incomingOffer.Citizen];
                    if (oCitizen.GetBuildingByLocation() == m_buildingId)
                    {
                        m_Matches.Insert(0, new MatchData(material, true, outgoingOffer, incomingOffer, deltaamount));
                    }
                }
            }

            // Limit matches to last 20.
            while (m_Matches.Count > 20)
            {
                m_Matches.RemoveAt(m_Matches.Count - 1);
            }
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
            Hide();
        }

        private List<StatusContainer> GetStatusList()
        {
            List<StatusContainer> listStatus = new List<StatusContainer>();

            if (m_buildingId != 0)
            {
                bool bAddedDead = false;
                bool bAddedSick = false;
                bool bAddedGarbage = false;
                bool bAddedFire = false;
                bool bAddedCrime = false;
                bool bAddedMail = false;

                List<ushort> vehicles = CitiesUtils.GetVehiclesForBuilding(m_buildingId);
                foreach (ushort vehicleId in vehicles) 
                {
                    Vehicle vehicle = VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                    if ((vehicle.m_flags & Vehicle.Flags.TransferToSource) == Vehicle.Flags.TransferToSource && vehicle.Info != null)
                    {
                        // Found a vehicle for this building
                        if (vehicle.Info.m_vehicleAI is HearseAI)
                        {
                            // Hearse
                            listStatus.Add(new StatusContainer(new StatusDataDead(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedDead = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is AmbulanceAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataSick(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedSick = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is GarbageTruckAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataGarbage(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedGarbage = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is FireTruckAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataFire(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedFire = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is FireCopterAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataFire2(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedFire = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is PoliceCarAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataCrime(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedCrime = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is PoliceCopterAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataCrime(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedCrime = true;
                        }
                        else if (vehicle.Info.m_vehicleAI is PostVanAI)
                        {
                            listStatus.Add(new StatusContainer(new StatusDataMail(m_buildingId, vehicle.m_sourceBuilding, vehicleId)));
                            bAddedMail = true;
                        }
                        
                    }
                }
                if (!bAddedDead)
                {
                    listStatus.Add(new StatusContainer(new StatusDataDead(m_buildingId, 0, 0)));
                }
                if (!bAddedSick)
                {
                    listStatus.Add(new StatusContainer(new StatusDataSick(m_buildingId, 0, 0)));
                }
                if (!bAddedGarbage)
                {
                    listStatus.Add(new StatusContainer(new StatusDataGarbage(m_buildingId, 0, 0)));
                }
                if (!bAddedFire)
                {
                    listStatus.Add(new StatusContainer(new StatusDataFire(m_buildingId, 0, 0)));
                }
                if (!bAddedCrime)
                {
                    listStatus.Add(new StatusContainer(new StatusDataCrime(m_buildingId, 0, 0)));
                }
                if (!bAddedMail)
                {
                    listStatus.Add(new StatusContainer(new StatusDataMail(m_buildingId, 0, 0)));
                }
            }

            return listStatus;
        }

        public void UpdatePanel()
        {
            if (m_buildingId != 0)
            {
                if (m_lblSource != null)
                {
                    m_lblSource.text = CitiesUtils.GetBuildingName(m_buildingId);
                }
                if (m_listOffers != null)
                {
                    List<OfferData> offers = TransferManagerUtils.GetOffersForBuilding(m_buildingId);
                    offers.Sort();
                    m_listOffers.SetItems(offers.ToArray());
                }
                if (m_listMatches != null)
                {
                    m_listMatches.SetItems(m_Matches.ToArray());
                }
                if (m_listStatus != null)
                {
                    List<StatusContainer> list = GetStatusList();
                    list.Sort();
                    m_listStatus.SetItems(list.ToArray());
                }
            }
        }
    }
}