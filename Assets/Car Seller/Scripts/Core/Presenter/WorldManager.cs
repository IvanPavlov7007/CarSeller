using UnityEngine;

public class WorldManager
{

    // On bootstrap, create the initial cities and warehouses
    public void AddWarehouse(City city, WarehouseConfig warehouseConfig)
    {
        Warehouse warehouse = new Warehouse(warehouseConfig);

        City.CityPosition pos;
        if (!string.IsNullOrEmpty(warehouseConfig.CityMarkerId) &&
            city.TryGetMarker(warehouseConfig.CityMarkerId, out var marker))
        {
            // Prefer exact graph anchor if available; otherwise snap world point to nearest graph
            pos = marker.PositionOnGraph.HasValue
                ? marker.PositionOnGraph.Value
                : city.GetClosestPosition(marker.WorldPosition);
        }
        else
        {
            pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
        }

        city.GetEmptyLocation(pos).Attach(warehouse);
        initializeWarehouse(warehouse, warehouseConfig);
    }

    private void initializeWarehouse(Warehouse warehouse, WarehouseConfig warehouseConfig)
    {
        foreach (var productToSpawn in warehouseConfig.initialProductsToSpawn)
        {
            ILocation insideWarehouseLocation = warehouse.GetEmptyLocation();
            switch (productToSpawn.productBaseConfig)
            {
                case CarBaseConfig carBaseConfig:
                    CarVariantConfig carVariantConfig = productToSpawn.productVariantConfig as CarVariantConfig;
                    G.Instance.ProductManager.CreateCar(carBaseConfig, carVariantConfig, insideWarehouseLocation);
                    break;
                case WheelBaseConfig wheelBaseConfig:
                    WheelVariantConfig wheelVariantConfig = productToSpawn.productVariantConfig as WheelVariantConfig;
                    G.Instance.ProductManager.CreateWheel(wheelBaseConfig, wheelVariantConfig, insideWarehouseLocation);
                    break;
                case EngineBaseConfig engineBaseConfig:
                    EngineVariantConfig engineVariantConfig = productToSpawn.productVariantConfig as EngineVariantConfig;
                    G.Instance.ProductManager.CreateEngine(engineBaseConfig, engineVariantConfig, insideWarehouseLocation);
                    break;
                case SpoilerBaseConfig spoilerBaseConfig:
                    SpoilerVariantConfig spoilerVariantConfig = productToSpawn.productVariantConfig as SpoilerVariantConfig;
                    G.Instance.ProductManager.CreateSpoiler(spoilerBaseConfig, spoilerVariantConfig, insideWarehouseLocation);
                    break;
            }
        }
    }

    public void InitializeCity(CityConfig cityConfig)
    {
        World.Reset();

        if (G.Instance.CityRoot == null)
        {
            G.Instance.CityRoot = GameObject.Instantiate(cityConfig.CityGraph.PrefabRoot);
            G.Instance.CityRoot.SetActive(false);
        }
        World.Instance.City = new City(cityConfig, G.Instance.CityRoot.transform);
        foreach (var warehouseConfig in cityConfig.warehouseConfigs)
        {
            AddWarehouse(World.Instance.City, warehouseConfig);
        }
        foreach (var productToSpawn in cityConfig.initialProductsToSpawn)
        {
            City.CityPosition randomOutsidePosition = World.Instance.City.GetRandomPosition();
            ILocation outsideLocation = World.Instance.City.GetEmptyLocation(randomOutsidePosition);
            switch (productToSpawn.productBaseConfig)
            {
                case CarBaseConfig carBaseConfig:
                    CarVariantConfig carVariantConfig = productToSpawn.productVariantConfig as CarVariantConfig;
                    G.Instance.ProductManager.CreateCar(carBaseConfig, carVariantConfig, outsideLocation);
                    break;
                default:
                    Debug.LogError($"Only cars can be spawned outside the warehouse. Tried to spawn {productToSpawn.productBaseConfig}.");
                    break;
            }
        }
    }
}