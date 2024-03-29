using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusNodeStop : StatusData
    {
        protected ushort m_nodeId;
        private Color m_color = Color.white;

        public StatusNodeStop(BuildingType eBuildingType, ushort m_buildingId, ushort nodeId, ushort targetVehicleId) :
            base(TransferReason.None, eBuildingType, m_buildingId, 0, targetVehicleId)
        {
            m_nodeId = nodeId;
        }

        protected abstract TransportInfo.TransportType GetTransportType();

        protected override string CalculateValue()
        {
            UpdateTextColor();
            return CalculatePassengerCount(m_nodeId, GetTransportType()).ToString();
        }

        protected override string CalculateTimer()
        {
            NetNode node = NetManager.instance.m_nodes.m_buffer[m_nodeId];
            if (node.m_flags != 0)
            {
                return $"W:{node.m_maxWaitTime}";
            }

            return "";
        }

        protected override string CalculateResponder()
        {
            NetManager instance = Singleton<NetManager>.instance;

            string sText = "";

            ushort buildingId = FindConnectionBuilding(m_nodeId);
            if (buildingId != 0)
            {
                sText = InstanceHelper.DescribeInstance(new InstanceID {  Building =  buildingId });
            }
            else
            {
                sText = $"Node:{m_nodeId}";
            }

            return sText;
        }

        public override Color GetTextColor()
        {
            return m_color;
        }

        public override string GetValueTooltip()
        {
            return "Passengers";
        }

        public override string GetResponderTooltip()
        {
            string sText = "";

            // Description
            ushort buildingId = FindConnectionBuilding(m_nodeId);
            if (buildingId != 0)
            {
                sText = InstanceHelper.DescribeInstance(new InstanceID { Building = buildingId });
            }

            // Node
            if (sText.Length > 0)
            {
                sText += " | ";
            }
            sText += $"Node:{m_nodeId}";

            // Errors
            int iErrorCount = GetNodePathError(out string sError, out int iCount);
            if (iErrorCount > 0)
            {
                if (sText.Length > 0)
                {
                    sText += " | ";
                }
                sText += $"PathFailed ({iErrorCount}/{iCount} {sError})"; 
            }

            return sText;
        }

        public override void OnClickResponder()
        {
            if (m_nodeId != 0)
            {
                InstanceHelper.ShowInstance(new InstanceID { NetNode = m_nodeId });
            }
        }

        public static int CalculatePassengerCount(ushort stop, TransportInfo.TransportType transportType)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenInstance[] CitizenInstances = Singleton<CitizenManager>.instance.m_instances.m_buffer;
            NetNode[] Nodes = Singleton<NetManager>.instance.m_nodes.m_buffer;

            if (stop == 0)
            {
                return 0;
            }

            ushort nextStop = TransportLine.GetNextStop(stop);
            if (nextStop == 0)
            {
                return 0;
            }

            Vector3 position = Nodes[stop].m_position;
            Vector3 position2 = Nodes[nextStop].m_position;
            float num = (transportType != 0 && transportType != TransportInfo.TransportType.EvacuationBus && transportType != TransportInfo.TransportType.TouristBus) ? 64f : 32f;
            int num2 = Mathf.Max((int)((position.x - num) / 8f + 1080f), 0);
            int num3 = Mathf.Max((int)((position.z - num) / 8f + 1080f), 0);
            int num4 = Mathf.Min((int)((position.x + num) / 8f + 1080f), 2159);
            int num5 = Mathf.Min((int)((position.z + num) / 8f + 1080f), 2159);
            int num6 = 0;

            for (int i = num3; i <= num5; i++)
            {
                for (int j = num2; j <= num4; j++)
                {
                    ushort num7 = instance.m_citizenGrid[i * 2160 + j];
                    int num8 = 0;
                    while (num7 != 0)
                    {
                        ushort nextGridInstance = CitizenInstances[num7].m_nextGridInstance;
                        if ((CitizenInstances[num7].m_flags & CitizenInstance.Flags.WaitingTransport) != 0)
                        {
                            Vector3 a = CitizenInstances[num7].m_targetPos;
                            float num9 = Vector3.SqrMagnitude(a - position);
                            if (num9 < num * num)
                            {
                                CitizenInfo info2 = CitizenInstances[num7].Info;
                                if (info2.m_citizenAI.TransportArriveAtSource(num7, ref CitizenInstances[num7], position, position2))
                                {
                                    num6++;
                                }
                            }
                        }

                        num7 = nextGridInstance;
                        if (++num8 > 65536)
                        {
                            Debug.Log("Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            // We update the m_maxWaitTime here as the vanilla code doesnt do it and it will spawn vehicles constantly.
            if (num6 == 0)
            {
                Nodes[stop].m_maxWaitTime = 0;
            }

            return num6;
        }

        private ushort FindConnectionBuilding(ushort stop)
        {
            Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            FastList<ushort> outsideConnections = instance.GetOutsideConnections();
            ushort result = 0;
            float num = 40000f;
            for (int i = 0; i < outsideConnections.m_size; i++)
            {
                ushort num2 = outsideConnections.m_buffer[i];
                float num3 = VectorUtils.LengthSqrXZ(instance.m_buildings.m_buffer[num2].m_position - position);
                if (num3 < num)
                {
                    result = num2;
                    num = num3;
                }
            }

            return result;
        }

        private int GetNodePathError(out string sError, out int iSegmentCount)
        {
            NetManager instance = Singleton<NetManager>.instance;

            int iErrorCount = 0;
            iSegmentCount = 0;
            sError = "Segments:";

            NetNode node = NetManager.instance.m_nodes.m_buffer[m_nodeId];
            if (node.m_flags != 0)
            {

                for (int i = 0; i < 8; i++)
                {
                    ushort segmentId = instance.m_nodes.m_buffer[m_nodeId].GetSegment(i);
                    if (segmentId != 0)
                    {
                        iSegmentCount++;

                        NetSegment segment = instance.m_segments.m_buffer[segmentId];
                        if ((segment.m_flags & NetSegment.Flags.PathFailed) != 0)
                        {
                            sError += $" {segmentId}";
                            iErrorCount++;
                        }
                    }
                }
            }

            return iErrorCount;
        }

        private void UpdateTextColor()
        {
            // Update color
            int iErrorCount = GetNodePathError(out string sError, out int iTotalCount);
            if (iErrorCount > 0)
            {
                if (iErrorCount < iTotalCount)
                {
                    m_color = Color.magenta;
                }
                else
                {
                    m_color = Color.red;
                }
            }
            else
            {
                m_color = Color.white;
            }
        }
    }
}