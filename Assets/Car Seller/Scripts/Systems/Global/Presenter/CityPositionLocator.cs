using UnityEngine;

public static class CityPositionLocator
{
    public static Warehouse GetWarehouse(Car car)
    {
        var warehouse = G.Instance.ProductLocationService.GetProductLocation(car) as Warehouse;
        if (warehouse == null)
        {
            Debug.LogWarning("Car is not located in any warehouse.");
        }
        return warehouse;
    }

    public static City.CityLocation GetWarehouseLocation(Warehouse warehouse)
    {
        return G.City.Locations[warehouse];
    }

    public static City.CityLocation GetCarLocation(Car car)
    {
        var warehouse = GetWarehouse(car);
        if (warehouse == null)
        {
            return G.City.Locations[car];
        }
        return GetWarehouseLocation(warehouse);
    }
}