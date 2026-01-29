public class StripCarTransactionData : ITransactionData
{
    public readonly Car Car;
    public readonly Warehouse TargetWarehouse;
    public readonly StrippingProcess StrippingProcess;

    public StripCarTransactionData(Car car, Warehouse targetWarehouse, StrippingProcess strippingProcess)
    {
        Car = car;
        TargetWarehouse = targetWarehouse;
        StrippingProcess = strippingProcess;
    }
}

public class PullCarFromWarehouseTransactionData : ITransactionData
{
    public readonly Car Car;
    public readonly Warehouse SourceWarehouse;
    public PullCarFromWarehouseTransactionData(Car car, Warehouse sourceWarehouse)
    {
        Car = car;
        SourceWarehouse = sourceWarehouse;
    }
}

public class PutCarInWarehouseTransactionData : ITransactionData
{
    public readonly Car Car;
    public readonly Warehouse TargetWarehouse;

    public PutCarInWarehouseTransactionData(Car car, Warehouse targetWarehouse)
    {
        Car = car;
        TargetWarehouse = targetWarehouse;
    }
}