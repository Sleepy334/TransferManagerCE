using System;
using System.Collections.Generic;
using System.IO;
using TransferManagerCE.CustomManager;
using UnityEngine;
using static TransferManager;

namespace TransferManagerCE
{
    public class MatchData : IComparable
    {
        private static int m_iMatchCounter = 1;

        private const ushort usTUPLE_ALIGNMENT = 0xFEFE;
        private int m_iMatchNumber = m_iMatchCounter++; // Allows us to sort correctly over midnight.
        public byte m_hour;
        public byte m_minute;
        public byte m_second;
        public CustomTransferReason m_material;
        public MatchOffer m_incoming;
        public MatchOffer m_outgoing;

        public MatchData()
        {
            m_material = TransferReason.None;
        }

        public MatchData(TransferReason material, TransferOffer outgoing, TransferOffer incoming)
        {
            m_hour = (byte)DateTime.Now.Hour;
            m_minute = (byte)DateTime.Now.Minute;
            m_second = (byte)DateTime.Now.Second;
            m_material = material;
            m_incoming = new MatchOffer(incoming);
            m_outgoing = new MatchOffer(outgoing);
        }

        public MatchData(MatchData second)
        {
            m_iMatchNumber = second.m_iMatchNumber;
            m_hour = second.m_hour;
            m_minute = second.m_minute;
            m_second = second.m_second;
            m_material = second.m_material;
            m_incoming = new MatchOffer(second.m_incoming);
            m_outgoing = new MatchOffer(second.m_outgoing);
        }

        public int CompareTo(object second)
        {
            if (second == null)
            {
                return 1;
            }

            MatchData oSecond = (MatchData)second;
            return oSecond.m_iMatchNumber - m_iMatchNumber;
        }

        public string Time()
        {
            return $"{m_hour.ToString("D2")}:{m_minute.ToString("D2")}:{m_second.ToString("D2")}";
        }

        public string GetActiveStatus()
        {
            string sTest = "";
            sTest += m_incoming.Active ? "A" : "P";
            sTest += "/";
            sTest += m_outgoing.Active ? "A" : "P";
            return sTest;
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

        public double GetDistance()
        {
            return Math.Sqrt(Vector3.SqrMagnitude(m_incoming.Position - m_outgoing.Position)) * 0.001;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(usTUPLE_ALIGNMENT);
            writer.Write(m_iMatchNumber);
            writer.Write(m_hour);
            writer.Write(m_minute);
            writer.Write(m_second);
            writer.Write((byte)(TransferReason)m_material);
            m_incoming.Write(writer);
            m_outgoing.Write(writer);
        }

        public void Read(BinaryReader reader)
        {
            ushort usTupleAlignment = reader.ReadUInt16();
            if (usTupleAlignment == usTUPLE_ALIGNMENT)
            {
                m_iMatchNumber = reader.ReadInt32();
                m_hour = reader.ReadByte();
                m_minute = reader.ReadByte();
                m_second = reader.ReadByte();
                m_material = new CustomTransferReason((TransferReason)reader.ReadByte());
                m_incoming = new MatchOffer();
                m_incoming.Read(reader);
                m_outgoing = new MatchOffer();
                m_outgoing.Read(reader);
            }
            else
            {
                Debug.LogError("Tuple Alignment not found.");
            }
        }
    }
}