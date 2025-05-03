using System;

namespace TransferManagerCE.Common
{
    public abstract class PriorityQueueNode
    {
        /// <summary>
        /// The Key under which this node will be stored
        /// </summary>
        public abstract ushort Key { get; }

        /// <summary>
        /// The priority for this node which determines position in min-queue
        /// </summary>
        public abstract float Priority { get; }
    }
}
