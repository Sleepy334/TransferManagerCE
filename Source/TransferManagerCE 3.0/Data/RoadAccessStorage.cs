using System;
using System.Collections.Generic;

namespace TransferManagerCE
{
    public class RoadAccessStorage
    {
        private static Dictionary<InstanceID, int> s_roadAccessIssues = new Dictionary<InstanceID, int>();

        public static int Count
        {
            get { return s_roadAccessIssues.Count; }
        }

        public static int GetInstanceCount(InstanceID instanceID)
        {
            if (s_roadAccessIssues.ContainsKey(instanceID))
            {
                return s_roadAccessIssues[instanceID];
            }
            return 0;
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
    }
}