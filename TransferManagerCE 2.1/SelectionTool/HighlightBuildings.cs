using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using UnityEngine;

namespace TransferManagerCE
{
    public class HighlightBuildings
    {
        private HashSet<KeyValuePair<ushort, Color>> m_highlightBuildings = new HashSet<KeyValuePair<ushort, Color>>();

        public void Highlight(ToolManager toolManager, Building[] BuildingBuffer, RenderManager.CameraInfo cameraInfo)
        {
            // Now highlight buildings
            foreach (KeyValuePair<ushort, Color> kvp in m_highlightBuildings)
            {
                SelectionTool.HighlightBuilding(toolManager, BuildingBuffer, kvp.Key, cameraInfo, kvp.Value);
            }
        }

        public void LoadMatches()
        {
            switch (ModSettings.GetSettings().HighlightMatches)
            {
                case ModSettings.HighlightMode.None:
                    {
                        m_highlightBuildings.Clear();
                        break;
                    }
                case ModSettings.HighlightMode.Matches:
                    {
                        ushort buildingId = BuildingPanel.Instance.GetBuildingId();
                        LoadBuildingMatches(buildingId);
                        break;
                    }
                case ModSettings.HighlightMode.Issues:
                    {
                        // Alternate mode load buildings depending on selected building type.
                        ushort buildingId = BuildingPanel.Instance.GetBuildingId();
                        BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(buildingId);
                        switch (eType)
                        {
                            case BuildingTypeHelper.BuildingType.Hospital:
                            case BuildingTypeHelper.BuildingType.MedicalHelicopterDepot:
                                {
                                    LoadBuildingsWithSickCitizens(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.Cemetery:
                                {
                                    LoadBuildingsWithDeadCitizens(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.Recycling:
                            case BuildingTypeHelper.BuildingType.Landfill:
                            case BuildingTypeHelper.BuildingType.WasteProcessing:
                            case BuildingTypeHelper.BuildingType.WasteTransfer:
                                {
                                    LoadBuildingsWithGarbage(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.PoliceHelicopterDepot:
                            case BuildingTypeHelper.BuildingType.PoliceStation:
                                {
                                    LoadBuildingsWithCrime(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.FireHelicopterDepot:
                            case BuildingTypeHelper.BuildingType.FireStation:
                                {
                                    LoadBuildingsWithFire(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.PostOffice:
                            case BuildingTypeHelper.BuildingType.PostSortingFacility:
                                {
                                    LoadBuildingsWithMail(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.Bank:
                                {
                                    LoadBuildingsWithCash(buildingId); 
                                    break;
                                }
                            default:
                                {
                                    m_highlightBuildings.Clear();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void LoadBuildingMatches(ushort usSourceBuildingId)
        {
            m_highlightBuildings.Clear();

            if (ModSettings.GetSettings().HighlightMatches == ModSettings.HighlightMode.Matches && 
                BuildingPanel.Instance != null)
            {
                // Limit the number of buildings to highlight
                const int iMAX_BUILDINGS = 100;

                List<BuildingMatchData>? listMatches = BuildingPanel.Instance.GetBuildingMatches().GetSortedBuildingMatches();
                if (listMatches != null && listMatches.Count > 0)
                {
                    int iCount = Math.Min(iMAX_BUILDINGS, listMatches.Count);
                    for (int i = 0; i < iCount; ++i)
                    {
                        BuildingMatchData matchData = listMatches[i];

                        // A match can now produce multiple buildings (Service point)
                        List<ushort> buildings;
                        if (matchData.m_outgoing.GetBuildings().Contains(usSourceBuildingId))
                        {
                            buildings = matchData.m_incoming.GetBuildings();
                        }
                        else
                        {
                            buildings = matchData.m_outgoing.GetBuildings();
                        }

                        foreach (ushort usBuildingId in buildings)
                        {
                            if (usBuildingId != 0 && usBuildingId != usSourceBuildingId)
                            {
                                Color color = TransferManagerModes.GetTransferReasonColor(matchData.m_material);
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>(usBuildingId, color));
                            }
                        }
                    }
                }
            }
        }

        private void LoadBuildingsWithSickCitizens(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case HospitalAI:
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                break;
                            }
                        default:
                            {
                                if (CitiesUtils.GetSick((ushort)i, building).Count > 0)
                                {
                                    int Priority = building.m_healthProblemTimer * 7 / 128;
                                    if (Priority >= 2)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsWithDeadCitizens(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case CemeteryAI:
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                break;
                            }
                        default:
                            {
                                if (CitiesUtils.GetDead((ushort)i, building).Count > 0)
                                {
                                    int Priority = building.m_deathProblemTimer * 7 / 128;
                                    if (Priority >= 2)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsWithGarbage(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case LandfillSiteAI:
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                break;
                            }
                        default:
                            {
                                if (building.m_garbageBuffer >= 1000)
                                {
                                    if (building.m_garbageBuffer >= 2000)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }
        private void LoadBuildingsWithCrime(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case PoliceStationAI:
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                break;
                            }
                        default:
                            {
                                if (building.m_citizenCount > 0 && building.m_crimeBuffer >= (building.m_citizenCount * 15))
                                {
                                    int Priority = building.m_crimeBuffer / Mathf.Max(1, building.m_citizenCount * 10);
                                    if (Priority >= 2)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsWithCash(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case BankOfficeAI:
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                break;
                            }
                        case CommercialBuildingAI:
                            {
                                int cashCapacity = StatusDataCash.GetCashCapacity((ushort) i, building);
                                if (cashCapacity != 0 && building.m_cashBuffer >= cashCapacity / 8)
                                {
                                    int iPriority = building.m_cashBuffer * 8 / cashCapacity;
                                    if (iPriority >= 2)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsWithFire(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    switch (building.Info.GetAI())
                    {
                        case FireStationAI:
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                break;
                            }
                        default:
                            {
                                if (building.m_fireIntensity > 0)
                                {
                                    int Priority = building.m_fireIntensity * 8 / 255;
                                    if (Priority >= 2)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsWithMail(ushort usSourceBuildingId)
        {
            // Highlight sick citizens
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;

            m_highlightBuildings.Clear();
            for (int i = 0; i < BuildingBuffer.Length; i++)
            {
                // Dont highlight current building
                if (i == usSourceBuildingId)
                {
                    continue;
                }

                Building building = BuildingBuffer[i];
                if (building.m_flags != 0)
                {
                    if (building.Info != null)
                    {
                        switch (building.Info.GetAI())
                        {
                            case PostOfficeAI:
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                                    break;
                                }
                            case ResidentialBuildingAI:
                                {
                                    int iMaxMail = CitiesUtils.GetHomeCount(building) * 50;
                                    if (iMaxMail > 0)
                                    {
                                        int iPriority = building.m_mailBuffer * 8 / iMaxMail;
                                        if (iPriority >= 2)
                                        {
                                            m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
        }
    }
}
