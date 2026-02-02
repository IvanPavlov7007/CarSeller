public class StealTransactionData : ITransactionData
{
    public Car Car { get; private set; }
    public Warehouse TargetWarehouse { get; set; }
    public StealTransactionData(Car car, Warehouse targetWarehouse)
    {
        Car = car;
        TargetWarehouse = targetWarehouse;
    }
}