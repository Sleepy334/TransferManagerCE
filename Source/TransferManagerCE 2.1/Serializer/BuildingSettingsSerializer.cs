using System;
using System.Collections.Generic;
using TransferManagerCE.Util;
using static TransferManagerCE.BuildingSettings;
using static TransferManagerCE.RestrictionSettings;

namespace TransferManagerCE
{
    public class BuildingSettingsSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        public static void SaveData(FastList<byte> Data)
        {
            // Write out metadata
            StorageData.WriteInt32(iBUILDING_SETTINGS_DATA_VERSION, Data);
            StorageData.WriteInt32(BuildingSettingsStorage.s_BuildingsSettings.Count, Data);

            // Write out each buildings settings
            foreach (KeyValuePair<ushort, BuildingSettings> kvp in BuildingSettingsStorage.s_BuildingsSettings)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                // Write actual settings
                StorageData.WriteInt32(kvp.Key, Data);
                BuildingSettings settings = kvp.Value;
                settings.SaveData(Data);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);
            }
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                // Read in the data version for these building settings
                int iBuildingSettingsVersion = StorageData.ReadInt32(Data, ref iIndex);
#if DEBUG
                DebugLog.LogInfo("Global: " + iGlobalVersion + " BuildingVersion: " + iBuildingSettingsVersion + " DataLength: " + Data.Length + " Index: " + iIndex);
