using System;
using System.Collections.Generic;
using TransferManagerCE.Util;

namespace TransferManagerCE
{
    public class BuildingSettings : IEquatable<BuildingSettings>
    {
        public enum PreferLocal
        {
            ALL_DISTRICTS,
            PREFER_LOCAL_DISTRICT,
            RESTRICT_LOCAL_DISTRICT,
            ALL_DISTRICTS_EXCEPT_FOR,
        }

        public enum ImportExport
        {
            ALLOW_IMPORT_AND_EXPORT,
            ALLOW_IMPORT_ONLY,
            ALLOW_EXPORT_ONLY,
            ALLOW_NEITHER,
        }

        public static Dictionary<ushort, BuildingSettings> s_BuildingsSettings = new Dictionary<ushort, BuildingSettings>();
        static readonly object s_dictionaryLock = new object();

        // Building settings
        public bool m_bAllowImport;
        public bool m_bAllowExport;
        
        public PreferLocal m_iPreferLocalDistrictsIncoming;
        public PreferLocal m_iPreferLocalDistrictsOutgoing;
        public bool m_bDistrictAllowServices;
        public int m_iServiceDistance;

        public bool m_bWarehouseOverride;
        public bool m_bWarehouseFirst;
        public int m_iWarehouseReserveTrucksPercent;
        public int m_iOutsideMultiplier;

        public bool m_bIncomingAllowLocalDistrict;
        public bool m_bIncomingAllowLocalPark;
        public bool m_bOutgoingAllowLocalDistrict;
        public bool m_bOutgoingAllowLocalPark;
        public HashSet<DistrictData> m_incomingDistrictAllowed;
        public HashSet<DistrictData> m_outgoingDistrictAllowed;

        public BuildingSettings()
        {
            m_bAllowImport = true;
            m_bAllowExport = true;

            m_iPreferLocalDistrictsIncoming = PreferLocal.ALL_DISTRICTS;
            m_iPreferLocalDistrictsOutgoing = PreferLocal.ALL_DISTRICTS;
            m_bDistrictAllowServices = true;
            m_iServiceDistance = 0;

            m_bWarehouseOverride = false;
            m_bWarehouseFirst = SaveGameSettings.GetSettings().WarehouseFirst;
            m_iWarehouseReserveTrucksPercent = SaveGameSettings.GetSettings().WarehouseReserveTrucksPercent;
            m_iOutsideMultiplier = 0;

            m_bIncomingAllowLocalDistrict = true;
            m_bIncomingAllowLocalPark = true;
            m_incomingDistrictAllowed = new HashSet<DistrictData>();

            m_bOutgoingAllowLocalDistrict = true;
            m_bOutgoingAllowLocalPark = true;
            m_outgoingDistrictAllowed = new HashSet<DistrictData>();
        }

        public BuildingSettings(BuildingSettings oSecond)
        {
            m_bAllowImport = oSecond.m_bAllowImport;
            m_bAllowExport = oSecond.m_bAllowExport;
            
            m_iPreferLocalDistrictsIncoming = oSecond.m_iPreferLocalDistrictsIncoming;
            m_iPreferLocalDistrictsOutgoing = oSecond.m_iPreferLocalDistrictsOutgoing;
            m_bDistrictAllowServices = oSecond.m_bDistrictAllowServices;
            m_iServiceDistance = oSecond.m_iServiceDistance;

            m_bWarehouseOverride = oSecond.m_bWarehouseOverride;
            m_bWarehouseFirst = oSecond.m_bWarehouseFirst;
            m_iWarehouseReserveTrucksPercent = oSecond.m_iWarehouseReserveTrucksPercent;
            m_iOutsideMultiplier = oSecond.m_iOutsideMultiplier;

            m_bIncomingAllowLocalDistrict = oSecond.m_bIncomingAllowLocalDistrict;
            m_bIncomingAllowLocalPark = oSecond.m_bIncomingAllowLocalPark;
            m_incomingDistrictAllowed = new HashSet<DistrictData>(oSecond.m_incomingDistrictAllowed);

            m_bOutgoingAllowLocalDistrict = oSecond.m_bOutgoingAllowLocalDistrict;
            m_bOutgoingAllowLocalPark = oSecond.m_bOutgoingAllowLocalPark;
            m_outgoingDistrictAllowed = new HashSet<DistrictData>(oSecond.m_outgoingDistrictAllowed);
        }

