using UnityEngine;

public class WorldManager
{

    // On bootstrap, create the initial cities and warehouses
    public void AddWarehouse(City city, WarehouseConfig warehouseConfig)
    {
        Warehouse warehouse = new Warehouse(warehouseConfig);
        var pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
        city.PlaceObjectAtPosition(warehouse, pos);
        initializeWarehouse(warehouse, warehouseConfig);
    }

    private void initializeWarehouse(Warehouse warehouse, WarehouseConfig warehouseConfig)
    {
        foreach (var productToSpawn in warehouseConfig.initialProductsToSpawn)
        {
            IProductLocation insideWarehouseLocation = warehouse.GetEmptyLocation();
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
        World.Instance.City = new City(cityConfig);
        foreach (var warehouseConfig in cityConfig.warehouseConfigs)
        {
            AddWarehouse(World.Instance.City, warehouseConfig);
        }
        foreach (var productToSpawn in cityConfig.initialProductsToSpawn)
        {
            City.CityPosition randomOutsidePosition = World.Instance.City.GetRandomPosition();
            IProductLocation outsideLocation = World.Instance.City.GetEmptyProductLocation(randomOutsidePosition);
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