#endif
                // Check we support reading this version
                if (iBuildingSettingsVersion <= iBUILDING_SETTINGS_DATA_VERSION)
                {
                    BuildingSettings defaultSettings = new BuildingSettings();

                    int iCount = StorageData.ReadInt32(Data, ref iIndex);
                    for (int i = 0; i < iCount; ++i)
                    {
                        // We now write out a building setting start and end tuple
                        CheckStartTuple($"Building({i})", iBuildingSettingsVersion, Data, ref iIndex);

                        int buildingId = StorageData.ReadInt32(Data, ref iIndex);

                        BuildingSettings? settings = null;
                        if (iBuildingSettingsVersion >= 11)
                        {
                            // In newer versions we pass in iBuildingSettingsVersion
                            settings = new BuildingSettings();
                            settings.LoadData(iBuildingSettingsVersion, Data, ref iIndex);
                        }
                        else
                        {
                            // Load older version settings
                            switch (iBuildingSettingsVersion)
                            {
                                case 1: settings = LoadDataVersion1(Data, ref iIndex); break;
                                case 2: settings = LoadDataVersion2(Data, ref iIndex); break;
                                case 3:
                                    {
                                        // Forgot to increment building setting value here, trigger off global
                                        if (iGlobalVersion == 3)
                                        {
                                            settings = LoadDataVersion3_3(Data, ref iIndex); // 3_3 = Building_Global
                                        }
                                        else if (iGlobalVersion == 4)
                                        {
                                            settings = LoadDataVersion3_4(Data, ref iIndex); // 3_4 = Building_Global
                                        }
                                        break;
                                    }
                                case 4: settings = LoadDataVersion4(Data, ref iIndex); break;
                                case 5: settings = LoadDataVersion5(Data, ref iIndex); break;
                                case 6: settings = LoadDataVersion6(Data, ref iIndex); break;
                                case 7: settings = LoadDataVersion7(Data, ref iIndex); break;
                                case 8: settings = LoadDataVersion8(Data, ref iIndex); break;
                                case 9: settings = LoadDataVersion9(Data, ref iIndex); break;
                                case 10: settings = LoadDataVersion10(Data, ref iIndex); break;
                                default:
                                    {
                                        Debug.Log("New data version, unable to load!");
                                        break;
                                    }
                            }

                            // We upgrade old settings for the new multiple rulesets
                            UpgradeSettings((ushort)buildingId, ref settings);
                        }

                        // Check we finished reading on the end tuple correctly
                        CheckEndTuple($"Building({i})", iBuildingSettingsVersion, Data, ref iIndex);

                        // Add this buildings settings
                        if (settings != null && !settings.Equals(defaultSettings))
                        {
                            BuildingSettingsStorage.s_BuildingsSettings[(ushort)buildingId] = settings;
                        }
                    }
                } 
                else
                {
                    Debug.Log($"New data version {iBuildingSettingsVersion}, unable to load!");
                }
            }
        }

        private static void UpgradeSettings(ushort buildingId, ref BuildingSettings? settings)
        {
            if (settings != null)
            {
                BuildingTypeHelper.BuildingType eType = BuildingTypeHelper.GetBuildingType(buildingId);
                switch (eType)
                {
                    case BuildingTypeHelper.BuildingType.Warehouse:
                    case BuildingTypeHelper.BuildingType.CargoFerryWarehouseHarbor:
                        {
                            // For warehouses we need to duplicate the settings to the second ruleset as well
                            RestrictionSettings restrictions = settings.GetRestrictions(0);
                            settings.SetRestrictionsDirect(1, new RestrictionSettings(restrictions));
                            break;
                        }
                    case BuildingTypeHelper.BuildingType.GenericProcessing:
                    case BuildingTypeHelper.BuildingType.ProcessingFacility:
                    case BuildingTypeHelper.BuildingType.UniqueFactory:
                    case BuildingTypeHelper.BuildingType.MedicalHelicopterDepot:
                    case BuildingTypeHelper.BuildingType.PostOffice:
                    case BuildingTypeHelper.BuildingType.PostSortingFacility:
                    case BuildingTypeHelper.BuildingType.WasteProcessing:
                    case BuildingTypeHelper.BuildingType.Recycling:
                    case BuildingTypeHelper.BuildingType.FishFactory:
                        {
                            // Need to split into Incoming / Outgoing
                            RestrictionSettings restrictionsIncoming = settings.GetRestrictions(0);
                            RestrictionSettings restrictionsOutgoing = new RestrictionSettings(restrictionsIncoming);

                            // Fix incoming
                            restrictionsIncoming.ResetDistrictRestrictionsOutgoing();
                            restrictionsIncoming.m_bAllowExport = true;
                            restrictionsIncoming.m_iServiceDistance = 0;
                            settings.SetRestrictionsDirect(0, restrictionsIncoming);

                            // Fix outgoing
                            restrictionsOutgoing.ResetDistrictRestrictionsIncoming();
                            restrictionsOutgoing.m_bAllowImport = true;
                            settings.SetRestrictionsDirect(1, restrictionsOutgoing);

                            break;
                        }
                    case BuildingTypeHelper.BuildingType.GenericFactory:
                        {
                            // Need to split into Incoming 1 / Incoming 2 / Outgoing
                            RestrictionSettings restrictionsIncoming = settings.GetRestrictions(0);
                            RestrictionSettings restrictionsOutgoing = new RestrictionSettings(restrictionsIncoming);

                            // Fix incoming 1
                            restrictionsIncoming.ResetDistrictRestrictionsOutgoing();
                            restrictionsIncoming.m_bAllowExport = true;
                            restrictionsIncoming.m_iServiceDistance = 0;
                            settings.SetRestrictions(0, restrictionsIncoming);

                            // Insert a copy for Incoming 2
                            settings.SetRestrictions(2, restrictionsIncoming);

                            // Fix outgoing
                            restrictionsOutgoing.ResetDistrictRestrictionsIncoming();
                            restrictionsOutgoing.m_bAllowImport = true;
                            settings.SetRestrictionsDirect(1, restrictionsOutgoing);

                            break;
                        }
                }
            }
        }

        private static BuildingSettings? LoadDataVersion10(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings(); 
            RestrictionSettings restrictions = new RestrictionSettings();

            restrictions.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_iServiceDistance = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_bIncomingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bIncomingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bOutgoingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bOutgoingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

            // Load arrays
            restrictions.m_incomingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
            restrictions.m_outgoingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);

            settings.SetRestrictionsDirect(0, restrictions);

            return settings;
        }

        private static BuildingSettings? LoadDataVersion9(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            RestrictionSettings restrictions = new RestrictionSettings();

            restrictions.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            bool bDistrictAllowServicesNotUsed = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_iServiceDistance = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);
            settings.m_iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_bIncomingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bIncomingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bOutgoingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bOutgoingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

            restrictions.m_incomingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
            restrictions.m_outgoingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);

            settings.SetRestrictionsDirect(0, restrictions);

            return settings;
        }

        private static BuildingSettings? LoadDataVersion8(byte[] Data, ref int iIndex)
        {
            // version 8 is the same as 7 except new district option ALL_DISTRICTS_EXCEPT_FOR
            return LoadDataVersion7(Data, ref iIndex);
        }

        private static BuildingSettings? LoadDataVersion7(byte[] Data, ref int iIndex)
        {
            BuildingSettings? settings = LoadDataVersion6(Data, ref iIndex);
            if (settings != null)
            {
                if (settings.m_restrictions.Count == 1)
                {
                    RestrictionSettings restrictions = settings.m_restrictions[0];
                    restrictions.m_bIncomingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
                    restrictions.m_bIncomingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);
                    restrictions.m_bOutgoingAllowLocalDistrict = StorageData.ReadBool(Data, ref iIndex);
                    restrictions.m_bOutgoingAllowLocalPark = StorageData.ReadBool(Data, ref iIndex);

                    restrictions.m_incomingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
                    restrictions.m_outgoingDistrictAllowed = LoadDistrictAllowed(Data, ref iIndex);
                    settings.m_restrictions[0] = restrictions;
                }
                else
                {
                    Debug.LogError("Error reading old settings, no restriction object created.");
                }
            }
            return settings;
        }

        private static BuildingSettings? LoadDataVersion6(byte[] Data, ref int iIndex)
        {
            BuildingSettings? settings = LoadDataVersion5(Data, ref iIndex);
            if (settings != null)
            {
                settings.m_iOutsideMultiplier = StorageData.ReadInt32(Data, ref iIndex);
            }
            return settings;
        }

        private static BuildingSettings? LoadDataVersion5(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            RestrictionSettings restrictions = new RestrictionSettings();

            restrictions.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            bool bDistrictAllowServicesNotUsed = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseOverride = StorageData.ReadBool(Data, ref iIndex);
            settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            settings.m_iWarehouseReserveTrucksPercent = StorageData.ReadInt32(Data, ref iIndex);

            settings.SetRestrictionsDirect(0, restrictions);

            return settings;
        }

        private static BuildingSettings? LoadDataVersion4(byte[] Data, ref int iIndex)
        {
            BuildingSettings? settings = LoadDataVersion3_4(Data, ref iIndex);
            if (settings != null)
            {
                bool bDistrictAllowServicesNotUsed = StorageData.ReadBool(Data, ref iIndex);
                settings.m_bWarehouseFirst = StorageData.ReadBool(Data, ref iIndex);
            }
            return settings;
        }

        private static BuildingSettings? LoadDataVersion3_4(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            RestrictionSettings restrictions = new RestrictionSettings();

            restrictions.m_bAllowImport = StorageData.ReadBool(Data, ref iIndex);
            restrictions.m_bAllowExport = StorageData.ReadBool(Data, ref iIndex);
            bool bReserveWarehouseTrucks = StorageData.ReadBool(Data, ref iIndex); // Ignore we no longer use
            restrictions.m_iPreferLocalDistrictsIncoming = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsOutgoing = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);

            settings.SetRestrictionsDirect(0, restrictions);

            return settings;
        }

        private static BuildingSettings? LoadDataVersion3_3(byte[] Data, ref int iIndex)
        {
            BuildingSettings settings = new BuildingSettings();
            RestrictionSettings restrictions = new RestrictionSettings();

            ImportExport eValue = (ImportExport)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_bAllowImport = (eValue == ImportExport.ALLOW_IMPORT_AND_EXPORT || eValue == ImportExport.ALLOW_IMPORT_ONLY);
            restrictions.m_bAllowExport = (eValue == ImportExport.ALLOW_IMPORT_AND_EXPORT || eValue == ImportExport.ALLOW_EXPORT_ONLY);
            bool bReserveWarehouseTrucks = StorageData.ReadBool(Data, ref iIndex); // Ignore we no longer use

            // Convert 1 prefer local to 2
            PreferLocal ePreferLocal = (PreferLocal)StorageData.ReadInt32(Data, ref iIndex);
            restrictions.m_iPreferLocalDistrictsIncoming = ePreferLocal;
            restrictions.m_iPreferLocalDistrictsOutgoing = ePreferLocal;

            settings.SetRestrictionsDirect(0, restrictions);

            return settings;
        }

        private static BuildingSettings? LoadDataVersion2(byte[] Data, ref int iIndex)
        {
            // Not supported, didn't really exist in the wild for long
            return null;
        }

        private static BuildingSettings? LoadDataVersion1(byte[] Data, ref int iIndex)
        {
            // Not supported, didn't really exist in the wild for long
            return null;
        }

        private static void CheckStartTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 14)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception($"Building start tuple not found at: {sTupleLocation}");
                }
            }
        }

        private static void CheckEndTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 14)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                if (iTupleStart != uiTUPLE_END)
                {
                    throw new Exception($"Building end tuple not found at: {sTupleLocation}");
                }
            }
        }
    }
}