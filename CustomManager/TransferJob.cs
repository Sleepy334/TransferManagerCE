using System.Runtime.InteropServices;

namespace TransferManagerCE.CustomManager
{
    /// <summary>
    /// TransferJob: individual work package for match maker thread
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class TransferJob
    {
        public TransferManager.TransferReason material;
        public ushort m_outgoingCount;
        public ushort m_incomingCount;
        public int m_outgoingCountRemaining;
        public int m_incomingCountRemaining;
        public int m_outgoingAmount;
        public int m_incomingAmount;
        public TransferManager.TransferOffer[] m_outgoingOffers; //Size: TransferManager.TRANSFER_OFFER_COUNT * TRANSFER_PRIORITY_COUNT
        public TransferManager.TransferOffer[] m_incomingOffers; //Size: TransferManager.TRANSFER_OFFER_COUNT * TRANSFER_PRIORITY_COUNT

        public TransferJob()
        {
            m_outgoingOffers = new TransferManager.TransferOffer[TransferManager.TRANSFER_OFFER_COUNT * TransferManager.TRANSFER_PRIORITY_COUNT];
            m_incomingOffers = new TransferManager.TransferOffer[TransferManager.TRANSFER_OFFER_COUNT * TransferManager.TRANSFER_PRIORITY_COUNT];
        }
    }
}
