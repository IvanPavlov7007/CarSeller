using System.Collections;
using UnityEngine;

public class WarehouseExitProcess : IProcess
{
    Car car;
    Warehouse warehouse;

    public WarehouseExitProcess(Car car, Warehouse warehouse)
    {
        this.car = car;
        this.warehouse = warehouse;
    }

    public IEnumerator Run()
    {
        Debug.Assert(car != null, "car != null");
        Debug.Assert(warehouse != null, "warehouse != null");
        Debug.Assert(CityLocatorHelper.GetWarehouse(car) == warehouse,
            "CityLocatorHelper.GetWarehouse(car) == warehouse");
        Debug.Assert(GameRules.CanRideOutCar.Check(car, warehouse),
            "GameRules.CanRideOutCar.Check(car, warehouse)");

        var result = G.TransactionProcessor.Process(new Transaction(
            TransactionType.PullCarFromWarehouse, new PullCarFromWarehouseTransactionData(
                car, warehouse)));
        if (result.Type != TransactionResultType.Success)
        {
            Debug.LogError("Failed to pull car from warehouse: " + result.Type);
        }
        yield return new WaitForSeconds(0.2f);
        if (!G.GameFlowController.TryDriveCar(car, out string reason))
        {
            Debug.LogWarning("Cannot drive car after exiting warehouse: " + reason);
        }
        G.WarehouseEntryCooldownService.NotifyExitedWarehouse(car,warehouse);
        G.GameFlowController.EnterCity();
    }
}