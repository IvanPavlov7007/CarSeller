using System;
using UnityEngine;

public class WorldManager
{
    //TODO register more things in the world registry
    World World => World.Instance;
    

    // On bootstrap, create the initial cities and warehouses
    private void addWarehouse(City city, WarehouseConfig warehouseConfig)
    {
        Warehouse warehouse = new Warehouse(warehouseConfig);

        City.CityPosition pos;

        if (warehouseConfig.Marker.IsValid)
        {
            // Read the baked marker from the selected graph
            var graph = warehouseConfig.Marker.Graph;
            var markerData = graph.Markers.Find(m => m.Id == warehouseConfig.Marker.MarkerId);

            if (markerData != null)
            {
                // WorldPoint markers remain unbound. Snap here if desired.
                if (markerData.Anchor.Kind == CityGraphAsset.MarkerAnchorKind.WorldPoint)
                {
                    pos = city.GetClosestPosition(markerData.Anchor.WorldPoint);
                }
                else if (markerData.Anchor.Kind == CityGraphAsset.MarkerAnchorKind.Node)
                {
                    // If you prefer to ignore node-edge binding and still snap, you can still do closest:
                    pos = city.GetClosestPosition(markerData.Anchor.WorldPoint);
                }
                else if (markerData.Anchor.Kind == CityGraphAsset.MarkerAnchorKind.Edge)
                {
                    // Edge anchors can be interpreted, but you can also snap anyway:
                    pos = city.GetClosestPosition(markerData.Anchor.WorldPoint);
                }
                else
                {
                    pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
                }
            }
            else
            {
                // Marker id not found; fallback
                pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
            }
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
        World.WorldRegistry.Register<Warehouse>(warehouse,warehouseConfig);
    }

    public void InitializeWorld(CityConfig cityConfig, EconomyConfig economyConfig)
    {
        initializeCity(cityConfig);
        // Initialize economy after city so that economy can reference city objects if needed
        initializeEconomy(economyConfig);
        CarSpawnManager.NewCarsRotation();
    }

    //1.
    private void initializeCity(CityConfig cityConfig)
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
            addWarehouse(World.Instance.City, warehouseConfig);
        }
        //spawnProductsInCity(World.Instance.City, cityConfig);
    }

    [Obsolete("Spawning products in city is deprecated")]
    private void spawnProductsInCity(City city, CityConfig cityConfig)
    {
        foreach (var productToSpawn in cityConfig.initialProductsToSpawn)
        {
            City.CityPosition randomOutsidePosition = city.GetRandomPosition();
            ILocation outsideLocation = city.GetEmptyLocation(randomOutsidePosition);
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

    //2.
    private void initializeEconomy(EconomyConfig economyConfig)
    {
        World.Instance.Economy = new Economy(economyConfig);
    }
}