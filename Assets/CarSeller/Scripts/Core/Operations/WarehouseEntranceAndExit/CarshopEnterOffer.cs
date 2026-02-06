using UnityEngine;

public class CarshopEnterOffer : AcceptOnceOffer
{
    public readonly Car car;
    public readonly Warehouse warehouse;

    public CarshopEnterOffer(Car car, Warehouse warehouse)
    {
        this.car = car;
        this.warehouse = warehouse;
    }

    public override Transaction Accept()
    {
        Debug.Assert(CanAccept());
        IsAccepted = true;
        return new Transaction(TransactionType.PutProductsInWarehouse, new PutProductsInWarehouseTransactionData(warehouse, car));
    }

    public override bool CanAccept()
    {
        return base.CanAccept();
    }
}