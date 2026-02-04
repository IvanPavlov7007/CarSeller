using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public static class BuyerManager
{
    public static Buyer CreateBuyer(Car car, CarSellOffer offer)
    {
        Warehouse warehouse = CityLocatorHelper.GetWarehouse(car);
        Debug.Assert(warehouse != null, "Warehouse should not be null when creating a buyer.");
        var warehouseLocation = CityLocatorHelper.GetCityLocation(warehouse);

        var randomBuyerMarker = G.City.GetRandomMarker("buyer", predicate: marker => marker.PositionOnGraph != null);

        if(randomBuyerMarker == null)
        {
            Debug.LogError("No valid buyer marker found in the city.");
            return null;
        }

        var pos = randomBuyerMarker.PositionOnGraph.Value;
        Buyer buyer = new Buyer(car.Name + "_buyer", createInfoText(offer));
        CityEntitiesCreationHelper.CreateBuyer(buyer, pos);
        return buyer;
    }

    private static string createInfoText(CarSellOffer offer)
    {
        return $"Interested in buying your {offer.Car.Name} for ${offer.InitialOfferPrice:F2}.";
    }
}

public class Buyer : CityDestroyable, ILocatable
{
    public readonly string Name;
    public readonly string InfoText;
    public Buyer(string name, string infoText)
    {
        this.Name = name;
        this.InfoText = infoText;
    }
}