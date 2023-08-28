using ColossalFramework;
using System;
using UnityEngine;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public abstract class StatusNodeStop : StatusData
    {
        private ushort m_nodeId;

        public StatusNodeStop(BuildingType eBuildingType, ushort m_buildingId, ushort nodeId) :
            base(TransferReason.None, eBuildingType, nodeId, 0, 0)
        {
            m_nodeId = nodeId;
        }

        protected abstract TransportInfo.TransportType GetTransportType();

        protected override string CalculateValue()
        {
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

        public override string GetResponder()
        {
            return $"Node:{m_nodeId}";
        }

        public override string GetTarget()
        {
            return "";
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
            if (stop == 0)
            {
                return 0;
            }

            ushort nextStop = TransportLine.GetNextStop(stop);
            if (nextStop == 0)
            {
                return 0;
            }

            float num = (transportType != 0 && transportType != TransportInfo.TransportType.EvacuationBus && transportType != TransportInfo.TransportType.TouristBus) ? 64f : 32f;
            CitizenManager instance = Singleton<CitizenManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            Vector3 position = instance2.m_nodes.m_buffer[stop].m_position;
            Vector3 position2 = instance2.m_nodes.m_buffer[nextStop].m_position;
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
                        ushort nextGridInstance = instance.m_instances.m_buffer[num7].m_nextGridInstance;
                        if ((instance.m_instances.m_buffer[num7].m_flags & CitizenInstance.Flags.WaitingTransport) != 0)
                        {
                            Vector3 a = instance.m_instances.m_buffer[num7].m_targetPos;
                            float num9 = Vector3.SqrMagnitude(a - position);
                            if (num9 < num * num)
                            {
                                CitizenInfo info2 = instance.m_instances.m_buffer[num7].Info;
                                if (info2.m_citizenAI.TransportArriveAtSource(num7, ref instance.m_instances.m_buffer[num7], position, position2))
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

            return num6;
        }
    }
}