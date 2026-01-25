using System;

public class CityActionService
{
    public bool PutCarInsideWarehouse(Car car, Warehouse warehouse)
    {
        var location = warehouse.GetEmptyLocation();
        if (location == null)
            return false;
        return G.ProductLocationService.MoveProduct(car, location);
    }
}