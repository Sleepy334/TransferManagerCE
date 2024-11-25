using System;
using TransferManagerCE.Util;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class StatusDataSeparator : StatusData
    {
        public StatusDataSeparator() :
            base(TransferReason.None, BuildingType.None, 0, 0, 0)
        {
        }

        public override bool IsSeparator()
        {
            return true;
        }

        public override string GetMaterialDescription()
        {
            return "";
        }

        public override string GetValue()
        {
            return "";
        }

        public override string GetValueTooltip()
        {
            return "";
        }

        public override string GetTimer()
        {
            return "";
        }

        public override void Update() { }

        public override ushort GetResponderId()
        {
            return 0;
        }

        public override string GetResponder()
        {
            return "";
        }

        public override void OnClickResponder()
        {
        }

        public override ushort GetTargetId()
        {
            return 0;
        }

        public override string GetTarget()
        {
            return "";
        }

        public override string GetLoad()
        {
            return "";
        }
        

        public override void OnClickTarget()
        {
        }
    }
}
