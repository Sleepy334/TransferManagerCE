using ColossalFramework.UI;
using SleepyCommon;
using System;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public enum InOut
    {
        Unknown,
        In,
        Out,
    }

    public class MatchData : IComparable
    {
        public DateTime m_TimeStamp; 
        public TransferReason m_material = TransferReason.None;
        public MatchOffer m_incoming;
        public MatchOffer m_outgoing;
        private int m_buildingId = 0;
        private InOut m_eInOrOut = InOut.Unknown;

        public MatchData(TransferReason material, TransferOffer outgoing, TransferOffer incoming, int iDeltaAmount)
        {
            m_buildingId = 0;
            m_eInOrOut = InOut.Unknown;
            m_material = material;
            m_incoming = new MatchOffer(incoming);
            m_outgoing = new MatchOffer(outgoing);
            m_TimeStamp = DateTime.Now;
         }

        public MatchData(ushort buildingId, bool bIncoming, MatchData second)
        {
            m_buildingId = buildingId;
            m_eInOrOut = bIncoming ? InOut.In : InOut.Out;
            m_material = second.m_material;
            m_incoming = new MatchOffer(second.m_incoming);
            m_outgoing = new MatchOffer(second.m_outgoing);
            m_TimeStamp = second.m_TimeStamp;
        }

        public static CustomTransferOffer GetCopy(CustomTransferOffer offer)
        {
            return new CustomTransferOffer(offer);
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }
            MatchData oSecond = (MatchData)second;
            return oSecond.m_TimeStamp.CompareTo(m_TimeStamp);
        }

        public string GetActiveStatus()
        {
            string sTest = "";
            sTest += m_incoming.Active ? "A" : "P";
            sTest += "/";
            sTest += m_outgoing.Active ? "A" : "P";
            return sTest;
        }

        public string GetInOutStatus()
        {
            if (m_eInOrOut != InOut.Unknown)
            {
                return (m_eInOrOut == InOut.In) ? "IN" : "OUT";
            }
            return "";
        }

        public string GetAmount()
        {
            return m_incoming.Amount + "/" + m_outgoing.Amount;
        }

        public string GetPriority()
        {
            return m_incoming.Priority + "/" + m_outgoing.Priority;
        }

        public string GetPark()
        {
            if (m_incoming.m_byLocalPark != m_outgoing.m_byLocalPark)
            {
                return $"{m_incoming.m_byLocalPark}/{m_outgoing.m_byLocalPark}";
            }
            return m_incoming.m_byLocalPark.ToString();
        }

        public string DisplayMatch()
        {
            switch (m_eInOrOut)
            {
                case InOut.In:
                    {
                        return m_outgoing.DisplayOffer();
                    }
                case InOut.Out:
                    {
                        return m_incoming.DisplayOffer();
                    }
            }

            return "OUT: " + m_outgoing.m_object.Type.ToString() + " " + m_outgoing.m_object.ToString() + " IN: " + m_outgoing.m_object.Type.ToString() + m_incoming.m_object.ToString();
        }

        public void Show()
        {
            switch (m_eInOrOut)
            {
                case InOut.In:
                    {
                        m_outgoing.Show();
                        break;
                    }
                case InOut.Out:
                    {
                        m_incoming.Show();
                        break;
                    }
            }
        }

        public double GetDistance()
        {
            return Math.Sqrt(Vector3.SqrMagnitude(m_incoming.Position - m_outgoing.Position)) * 0.001;
        }
    }
}