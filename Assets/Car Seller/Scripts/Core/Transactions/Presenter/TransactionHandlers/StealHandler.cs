public class StealHandler : ITransactionHandler
{
    public bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Steal;

    public TransactionResult Handle(Transaction transaction)
    {
        var data = transaction.Data as StealTransactionData;
        if (data == null)
        {
            return TransactionResult.InvalidTransaction("Invalid data for steal transaction.");
        }
        var car = data.Car;
        var targetWarehouse = data.TargetWarehouse;
        // Logic to add the stolen car to the target warehouse

        TransactionResult result;

        if (G.Instance.CityActionService.PutCarInsideWarehouse(car, targetWarehouse))
        {
            result = TransactionResult.Success();
        }
        else // couldn't find space in the warehouse
        {
            result = new TransactionResult(TransactionResultType.Failure, new WarehousePlacingFailureData(targetWarehouse));
        }
        return result;
    }
}

public class WarehousePlacingFailureData : ITransactionResultData
{
    public Warehouse warehouse;
    public WarehousePlacingFailureData(Warehouse warehouse)
    {
        this.warehouse = warehouse;
    }
}