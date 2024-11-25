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
        public ushort sourceBuilding;
        public ushort targetBuilding;

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
        static Dictionary<PATHFINDPAIR, long> _pathfindFails = new Dictionary<PATHFINDPAIR, long>(512);

        // building-to-outsideconnection
        static Dictionary<PATHFINDPAIR, long> _outsideConnectionFails = new Dictionary<PATHFINDPAIR, long>(512);
        static readonly object _dictionaryLock = new object();

        // Transfer Issue building fail counters
        static Dictionary<ushort, int> _totalPathfindBuildingsCounter = new Dictionary<ushort, int>();
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

        public static int GetTotalPathFailures()
        {
            return _pathfindFails.Count + _outsideConnectionFails.Count;
        }

        public static int GetTotalPathFailures(ushort usBuilding)
        {
            lock (s_pathCounterLock)
            {
                int iValue;
                if (_totalPathfindBuildingsCounter.TryGetValue(usBuilding, out iValue))
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

        /// <summary>
        /// Increase buidign fail count
        /// </summary>
        private static void UpdateBuildingFailCount(ushort buildingID)
        {
            lock (s_pathCounterLock)
            {
                int failcount;

                // Total count, doesn't reset
                if (_totalPathfindBuildingsCounter.TryGetValue(buildingID, out failcount))
                {
                    failcount++;
                    _totalPathfindBuildingsCounter[buildingID] = failcount;
                }
                else
                {
                    _totalPathfindBuildingsCounter.Add(buildingID, 1);
                }
            }
        }

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
                _pathfindFails.Add(_pair, DateTime.Now.Ticks);
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
            if (data.m_sourceBuilding != 0 && data.m_targetBuilding != 0)
            {
                InstanceID target = VehicleTypeHelper.GetVehicleTarget(data);
                if (target.Building != 0 && target.Building < BuildingManager.instance.m_buildings.m_size)
                {
                    Building sourceBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_sourceBuilding];
                    Building targetBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[target.Building];

                    bool bSourceOutside = sourceBuilding.Info?.m_buildingAI is OutsideConnectionAI;
                    bool btargetOutside = targetBuilding.Info?.m_buildingAI is OutsideConnectionAI;
                    if (bSourceOutside || btargetOutside)
                    {
                        AddOutsideConnectionFail(data.m_sourceBuilding, target.Building);
                    }
                    else
                    {
                        AddFailPair(data.m_sourceBuilding, target.Building);
                    }
                }
            }
        }
    }
}
