public abstract class TransactionHandler : ITransactionHandler
{
    public abstract bool CanHandle(Transaction transaction);
    public abstract TransactionResult Handle(Transaction transaction);
}

public interface ITransactionHandler
{
    bool CanHandle(Transaction transaction);
    TransactionResult Handle(Transaction transaction);
}