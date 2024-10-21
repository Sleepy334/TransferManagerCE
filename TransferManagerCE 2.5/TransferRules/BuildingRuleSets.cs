using System.Collections.Generic;
using TransferManagerCE.Common;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.TransferRules
{
    public class BuildingRuleSets
    {
        private static Dictionary<BuildingType, List<ReasonRule>> BuildingRules = new Dictionary<BuildingType, List<ReasonRule>>();
        private static readonly object s_dictionaryLock = new object();
        private static bool s_initNeeded = true;

        private static HashSet<CustomTransferReason.Reason> s_districtReasons = new HashSet<CustomTransferReason.Reason>();
        private static HashSet<CustomTransferReason.Reason> s_buildingReasons = new HashSet<CustomTransferReason.Reason>();
        private static HashSet<CustomTransferReason.Reason> s_distanceReasons = new HashSet<CustomTransferReason.Reason>();

        public static bool IsDistrictRestrictionsSupported(CustomTransferReason.Reason material)
        {
            if (s_initNeeded)
            {
                lock (s_dictionaryLock)
                {
                    Init();
                }
            }

            return s_districtReasons.Contains(material);
        }

        public static bool IsBuildingRestrictionsSupported(CustomTransferReason.Reason material)
        {
            if (s_initNeeded)
            {
                lock (s_dictionaryLock)
                {
                    Init();
                }
            }

            return s_buildingReasons.Contains(material);
        }

        public static bool IsDistanceRestrictionsSupported(CustomTransferReason.Reason material)
        {
            if (s_initNeeded)
            {
                lock (s_dictionaryLock)
                {
                    Init();
                }
            }

            return s_distanceReasons.Contains(material);
        }

        public static int GetRestrictionId(BuildingType eBuildingType, CustomTransferReason.Reason material)
        {
            lock (s_dictionaryLock)
            {
                Init();

                if (BuildingRules.ContainsKey(eBuildingType))
                {
                    foreach (ReasonRule rule in BuildingRules[eBuildingType])
                    {
                        if (rule.m_reasons.Contains(material))
                        {
                            return rule.m_id;
                        }
                    }
                }
                return -1;
            }
        }

        public static bool HasIncomingDistrictRules(BuildingType eBuildingType, CustomTransferReason.Reason material)
        {
            lock (s_dictionaryLock)
            {
                Init();

                if (BuildingRules.ContainsKey(eBuildingType))
                {
                    foreach (ReasonRule rule in BuildingRules[eBuildingType])
                    {
                        if (rule.m_reasons.Contains(material))
                        {
                            return rule.m_incomingDistrict;
                        }
                    }
                }

                return false;
            }
        }

        public static bool HasOutgoingDistrictRules(BuildingType eBuildingType, CustomTransferReason.Reason material)
        {
            lock (s_dictionaryLock)
            {
                Init();

                if (BuildingRules.ContainsKey(eBuildingType))
                {
                    foreach (ReasonRule rule in BuildingRules[eBuildingType])
                    {
                        if (rule.m_reasons.Contains(material))
                        {
                            return rule.m_outgoingDistrict;
                        }
                    }
                }

                return false;
            }
        }

        public static bool HasDistanceRules(BuildingType eBuildingType, CustomTransferReason.Reason material)
        {
            lock (s_dictionaryLock)
            {
                Init();

                if (BuildingRules.ContainsKey(eBuildingType))
                {
                    foreach (ReasonRule rule in BuildingRules[eBuildingType])
                    {
                        if (rule.m_reasons.Contains(material))
                        {
                            return rule.m_distance;
                        }
                    }
                }

                return false;
            }
        }

        public static List<ReasonRule> GetRules(BuildingType eBuildingType)
        {
            lock (s_dictionaryLock)
            {
                Init();
                if (BuildingRules.ContainsKey(eBuildingType))
                {
                    return BuildingRules[eBuildingType];
                }
                else
                {
                    return new List<ReasonRule>();
                }
            }
        }
        
        public static List<ReasonRule> GetRules(BuildingType eBuildingType, ushort buildingId)
        {
            lock (s_dictionaryLock)
            {
                Init();

                if (BuildingRules.ContainsKey(eBuildingType))
                {
                    List<ReasonRule> buildingRules = new List<ReasonRule>(BuildingRules[eBuildingType]);

                    // Select appropriate rulesets for certain types
                    switch (eBuildingType)
                    {
                        case BuildingType.Warehouse:
                        case BuildingType.WarehouseStation:
                            {
                                // Warehouses, just return the actual material they store
                                List<ReasonRule> rules = new List<ReasonRule>();

                                CustomTransferReason.Reason actualTransferReason = GetWarehouseTransferReason(buildingId);
                                if (actualTransferReason != CustomTransferReason.Reason.None)
                                {
                                    foreach (ReasonRule rule in buildingRules)
                                    {
                                        if (rule is not null && rule.m_reasons.Contains(actualTransferReason))
                                        {
                                            rules.Add(rule);
                                            break;
                                        }
                                    }
                                }

                                return rules;
                            }
                        case BuildingType.CargoFerryWarehouseHarbor:
                            {
                                // Warehouses, just return the actual material they store
                                List<ReasonRule> rules = new List<ReasonRule>();

                                CustomTransferReason.Reason material = GetCargoFerryWarehouseActualTransferReason(buildingId);
                                if (material != CustomTransferReason.Reason.None)
                                {
                                    foreach (ReasonRule rule in buildingRules)
                                    {
                                        if (rule is not null && rule.m_reasons.Contains(material))
                                        {
                                            rules.Add(rule);
                                            break;
                                        }
                                    }
                                }
                                
                                return rules;
                            }
                        case BuildingType.UniqueFactory:
                            {
                                // Remove outgoing rule if unique factory has no vehicles.
                                if (!HasVehicles(eBuildingType, buildingId))
                                {
                                    List<ReasonRule> rules = new List<ReasonRule>
                                    {
                                        buildingRules[0]
                                    };
                                    return rules;
                                }
                                break;
                            }
                    }

                    return buildingRules;
                }
            }

            return new List<ReasonRule>();
        }

        private static void Init()
        {
            if (s_initNeeded)
            {
                s_initNeeded = false;
                BuildingRules.Clear();

                ElementarySchool();
                HighSchool();
                University();
                AirportMainTerminal();

                // Services
                Cemetery();
                Hospital();
                MedicalHelicopterDepot();
                PoliceStation();
                PoliceHelicopterDepot();
                Prison();
                Bank();
                FireStation();
                FireHelicopterDepot();
                ParkMaintenanceDepot();
                RoadMaintenanceDepot();
                TaxiDepot();
                TaxiStand();
                DisasterResponseUnit();
                SnowDump();

                // Garbage
                LandFill();
                IncinerationPlant();
                Recycling();
                WasteTransfer();
                WasteProcessing();

                // Mail
                PostOffice();
                PostSortingFacility();

                Commercial();
                MainIndustryBuilding();
                ExtractionFacility();
                ProcessingFacility();
                UniqueFactory();

                GenericExtractor();
                GenericProcessing();
                GenericFactory();

                FishFarm();
                FishHarbor();
                FishFactory();
                FishMarket();

                Warehouse();
                OutsideConnection();

                CoalPowerPlant();
                PetrolPowerPlant();
                BoilerPlant();
                DisasterShelter();
                PumpingService();

                // Load transfer reasons into HashSet so we can check if supported
                foreach (KeyValuePair<BuildingType, List<ReasonRule>> kvp in BuildingRules)
                {
                    foreach (ReasonRule rule in kvp.Value)
                    {
                        // Districts
                        if (rule.m_incomingDistrict || rule.m_outgoingDistrict)
                        {
                            foreach (CustomTransferReason.Reason material in rule.m_reasons)
                            {
                                s_districtReasons.Add(material);
                            }
                        }

                        // Buildings
                        if (rule.m_incomingBuilding || rule.m_outgoingBuilding)
                        {
                            foreach (CustomTransferReason.Reason material in rule.m_reasons)
                            {
                                s_buildingReasons.Add(material);
                            }
                        }

                        // Distance
                        if (rule.m_distance)
                        {
                            foreach (CustomTransferReason.Reason material in rule.m_reasons)
                            {
                                s_distanceReasons.Add(material);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// Services
        /// </summary>
        
        private static void ElementarySchool()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonStudent1"); //"Students";
                rule.AddReason(CustomTransferReason.Reason.StudentES);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.ElementartySchool] = list;
        }
        private static void HighSchool()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonStudent2"); //"Students";
                rule.AddReason(CustomTransferReason.Reason.StudentHS);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.HighSchool] = list;
        }
        private static void University()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonStudent3"); //"Students";
                rule.AddReason(CustomTransferReason.Reason.StudentUni);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.University] = list;
        }

        private static void AirportMainTerminal()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCrime"); //"Crime";
                rule.AddReason(CustomTransferReason.Reason.Crime);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonGarbage"); //"Garbage";
                rule.AddReason(CustomTransferReason.Reason.Garbage);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.AirportMainTerminal] = list;
            BuildingRules[BuildingType.AirportCargoTerminal] = list;
        }

        private static void Cemetery()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // Dead
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonDead"); //"Collecting Dead";
                rule.AddReason(CustomTransferReason.Reason.Dead);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            // DeadMove
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonDeadMove"); //"Moving Dead";
                rule.AddReason(CustomTransferReason.Reason.DeadMove);
                rule.m_incomingDistrict = true;
                rule.m_outgoingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Cemetery] = list;
        }

        private static void Hospital()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // Sick
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonSick"); //"Collecting Sick";
                rule.AddReason(CustomTransferReason.Reason.Sick);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            // SickMove, IN from medical helicopters
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonSickMove"); //"Moving Sick";
                rule.AddReason(CustomTransferReason.Reason.SickMove);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Hospital] = list;
        }
        private static void MedicalHelicopterDepot()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // Sick2 IN to request a helicopter
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonSick"); //"Collecting Sick";
                rule.AddReason(CustomTransferReason.Reason.Sick2);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            // SickMove OUT (From helicopter) after picking up a sick patient this is used to find a nearby hospital
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonSickMove"); //"Moving Sick";
                rule.AddReason(CustomTransferReason.Reason.SickMove);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.MedicalHelicopterDepot] = list;
        }
        
        private static void PoliceStation()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // Crime
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCrime"); //"Crime";
                rule.AddReason(CustomTransferReason.Reason.Crime);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            // CrimeMove
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonCrimeMove"); //"Moving Criminals";
                rule.AddReason(CustomTransferReason.Reason.CriminalMove);
                rule.m_incomingDistrict = true;
                rule.m_outgoingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.PoliceStation] = list;
        }
        private static void PoliceHelicopterDepot()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // Crime
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCrime"); //"Crime";
                rule.AddReason(CustomTransferReason.Reason.Crime2);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            // CrimeMove
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonCrimeMove"); //"Moving Criminals";
                rule.AddReason(CustomTransferReason.Reason.CriminalMove);
                rule.m_incomingDistrict = true;
                rule.m_outgoingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.PoliceHelicopterDepot] = list;
        }
        private static void Prison()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // CrimeMove
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCrimeMove"); //"Moving Criminals";
                rule.AddReason(CustomTransferReason.Reason.CriminalMove);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Prison] = list;
        }
        private static void Bank()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            // CrimeMove
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCash"); //"Moving Criminals";
                rule.AddReason(CustomTransferReason.Reason.Cash);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Bank] = list;
        }
        
        private static void FireStation()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonFire"); //"Fire";
                rule.AddReason(CustomTransferReason.Reason.Fire);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.FireStation] = list;
        }
        private static void FireHelicopterDepot()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonFire2"); //"Fire Helicopter";
                rule.AddReason(CustomTransferReason.Reason.Fire2);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            { 
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonForestFire"); //"Forest Fire";
                rule.AddReason(CustomTransferReason.Reason.ForestFire);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.FireHelicopterDepot] = list;
        }
        private static void LandFill()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonGarbage"); //"Garbage Collection";
                rule.AddReason(CustomTransferReason.Reason.Garbage);
                rule.m_incomingDistrict = true; // Active
                rule.m_incomingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonGarbageMove"); //"Garbage Move";
                rule.AddReason(CustomTransferReason.Reason.GarbageMove);
                rule.m_incomingDistrict = true; // Passive
                rule.m_outgoingDistrict = true; // Active
                rule.m_incomingBuilding = true; // Passive
                rule.m_outgoingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 2;
                rule.m_name = Localization.Get("reasonGarbageTransfer"); //"Garbage Transfer";
                rule.AddReason(CustomTransferReason.Reason.GarbageTransfer);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Landfill] = list;
        }
        private static void IncinerationPlant()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonGarbage"); //"Garbage Collection";
                rule.AddReason(CustomTransferReason.Reason.Garbage);
                rule.m_incomingDistrict = true; // Active
                rule.m_incomingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonGarbageMove"); //"Garbage Move";
                rule.AddReason(CustomTransferReason.Reason.GarbageMove);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true; // Active
                list.Add(rule);
            }

            BuildingRules[BuildingType.IncinerationPlant] = list;
        }
        private static void Recycling()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonGarbage"); //"Garbage Collection";
                rule.AddReason(CustomTransferReason.Reason.Garbage);
                rule.m_incomingDistrict = true; // Active
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonGarbageMove"); //"Garbage Move";
                rule.AddReason(CustomTransferReason.Reason.GarbageMove);
                rule.m_incomingDistrict = true; // Passive from land fills
                rule.m_incomingBuilding = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 2;
                rule.m_name = Localization.Get("reasonMaterialOut"); //"Outgoing Material";
                rule.AddReason(CustomTransferReason.Reason.Coal);
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.m_outgoingDistrict = true; // Active
                rule.m_outgoingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Recycling] = list;
        }
        private static void WasteTransfer()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            { 
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonGarbage"); //"Garbage Collection";
                rule.AddReason(CustomTransferReason.Reason.Garbage);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonGarbageMove"); //"Garbage Move";
                rule.AddReason(CustomTransferReason.Reason.GarbageMove);
                rule.m_incomingDistrict = true; // Passive from land fills
                rule.m_outgoingDistrict = true; // When in "Empty" mode
                rule.m_incomingBuilding = true; // Passive from land fills
                rule.m_outgoingBuilding = true; // When in "Empty" mode
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 2;
                rule.m_name = Localization.Get("reasonGarbageTransfer"); //"Garbage Transfer";
                rule.AddReason(CustomTransferReason.Reason.GarbageTransfer);
                rule.m_outgoingDistrict = true; // Passive
                rule.m_outgoingBuilding = true; // Passive
                list.Add(rule);
            }

            BuildingRules[BuildingType.WasteTransfer] = list;
        }
        private static void WasteProcessing()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonGarbageTransfer"); //"Garbage Transfer";
                rule.AddReason(CustomTransferReason.Reason.GarbageTransfer);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonMaterialOut"); //"Outgoing Material";
                rule.AddReason(CustomTransferReason.Reason.Coal);
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.WasteProcessing] = list;
        }
        private static void PostOffice()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonMail"); //"Mail";
                rule.AddReason(CustomTransferReason.Reason.Mail);
                rule.m_incomingDistrict = true; // Active
                rule.m_incomingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 3;
                rule.m_name = Localization.Get("reasonMail2"); //"Mail";
                rule.AddReason(CustomTransferReason.Reason.Mail2);
                rule.m_incomingDistrict = true; // Active
                rule.m_incomingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonUnsortedMail"); //"Unsorted Mail";
                rule.AddReason(CustomTransferReason.Reason.UnsortedMail);
                rule.m_outgoingDistrict = true; // Active
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 2;
                rule.m_name = Localization.Get("reasonSortedMail"); //"Sorted Mail";
                rule.AddReason(CustomTransferReason.Reason.SortedMail);
                rule.m_incomingDistrict = true; // Passive
                rule.m_incomingBuilding = true; // Passive
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.PostOffice] = list;
        }
        private static void PostSortingFacility()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 2;
                rule.m_name = Localization.Get("reasonMail2"); //"Mail";
                rule.AddReason(CustomTransferReason.Reason.Mail2);
                rule.m_incomingDistrict = true; // Active
                rule.m_incomingBuilding = true; // Active
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonUnsortedMail"); //"Unsorted Mail";
                rule.AddReason(CustomTransferReason.Reason.UnsortedMail);
                rule.AddReason(CustomTransferReason.Reason.OutgoingMail);
                rule.m_incomingDistrict = true; // Passive
                rule.m_incomingBuilding = true; // Passive
                rule.m_import = true;
                rule.m_export = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonSortedMail"); //"Sorted Mail";
                rule.AddReason(CustomTransferReason.Reason.SortedMail);
                rule.AddReason(CustomTransferReason.Reason.IncomingMail);
                rule.m_outgoingDistrict = true; // Active
                rule.m_outgoingBuilding = true; // Active
                rule.m_distance = true;
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.PostSortingFacility] = list;
        }
        private static void ParkMaintenanceDepot()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonParkMaintenance"); //"Park Maintenance";
                rule.AddReason(CustomTransferReason.Reason.ParkMaintenance);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            BuildingRules[BuildingType.ParkMaintenanceDepot] = list;
        }
        private static void RoadMaintenanceDepot()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonRoadMaintenance"); //"Road Maintenance";
                rule.AddReason(CustomTransferReason.Reason.RoadMaintenance);
                rule.m_outgoingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            BuildingRules[BuildingType.RoadMaintenanceDepot] = list;
        }
        private static void TaxiDepot()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonTaxi"); //"Taxi";
                rule.AddReason(CustomTransferReason.Reason.Taxi);
                rule.m_outgoingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonTaxiMove"); //"TaxiMove";
                rule.AddReason(CustomTransferReason.Reason.TaxiMove);
                rule.m_outgoingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            BuildingRules[BuildingType.TaxiDepot] = list;
        }
        private static void TaxiStand()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonTaxi"); //"Taxi";
                rule.AddReason(CustomTransferReason.Reason.Taxi);
                rule.m_outgoingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonTaxiMove"); //"TaxiMove";
                rule.AddReason(CustomTransferReason.Reason.TaxiMove);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            BuildingRules[BuildingType.TaxiStand] = list;
        }
        private static void DisasterResponseUnit()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCollapsed"); //"Trucks";
                rule.AddReason(CustomTransferReason.Reason.Collapsed);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                // Outgoing product
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonCollapsed2"); //"Helicopters";
                rule.AddReason(CustomTransferReason.Reason.Collapsed2);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.DisasterResponseUnit] = list;
        }
        private static void SnowDump()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonSnow");
                rule.AddReason(CustomTransferReason.Reason.Snow);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }
            {
                // Outgoing product
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonSnowMove");
                rule.AddReason(CustomTransferReason.Reason.SnowMove);
                rule.m_incomingDistrict = true;
                rule.m_outgoingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.SnowDump] = list;
        }
        

        private static void CoalPowerPlant()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Coal);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.CoalPowerPlant] = list;
        }
        private static void PetrolPowerPlant()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.PetrolPowerPlant] = list;
        }
        private static void BoilerPlant()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.BoilerStation] = list;
        }
        private static void DisasterShelter()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Goods);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.DisasterShelter] = list;
        }

        private static void PumpingService()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonFloodWater");
                rule.AddReason(CustomTransferReason.Reason.FloodWater);
                rule.m_incomingDistrict = true;
                rule.m_distance = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.PumpingService] = list;
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// Goods
        /// </summary>
        private static void Commercial()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial") + " 1";// "Incoming Goods";
                rule.AddReason(CustomTransferReason.Reason.Goods);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_import = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonIncomingMaterial") + " 2";// "Incoming LuxuryProducts";
                rule.AddReason(CustomTransferReason.Reason.LuxuryProducts);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Commercial] = list;
        }
        private static void MainIndustryBuilding()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonCrime"); //"Crime";
                rule.AddReason(CustomTransferReason.Reason.Crime);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonGarbage"); //"Garbage";
                rule.AddReason(CustomTransferReason.Reason.Garbage);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.MainIndustryBuilding] = list;
        }
        private static void ExtractionFacility()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonRawMaterial"); //"Raw Material";
                rule.AddReason(CustomTransferReason.Reason.Oil);
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.Ore);
                rule.AddReason(CustomTransferReason.Reason.ForestProducts);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.ExtractionFacility] = list;
        }
        private static void ProcessingFacility()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Oil);
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.Ore);
                rule.AddReason(CustomTransferReason.Reason.ForestProducts);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_import = true;
                list.Add(rule);
            }
            {
                // DLC intermediate products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Outgoing Material";

                // DLC intermediate materials
                rule.AddReason(CustomTransferReason.Reason.PlanedTimber);
                rule.AddReason(CustomTransferReason.Reason.Paper);
                rule.AddReason(CustomTransferReason.Reason.Glass);
                rule.AddReason(CustomTransferReason.Reason.Metals);
                rule.AddReason(CustomTransferReason.Reason.Petroleum);
                rule.AddReason(CustomTransferReason.Reason.Plastics);
                rule.AddReason(CustomTransferReason.Reason.AnimalProducts);
                rule.AddReason(CustomTransferReason.Reason.Flours);

                // We now also add generic intermediate materials due to Industries Remastered assets
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.AddReason(CustomTransferReason.Reason.Food);
                rule.AddReason(CustomTransferReason.Reason.Coal);

                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.ProcessingFacility] = list;
        }
        private static void UniqueFactory()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";

                // DLC intermediate materials
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.PlanedTimber);
                rule.AddReason(CustomTransferReason.Reason.Paper);
                rule.AddReason(CustomTransferReason.Reason.Glass);
                rule.AddReason(CustomTransferReason.Reason.Metals);
                rule.AddReason(CustomTransferReason.Reason.Petroleum);
                rule.AddReason(CustomTransferReason.Reason.Plastics);
                rule.AddReason(CustomTransferReason.Reason.AnimalProducts);
                rule.AddReason(CustomTransferReason.Reason.Flours);

                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                list.Add(rule);
            }
            {
                // Outgoing product
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Outgoing Material";

                // DLC final materials
                rule.AddReason(CustomTransferReason.Reason.LuxuryProducts);

                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.UniqueFactory] = list;
        }
        private static void GenericExtractor()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Oil);
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.Ore);
                rule.AddReason(CustomTransferReason.Reason.ForestProducts);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.GenericExtractor] = list;
        }
        private static void GenericProcessing()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Oil);
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.Ore);
                rule.AddReason(CustomTransferReason.Reason.ForestProducts);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                rule.m_import = true;
                list.Add(rule);
            }
            {
                // Generic intermediate products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Outgoing Material";
                rule.AddReason(CustomTransferReason.Reason.Coal);
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.AddReason(CustomTransferReason.Reason.Food);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.GenericProcessing] = list;
        }
        private static void GenericFactory()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial") + " 1";

                // Generic production materials
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.AddReason(CustomTransferReason.Reason.Food);
                rule.AddReason(CustomTransferReason.Reason.Coal);

                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = false;
                rule.m_import = true;
                list.Add(rule);
            }
            {
                // Raw products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 2;
                rule.m_name = Localization.Get("reasonIncomingMaterial") + " 2";

                // DLC production materials
                rule.AddReason(CustomTransferReason.Reason.PlanedTimber);
                rule.AddReason(CustomTransferReason.Reason.Paper);
                rule.AddReason(CustomTransferReason.Reason.Glass);
                rule.AddReason(CustomTransferReason.Reason.Metals);
                rule.AddReason(CustomTransferReason.Reason.Petroleum);
                rule.AddReason(CustomTransferReason.Reason.Plastics);
                rule.AddReason(CustomTransferReason.Reason.AnimalProducts);
                rule.AddReason(CustomTransferReason.Reason.Flours);

                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = false;
                list.Add(rule);
            }
            {
                // Generic factory output
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonOutgoingMaterial");
                rule.AddReason(CustomTransferReason.Reason.Goods);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.GenericFactory] = list;
        }
        private static void FishFarm()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // DLC intermediate products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Outgoing Material";
                rule.AddReason(CustomTransferReason.Reason.Fish);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.FishFarm] = list;
        }
        private static void FishHarbor()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // DLC intermediate products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Outgoing Material";
                rule.AddReason(CustomTransferReason.Reason.Fish);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.FishHarbor] = list;
        }
        private static void FishFactory()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Fish);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                rule.m_import = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonOutgoingMaterial"); //"Outgoing Material";
                rule.AddReason(CustomTransferReason.Reason.Goods);
                rule.m_outgoingDistrict = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.FishFactory] = list;
        }
        private static void FishMarket()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonIncomingMaterial"); //"Incoming Material";
                rule.AddReason(CustomTransferReason.Reason.Fish);
                rule.m_incomingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_distance = true;
                rule.m_import = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.FishMarket] = list;
        }
        private static void Warehouse()
        {
            List<ReasonRule> list = new List<ReasonRule>();

            {
                // DLC intermediate products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonWarehouse"); //"Storage";
                rule.AddReason(CustomTransferReason.Reason.PlanedTimber);
                rule.AddReason(CustomTransferReason.Reason.Paper);
                rule.AddReason(CustomTransferReason.Reason.Glass);
                rule.AddReason(CustomTransferReason.Reason.Metals);
                rule.AddReason(CustomTransferReason.Reason.Petroleum);
                rule.AddReason(CustomTransferReason.Reason.Plastics);
                rule.AddReason(CustomTransferReason.Reason.AnimalProducts);
                rule.AddReason(CustomTransferReason.Reason.Flours);
                rule.AddReason(CustomTransferReason.Reason.LuxuryProducts);
                rule.AddReason(CustomTransferReason.Reason.Fish);
                rule.m_incomingDistrict = true;
                rule.m_outgoingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_export = true;
                list.Add(rule);
            }
            {
                // Generic industries intermediate products
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonWarehouse"); //"Storage";
                rule.AddReason(CustomTransferReason.Reason.Oil);
                rule.AddReason(CustomTransferReason.Reason.Ore);
                rule.AddReason(CustomTransferReason.Reason.ForestProducts);
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.Coal);
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.AddReason(CustomTransferReason.Reason.Food);
                rule.AddReason(CustomTransferReason.Reason.Goods);
                rule.m_incomingDistrict = true;
                rule.m_outgoingDistrict = true;
                rule.m_incomingBuilding = true;
                rule.m_outgoingBuilding = true;
                rule.m_distance = true;
                rule.m_import = true;
                rule.m_export = true;
                list.Add(rule);
            }

            BuildingRules[BuildingType.Warehouse] = list;
            BuildingRules[BuildingType.WarehouseStation] = list;
            BuildingRules[BuildingType.CargoFerryWarehouseHarbor] = list;
        }

        private static void OutsideConnection()
        {
            List<ReasonRule> list = new List<ReasonRule>();
            
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 0;
                rule.m_name = Localization.Get("reasonGoods"); //"Goods";
                rule.AddReason(CustomTransferReason.Reason.Oil);
                rule.AddReason(CustomTransferReason.Reason.Ore);
                rule.AddReason(CustomTransferReason.Reason.ForestProducts);
                rule.AddReason(CustomTransferReason.Reason.Crops);
                rule.AddReason(CustomTransferReason.Reason.Goods);
                rule.AddReason(CustomTransferReason.Reason.Coal);
                rule.AddReason(CustomTransferReason.Reason.Lumber);
                rule.AddReason(CustomTransferReason.Reason.Petrol);
                rule.AddReason(CustomTransferReason.Reason.Food);
                rule.AddReason(CustomTransferReason.Reason.PlanedTimber);
                rule.AddReason(CustomTransferReason.Reason.Paper);
                rule.AddReason(CustomTransferReason.Reason.Glass);
                rule.AddReason(CustomTransferReason.Reason.Metals);
                rule.AddReason(CustomTransferReason.Reason.Petroleum);
                rule.AddReason(CustomTransferReason.Reason.Plastics);
                rule.AddReason(CustomTransferReason.Reason.AnimalProducts);
                rule.AddReason(CustomTransferReason.Reason.Flours);
                rule.AddReason(CustomTransferReason.Reason.LuxuryProducts);
                rule.AddReason(CustomTransferReason.Reason.Fish);
                rule.m_import = true;
                rule.m_export = true;
                list.Add(rule);
            }
            {
                ReasonRule rule = new ReasonRule();
                rule.m_id = 1;
                rule.m_name = Localization.Get("reasonMail"); //"Mail";
                rule.AddReason(CustomTransferReason.Reason.SortedMail);
                rule.AddReason(CustomTransferReason.Reason.IncomingMail);
                rule.AddReason(CustomTransferReason.Reason.UnsortedMail);
                rule.AddReason(CustomTransferReason.Reason.OutgoingMail);
                rule.m_import = true;
                rule.m_export = true;
                list.Add(rule);
            }
            BuildingRules[BuildingType.OutsideConnection] = list;
        }
    }
}
