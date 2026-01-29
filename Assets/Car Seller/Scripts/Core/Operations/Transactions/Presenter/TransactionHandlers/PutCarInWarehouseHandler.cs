public class PutCarInWarehouseHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.PutCarInWarehouse;
    public override TransactionResult Handle(Transaction transaction)
    {
        var data = transaction.Data as PutCarInWarehouseTransactionData;
        if (data == null)
        {
            return TransactionResult.InvalidTransaction("Invalid data for put car in warehouse transaction.");
        }
        var car = data.Car;
        var targetWarehouse = data.TargetWarehouse;
        TransactionResult result;
        if (G.CityActionService.PutCarInsideWarehouse(car, targetWarehouse))
        {
            result = TransactionResult.Success();
        }
        else // couldn't find space in the warehouse
        {
            result = new TransactionResult(TransactionResultType.Failure, data: new WarehousePlacingFailureData(targetWarehouse));
        }
        return result;
    }
}