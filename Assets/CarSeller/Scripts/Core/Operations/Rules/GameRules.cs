using System;
using System.Diagnostics;

// TODO sort out not used anymore
public static class GameRules
{
    public static readonly WarehouseCanStoreCar WarehouseCanStoreCar = new WarehouseCanStoreCar();
    public static readonly WarehouseCanStripCar WarehouseCanStripCar = new WarehouseCanStripCar();
    public static readonly CanRideOutCar CanRideOutCar = new CanRideOutCar();
    public static readonly CarCanBeExited carCanBeExited = new CarCanBeExited();
    public static readonly CarBelongsToPlayer CarBelongsToPlayer = new CarBelongsToPlayer();
    public static readonly CarCanBeDisassembled CarCanBeDisassembled = new CarCanBeDisassembled();
    public static readonly ModelControlledByPlayer ModelControlledByPlayer = new ModelControlledByPlayer();
    public static readonly CanBePurchased CanBePurchased = new CanBePurchased();
}

public class CarBelongsToPlayer
{
    public bool Check(Car car)
    {
        if (car == null)
            return false;

        return G.Player.Owns(car);
    }
}

public class CarCanBeDisassembled
{
    public bool Check(Car car)
    {
        if (car == null)
            return false;

        foreach (var location in car.carParts.Keys)
        {
            if (location.Occupant != null)
            {
                return true;
            }
        }
        return false;
    }
}

public abstract class CarWarehouseRule
{
    public abstract bool Check(Car car, Warehouse warehouse);
}

public class CanRideOutCar : CarWarehouseRule
{
    public override bool Check(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return false;

        var carWarehouse = CityLocatorHelper.GetWarehouse(car);
        return car.IsComplete() && carWarehouse == warehouse;
    }
}

public class WarehouseCanStoreCar : CarWarehouseRule
{
    public override bool Check(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return false;

        //TODO check if wh have capacity and if car is storable
        return warehouse.AvailableCarParkingSpots > 0;
    }
}

public class WarehouseCanStripCar : CarWarehouseRule
{
    public override bool Check(Car car, Warehouse warehouse)
    {
        if (car == null || warehouse == null)
            return false;

        //TODO check if warehouse has capacity and car is strippable
        return true;
    }
}

public class WarehouseCanReceivePart
{
    public bool Check(Product part, Warehouse warehouse)
    {
        if (part == null || warehouse == null)
            return false;
        if (part is Car)
        {
            return warehouse.AvailableCarParkingSpots > 0;
        }

        //TODO check if warehouse has capacity for part
        return true;
    }
}

public class ModelControlledByPlayer
{
    public bool Check(ILocatable model)
    {
        var state = G.GameState;
        Debug.Assert(state != null, "GameState is null");
        Debug.Assert(model != null, "Model is null");

        return G.VehicleController.CurrentCar == model;
    }
}

public class CarCanBeExited
{
    public bool Check(Car car)
    {
        if(car == null)
            return false;
        bool isCurrentCar = G.VehicleController.CurrentCar == car;
        return isCurrentCar && !G.VehicleController.IsInPrimaryVehicle;
    }
}

public class CanBePurchased
{
    public bool Check(IPurchasable purchasable)
    {
        return G.Player.Money >= purchasable.Price && purchasable.IsAvailable;
    }

    public string GetUnavailabilityReason(IPurchasable purchasable)
    {
        if (G.Player.Money < purchasable.Price)
            return "Not enough money";
        if (!purchasable.IsAvailable)
            return "Not available";
        return "";
    }
}