using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class RoadAccessData : IComparable
    {
        private static Dictionary<InstanceID, int> s_roadAccessIssues = new Dictionary<InstanceID, int>();
        
        public static int Count
        {
            get { return s_roadAccessIssues.Count; }
        }

        public static void Reset()
        {
            s_roadAccessIssues.Clear();
        }

        public static void AddInstance(InstanceID instance)
        {
            if (s_roadAccessIssues.ContainsKey(instance))
            {
                s_roadAccessIssues[instance]++;
            }
            else
            {
                s_roadAccessIssues.Add(instance, 1);
            }
        }

        public static List<RoadAccessData> GetRoadAccessIssues()
        {
            Building[] BuildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
            Vehicle[] VehicleBuffer = VehicleManager.instance.m_vehicles.m_buffer;

            List<RoadAccessData> result = new List<RoadAccessData>();
            List<InstanceID> resolved = new List<InstanceID>();

            foreach (KeyValuePair<InstanceID, int> kvp in s_roadAccessIssues)
            {
                switch (kvp.Key.Type)
                {
                    case InstanceType.Building:
                        {
                            Building building = BuildingBuffer[kvp.Key.Building];
                            if (building.m_flags != 0 && building.m_accessSegment == 0)
                            {
                                result.Add(new RoadAccessData(kvp.Key, kvp.Value));
                            }
                            else
                            {
                                // Its been resolved
                                resolved.Add(kvp.Key);
                            }
                            break;
                        }
                    case InstanceType.Vehicle:
                        {
                            Vehicle vehicle = VehicleBuffer[kvp.Key.Vehicle];
                            if (vehicle.m_flags != 0)
                            {
                                result.Add(new RoadAccessData(kvp.Key, kvp.Value));
                            }
                            else
                            {
                                // Its been resolved
                                resolved.Add(kvp.Key);
                            }
                            break;
                        }
                    case InstanceType.Citizen:
                        {
                            // Ignore citizens for now
                            break;
                        }
                    default:
                        {
                            result.Add(new RoadAccessData(kvp.Key, kvp.Value));
                            break;
                        }
                }
            }

            // Remove resolved
            foreach (InstanceID instance in resolved)
            {
                if (s_roadAccessIssues.ContainsKey(instance))
                {
                    s_roadAccessIssues.Remove(instance);
                }
            }

            return result;
        }

        public InstanceID m_source;
        public int m_iCount;

        public RoadAccessData(InstanceID source, int iCount)
        {
            m_source = source;
            m_iCount = iCount;
        }

        public RoadAccessData(RoadAccessData oSecond)
        {
            m_source = oSecond.m_source;
            m_iCount = oSecond.m_iCount;
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }

            RoadAccessData oSecond = (RoadAccessData)second;
            return oSecond.m_iCount.CompareTo(m_iCount);
        }
    }
}