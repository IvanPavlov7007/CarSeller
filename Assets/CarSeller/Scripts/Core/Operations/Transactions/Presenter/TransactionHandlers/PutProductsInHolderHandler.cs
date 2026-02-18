public class PutProductsInHolderHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.PutProductsInWarehouse;
    public override TransactionResult Handle(Transaction transaction)
    {
        var data = transaction.Data as PutProductsInHolderTransactionData;
        if (data == null)
        {
            return TransactionResult.InvalidTransaction("Invalid data for put car in warehouse transaction.");
        }
        var products = data.Products;
        var targetHolder = data.TargetHolder;
        
        var resultData = new ProductsPlacingResultData(targetHolder.PutProducts(products));
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

public class ProductsPlacingResultData : ITransactionResultData
{
    public readonly ProductsPutInsideResult PuttingResult;

    public ProductsPlacingResultData(ProductsPutInsideResult puttingResult)
    {
        PuttingResult = puttingResult;
    }
}