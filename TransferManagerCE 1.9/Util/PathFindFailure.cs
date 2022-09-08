using ColossalFramework;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.Linq;
using TransferManagerCE.Settings;

namespace TransferManagerCE.Util
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PATHFINDPAIR
    {
        public InstanceID m_source;
        public InstanceID m_target;

        public PATHFINDPAIR(InstanceID source, InstanceID target)
        {
            m_source = source;
            m_target = target;
        }

        public string PrintKey()
        {
            return $"(src:{m_source} / tgt:{m_target})";
        }
    }


    public sealed class PathFindFailure
    {
        // building-to-building
        static Dictionary<PATHFINDPAIR, long> _pathfindFails = new Dictionary<PATHFINDPAIR, long>(512);

        // building-to-outsideconnection
        static Dictionary<PATHFINDPAIR, long> _outsideConnectionFails = new Dictionary<PATHFINDPAIR, long>(512);
        static readonly object _dictionaryLock = new object();

        // Transfer Issue building fail counters
        static Dictionary<InstanceID, int> _totalPathfindBuildingsCounter = new Dictionary<InstanceID, int>();
        static readonly object s_pathCounterLock = new object();

        public const long PATH_TIMEOUT_INTERNAL = TimeSpan.TicksPerMinute * 5; // Increased to 5 mintutes
        static long lru_lastCleanedInternal;

        public const long PATH_TIMEOUT_OUTSIDE = TimeSpan.TicksPerMinute; // 1 minute
        static long lru_lastCleanedOutside;

        public static Dictionary<PATHFINDPAIR, long> GetPathFailsCopy()
        {
            Dictionary<PATHFINDPAIR, long> copy;
            lock (_dictionaryLock)
            {
                copy = new Dictionary<PATHFINDPAIR, long>(_pathfindFails);
            }
            return copy;
        }

        public static Dictionary<PATHFINDPAIR, long> GetOutsideFailsCopy()
        {
            Dictionary<PATHFINDPAIR, long> copy;
            lock (_dictionaryLock)
            {
                copy = new Dictionary<PATHFINDPAIR, long>(_outsideConnectionFails);
            }
            return copy;
        }

        public static int GetPathFailureCount()
        {
            return _pathfindFails.Count;
        }

        public static int GetOutsidePathFailureCount()
        {
            return _outsideConnectionFails.Count;
        }

        public static int GetTotalPathFailures(InstanceID instance)
        {
            lock (s_pathCounterLock)
            {
                int iValue;
                if (_totalPathfindBuildingsCounter.TryGetValue(instance, out iValue))
                {
                    return iValue;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static void ResetPathingStatistics()
        {
            lock (s_pathCounterLock)
            {
                _totalPathfindBuildingsCounter.Clear();
            }
            lock (_dictionaryLock)
            {
                if (_pathfindFails != null)
                {
                    _pathfindFails.Clear();
                }
                if (_outsideConnectionFails != null)
                {
                    _outsideConnectionFails.Clear();
                }
            }
        }

        public static void ResetPathingStatistics(ushort buildingId)
        {
            InstanceID instance = new InstanceID { Building = buildingId };
            lock (s_pathCounterLock)
            {
                _totalPathfindBuildingsCounter[instance] = 0;
            }

            lock (_dictionaryLock)
            {
                if (_pathfindFails != null)
                {
                    Dictionary<PATHFINDPAIR, long> newpathfindFails = new Dictionary<PATHFINDPAIR, long>();
                    foreach (KeyValuePair<PATHFINDPAIR, long> pair in _pathfindFails)
                    {
                        bool bBuildingInPair = pair.Key.m_source == instance || pair.Key.m_target == instance;
                        if (!bBuildingInPair)
                        {
                            newpathfindFails.Add(pair.Key, pair.Value);
                        }
                    }

                    // Replace array
                    _pathfindFails = newpathfindFails;
                }
                if (_outsideConnectionFails != null)
                {
                    Dictionary<PATHFINDPAIR, long> newpathfindFails = new Dictionary<PATHFINDPAIR, long>();
                    foreach (KeyValuePair<PATHFINDPAIR, long> pair in _outsideConnectionFails)
                    {
                        bool bBuildingInPair = pair.Key.m_source == instance || pair.Key.m_target == instance;
                        if (!bBuildingInPair)
                        {
                            newpathfindFails.Add(pair.Key, pair.Value);
                        }
                    }

                    // Replace array
                    _outsideConnectionFails = newpathfindFails;
                }
            }
        }

        /// <summary>
        /// Increase buidign fail count
        /// </summary>
        private static void UpdateBuildingFailCount(InstanceID instance)
        {
            lock (s_pathCounterLock)
            {
                int failcount;

                // Total count, doesn't reset
                if (_totalPathfindBuildingsCounter.TryGetValue(instance, out failcount))
                {
                    _totalPathfindBuildingsCounter[instance] = failcount + 1;
                }
                else
                {
                    _totalPathfindBuildingsCounter.Add(instance, 1);
                }
            }
        }

        /// <summary>
        /// Add or update failure pair
        /// </summary>
        private static void AddFailPair(InstanceID source, InstanceID target)
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
                _pathfindFails.Add(_pair, DateTime.Now.Ticks);
            }
                
            UpdateBuildingFailCount(source);
            UpdateBuildingFailCount(target);
        }


        /// <summary>
        /// Add or update outside connection fail
        /// </summary>
        private static void AddOutsideConnectionFail(InstanceID source, InstanceID target)
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
                _outsideConnectionFails.Add(_pair, DateTime.Now.Ticks);
            }

            UpdateBuildingFailCount(source);
            UpdateBuildingFailCount(target);
        }

        /// <summary>
        /// Cleanup old entries by last used
        /// </summary>
        public static void RemoveOldEntries()
        {
            int failpair_remove_count = 0;
            int failoutside_remove_count = 0;

            {
                long diffTimeInternal = DateTime.Now.Ticks - lru_lastCleanedInternal;
                if (diffTimeInternal > PATH_TIMEOUT_INTERNAL)
                {
                    lock (_dictionaryLock)
                    {
                        foreach (var item in _pathfindFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - PATH_TIMEOUT_INTERNAL)).ToList())
                        {
                            _pathfindFails.Remove(item.Key);
                            failpair_remove_count++;
                        }
                    }

                    lru_lastCleanedInternal = DateTime.Now.Ticks;
                }
            }

            {
                long diffTimeOutside = DateTime.Now.Ticks - lru_lastCleanedOutside;
                if (diffTimeOutside > PATH_TIMEOUT_OUTSIDE)
                {
                    lock (_dictionaryLock)
                    {
                        foreach (var item in _outsideConnectionFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - PATH_TIMEOUT_OUTSIDE)).ToList())
                        {
                            _outsideConnectionFails.Remove(item.Key);
                            failoutside_remove_count++;
                        }
                    }

                    lru_lastCleanedOutside = DateTime.Now.Ticks;
                }
            }  
        }


        /// <summary>
        /// Returns true when pair exists in exclusion list
        /// </summary>
        public static bool FindPathPair(InstanceID source, InstanceID target)
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
        public static bool FindOutsideConnectionPair(InstanceID source, InstanceID target)
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
        /// </summary>
        public static void RecordPathFindFailure(ushort vehicleID, ref Vehicle data)
        {
            InstanceID source = new InstanceID { Building = data.m_sourceBuilding };
            InstanceID target = VehicleTypeHelper.GetVehicleTarget(vehicleID, data);

            if (!source.IsEmpty && !target.IsEmpty && source != target)
            {
                bool bSourceOutside = false;
                if (source.Building != 0)
                {
                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[source.Building];
                    if (building.m_flags != 0)
                    {
                        bSourceOutside = building.Info?.m_buildingAI is OutsideConnectionAI;
                    }
                }
                bool btargetOutside = false;
                if (target.Building != 0)
                {
                    Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[target.Building];
                    if (building.m_flags != 0)
                    {
                        btargetOutside = building.Info?.m_buildingAI is OutsideConnectionAI;
                    }
                }
                if (bSourceOutside || btargetOutside)
                {
                    AddOutsideConnectionFail(source, target);
                }
                else
                {
                    AddFailPair(source, target);
                }
            }
        }
    }
}
