public class PurchaseHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction)
    {
        return transaction.Type == TransactionType.Purchase;
    }

    public override TransactionResult Handle(Transaction transaction)
    {
        throw new System.NotImplementedException();
    }
}