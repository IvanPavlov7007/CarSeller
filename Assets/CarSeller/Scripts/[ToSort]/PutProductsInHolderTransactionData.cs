public class PutProductsInHolderTransactionData : ITransactionData
{
    public readonly Product[] Products;
    public readonly ITargetProductsHolder TargetHolder;

    public PutProductsInHolderTransactionData(ITargetProductsHolder targetProductsHolder, params Product[] products)
    {
        Products = products;
        TargetHolder = targetProductsHolder;
    }
}