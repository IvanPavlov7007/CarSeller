using UnityEngine;

public static class WarehouseLocator
{
    public static Warehouse getWarehouse(Car car)
    {
        var warehouse = G.Instance.ProductLocationService.GetProductLocation(car) as Warehouse;
        if (warehouse == null)
        {
            Debug.LogWarning("Car is not located in any warehouse.");
        }
        return warehouse;
    }

    public static City.CityLocation getWarehouseLocation(Warehouse warehouse)
    {
        return G.City.Locations[warehouse];
    }
}