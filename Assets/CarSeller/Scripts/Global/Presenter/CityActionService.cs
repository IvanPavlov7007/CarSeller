using System;

public class CityActionService
{
    public bool PutCarInsideWarehouse(Car car, Warehouse warehouse)
    {
        var location = warehouse.GetEmptyLocation();
        if (location == null)
            return false;
        return G.ProductLifetimeService.MoveProduct(car, location);
    }

    public bool PutCarOutsideWarehouse(Car car, Warehouse warehouse)
    {
        return CityEntitiesCreationHelper.MoveInExistingCar(
            car,
            CityLocatorHelper.GetCityLocation(warehouse).Position
            ) != null;
    }
}