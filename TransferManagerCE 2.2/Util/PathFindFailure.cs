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
        private const long PATH_TIMEOUT_INTERNAL = TimeSpan.TicksPerMinute * 5; // Increased to 5 mintutes
        private const long PATH_TIMEOUT_OUTSIDE = TimeSpan.TicksPerMinute; // 1 minute

        // building-to-building
        private static Dictionary<PATHFINDPAIR, long>? s_pathfindFails = null;
        private static Dictionary<PATHFINDPAIR, long>? s_outsideConnectionFails = null;
        private static readonly object s_dictionaryLock = new object();

        // Transfer Issue building fail counters
        private static Dictionary<InstanceID, int>? s_totalPathfindBuildingsCounter = null;
        private static readonly object s_pathCounterLock = new object();

        public static void Init()
        {
            if (s_pathfindFails == null)
            {
                s_pathfindFails = new Dictionary<PATHFINDPAIR, long>(512);
            }
            if (s_outsideConnectionFails == null)
            {
                s_outsideConnectionFails = new Dictionary<PATHFINDPAIR, long>(512);
            }
            if (s_totalPathfindBuildingsCounter == null)
            {
                s_totalPathfindBuildingsCounter = new Dictionary<InstanceID, int>();
            }
        }

        public static void Delete()
        {
            if (s_pathCounterLock != null)
            {
                lock (s_pathCounterLock)
                {
                    s_pathfindFails = null;
                    s_outsideConnectionFails = null;
                    s_totalPathfindBuildingsCounter = null;
                }
            }
        }

        public static Dictionary<PATHFINDPAIR, long>? GetPathFailsCopy()
        {
            Dictionary<PATHFINDPAIR, long>? copy = null;
            lock (s_dictionaryLock)
            {
                copy = new Dictionary<PATHFINDPAIR, long>(s_pathfindFails);
            }
            return copy;
        }

        public static Dictionary<PATHFINDPAIR, long>? GetOutsideFailsCopy()
        {
            Dictionary<PATHFINDPAIR, long> copy = null;
            lock (s_dictionaryLock)
            {
                copy = new Dictionary<PATHFINDPAIR, long>(s_outsideConnectionFails);
            }
            return copy;
        }

        public static int GetPathFailureCount()
        {
            if (s_pathfindFails != null)
            {
                lock (s_dictionaryLock)
                {
                    return s_pathfindFails.Count;
                }
            }
            return 0;
        }

        public static int GetOutsidePathFailureCount()
        {
            if (s_outsideConnectionFails != null)
            {
                lock (s_dictionaryLock)
                {
                    return s_outsideConnectionFails.Count;
                }
            }
            return 0;
        }

        public static int GetTotalPathFailures(InstanceID instance)
        {
            if (s_totalPathfindBuildingsCounter != null)
            {
                lock (s_pathCounterLock)
                {
                    int iValue;
                    if (s_totalPathfindBuildingsCounter.TryGetValue(instance, out iValue))
                    {
                        return iValue;
                    }
                }
            }

            return 0;
        }

        public static void Reset()
        {
            if (s_totalPathfindBuildingsCounter != null)
            {
                lock (s_pathCounterLock)
                {
                    s_totalPathfindBuildingsCounter.Clear();
                }
            }
            if (s_pathfindFails != null && s_outsideConnectionFails != null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_pathfindFails != null)
                    {
                        s_pathfindFails.Clear();
                    }
                    if (s_outsideConnectionFails != null)
                    {
                        s_outsideConnectionFails.Clear();
                    }
                }
            }
            
        }

        public static void ResetPathingStatistics(ushort buildingId)
        {
            InstanceID instance = new InstanceID { Building = buildingId };

            if (s_totalPathfindBuildingsCounter != null)
            {
                lock (s_pathCounterLock)
                {
                    s_totalPathfindBuildingsCounter[instance] = 0;
                }
            }

            if (s_pathfindFails != null && s_outsideConnectionFails != null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_pathfindFails != null)
                    {
                        Dictionary<PATHFINDPAIR, long> newpathfindFails = new Dictionary<PATHFINDPAIR, long>();
                        foreach (KeyValuePair<PATHFINDPAIR, long> pair in s_pathfindFails)
                        {
                            bool bBuildingInPair = pair.Key.m_source == instance || pair.Key.m_target == instance;
                            if (!bBuildingInPair)
                            {
                                newpathfindFails.Add(pair.Key, pair.Value);
                            }
                        }

                        // Replace array
                        s_pathfindFails = newpathfindFails;
                    }
                    if (s_outsideConnectionFails != null)
                    {
                        Dictionary<PATHFINDPAIR, long> newpathfindFails = new Dictionary<PATHFINDPAIR, long>();
                        foreach (KeyValuePair<PATHFINDPAIR, long> pair in s_outsideConnectionFails)
                        {
                            bool bBuildingInPair = pair.Key.m_source == instance || pair.Key.m_target == instance;
                            if (!bBuildingInPair)
                            {
                                newpathfindFails.Add(pair.Key, pair.Value);
                            }
                        }

                        // Replace array
                        s_outsideConnectionFails = newpathfindFails;
                    }
                }
            }
        }

        /// <summary>
        /// Increase buidign fail count
        /// </summary>
        private static void UpdateBuildingFailCount(InstanceID instance)
        {
            if (s_totalPathfindBuildingsCounter != null)
            {
                lock (s_pathCounterLock)
                {
                    int failcount;

                    // Total count, doesn't reset
                    if (s_totalPathfindBuildingsCounter.TryGetValue(instance, out failcount))
                    {
                        s_totalPathfindBuildingsCounter[instance] = failcount + 1;
                    }
                    else
                    {
                        s_totalPathfindBuildingsCounter.Add(instance, 1);
                    }
                }
            }
            
        }

        public static void AddFailPair(InstanceID source, InstanceID target, bool bOutside)
        {
            if (bOutside)
            {
                AddOutsideConnectionFail(source, target);
            }
            else
            {
                AddLocalFailPair(source, target);
            }
        }

        /// <summary>
        /// Add or update failure pair
        /// </summary>
        private static void AddLocalFailPair(InstanceID source, InstanceID target)
        {
            long _info;
            PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);
            
            if (s_pathfindFails != null)
            {
                lock (s_dictionaryLock)
                {
                    if (s_pathfindFails.TryGetValue(_pair, out _info))
                    {
                        _info = DateTime.Now.Ticks;
                        s_pathfindFails[_pair] = _info;
                    }
                    else
                    {
                        s_pathfindFails.Add(_pair, DateTime.Now.Ticks);
                    }
                }
            }
                
            UpdateBuildingFailCount(source);
            UpdateBuildingFailCount(target);
        }


        /// <summary>
        /// Add or update outside connection fail
        /// </summary>
        private static void AddOutsideConnectionFail(InstanceID source, InstanceID target)
        {
            if (s_outsideConnectionFails != null)
            {
                long _info;
                PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);

                lock (s_dictionaryLock)
                {
                    if (s_outsideConnectionFails.TryGetValue(_pair, out _info))
                    {
                        _info = DateTime.Now.Ticks;
                        s_outsideConnectionFails[_pair] = _info;
                    }
                    else
                    {
                        s_outsideConnectionFails.Add(_pair, DateTime.Now.Ticks);
                    }
                }
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

            if (s_pathfindFails != null)
            {
                lock (s_dictionaryLock)
                {
                    foreach (var item in s_pathfindFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - PATH_TIMEOUT_INTERNAL)).ToList())
                    {
                        s_pathfindFails.Remove(item.Key);
                        failpair_remove_count++;
                    }
                }
            }

            if (s_outsideConnectionFails != null)
            {
                lock (s_dictionaryLock)
                {
                    foreach (var item in s_outsideConnectionFails.Where(kvp => kvp.Value < (DateTime.Now.Ticks - PATH_TIMEOUT_OUTSIDE)).ToList())
                    {
                        s_outsideConnectionFails.Remove(item.Key);
                        failoutside_remove_count++;
                    }
                }
            }  
        }


        /// <summary>
        /// Returns true when pair exists in exclusion list
        /// </summary>
        public static bool FindPathPair(InstanceID source, InstanceID target)
        {
            if (s_pathfindFails != null)
            {
                long _info;
                PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);

                lock (s_dictionaryLock)
                {
                    if (s_pathfindFails.TryGetValue(_pair, out _info))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true when buildingID in outsideconnection fail list
        /// </summary>
        public static bool FindOutsideConnectionPair(InstanceID source, InstanceID target)
        {
            if (s_outsideConnectionFails != null)
            {
                long _info;
                PATHFINDPAIR _pair = new PATHFINDPAIR(source, target);

                lock (s_dictionaryLock)
                {
                    if (s_outsideConnectionFails.TryGetValue(_pair, out _info))
                    {
                        return true;
                    }
                }
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
                AddFailPair(source, target, bSourceOutside || btargetOutside);
            }
        }
    }
}
