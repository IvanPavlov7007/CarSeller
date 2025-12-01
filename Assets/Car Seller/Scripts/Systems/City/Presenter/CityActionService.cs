public class CityActionService
{
    public void PutCarInsideWarehouse(Car car, Warehouse warehouse)
    {
        G.Instance.LocationService.MoveProduct(car, warehouse.GetEmptyLocation());
    }
}