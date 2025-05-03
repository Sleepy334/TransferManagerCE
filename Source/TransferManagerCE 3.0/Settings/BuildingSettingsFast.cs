using System;
using TransferManagerCE.TransferRules;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Settings
{
    internal class BuildingSettingsFast
    {
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

        public static float GetDistanceRestrictionSquaredMeters(ushort buildingId, BuildingType eBuildingType, CustomTransferReason.Reason material, bool bIncoming)
        {

            // Try and get local distance setting
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null && 
                settings.HasRestrictionSettings() &&
                BuildingRuleSets.HasDistanceRules(eBuildingType, material))
            {
                int iRestrictionId = BuildingRuleSets.GetRestrictionId(eBuildingType, material, bIncoming);
                RestrictionSettings? restrictions = settings.GetRestrictions(iRestrictionId);
                if (restrictions is not null)
                {
                    int iDistanceMeters = restrictions.m_iServiceDistanceMeters;
                    if (iDistanceMeters > 0)
                    {
                        return (float)Math.Pow(iDistanceMeters, 2);
                    }
                }
            }

            return float.MaxValue;
        }

        public static int GetEffectiveOutsideMultiplier(ushort buildingId)
        {
            BuildingSettings? settings = BuildingSettingsStorage.GetSettings(buildingId);
            if (settings is not null)
            {
                int multiplier = settings.m_iOutsideMultiplier;
                if (multiplier > 0)
                {
                    // Apply building multiplier
                    return multiplier;
                }
            }

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
}
