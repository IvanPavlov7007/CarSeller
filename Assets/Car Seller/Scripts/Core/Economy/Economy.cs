using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Economy
{
    public EconomyConfig Config { get; private set; }

    public Player Player;

    public WarehouseOfferProvider WarehouseOfferProvider;

    public CarSellOneTimeOfferProvider CarSellOfferProvider = new CarSellOneTimeOfferProvider();

    public ProductPriceCalculator ProductPriceCalculator = new ProductPriceCalculator();

    public Economy(EconomyConfig config)
    {
        Config = config;
        Player = new Player();
        initializePlayerStartState();
        initializeWarehouseOffers();
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
            Player.Possessions.Add(warehouse);
            foreach (var productLocation in warehouse.products)
            {
                if (productLocation.Occupant != null)
                {
                    //DIRTY FIX: since ownership system is not finished, we only add cars to possessions
                    //Because they are removed later by game logic (currently when selling)
                    if (productLocation.Occupant is Car)
                        Player.Possessions.Add(productLocation.Occupant as IPossession);
                }
            }
        }
    }

    private void initializeWarehouseOffers()
    {
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