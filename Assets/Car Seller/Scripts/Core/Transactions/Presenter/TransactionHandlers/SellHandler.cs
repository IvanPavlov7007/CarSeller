public class SellHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Sell;

    public override TransactionResult Handle(Transaction transaction)
    {

    }
}