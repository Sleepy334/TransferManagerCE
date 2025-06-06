using SleepyCommon;
using System;
using System.Collections.Generic;
using TransferManagerCE.CustomManager;
using TransferManagerCE.Data;
using TransferManagerCE.Settings;
using TransferManagerCE.UI;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

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
                RendererUtils.HighlightBuilding(BuildingBuffer, kvp.Key, cameraInfo, kvp.Value);
            }
        }

        public void LoadMatches()
        {
            //LoadBuildingsWorkers(BuildingPanel.Instance.GetBuildingId());
            //return;

            //Stopwatch stopwatch = Stopwatch.StartNew();
            //long startTicks = stopwatch.ElapsedTicks;

            switch ((ModSettings.BuildingHighlightMode) ModSettings.GetSettings().HighlightMatchesState)
            {
                case ModSettings.BuildingHighlightMode.None:
                    {
                        m_highlightBuildings.Clear();
                        break;
                    }
                case ModSettings.BuildingHighlightMode.Matches:
                    {
                        ushort buildingId = BuildingPanel.Instance.GetBuildingId();
                        LoadBuildingMatches(buildingId);
                        break;
                    }
                case ModSettings.BuildingHighlightMode.Issues:
                    {
                        // Alternate mode load buildings depending on selected building type.
                        ushort buildingId = BuildingPanel.Instance.GetBuildingId();

                        BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(buildingId);
                        switch (eType)
                        {
                            case BuildingType.Hospital:
                            case BuildingType.MedicalHelicopterDepot:
                            case BuildingType.UniversityHospital:
                            case BuildingType.Eldercare:
                            case BuildingType.Childcare:
                                {
                                    LoadBuildingsWithSick(buildingId);
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
                            case BuildingTypeHelper.BuildingType.Prison:
                                {
                                    LoadBuildingsWithCrime(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.FireHelicopterDepot:
                            case BuildingTypeHelper.BuildingType.FireStation:
                            case BuildingTypeHelper.BuildingType.FirewatchTower:
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
                            case BuildingTypeHelper.BuildingType.ProcessingFacility:
                            case BuildingTypeHelper.BuildingType.UniqueFactory:
                            case BuildingTypeHelper.BuildingType.GenericExtractor:
                            case BuildingTypeHelper.BuildingType.GenericProcessing:
                            case BuildingTypeHelper.BuildingType.GenericFactory:
                                {
                                    LoadBuildingsFactoryIssues(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.Commercial:
                                {
                                    LoadBuildingsCommercialIssues(buildingId);
                                    break;
                                }
                            case BuildingTypeHelper.BuildingType.ServicePoint:
                                {
                                    LoadBuildingsServicePoints(buildingId);
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

            //long stopTicks = stopwatch.ElapsedTicks;
            //CDebug.Log($"{((double)(stopTicks - startTicks) * 0.0001).ToString("F")}ms");
        }

        private void LoadBuildingMatches(ushort usSourceBuildingId)
        {
            m_highlightBuildings.Clear();

            if ((ModSettings.BuildingHighlightMode)ModSettings.GetSettings().HighlightMatchesState == ModSettings.BuildingHighlightMode.Matches && 
                BuildingPanel.Exists)
            {
                // Limit the number of buildings to highlight
                const int iMAX_BUILDINGS = 100;

                List<BuildingMatchData>? listMatches = BuildingPanel.Instance.GetBuildingMatches().GetSortedBuildingMatches();
                if (listMatches is not null && listMatches.Count > 0)
                {
                    int iCount = Math.Min(iMAX_BUILDINGS, listMatches.Count);
                    for (int i = 0; i < iCount; ++i)
                    {
                        BuildingMatchData matchData = listMatches[i];

                        // A match can now produce multiple buildings (Service point)
                        HashSet<ushort> buildings;
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

        private void LoadBuildingsWithSick(ushort usSourceBuildingId)
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
                    if (building.Info.GetService() == ItemClass.Service.HealthCare && building.Info.GetAI() is not CemeteryAI)
                    {
                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                    }
                    else if (BuildingUtils.GetSickCount((ushort)i, building) > 0)
                    {
                        if (building.m_healthProblemTimer >= SickHandler.iSICK_MAJOR_PROBLEM_TIMER_VALUE)
                        {
                            m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.orange));
                        }
                        else if (building.m_healthProblemTimer >= SickHandler.iSICK_MINOR_PROBLEM_TIMER_VALUE)
                        {
                            m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                        }
                        else
                        {
                            m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
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
                                if (BuildingUtils.GetDeadCount((ushort)i, building) > 0)
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
                                if (building.m_garbageBuffer >= 3000)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                }
                                else if (building.m_garbageBuffer >= 2000)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
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
                    if (building.Info.GetService() == ItemClass.Service.PoliceDepartment)
                    {
                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                    }
                    else
                    {
                        int iCitizenCount = CrimeCitizenCountStorage.GetCitizenCount(usSourceBuildingId, building);
                        if (iCitizenCount > 0)
                        {
                            // Show high priority buildings first
                            if (building.m_crimeBuffer > 0)
                            {
                                int iCrimeRate = building.m_crimeBuffer / iCitizenCount;
                                if (iCrimeRate >= StatusDataCrime.iMAJOR_CRIME_RATE)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.orange));
                                    continue;
                                }
                                else if (iCrimeRate >= StatusDataCrime.iMINOR_CRIME_RATE)
                                { 
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    continue;
                                }
                                else if (iCrimeRate >= 10)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    continue;
                                }
                            }

                            // If we havent colored it anything else then highlight any buildings with more than 1 criminals in them
                            int iCriminalCount = BuildingUtils.GetCriminalCount((ushort)i, building);
                            if (iCriminalCount > 1)
                            {
                                m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.yellow));
                            }
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
                                    if (building.m_cashBuffer >= cashCapacity)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.orange));
                                    }
                                    else
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
                    if (building.Info is not null)
                    {
                        if (building.Info.GetAI() is PostOfficeAI)
                        {
                            m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                        } 
                        else
                        {
                            int iMaxMail = StatusDataBuildingMail.GetMaxMail((ushort) i, building);
                            if (iMaxMail > 0)
                            {
                                if (building.m_mailBuffer >= iMaxMail)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.orange));
                                }
                                else
                                {
                                    int iPriority = building.m_mailBuffer * 8 / iMaxMail;
                                    if (iPriority >= 5)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                    }
                                    else if (iPriority >= 2)
                                    {
                                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadBuildingsFactoryIssues(ushort usSourceBuildingId)
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
                    BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                    switch (eType)
                    {
                        case BuildingTypeHelper.BuildingType.ProcessingFacility:
                        case BuildingTypeHelper.BuildingType.UniqueFactory:
                        case BuildingTypeHelper.BuildingType.GenericExtractor:
                        case BuildingTypeHelper.BuildingType.GenericProcessing:
                        case BuildingTypeHelper.BuildingType.GenericFactory:
                            {
                                if (building.m_incomingProblemTimer > 32)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                }
                                else if (building.m_incomingProblemTimer > 0)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                }
                                else if (building.m_outgoingProblemTimer > 32)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.orange));
                                }
                                else if (building.m_outgoingProblemTimer > 0)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.gold));
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsCommercialIssues(ushort usSourceBuildingId)
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
                    BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(building);
                    switch (eType)
                    {
                        case BuildingTypeHelper.BuildingType.Commercial:
                            {
                                if (building.m_incomingProblemTimer > 32)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                                }
                                else if (building.m_incomingProblemTimer > 0)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.cyan));
                                }
                                if (building.m_outgoingProblemTimer > 32)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.orange));
                                }
                                else if (building.m_outgoingProblemTimer > 0)
                                {
                                    m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, KnownColor.gold));
                                }
                                break;
                            }
                    }
                }
            }
        }

        private void LoadBuildingsServicePoints(ushort usSourceBuildingId)
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
                    if (BuildingTypeHelper.GetBuildingType(building) == BuildingTypeHelper.BuildingType.ServicePoint)
                    {
                        m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.green));
                    }
                }
            }
        }

        private void LoadBuildingsWorkers(ushort usSourceBuildingId)
        {
            // Highlight workers
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
                    int iWorkerPlaces = BuildingUtils.GetTotalWorkerPlaces((ushort) i, building, out int workPlaces0, out int workPlaces1, out int workPlaces2, out int workPlaces3);
                    if (iWorkerPlaces > 0)
                    {
                        int iWorkerCount = BuildingUtils.GetCurrentWorkerCount((ushort)i, building, out int worker0, out int worker1, out int worker2, out int worker3);
                        if (iWorkerCount < iWorkerPlaces)
                        {
                            m_highlightBuildings.Add(new KeyValuePair<ushort, Color>((ushort)i, Color.blue));
                        }
                    }
                }
            }
        }
    }
}
