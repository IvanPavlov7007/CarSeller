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

        if (warehouseConfig.Marker != null && warehouseConfig.Marker.IsValid)
        {
            var markerData = warehouseConfig.Marker.GetMarkerData(warehouseConfig);

            if (markerData != null)
            {
                var anchor = markerData.Anchor;
                //TODO differtiate based on anchor kind -> use node pos 
                // if on node, edge pos if on edge, world point if world point
                pos = city.GetClosestPosition(anchor.WorldPoint);
            }
            else
            {
                pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
            }
        }
        else
        {
            Debug.LogWarning($"WarehouseConfig '{warehouseConfig.name}' does not have a valid marker assigned. Falling back to closest position.");
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

    public void InitializeWorld(CityConfig cityConfig, EconomyConfig economyConfig, WorldMissionsConfig worldMissionsConfig)
    {
        initializeCity(cityConfig);
        // Initialize economy after city so that economy can reference city objects if needed
        initializeEconomy(economyConfig);
        initializeWorldMissions(worldMissionsConfig);
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

    private void initializeWorldMissions(WorldMissionsConfig worldMissionsConfig)
    {
        if(worldMissionsConfig == null)
        {
            Debug.LogWarning("WorldMissionsConfig is null. Skipping mission controller initialization.");
            return;
        }

        G.Instance.MissionController = new MissionController(worldMissionsConfig.allMissions);
        // Unlock starting missions
        foreach (var startingMission in worldMissionsConfig.startingMissions)
        {
            G.Instance.MissionController.UnlockMission(startingMission);
        }
    }
}