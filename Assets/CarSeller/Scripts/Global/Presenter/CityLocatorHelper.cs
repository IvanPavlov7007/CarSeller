using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Streamlined helper methods for locating objects within the city.
/// Use it to find warehouses, cars, and their respective locations.
/// </summary>
public static class CityLocatorHelper
{
    public static bool IsInCity(ILocatable locatable)
    {
        return G.City.Locations.ContainsKey(locatable);
    }

    public static Warehouse GetWarehouse(Car car)
    {
        var warehouse = G.ProductLifetimeService.GetProductLocation(car).Holder as Warehouse;
        if (warehouse == null)
        {
            Debug.LogWarning("Car is not located in any warehouse.");
        }
        return warehouse;
    }

    /// <summary>
    /// Gets the city location of the parent warehouse if the car is in a warehouse,
    /// </summary>
    /// <param name="car"></param>
    /// <returns></returns>
    public static City.CityLocation GetCarParentLocation(Car car)
    {
        var warehouse = GetWarehouse(car);
        if (warehouse == null)
        {
            return G.City.Locations[car];
        }
        return GetCityLocation(warehouse);
    }

    public static Warehouse GetClosestWarehouse(Car car, out float distance)
    {


        var city = World.Instance.City;
        Debug.Assert(city.Locations.ContainsKey(car), "CityInteractionManager: Car position not found in city positions");
        Vector2 carPosition = city.Locations[car].CityPosition.WorldPosition;
        Dictionary<Warehouse, float> warehouseDistances = new Dictionary<Warehouse, float>();
        foreach (var obj in city.Locations.Keys)
        {
            if (obj is not Warehouse)
                continue;
            var warehousePosition = city.Locations[obj].CityPosition.WorldPosition;
            warehouseDistances.Add(obj as Warehouse, Vector2.Distance(warehousePosition, carPosition));
        }
        warehouseDistances = warehouseDistances.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        distance = warehouseDistances.First().Value;
        return warehouseDistances.First().Key;
    }

    public static Car GetClosestCar(Warehouse warehouse, out float distance)
    {
        var city = World.Instance.City;
        Vector2 warehousePosition = city.Locations[warehouse].CityPosition.WorldPosition;
        Dictionary<Car, float> carDistances = new Dictionary<Car, float>();
        foreach (var obj in city.Locations.Keys)
        {
            if (obj is not Car)
                continue;
            var carPosition = city.Locations[obj].CityPosition.WorldPosition;
            carDistances.Add(obj as Car, Vector2.Distance(carPosition, warehousePosition));
        }


        if (carDistances.Count == 0)
        {
            distance = float.MaxValue;
            return null;
        }
        carDistances = carDistances.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        distance = carDistances.First().Value;
        return carDistances.First().Key;
    }

    public static City.CityLocation GetCityLocation(ILocatable locatable)
    {
        return G.City.Locations[locatable];
    }
}