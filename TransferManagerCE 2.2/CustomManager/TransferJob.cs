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
        public int m_outgoingCount; // Total number of OUT offers
        public int m_incomingCount; // Total number of IN offers
        public int m_outgoingCountRemaining; // Total number of OUT offers that still have some Amount remaining
        public int m_incomingCountRemaining; // Total number of IN offers that still have some Amount remaining
        public int m_outgoingAmount;
        public int m_incomingAmount;
        public CustomTransferOffer[] m_outgoingOffers; //Size: TransferManager.TRANSFER_OFFER_COUNT * TRANSFER_PRIORITY_COUNT
        public CustomTransferOffer[] m_incomingOffers; //Size: TransferManager.TRANSFER_OFFER_COUNT * TRANSFER_PRIORITY_COUNT

        public TransferJob()
        {
            m_outgoingOffers = new CustomTransferOffer[TransferManager.TRANSFER_OFFER_COUNT * TransferManager.TRANSFER_PRIORITY_COUNT];
            m_incomingOffers = new CustomTransferOffer[TransferManager.TRANSFER_OFFER_COUNT * TransferManager.TRANSFER_PRIORITY_COUNT];
        }
    }
}
