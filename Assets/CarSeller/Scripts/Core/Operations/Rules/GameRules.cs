public static class GameRules
{
    public static readonly WarehouseCanStoreCar WarehouseCanStoreCar = new WarehouseCanStoreCar();
    public static readonly WarehouseCanStripCar WarehouseCanStripCar = new WarehouseCanStripCar();
    public static readonly CanRideOutCar CanRideOutCar = new CanRideOutCar();
    public static readonly CarCanBeStored CarCanBeStored = new CarCanBeStored();
    public static readonly CarCanBeDisassembled CarCanBeDisassembled = new CarCanBeDisassembled();
}

public class CarCanBeStored
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
        return GameRules.CarCanBeStored.Check(car) && warehouse.AvailableCarParkingSpots > 0;
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