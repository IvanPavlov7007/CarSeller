public class PutProductsInWarehouseHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.PutProductsInWarehouse;
    public override TransactionResult Handle(Transaction transaction)
    {
        var data = transaction.Data as PutProductsInWarehouseTransactionData;
        if (data == null)
        {
            return TransactionResult.InvalidTransaction("Invalid data for put car in warehouse transaction.");
        }
        var products = data.Products;
        var targetWarehouse = data.TargetWarehouse;
        
        var resultData = new WarehousePlacingResultData( WarehouseActionsHelper.TryPutProductsInsideWarehouse(targetWarehouse, products), targetWarehouse);
        TransactionResult result;
        if (resultData.PuttingResult.putInsideProducts.Count > 0)
        {
            result = TransactionResult.Success(resultData);
        }
        else // couldn't find space in the warehouse
        {
            result = new TransactionResult(TransactionResultType.Failure,resultData);
        }
        return result;
    }
}

public class WarehousePlacingResultData : ITransactionResultData
{
    public readonly ProductsPutInsideResult PuttingResult;
    public readonly Warehouse Warehouse;

    public WarehousePlacingResultData(ProductsPutInsideResult puttingResult, Warehouse warehouse)
    {
        PuttingResult = puttingResult;
        Warehouse = warehouse;
    }
}