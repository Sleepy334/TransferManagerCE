using System;
using TransferManagerCore.Common;
using TransferManagerCore.Util;
using UnityEngine;
using static TransferManager;
using static TransferManagerCore.BuildingTypeHelper;

namespace TransferManagerCore.Data
{
    public class VehicleDataSeparator : VehicleData
    {
        public VehicleDataSeparator() :
            base(Vector3.zero, 0)
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

        public override string GetTimer()
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
