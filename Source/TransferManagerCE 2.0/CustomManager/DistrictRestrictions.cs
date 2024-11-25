using System;
using System.Collections.Generic;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.CustomManager
{
    public class DistrictRestrictions
    {
        // Stores the mapping between building type and material
        private static Dictionary<BuildingType, HashSet<TransferReason>> s_incomingRestrictions = new Dictionary<BuildingType, HashSet<TransferReason>>();
        private static Dictionary<BuildingType, HashSet<TransferReason>> s_outgoingRestrictions = new Dictionary<BuildingType, HashSet<TransferReason>>();

        // Store total reasons for restrictions as a speedy initial cull
        private static HashSet<TransferReason> s_TotalReasons = new HashSet<TransferReason>();

        // static empty set so we don't have to create one each time
        private static readonly HashSet<TransferReason> s_emptySet = new HashSet<TransferReason>();

        public static bool IsDistrictRestrictionsSupported(TransferReason material)
        {
            return IsGlobalDistrictRestrictionsSupported(material) || IsBuildingDistrictRestrictionsSupported(material);
        }

        private static void InitIncoming()
        {
            if (s_incomingRestrictions.Count == 0)
            {
                s_incomingRestrictions[BuildingType.ProcessingFacility] = new HashSet<TransferReason>()
                {
                    TransferReason.Oil,
                    TransferReason.Ore,
                    TransferReason.Logs,
                    TransferReason.Grain,
                };
                s_incomingRestrictions[BuildingType.FishFactory] = new HashSet<TransferReason>()
                {
                    TransferReason.Fish,
                };
                // Generic warehouse, raw warehouses handled separately
                s_incomingRestrictions[BuildingType.Warehouse] = new HashSet<TransferReason>()
                {
                    TransferReason.Coal,
                    TransferReason.Petrol,
                    TransferReason.Food,
                    TransferReason.Lumber,
                    TransferReason.Flours,
                    TransferReason.AnimalProducts,
                    TransferReason.Paper,
                    TransferReason.PlanedTimber,
                    TransferReason.Plastics,
                    TransferReason.Petroleum,
                    TransferReason.Glass,
                    TransferReason.Metals,
                    TransferReason.Goods,
                    TransferReason.LuxuryProducts,
                    TransferReason.Fish,
                };
                s_incomingRestrictions[BuildingType.UniqueFactory] = new HashSet<TransferReason>()
                {
                    TransferReason.Grain, // Currently the only primary resource factories can use.
                    TransferReason.Flours,
                    TransferReason.AnimalProducts,
                    TransferReason.Petroleum,
                    TransferReason.Plastics,
                    TransferReason.Glass,
                    TransferReason.Metals,
                    TransferReason.Paper,
                    TransferReason.PlanedTimber,
                };
                s_incomingRestrictions[BuildingType.GenericProcessing] = new HashSet<TransferReason>()
                {
                    TransferReason.Coal,
                    TransferReason.Petrol,
                    TransferReason.Food,
                    TransferReason.Lumber,
                    TransferReason.Flours,
                    TransferReason.AnimalProducts,
                    TransferReason.Paper,
                    TransferReason.PlanedTimber,
                    TransferReason.Plastics,
                    TransferReason.Glass,
                    TransferReason.Petroleum,
                    TransferReason.Metals,
                };
                s_incomingRestrictions[BuildingType.Commercial] = new HashSet<TransferReason>()
                {
                    TransferReason.Goods,
                    TransferReason.LuxuryProducts,
                };
                s_incomingRestrictions[BuildingType.FishMarket] = new HashSet<TransferReason>()
                {
                    TransferReason.Fish,
                }; 
                s_incomingRestrictions[BuildingType.Landfill] = new HashSet<TransferReason>()
                {
                    TransferReason.Garbage,
                };

                s_incomingRestrictions[BuildingType.Recycling] = new HashSet<TransferReason>()
                {
                    TransferReason.Garbage,
                };
                s_incomingRestrictions[BuildingType.WasteTransfer] = new HashSet<TransferReason>()
                {
                    TransferReason.Garbage,
                };
                s_incomingRestrictions[BuildingType.WasteProcessing] = new HashSet<TransferReason>()
                {
                    TransferReason.Garbage,
                };
                s_incomingRestrictions[BuildingType.PoliceStation] = new HashSet<TransferReason>()
                {
                    TransferReason.Crime,
                };
                s_incomingRestrictions[BuildingType.PoliceHelicopterDepot] = new HashSet<TransferReason>()
                {
                    TransferReason.Crime,
                };
                s_incomingRestrictions[BuildingType.FireStation] = new HashSet<TransferReason>()
                {
                    TransferReason.Fire,
                };
                s_incomingRestrictions[BuildingType.FireHelicopterDepot] = new HashSet<TransferReason>()
                {
                    TransferReason.Fire,
                    TransferReason.Fire2,
                    TransferReason.ForestFire,
                };
                s_incomingRestrictions[BuildingType.Hospital] = new HashSet<TransferReason>()
                {
                    TransferReason.Sick,
                };
                s_incomingRestrictions[BuildingType.MedicalHelicopterDepot] = new HashSet<TransferReason>()
                {
                    TransferReason.Sick2,
                };
                s_incomingRestrictions[BuildingType.Cemetery] = new HashSet<TransferReason>()
                {
                    TransferReason.Dead,
                };
                s_incomingRestrictions[BuildingType.PostOffice] = new HashSet<TransferReason>()
                {
                    TransferReason.Mail,
                    // TransferReason.SortedMail, -- We need separate restrictions to support this
                };
                s_incomingRestrictions[BuildingType.PostSortingFacility] = new HashSet<TransferReason>()
                {
                    TransferReason.UnsortedMail,
                };
                s_incomingRestrictions[BuildingType.ParkMaintenanceDepot] = new HashSet<TransferReason>()
                {
                    TransferReason.ParkMaintenance,
                };
                s_incomingRestrictions[BuildingType.DisasterResponseUnit] = new HashSet<TransferReason>()
                {
                    TransferReason.Collapsed,
                    TransferReason.Collapsed2,
                };
                s_incomingRestrictions[BuildingType.SnowDump] = new HashSet<TransferReason>()
                {
                    TransferReason.Snow,
                };
                s_incomingRestrictions[BuildingType.ElementartySchool] = new HashSet<TransferReason>()
                {
                    TransferReason.Student1,
                };
                s_incomingRestrictions[BuildingType.HighSchool] = new HashSet<TransferReason>()
                {
                    TransferReason.Student2,
                };
                s_incomingRestrictions[BuildingType.University] = new HashSet<TransferReason>()
                {
                    TransferReason.Student3,
                };

                // Build a combined list as well
                foreach (var item in s_incomingRestrictions)
                {
                    foreach (TransferReason reason in item.Value)
                    {
                        s_TotalReasons.Add(reason);
                    }
                }
            }
        }
        private static void InitOutgoing()
        {
            if (s_outgoingRestrictions.Count == 0)
            {
                // Generic warehouse, raw warehouses handled separately
                s_outgoingRestrictions[BuildingType.Warehouse] = new HashSet<TransferReason>()
                {
                    TransferReason.Coal,
                    TransferReason.Petrol,
                    TransferReason.Food,
                    TransferReason.Lumber,
                    TransferReason.Flours,
                    TransferReason.AnimalProducts,
                    TransferReason.Paper,
                    TransferReason.PlanedTimber,
                    TransferReason.Plastics,
                    TransferReason.Petroleum,
                    TransferReason.Glass,
                    TransferReason.Metals,
                    TransferReason.Goods,
                    TransferReason.LuxuryProducts,
                    TransferReason.Fish,
                };

                s_outgoingRestrictions[BuildingType.ExtractionFacility] = new HashSet<TransferReason>()
                {
                    TransferReason.Oil,
                    TransferReason.Ore,
                    TransferReason.Logs,
                    TransferReason.Grain,
                };
                s_outgoingRestrictions[BuildingType.GenericExtractor] = new HashSet<TransferReason>()
                {
                    TransferReason.Oil,
                    TransferReason.Ore,
                    TransferReason.Logs,
                    TransferReason.Grain,
                };
                s_outgoingRestrictions[BuildingType.FishHarbor] = new HashSet<TransferReason>()
                {
                    TransferReason.Fish,
                };
                s_outgoingRestrictions[BuildingType.FishFarm] = new HashSet<TransferReason>()
                {
                    TransferReason.Fish,
                };
                
                s_outgoingRestrictions[BuildingType.GenericProcessing] = new HashSet<TransferReason>()
                {
                    TransferReason.Goods,
                };
               
                s_outgoingRestrictions[BuildingType.FishFactory] = new HashSet<TransferReason>()
                {
                    TransferReason.Goods,
                };
                s_outgoingRestrictions[BuildingType.Recycling] = new HashSet<TransferReason>()
                {
                    TransferReason.Coal,
                    TransferReason.Petrol,
                    TransferReason.Lumber,
                };
                s_outgoingRestrictions[BuildingType.WasteProcessing] = new HashSet<TransferReason>()
                {
                    TransferReason.Coal,
                    TransferReason.Petrol,
                    TransferReason.Lumber,
                };
                s_outgoingRestrictions[BuildingType.TaxiDepot] = new HashSet<TransferReason>()
                {
                    TransferReason.Taxi,
                };
                s_outgoingRestrictions[BuildingType.AirportMainBuilding] = new HashSet<TransferReason>()
                {
                    TransferReason.Garbage,
                    TransferReason.Crime,
                };
                s_outgoingRestrictions[BuildingType.MainIndustryBuilding] = new HashSet<TransferReason>()
                {
                    TransferReason.Garbage,
                    TransferReason.Crime,
                };
                s_outgoingRestrictions[BuildingType.ProcessingFacility] = new HashSet<TransferReason>()
                {
                    TransferReason.Flours,
                    TransferReason.AnimalProducts,
                    TransferReason.Paper,
                    TransferReason.PlanedTimber,
                    TransferReason.Plastics,
                    TransferReason.Petroleum,
                    TransferReason.Glass,
                    TransferReason.Metals,
                };
                s_outgoingRestrictions[BuildingType.UniqueFactory] = new HashSet<TransferReason>()
                {
                    TransferReason.LuxuryProducts,
                };
                s_outgoingRestrictions[BuildingType.PostOffice] = new HashSet<TransferReason>()
                {
                    TransferReason.UnsortedMail,
                };
                s_outgoingRestrictions[BuildingType.PostSortingFacility] = new HashSet<TransferReason>()
                {
                    TransferReason.SortedMail,
                };
                s_outgoingRestrictions[BuildingType.MedicalHelicopterDepot] = new HashSet<TransferReason>()
                {
                    TransferReason.SickMove,
                };

                // Build a combined list as well
                foreach (var item in s_outgoingRestrictions)
                {
                    foreach (TransferReason reason in item.Value)
                    {
                        s_TotalReasons.Add(reason);
                    }
                }
            }
        }

        // Services subject to global prefer local services:
        private static bool IsGlobalDistrictRestrictionsSupported(TransferReason material)
        {
            switch (material)
            {
                case TransferReason.Garbage:
                case TransferReason.Crime:
                case TransferReason.Fire:
                case TransferReason.Fire2:
                case TransferReason.ForestFire:
                case TransferReason.Sick:
                case TransferReason.Sick2:
                case TransferReason.Collapsed:
                case TransferReason.Collapsed2:
                case TransferReason.ParkMaintenance:
                case TransferReason.Mail:
                case TransferReason.Taxi:
                case TransferReason.Dead:
                    return true;

                // Goods subject to prefer local:
                // -none- it is too powerful, city will fall apart

                default:
                    return false;  //guard: dont apply district logic to other materials
            }
        }

        public static bool IsBuildingDistrictRestrictionsSupported(TransferReason material)
        {
            InitIncoming();
            InitOutgoing();
            return s_TotalReasons.Contains(material);
        }

        // Services subject to building prefer local services:
        public static HashSet<TransferReason> IncomingBuildingDistrictRestrictionsSupported(BuildingType eBuildingType, ushort buildingId)
        {
            InitIncoming();

            switch (eBuildingType)
            {
                case BuildingType.Warehouse:
                case BuildingType.CargoFerryWarehouseHarbor:
                    {
                        BuildingSubType eSubType = GetBuildingSubType(buildingId);
                        switch (eSubType)
                        {
                            case BuildingSubType.WarehouseOil:
                                return new HashSet<TransferReason>() { TransferReason.Oil };
                            case BuildingSubType.WarehouseOre:
                                return new HashSet<TransferReason>() { TransferReason.Ore };
                            case BuildingSubType.WarehouseCrops:
                                return new HashSet<TransferReason>() { TransferReason.Grain };
                            case BuildingSubType.WarehouseLogs:
                                return new HashSet<TransferReason>() { TransferReason.Logs };
                            default:
                                {
                                    return s_incomingRestrictions[BuildingType.Warehouse];
                                }
                        }
                    }
                default:
                    {
                        if (s_incomingRestrictions.ContainsKey(eBuildingType))
                        {
                            return s_incomingRestrictions[eBuildingType];
                        }
                        break;
                    }
            }

            return s_emptySet;
        }

        public static HashSet<TransferReason> OutgoingBuildingDistrictRestrictionsSupported(BuildingType eBuildingType, ushort buildingId)
        {
            InitOutgoing();
            switch (eBuildingType)
            {
                case BuildingType.Warehouse:
                case BuildingType.CargoFerryWarehouseHarbor:
                    {
                        BuildingSubType eSubType = GetBuildingSubType(buildingId);
                        switch (eSubType)
                        {
                            case BuildingSubType.WarehouseOil:
                                return new HashSet<TransferReason>() { TransferReason.Oil };
                            case BuildingSubType.WarehouseOre:
                                return new HashSet<TransferReason>() { TransferReason.Ore };
                            case BuildingSubType.WarehouseCrops:
                                return new HashSet<TransferReason>() { TransferReason.Grain };
                            case BuildingSubType.WarehouseLogs:
                                return new HashSet<TransferReason>() { TransferReason.Logs };
                            default:
                                {
                                    return s_outgoingRestrictions[BuildingType.Warehouse];
                                }
                        }
                    }
                case BuildingType.UniqueFactory:
                case BuildingType.ProcessingFacility:
                    {
                        if (IsProcessingFacilityWithVehicles(buildingId))
                        {
                            return s_outgoingRestrictions[eBuildingType];
                        }
                        break;
                    }
                default:
                    {
                        if (s_outgoingRestrictions.ContainsKey(eBuildingType))
                        {
                            return s_outgoingRestrictions[eBuildingType];
                        }
                        break;
                    }
            }

            return s_emptySet;
        }
  
        public static bool CanTransfer(CustomTransferOffer offerIn, CustomTransferOffer offerOut, TransferReason material)
        {
            // 4 is the maximum value for warehouse matching eg. 2/2.
            // This allows warehouse transfer between districts but only when both really want it.
            const int PREFER_LOCAL_DISTRICT_THRESHOLD = 4;     

            // Check if it is an Import/Export
            if (offerIn.IsOutside() || offerOut.IsOutside())
            {
                // Don't restrict Import/Export with district restrictions
                return true;
            }

            // Find the maximum setting from both buildings
            BuildingSettings.PreferLocal eInBuildingLocalDistrict = offerIn.GetDistrictRestriction(true, material);
            BuildingSettings.PreferLocal eOutBuildingLocalDistrict = offerOut.GetDistrictRestriction(false, material);

            // Check max priority of both buildings
            BuildingSettings.PreferLocal ePreferLocalDistrict = (BuildingSettings.PreferLocal)Math.Max((int)eInBuildingLocalDistrict, (int)eOutBuildingLocalDistrict);
            switch (ePreferLocalDistrict)
            {
                case BuildingSettings.PreferLocal.ALL_DISTRICTS:
                    {
                        return true;// Any match is fine
                    }
                case BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                    {
                        // Combined priority needs to be equal or greater than PREFER_LOCAL_DISTRICT_THRESHOLD
                        int priority = offerIn.Priority + offerOut.Priority;
                        if (priority >= PREFER_LOCAL_DISTRICT_THRESHOLD)
                        {
                            return true; // Priority is high enough to allow match
                        }
                        break;
                    }
                case BuildingSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case BuildingSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        // Not allowed to match unless in same allowed districts
                        break;
                    }
                case BuildingSettings.PreferLocal.UNKNOWN:
                    {
                        Debug.Log("Error district restriction unknown");
                        return true;
                    }
            }

            // get respective districts
            HashSet<DistrictData> incomingActualDistricts = offerIn.GetActualDistrictList();
            HashSet<DistrictData> outgoingActualDistricts = offerOut.GetActualDistrictList();

            if (SaveGameSettings.GetSettings().PreferLocalService && ePreferLocalDistrict == BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT)
            {
                // If setting is prefer local district and it's from the global settings then also allow matching Active offers, where it's not our vehicle
                if (offerIn.Active && incomingActualDistricts.Count == 0)
                {
                    // Active side is outside any district so allow it
                    return true;
                }
                else if (offerOut.Active && outgoingActualDistricts.Count == 0)
                {
                    // Active side is outside any district so allow it
                    return true;
                }
            }

            // Now we check allowed districts against actual districts for both sides
            bool bInIsValid = false;
            switch (eInBuildingLocalDistrict)
            {
                case BuildingSettings.PreferLocal.ALL_DISTRICTS:
                    {
                        bInIsValid = true;
                        break;
                    }
                case BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                case BuildingSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                    {
                        HashSet<DistrictData> incomingAllowedDistricts = offerIn.GetAllowedIncomingDistrictList();
                        bInIsValid = DistrictData.Intersect(incomingAllowedDistricts, outgoingActualDistricts);
                        break;
                    }
                case BuildingSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                    {
                        HashSet<DistrictData> incomingBannedDistricts = offerIn.GetAllowedIncomingDistrictList();
                        bInIsValid = !DistrictData.Intersect(incomingBannedDistricts, outgoingActualDistricts); // Valid if they DON'T intersect
                        break;
                    }
            }

            bool bOutIsValid = false;
            if (bInIsValid)
            {
                // Finally check outgoing district restrictions are fine
                switch (eOutBuildingLocalDistrict)
                {
                    case BuildingSettings.PreferLocal.ALL_DISTRICTS:
                        {
                            bOutIsValid = true;
                            break;
                        }
                    case BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT:
                    case BuildingSettings.PreferLocal.RESTRICT_LOCAL_DISTRICT:
                        {
                            HashSet<DistrictData> outgoingAllowedDistricts = offerOut.GetAllowedOutgoingDistrictList();
                            bOutIsValid = DistrictData.Intersect(outgoingAllowedDistricts, incomingActualDistricts);
                            break;
                        }
                    case BuildingSettings.PreferLocal.ALL_DISTRICTS_EXCEPT_FOR:
                        {
                            HashSet<DistrictData> outgoingBannedDistricts = offerOut.GetAllowedOutgoingDistrictList();
                            bOutIsValid = !DistrictData.Intersect(outgoingBannedDistricts, incomingActualDistricts); // Valid if they DON'T intersect
                            break;
                        }
                }
            }

            return (bInIsValid && bOutIsValid);
        }

        // Determine current local district setting by combining building and global settings
        public static BuildingSettings.PreferLocal GetPreferLocal(bool bIncoming, TransferReason material, CustomTransferOffer offer)
        {
            BuildingSettings.PreferLocal ePreferLocalDistrict = BuildingSettings.PreferLocal.ALL_DISTRICTS;

            // Global setting is only applied to certain services as it is too powerful otherwise.
            if (bIncoming && SaveGameSettings.GetSettings().PreferLocalService && IsGlobalDistrictRestrictionsSupported(material))
            {
                ePreferLocalDistrict = BuildingSettings.PreferLocal.PREFER_LOCAL_DISTRICT;
            }

            // Local setting
            ushort buildingId = offer.GetBuilding();
            if (buildingId != 0)
            {
                BuildingType eBuildingType = offer.GetBuildingType();
                if (bIncoming)
                {
                    if (IncomingBuildingDistrictRestrictionsSupported(eBuildingType, buildingId).Contains(material))
                    {
                        ePreferLocalDistrict = (BuildingSettings.PreferLocal)Math.Max((int)BuildingSettings.PreferLocalDistrictServicesIncoming(buildingId), (int)ePreferLocalDistrict);
                    }
                }
                else
                {
                    if (OutgoingBuildingDistrictRestrictionsSupported(eBuildingType, buildingId).Contains(material))
                    {
                        ePreferLocalDistrict = (BuildingSettings.PreferLocal)Math.Max((int)BuildingSettings.PreferLocalDistrictServicesOutgoing(buildingId), (int)ePreferLocalDistrict);
                    }
                }
            }

            return ePreferLocalDistrict;
        }
    }
}