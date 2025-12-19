using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Economy
{
    public EconomyConfig Config { get; private set; }

    public Player Player;

    public WarehouseOfferProvider WarehouseOfferProvider;

    public CarSellOneTimeOfferProvider CarSellOfferProvider;

    public ProductPriceCalculator ProductPriceCalculator = new ProductPriceCalculator();

    public Economy(EconomyConfig config)
    {
        Config = config;
        Player = new Player();
        initializeWarehouseOffers();
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
    }
}