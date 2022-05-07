using ColossalFramework;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Linq;
using TransferManagerCE.Settings;

namespace TransferManagerCE.Util
{
    [StructLayout(LayoutKind.Sequential)]
    struct PATHFINDPAIR
    {
        ushort sourceBuilding;
        ushort targetBuilding;

        public PATHFINDPAIR(ushort source, ushort target)
        {
            this.sourceBuilding = source;
            this.targetBuilding = target;
        }

        public string PrintKey()
        {
            return $"(src:{sourceBuilding} / tgt:{targetBuilding})";
        }
    }


    public sealed class PathFindFailure
    {
        // building-to-building
        const int MAX_PATHFIND = 128;
        static Dictionary<PATHFINDPAIR, long> _pathfindFails = new Dictionary<PATHFINDPAIR, long>(MAX_PATHFIND);
        static Dictionary<ushort, int> _pathfindBuildingsCounter = new Dictionary<ushort, int>(MAX_PATHFIND);

        // building-to-outsideconnection
        const int MAX_OUTSIDECONNECTIONS = 256;
        static Dictionary<PATHFINDPAIR, long> _outsideConnectionFails = new Dictionary<PATHFINDPAIR, long>(MAX_OUTSIDECONNECTIONS);

        // Statistics
        #region STATISTICS
        static int max_pathfindfails = 0;
        static int max_outsideconnectionfails = 0;
        static int total_chirps_sent = 0;        

        public static int GetMaxUsagePathFindFails() => max_pathfindfails;
        public static int GetMaxUsageOutsideFails() => max_outsideconnectionfails;
        public static int GetTotalChirps() => total_chirps_sent;
        #endregion

        static readonly object _dictionaryLock = new object();
        const long LRU_INTERVALL = TimeSpan.TicksPerMillisecond * 1000 * 15; //15 sec
        const long CHIRP_INTERVALL = TimeSpan.TicksPerMillisecond * 1000 * 60; //1 min
        static long lru_lastCleaned;
        static long last_chirp;


        /// <summary>
        /// Add or update failure pair
        /// </summary>
        private static void AddFailPair(ushort source, ushort target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);
            
            if (_pathfindFails.TryGetValue(_pair, out _info))
            {
                _info = DateTime.Now.Ticks;
                _pathfindFails[_pair] = _info;
            }
            else
            {
                if (_pathfindFails.Count >= MAX_PATHFIND)
                {
                    DebugLog.LogInfo($"Pathfindfailure: Count is {_pathfindFails.Count}, Removing key {_pathfindFails.OrderBy(x => x.Value).First().Key.PrintKey()}");
                    lock (_dictionaryLock)
                    {
                        _pathfindFails.Remove(_pathfindFails.OrderBy(x => x.Value).First().Key);
                    }
                }

                _pathfindFails.Add(_pair, DateTime.Now.Ticks);
                DebugLog.LogInfo($"Pathfindfailure: Added key {_pair.PrintKey()}, Count is {_pathfindFails.Count}");
            }


            UpdateBuildingFailCount(source);
            UpdateBuildingFailCount(target);
        }


        /// <summary>
        /// Add or update outside connection fail
        /// </summary>
        private static void AddOutsideConnectionFail(ushort source, ushort target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);

            if (_outsideConnectionFails.TryGetValue(_pair, out _info))
            {
                _info = DateTime.Now.Ticks;
                _outsideConnectionFails[_pair] = _info;
            }
            else
            {
                if (_outsideConnectionFails.Count >= MAX_OUTSIDECONNECTIONS)
                {
                    DebugLog.LogInfo($"Pathfindfailure: outsideconnection fail count is {_outsideConnectionFails.Count}, Removing key {_outsideConnectionFails.OrderBy(x => x.Value).First().Key.PrintKey()}");
                    lock (_dictionaryLock)
                    {
                        _outsideConnectionFails.Remove(_outsideConnectionFails.OrderBy(x => x.Value).First().Key);
                    }
                }

                _outsideConnectionFails.Add(_pair, DateTime.Now.Ticks);
                DebugLog.LogInfo($"Pathfindfailure: Added outsideconnection fail {_pair.PrintKey()}, Count is {_outsideConnectionFails.Count}");
            }

        }


        /// <summary>
        /// Increase buidign fail count
        /// </summary>
        private static void UpdateBuildingFailCount(ushort buildingID)
        {
            int failcount;
            if (_pathfindBuildingsCounter.TryGetValue(buildingID, out failcount))
            {
                failcount++;
                _pathfindBuildingsCounter[buildingID] = failcount;
            }
            else
                _pathfindBuildingsCounter.Add(buildingID, 1);
        }


        /// <summary>
        /// Cleanup old entries by last used
        /// </summary>
        public static void RemoveOldEntries()
        {
            long diffTime = DateTime.Now.Ticks - lru_lastCleaned;
            int failpair_remove_count = 0;
            int failoutside_remove_count = 0;

            // Statistics:
            max_pathfindfails = Math.Max(max_pathfindfails, _pathfindFails.Count);
            max_outsideconnectionfails = Math.Max(max_outsideconnectionfails, _outsideConnectionFails.Count);


            if (diffTime > LRU_INTERVALL)
            {
                lru_lastCleaned = DateTime.Now.Ticks;

                lock (_dictionaryLock)
                {
                    foreach (var item in _pathfindFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - LRU_INTERVALL)).ToList())
                    {
                        _pathfindFails.Remove(item.Key);
                        failpair_remove_count++;
                    }

                    foreach (var item in _outsideConnectionFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - LRU_INTERVALL)).ToList())
                    {
                        _outsideConnectionFails.Remove(item.Key);
                        failoutside_remove_count++;
                    }
                }

                DebugLog.LogInfo($"Pathfindfailure: LRU removed {failpair_remove_count} pairs + {failoutside_remove_count} outsideconnections, new count is {_pathfindFails.Count} pairs + {_outsideConnectionFails.Count} outsideconnections");
            }
        }


        /// <summary>
        /// Returns true when pair exists in exclusion list
        /// </summary>
        public static bool FindBuildingPair(ushort source, ushort target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);
            if (_pathfindFails.TryGetValue(_pair, out _info))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true when buildingID in outsideconnection fail list
        /// </summary>
        public static bool FindOutsideConnectionPair(ushort source, ushort target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);
            if (_outsideConnectionFails.TryGetValue(_pair, out _info))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Called from CarAIPatch: record a failed pair
        /// (does not apply to outside connections)
        /// </summary>
        public static void RecordPathFindFailure(ushort vehicleID, ref Vehicle data)
        {
                var sourceBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
                var targetBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding];

