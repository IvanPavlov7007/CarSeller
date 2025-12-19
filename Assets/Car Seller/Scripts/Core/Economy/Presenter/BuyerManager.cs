using System.Linq;
using UnityEngine.Timeline;

public class BuyerManager
{
    public Buyer CreateBuyer(Car car, City.CityLocation warehouseLocation)
    {
        var randomBuyerMarker = G.City.GetRandomMarker("buyer", predicate: marker => marker.PositionOnGraph != null);

        var location = G.City.GetEmptyLocation(randomBuyerMarker.PositionOnGraph.Value);
        Buyer buyer = new Buyer(car.Name + "_buyer");
        G.Instance.LocationService.RegisterProductLocation(buyer, location);
    }
}