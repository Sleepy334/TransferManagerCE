using System;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class BuildingMatchData : MatchData
    {
        public enum InOut
        {
            Unknown,
            In,
            Out,
        }

        private string? m_description = null;
        private double? m_distance = null;

        // Stores whether this building is on the IN or OUT side of the match data.
        private InOut m_eInOrOut = InOut.Unknown;

        public BuildingMatchData(bool bIncoming, MatchData data) :
            base(data)
        {
            m_eInOrOut = bIncoming ? InOut.In : InOut.Out;
        }

        public string GetInOutStatus()
        {
            if (m_eInOrOut != InOut.Unknown)
            {
                return (m_eInOrOut == InOut.In) ? "IN" : "OUT";
            }
            return "";
        }

        public string DisplayMatch()
        {
            if (m_description == null)
            {
                switch (m_eInOrOut)
                {
                    case InOut.In:
                        {
                            m_description = m_outgoing.DisplayOffer();
                            break;
                        }
                    case InOut.Out:
                        {
                            m_description = m_incoming.DisplayOffer();
                            break;
                        }
                    default:
                        {
                            m_description = "OUT: " + m_outgoing.m_object.Type.ToString() + " " + m_outgoing.m_object.ToString() + " IN: " + m_outgoing.m_object.Type.ToString() + m_incoming.m_object.ToString();
                            break;
                        }
                }
            }
            
            return m_description;
        }

        public override double GetDistance()
        {
            if (m_distance == null)
            {
                m_distance = base.GetDistance();
            }
            return m_distance.Value;
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

        public bool Contains(string search)
        {
            if (m_material.ToString().ToUpper().Contains(search))
            {
                return true;
            }
            if (DisplayMatch().ToUpper().Contains(search))
            {
                return true;
            }
            return false;
        }
    }
}