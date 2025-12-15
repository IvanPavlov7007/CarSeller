using System.Collections.Generic;

public class TransactionProcessor
{
    Dictionary<TransactionType, ITransactionHandler> Handlers { get; private set; }

    public TransactionProcessor(Dictionary<TransactionType, ITransactionHandler> handlers)
    {
        Handlers = handlers;
    }

    public TransactionResult Process(Transaction transaction)
    {
        TransactionResult result;
        if (Handlers.TryGetValue(transaction.Type, out var handler) && handler.CanHandle(transaction))
        {
            result = handler.Handle(transaction);
        }
        else
            result = TransactionResult.InvalidTransaction($"No handler found for transaction type {transaction.Type}");
        transaction.FinalizeResult(result.Type);
        return result;
    }

}