        public bool Equals(BuildingSettings oSecond)
        {
            return m_bAllowImport == oSecond.m_bAllowImport &&
                    m_bAllowExport == oSecond.m_bAllowExport &&
                    m_iPreferLocalDistrictsIncoming == oSecond.m_iPreferLocalDistrictsIncoming &&
                    m_iPreferLocalDistrictsOutgoing == oSecond.m_iPreferLocalDistrictsOutgoing &&
                    m_bDistrictAllowServices == oSecond.m_bDistrictAllowServices &&
                    m_iServiceDistance == oSecond.m_iServiceDistance &&
                    m_bWarehouseOverride == oSecond.m_bWarehouseOverride &&
                    m_bWarehouseFirst == oSecond.m_bWarehouseFirst &&
                    m_iWarehouseReserveTrucksPercent == oSecond.m_iWarehouseReserveTrucksPercent &&
                    m_iOutsideMultiplier == oSecond.m_iOutsideMultiplier &&
                    m_bIncomingAllowLocalDistrict == oSecond.m_bIncomingAllowLocalDistrict &&
                    m_bIncomingAllowLocalPark == oSecond.m_bIncomingAllowLocalPark &&
                    m_bOutgoingAllowLocalDistrict == oSecond.m_bOutgoingAllowLocalDistrict &&
                    m_bOutgoingAllowLocalPark == oSecond.m_bOutgoingAllowLocalPark &&
                    m_incomingDistrictAllowed == oSecond.m_incomingDistrictAllowed &&
                    m_outgoingDistrictAllowed == oSecond.m_outgoingDistrictAllowed;
        }

        public static BuildingSettings GetSettings(ushort buildingId)
        {
            if (s_BuildingsSettings != null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_BuildingsSettings.ContainsKey(buildingId))
                    {
                        return s_BuildingsSettings[buildingId];
                    }
                }
            }

