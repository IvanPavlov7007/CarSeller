public class PullCarFromWarehouseHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.PullCarFromWarehouse;

    public override TransactionResult Handle(Transaction transaction)
    {
        var data = transaction.Data as PullCarFromWarehouseTransactionData;
        if (data == null)
            return TransactionResult.InvalidTransaction("Invalid data for pull car from warehouse transaction.");
        if (data.Car == null)
            return TransactionResult.InvalidTransaction("Invalid car for pull car from warehouse transaction");
        if (data.SourceWarehouse == null)
            return TransactionResult.InvalidTransaction("No source warehouse for car pulling from warehouse");

        TransactionResult result;

        if (G.CityActionService.PutCarOutsideWarehouse(data.Car, data.SourceWarehouse))
        {
            result = TransactionResult.Success();
        }
        else // couldn't put out of the warehouse
        {
            result = new TransactionResult(TransactionResultType.Failure, data: new PuttingCarOutsideFailureData(data.Car));
        }
        return result;

    }
}

public class PuttingCarOutsideFailureData : ITransactionResultData
{
    Car car;

    public PuttingCarOutsideFailureData(Car car)
    {
        this.car = car;
    }
}