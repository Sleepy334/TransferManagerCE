using SleepyCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using TransferManagerCE.TransferRules;
using UnityEngine;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Settings
{
    internal class BuildingSettingsFast
    {
        private static HashSet<ushort> Empty = new HashSet<ushort>();

        public static bool IsImprovedWarehouseMatching(ushort buildingId)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null)
            {
                return settings.IsImprovedWarehouseMatching();
            }

            return SaveGameSettings.GetSettings().ImprovedWarehouseMatching;
        }

        public static int ReserveCargoTrucksPercent(ushort buildingId)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null)
            {
                return settings.ReserveCargoTrucksPercent();
            }

            return SaveGameSettings.GetSettings().WarehouseReserveTrucksPercent;
        }

        public static bool IsImportAllowed(ushort buildingId, BuildingType eBuildingType, CustomTransferReason.Reason material, bool bIncoming)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null && settings.HasRestrictionSettings())
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(eBuildingType, material, bIncoming);
                RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                if (restrictions is not null)
                {
                    return restrictions.m_bAllowImport;
                }
            }

            return true;
            
        }

        public static bool IsExportAllowed(ushort buildingId, BuildingType eBuildingType, CustomTransferReason.Reason material, bool bIncoming)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null && settings.HasRestrictionSettings())
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(eBuildingType, material, bIncoming);
                RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                if (restrictions is not null)
                {
                    return restrictions.m_bAllowExport;
                }
            }

            return true;
        }

        public static HashSet<ushort> GetOutsideConnectionExclusionList(ushort buildingId, BuildingType eBuildingType, CustomTransferReason.Reason material, bool bIncoming)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null && settings.HasRestrictionSettings())
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(eBuildingType, material, bIncoming);
                if (iRestrictionId != -1)
                {
                    RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                    if (restrictions is not null)
                    {
                        return restrictions.m_excludedOutsideConnections;
                    }
                }
            }

            return Empty;
        }

        public static float GetDistanceRestrictionSquaredMeters(ushort buildingId, BuildingType eBuildingType, CustomTransferReason.Reason material, bool bIncoming)
        {
            if (BuildingRuleSets.HasDistanceRules(bIncoming, eBuildingType, material))
            {
                // Try and get local distance setting
                BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
                if (settings is not null && settings.HasRestrictionSettings())
                {
                    int iRestrictionId = BuildingRuleSets.GetRestrictionId(eBuildingType, material, bIncoming);
                    RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);

                    if (restrictions is not null)
                    {
                        if (bIncoming)
                        {
                            int iDistanceMeters = restrictions.m_incomingServiceDistanceMeters;
                            if (iDistanceMeters > 0)
                            {
                                return (float)Math.Pow(iDistanceMeters, 2);
                            }
                        }
                        else
                        {
                            int iDistanceMeters = restrictions.m_outgoingServiceDistanceMeters;
                            if (iDistanceMeters > 0)
                            {
                                return (float)Math.Pow(iDistanceMeters, 2);
                            }
                        }
                    }
                }
            }

            return float.MaxValue;
        }

        public static int GetEffectiveOutsidePriority(ushort buildingId)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null)
            {
                int priority = settings.m_iOutsidePriority;
                if (priority >= 0)
                {
                    // Apply building multiplier
                    return priority;
                }
            }

            // Apply global multiplier
            BuildingTypeHelper.OutsideType eType = BuildingTypeHelper.GetOutsideConnectionType(buildingId);
            switch (eType)
            {
                case BuildingTypeHelper.OutsideType.Ship: return SaveGameSettings.GetSettings().OutsideShipPriority;
                case BuildingTypeHelper.OutsideType.Plane: return SaveGameSettings.GetSettings().OutsidePlanePriority;
                case BuildingTypeHelper.OutsideType.Train: return SaveGameSettings.GetSettings().OutsideTrainPriority;
                case BuildingTypeHelper.OutsideType.Road: return SaveGameSettings.GetSettings().OutsideRoadPriority;
                default: return 1;
            }
        }
    }
}
