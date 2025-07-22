using ColossalFramework;

namespace TransferManagerCE
{
    public class WarehouseUtils
    {
        public enum WarehouseMode
        {
            None,
            Empty,
            Balanced,
            Fill,
        }

        private enum VanillaWarehouseMode
        {
            Balanced,
            Import,
            Export
        }

        public static ushort GetWarehouseBuildingId(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            if (building.Info.GetAI() is WarehouseStationAI)
            {
                return building.m_parentBuilding;
            }
            else
            {
                return buildingId;
            }
        }

        public static WarehouseMode GetWarehouseMode(ushort buildingId)
        {
            Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
            return GetWarehouseMode(building);
        }

        public static WarehouseMode GetWarehouseMode(Building building)
        {
            WarehouseMode mode = WarehouseMode.None;

            if (building.m_flags != 0)
            {
                if ((building.m_flags & Building.Flags.Filling) == Building.Flags.Filling)
                {
                    mode = WarehouseMode.Fill;
                }
                else if ((building.m_flags & Building.Flags.Downgrading) == Building.Flags.Downgrading)
                {
                    mode = WarehouseMode.Empty;
                }
                else
                {
                    mode = WarehouseMode.Balanced;
                }
            }

            return mode;
        }

        public static string GetLocalisedWarehouseMode(WarehouseMode mode)
        {
            switch (mode)
            {
                case WarehouseMode.Balanced:
                    {
                        return ColossalFramework.Globalization.Locale.Get("WAREHOUSEPANEL_MODE", VanillaWarehouseMode.Balanced.ToString());
                    }
                case WarehouseMode.Empty:
                    {
                        return ColossalFramework.Globalization.Locale.Get("WAREHOUSEPANEL_MODE", VanillaWarehouseMode.Export.ToString());
                    }
                case WarehouseMode.Fill:
                    {
                        return ColossalFramework.Globalization.Locale.Get("WAREHOUSEPANEL_MODE", VanillaWarehouseMode.Import.ToString());
                    }
            }
            return string.Empty;
        }

        public static int GetWarehouseTruckCount(ushort buildingId)
        {
            if (buildingId != 0)
            {
                Building building = BuildingManager.instance.m_buildings.m_buffer[buildingId];
                if (building.Info is not null)
                {
                    WarehouseAI? warehouse = building.Info.m_buildingAI as WarehouseAI;
                    if (warehouse is not null)
                    {
                        // Factor in budget
                        int budget = Singleton<EconomyManager>.instance.GetBudget(building.Info.m_class);
                        int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
                        return (productionRate * warehouse.m_truckCount + 99) / 100;
                    }
                    else if (building.Info?.m_buildingAI.GetType().ToString() == "CargoFerries.AI.CargoFerryWarehouseHarborAI")
                    {
                        return 25; // Just return default number for now
                    }
                }
            }

            return 0;
        }
    }
}
