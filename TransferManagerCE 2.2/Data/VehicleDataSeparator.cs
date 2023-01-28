using System;
using TransferManagerCE.Common;
using TransferManagerCE.Util;
using static TransferManager;
using static TransferManagerCE.BuildingTypeHelper;

namespace TransferManagerCE.Data
{
    public class VehicleDataSeparator : VehicleData
    {
        public VehicleDataSeparator() :
            base(0)
        {
        }

        public override string GetMaterialDescription()
        {
            return "";
        }

        public override string GetValue()
        {
            return "";
        }

        public virtual string GetTimer()
        {
            return "";
        }

        public override string GetTarget()
        {
            return "";
        }

        public override string GetVehicle()
        {
            return "";
        }
    }
}
