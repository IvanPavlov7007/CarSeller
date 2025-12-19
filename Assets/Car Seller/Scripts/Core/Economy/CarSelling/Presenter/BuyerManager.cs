using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public static class BuyerManager
{
    public static Buyer CreateBuyer(Car car)
    {
        Warehouse warehouse = WarehouseLocator.getWarehouse(car);
        Debug.Assert(warehouse != null, "Warehouse should not be null when creating a buyer.");
        var warehouseLocation = WarehouseLocator.getWarehouseLocation(warehouse);

        var randomBuyerMarker = G.City.GetRandomMarker("buyer", predicate: marker => marker.PositionOnGraph != null);

        var location = G.City.GetEmptyLocation(randomBuyerMarker.PositionOnGraph.Value);
        Buyer buyer = new Buyer(car.Name + "_buyer", location);
        return buyer;
    }
}