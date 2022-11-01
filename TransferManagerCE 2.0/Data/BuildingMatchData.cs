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
    }
}