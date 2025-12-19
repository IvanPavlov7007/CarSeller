public class TransactionEventData
{
    public TransactionEventData(Transaction transaction)
    {
        this.Transaction = transaction;
    }
    public Transaction Transaction{ get; private set; }
}