            return new BuildingSettings();
        }

        public static void SetSettings(ushort buildingId, BuildingSettings settings)
        {
            if (s_BuildingsSettings != null)
            {
                lock (s_dictionaryLock)
                {
                    if (settings.Equals(new BuildingSettings()))
                    {
                        if (s_BuildingsSettings.ContainsKey(buildingId))
                        {
                            // Default values, just remove settings
                            s_BuildingsSettings.Remove(buildingId);
                        }
                    }
                    else
                    {
                        // Save a copy not the pointer
                        s_BuildingsSettings[buildingId] = new BuildingSettings(settings);
                    }
                }
            }
        }

        public static void SetImport(ushort buildingId, bool bAllow)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_bAllowImport = bAllow;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static bool GetImport(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bAllowImport;
                }
                return true;
            }
        }

        public static void SetExport(ushort buildingId, bool bAllow)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_bAllowExport = bAllow;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static bool GetExport(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bAllowExport;
                }
                return true;
            }
        }

        public static bool IsExportDisabled(ushort buildingId)
        {
            return !GetExport(buildingId);
        }

        public static bool IsImportDisabled(ushort buildingId)
        {
            return !GetImport(buildingId);
        }

        public static PreferLocal PreferLocalDistrictServicesIncoming(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return (PreferLocal)s_BuildingsSettings[buildingId].m_iPreferLocalDistrictsIncoming;
                }
                return PreferLocal.ALL_DISTRICTS;
            }
        }

        public static PreferLocal PreferLocalDistrictServicesOutgoing(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return (PreferLocal)s_BuildingsSettings[buildingId].m_iPreferLocalDistrictsOutgoing;
                }
                return PreferLocal.ALL_DISTRICTS;
            }
        }

        public static void PreferLocalDistrictServicesIncoming(ushort buildingId, PreferLocal value)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_iPreferLocalDistrictsIncoming = value;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static void PreferLocalDistrictServicesOutgoing(ushort buildingId, PreferLocal value)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_iPreferLocalDistrictsOutgoing = value;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static void SetDistrictAllowServices(ushort buildingId, bool bChecked)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_bDistrictAllowServices = bChecked;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static bool GetDistrictAllowServices(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bDistrictAllowServices;
                }
                return true;
            }
        }

        public static int ReserveCargoTrucksPercent(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId) && s_BuildingsSettings[buildingId].m_bWarehouseOverride)
                {
                    return s_BuildingsSettings[buildingId].m_iWarehouseReserveTrucksPercent;
                }
                return SaveGameSettings.GetSettings().WarehouseReserveTrucksPercent;
            }
        }

        public static void ReserveCargoTrucksPercent(ushort buildingId, int iPercent)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_iWarehouseReserveTrucksPercent = iPercent;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static bool IsWarehouseFirst(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId) && s_BuildingsSettings[buildingId].m_bWarehouseOverride)
                {
                    return s_BuildingsSettings[buildingId].m_bWarehouseFirst;
                }
                return SaveGameSettings.GetSettings().WarehouseFirst;
            }
        }

        public static void WarehouseFirst(ushort buildingId, bool bEnable)
        {
            if (buildingId != 0)
            {
                BuildingSettings buildingSettings = GetSettings(buildingId);
                buildingSettings.m_bWarehouseFirst = bEnable;
                SetSettings(buildingId, buildingSettings);
            }
        }

        public static bool IsWarehouseOverride(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].m_bWarehouseOverride;
                }
                return false;
            }
        }

        public static void WarehouseOverride(ushort buildingId, bool bEnable)
        {
            BuildingSettings buildingSettings = GetSettings(buildingId);
            buildingSettings.m_bWarehouseOverride = bEnable;
            SetSettings(buildingId, buildingSettings);
        }

        public void WarehouseOverride(bool bEnable)
        {
            m_bWarehouseOverride = bEnable;
        }

        private static int GetOutsideMultiplier(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    return s_BuildingsSettings[buildingId].GetOutsideMultiplier();
                }
                return 0;
            }
        }

        public static int GetEffectiveOutsideMultiplier(ushort buildingId)
        {
            int iBuildingMultipler = GetOutsideMultiplier(buildingId);
            if (iBuildingMultipler > 0)
            {
                // Apply building multiplier
                return iBuildingMultipler;
            }
            else
            {
                // Apply global multiplier
                BuildingTypeHelper.OutsideType eType = BuildingTypeHelper.GetOutsideConnectionType(buildingId);
                switch (eType)
                {
                    case BuildingTypeHelper.OutsideType.Ship: return SaveGameSettings.GetSettings().OutsideShipMultiplier;
                    case BuildingTypeHelper.OutsideType.Plane: return SaveGameSettings.GetSettings().OutsidePlaneMultiplier;
                    case BuildingTypeHelper.OutsideType.Train: return SaveGameSettings.GetSettings().OutsideTrainMultiplier;
                    case BuildingTypeHelper.OutsideType.Road: return SaveGameSettings.GetSettings().OutsideRoadMultiplier;
                    default: return 1;
                }
            }
        }

        public int GetOutsideMultiplier()
        {
            return m_iOutsideMultiplier;
        }

        public void SetOutsideMultiplier(int iMultiplier)
        {
            m_iOutsideMultiplier = iMultiplier;
        }

        public static HashSet<DistrictData> GetAllowedDistrictsIncoming(ushort buildingId)
        {
            BuildingSettings settings = GetSettings(buildingId);
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(settings.m_incomingDistrictAllowed);

            // Add current district if allowed
            if (settings.m_bIncomingAllowLocalDistrict)
            {
                byte district = DistrictManager.instance.GetDistrict(building.m_position);
                if (district != 0)
                {
                    DistrictData data = new DistrictData(DistrictData.DistrictType.District, district);
                    if (!list.Contains(data))
                    {
                        list.Add(data);
                    }
                }
            }

            // Add current park if allowed
            if (settings.m_bIncomingAllowLocalPark)
            {
                byte park = DistrictManager.instance.GetPark(building.m_position);
                if (park != 0)
                {
                    DistrictData data = new DistrictData(DistrictData.DistrictType.Park, park);
                    if (!list.Contains(data))
                    {
                        list.Add(data);
                    }
                }
            }

            return list;
        }

        public static HashSet<DistrictData> GetAllowedDistrictsOutgoing(ushort buildingId)
        {
            BuildingSettings settings = GetSettings(buildingId);
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];

            HashSet<DistrictData> list = new HashSet<DistrictData>(settings.m_outgoingDistrictAllowed);

            // Add current district if allowed
            if (settings.m_bOutgoingAllowLocalDistrict)
            {
                byte district = DistrictManager.instance.GetDistrict(building.m_position);
                if (district != 0)
                {
                    list.Add(new DistrictData(DistrictData.DistrictType.District, district));
                }
            }

            // Add current park if allowed
            if (settings.m_bOutgoingAllowLocalPark)
            {
                byte park = DistrictManager.instance.GetPark(building.m_position);
                if (park != 0)
                {
                    list.Add(new DistrictData(DistrictData.DistrictType.Park, park));
                }
            }

            return list;
        }

        public bool IsIncomingDistrictAllowed(DistrictData.DistrictType eType, int district)
        {
            return m_incomingDistrictAllowed.Contains(new DistrictData(eType, district));
        }

        public void SetIncomingDistrictAllowed(DistrictData.DistrictType eType, int district, bool bChecked)
        {
            bool bContains = IsIncomingDistrictAllowed(eType, district);
            if (bChecked)
            {
                if (!bContains)
                {
                    m_incomingDistrictAllowed.Add(new DistrictData(eType, district));
                }
            }
            else 
            {
                if (bContains)
                {
                    m_incomingDistrictAllowed.Remove(new DistrictData(eType, district));
                }
            }
        }

        public bool IsOutgoingDistrictAllowed(DistrictData.DistrictType eType, int district)
        {
            return m_outgoingDistrictAllowed.Contains(new DistrictData(eType, district));
        }

        public void SetOutgoingDistrictAllowed(DistrictData.DistrictType eType, int district, bool bChecked)
        {
            bool bContains = IsOutgoingDistrictAllowed(eType, district);
            if (bChecked)
            {
                if (!bContains)
                {
                    m_outgoingDistrictAllowed.Add(new DistrictData(eType, district));
                }
            }
            else
            {
                if (bContains)
                {
                    m_outgoingDistrictAllowed.Remove(new DistrictData(eType, district));
                }
            }
        }

        public string GetIncomingDistrictTooltip(ushort buildingId)
        {
            string sMessage = "";
            switch (m_iPreferLocalDistrictsIncoming)
            {
                case PreferLocal.ALL_DISTRICTS:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        sMessage += "\r\n- " + Localization.Get("dropdownBuildingPanelPreferLocal1");
                        return sMessage;
                    }
                case PreferLocal.PREFER_LOCAL_DISTRICT:
                case PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        break;
                    }
                case PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        sMessage += Localization.Get("txtBlockedDistricts") + ":";
                        break;
                    }
            }

            // Add allowed districts to message
            HashSet<DistrictData> list = GetAllowedDistrictsIncoming(buildingId);
            foreach (DistrictData district in list)
            {
                if (district.m_eType == DistrictData.DistrictType.District)
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetDistrictName(district.m_iDistrictId);
                }
                else
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetParkName(district.m_iDistrictId);
                }
            }

            return sMessage;
        }

        public string GetOutgoingDistrictTooltip(ushort buildingId)
        {
            string sMessage = "";
            switch (m_iPreferLocalDistrictsOutgoing)
            {
                case PreferLocal.ALL_DISTRICTS:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        sMessage += "\r\n- " + Localization.Get("dropdownBuildingPanelPreferLocal1");
                        return sMessage;
                    }
                case PreferLocal.PREFER_LOCAL_DISTRICT:
                case PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        sMessage += Localization.Get("txtAllowedDistricts") + ":";
                        break;
                    }
                case PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        sMessage += Localization.Get("txtBlockedDistricts") + ":";
                        break;
                    }
            }

            // Add allowed districts to message
            HashSet<DistrictData> list = GetAllowedDistrictsOutgoing(buildingId);
            foreach (DistrictData district in list)
            {
                if (district.m_eType == DistrictData.DistrictType.District)
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetDistrictName(district.m_iDistrictId);
                }
                else
                {
                    sMessage += "\r\n- " + DistrictManager.instance.GetParkName(district.m_iDistrictId);
                }
            }

            return sMessage;
        }

        // Hooks into BuildingManager do not change
        public static void ReleaseBuilding(ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                if (s_BuildingsSettings.ContainsKey(buildingId))
                {
                    s_BuildingsSettings.Remove(buildingId);
                }
            }
        }

        public string DebugSettings()
        {
            string sMessage = "";

            sMessage += " Import:" + m_bAllowImport;
            sMessage += " Export:" + m_bAllowExport;
            sMessage += " PreferIn:" + m_iPreferLocalDistrictsIncoming;
            sMessage += " PreferOut:" + m_iPreferLocalDistrictsOutgoing;
            sMessage += " DistrictAllowServices:" + m_bDistrictAllowServices;
            sMessage += " ServiceDistance:" + m_iServiceDistance;
            sMessage += " WarehouseOverride:" + m_bWarehouseOverride;
            sMessage += " WarehouseFirst:" + m_bWarehouseFirst;
            sMessage += " WarehouseReserve:" + m_iWarehouseReserveTrucksPercent;
            sMessage += " OutsideMultiplier:" + m_iOutsideMultiplier;
            sMessage += " IncomingAllowLocalDistrict:" + m_bIncomingAllowLocalDistrict;
            sMessage += " IncomingAllowLocalPark:" + m_bIncomingAllowLocalPark;
            sMessage += " OutgoingAllowLocalDistrict:" + m_bOutgoingAllowLocalDistrict;
            sMessage += " OutgoingAllowLocalPark:" + m_bOutgoingAllowLocalPark;

            return sMessage;
        }

        public static string DebugSettings(ushort buildingId)
        {
            if (s_BuildingsSettings != null && s_BuildingsSettings.ContainsKey(buildingId))
            {
                BuildingSettings settings = s_BuildingsSettings[buildingId];
                return "Building: " + buildingId + settings.DebugSettings();
            }
            else
            {
                return "Not found";
            }
        }

        public HashSet<DistrictData> ValidateDistricts(HashSet<DistrictData> districts)
        {
            HashSet<DistrictData> newSet = new HashSet<DistrictData>();

            foreach (DistrictData districtId in districts)
            {
                if (districtId.IsDistrict())
                {
                    District district = DistrictManager.instance.m_districts.m_buffer[districtId.m_iDistrictId];
                    if ((district.m_flags & District.Flags.Created) != 0)
                    {
                        newSet.Add(districtId);
                    }
                    else
                    {
                        // District doesn't exist any more
                        Debug.Log("District missing: " + districtId.m_iDistrictId);
                    }
                }
                else
                {
                    DistrictPark park = DistrictManager.instance.m_parks.m_buffer[districtId.m_iDistrictId];
                    if ((park.m_flags & DistrictPark.Flags.Created) != 0)
                    {
                        newSet.Add(districtId);
                    }
                    else
                    {
                        // District doesn't exist any more
                        Debug.Log("Park missing: " + districtId.m_iDistrictId);
                    }
                }
            }

            return newSet;
        }

        public bool Validate()
        {
            // Check districts are still valid
            bool bChanged = false;

            if (m_incomingDistrictAllowed.Count > 0)
            {
                HashSet<DistrictData> newIncoming = ValidateDistricts(m_incomingDistrictAllowed);
                if (newIncoming != m_incomingDistrictAllowed)
                {
                    m_incomingDistrictAllowed = newIncoming;
                    bChanged = true;
                }

                HashSet<DistrictData> newOutgoing = ValidateDistricts(m_outgoingDistrictAllowed);
                if (newOutgoing != m_outgoingDistrictAllowed)
                {
                    m_outgoingDistrictAllowed = newOutgoing;
                    bChanged = true;
                }
            }

            return bChanged;
        }

        public static void ValidateSettings()
        {
            if (s_BuildingsSettings != null)
            {
                Dictionary<ushort, BuildingSettings> updatedSettings = new Dictionary<ushort, BuildingSettings>();

                foreach (KeyValuePair<ushort, BuildingSettings> kvp in s_BuildingsSettings)
                {
                    BuildingSettings settings = kvp.Value;
                    if (settings.Validate())
                    {
                        updatedSettings[kvp.Key] = settings;
                    }
                }

                // Now update actual settings objects
                foreach (KeyValuePair<ushort, BuildingSettings> kvp in updatedSettings)
                {
                    s_BuildingsSettings[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}