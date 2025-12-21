using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public static class BuyerManager
{
    public static Buyer CreateBuyer(Car car, CarSellOffer offer)
    {
        Warehouse warehouse = CityPositionLocator.GetWarehouse(car);
        Debug.Assert(warehouse != null, "Warehouse should not be null when creating a buyer.");
        var warehouseLocation = CityPositionLocator.GetWarehouseLocation(warehouse);

        var randomBuyerMarker = G.City.GetRandomMarker("buyer", predicate: marker => marker.PositionOnGraph != null);

        var location = G.City.GetEmptyLocation(randomBuyerMarker.PositionOnGraph.Value);
        Buyer buyer = new Buyer(car.Name + "_buyer", createInfoText(offer), location);
        return buyer;
    }

    private static string createInfoText(CarSellOffer offer)
    {
        return $"Interested in buying your {offer.Car.Name} for ${offer.InitialOfferPrice:F2}.";
    }
}

public class Buyer : CityObject
{
    public Buyer(string name, string infoText, ILocation location) : base(name, infoText, location)
    {
    }
}