using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Economy
{
    public EconomyConfig Config { get; private set; }

    

    public WarehouseOfferProvider WarehouseOfferProvider;

    public CarSellOneTimeOfferProvider CarSellOfferProvider = new CarSellOneTimeOfferProvider();

    public CarShopOfferProvider CarShopOfferProvider;

    public ProductPriceCalculator ProductPriceCalculator = new ProductPriceCalculator();

    public CarSpawnManager CarSpawnManager = new CarSpawnManager();

    public Player Player => G.Player;

    public Economy(EconomyConfig config)
    {
        Config = config;
        initializePlayerStartState();
        initializeWarehouseOffers();
        initializeCarShopOfferProvider();
    }

    private void initializePlayerStartState()
    {
        Player.ChangeMoney(Config.PlayerStartState.initialMoney);
        foreach (var warehouseConfig in Config.PlayerStartState.ownWarehouses)
        {
            var registryList = World.Instance.WorldRegistry.GetByConfig<Warehouse>(warehouseConfig);
            Debug.Assert(registryList != null, $"No warehouses found for config {warehouseConfig}.");
            if (registryList == null)
                continue;
            Debug.Assert(registryList.Count == 1, $"Expected exactly one warehouse for config {warehouseConfig}, but found {registryList.Count}.");
            Warehouse warehouse = registryList.First();

            // Transfer ownership to player
            // Note: products inside the warehouse remain not owned
            G.OwnershipService.TransferOwnership(warehouse, Player);
        }
    }

    private void initializeCarShopOfferProvider()
    {
        if(Config.CarShopOffersConfig == null)
        {
            Debug.LogWarning("Economy: No CarShopOffersConfig found in EconomyConfig. Car Shop offers are disabled");
            return;
        }

        var carOptions = new List<Car>();
        foreach (var offerConfig in Config.CarShopOffersConfig.offerConfigs)
        {
            // Pre-generate cars for sale in Car Shop
            var emptyLocation = World.Instance.HiddenSpace.GetEmptyLocation();
            var car = offerConfig.carSpawnConfig.GenerateCar(emptyLocation);
            carOptions.Add(car);
        }
        CarShopOfferProvider = new CarShopOfferProvider(carOptions);
    }

    private void initializeWarehouseOffers()
    {
        if(Config.WarehouseOffersConfig == null)
        {
            Debug.LogWarning("Economy: No WarehouseOffersConfig found in EconomyConfig. Warehouses acquirement is impossible");
            return;
        }

        WarehouseOfferProvider = new WarehouseOfferProvider();
        var dictionary = new Dictionary<Warehouse, WarehouseOffer>();

        foreach (var offerConfig in Config.WarehouseOffersConfig.Offers)
        {
            var registryList = World.Instance.WorldRegistry.GetByConfig<Warehouse>(offerConfig.Warehouse);
            Debug.Assert(registryList != null, $"No warehouses found for config {offerConfig.Warehouse}.");
            if (registryList == null)
                continue;
            Debug.Assert(registryList.Count == 1, $"Expected exactly one warehouse for config {offerConfig.Warehouse}, but found {registryList.Count}.");

            Warehouse warehouse = registryList.First();
            var warehouseOffer = new WarehouseOffer(
                WarehouseOfferProvider,
                warehouse,
                offerConfig.Price
            );
            dictionary[warehouse] = warehouseOffer;
        }

        WarehouseOfferProvider.Initialize(dictionary);
    }
}