#if (DEBUG)
                var instB = default(InstanceID);
                instB.Building = data.m_sourceBuilding;
                var sourceName = Singleton<InstanceManager>.instance.GetName(instB);
                instB.Building = data.m_targetBuilding;
                var targetName = Singleton<InstanceManager>.instance.GetName(instB);
                DebugLog.LogInfo($"Pathfindfailure: from '{sourceName}'[{data.m_sourceBuilding}: {sourceBuilding.Info?.name}({sourceBuilding.Info?.m_class})] --> '{targetName}'[{data.m_targetBuilding}: {targetBuilding.Info?.name}({targetBuilding.Info?.m_class})]");
#endif

            if (((data.m_targetBuilding != 0) && !(targetBuilding.Info?.m_buildingAI is OutsideConnectionAI)) &&
                ((data.m_sourceBuilding != 0) && !(sourceBuilding.Info?.m_buildingAI is OutsideConnectionAI)))
            {
                AddFailPair(data.m_sourceBuilding, data.m_targetBuilding);
            }
            else if (((data.m_targetBuilding != 0) && (targetBuilding.Info?.m_buildingAI is OutsideConnectionAI)) ||
                     ((data.m_sourceBuilding != 0) && (sourceBuilding.Info?.m_buildingAI is OutsideConnectionAI)))
            {
                AddOutsideConnectionFail(data.m_sourceBuilding, data.m_targetBuilding);
            }

        }


        /// <summary>
        /// Create chirper message about pathfind issues
        /// </summary>
        public static void SendPathFindChirp()
        {
            long diffTime = DateTime.Now.Ticks - last_chirp;
            if (diffTime > CHIRP_INTERVALL)
            {
                last_chirp = DateTime.Now.Ticks;

                if (ModSettings.GetSettings().optionPathfindChirper && (_pathfindBuildingsCounter.Count > 0))
                {
                    // get top failed building
                    var pair = _pathfindBuildingsCounter.OrderByDescending(x => x.Value).First();
                    ushort buildingKey = pair.Key;
                    int buildingFailedCount = pair.Value;

                    var instB = default(InstanceID);
                    instB.Building = buildingKey;
                    var senderBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingKey];
                    var buildingName = Singleton<InstanceManager>.instance.GetName(instB);
                        
                    if (buildingFailedCount > 1)
                    {
                        uint sender = FindCitizenOfBuilding(senderBuilding);
                        string hashtag = ((senderBuilding.Info.m_class.GetZone() == ItemClass.Zone.ResidentialLow) || (senderBuilding.Info.m_class.GetZone() == ItemClass.Zone.ResidentialHigh)) ? "#ilivethere" : "#iworkthere";
                        Singleton<MessageManager>.instance.QueueMessage(new CustomCitizenMessage(sender, $"@mayor: FIX YOUR ROAD NETWORK!\n{_pathfindBuildingsCounter.Count} unrouted transfers recenty! Most common:\n{buildingName}({senderBuilding.Info?.name}) -- #ROUTING #{buildingFailedCount} FAILS! {hashtag}!", null));
                        DebugLog.LogInfo($"@mayor: FIX YOUR ROAD NETWORK!\n{_pathfindBuildingsCounter.Count} unrouted transfers recently! Most common:\n{buildingName}({senderBuilding.Info?.name}) -- #ROUTING #{buildingFailedCount} FAILS! {hashtag}!");
                        total_chirps_sent++;
                    }
                }

                // clear building fail counter
                _pathfindBuildingsCounter.Clear();
            }
        }


        /// <summary>
        /// FInd citizen working there or living there for chirp origin
        /// </summary>
        private static uint FindCitizenOfBuilding(Building building)
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint mCitizenUnits = building.m_citizenUnits;

            while (mCitizenUnits != 0)
            {
                uint mNextUnit = citizenManager.m_units.m_buffer[mCitizenUnits].m_nextUnit;
                CitizenUnit.Flags flags = CitizenUnit.Flags.Work | CitizenUnit.Flags.Home;
                if ((ushort)(citizenManager.m_units.m_buffer[mCitizenUnits].m_flags & flags) != 0)
                {
                    return citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen0 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen0 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen1 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen1 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen2 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen2 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen3 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen3 :
                            citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen4 != 0 ? citizenManager.m_units.m_buffer[mCitizenUnits].m_citizen4 :
                            (uint)0;
                }

                mCitizenUnits = mNextUnit;
            }
            return (uint)0;
        }

    }
}
