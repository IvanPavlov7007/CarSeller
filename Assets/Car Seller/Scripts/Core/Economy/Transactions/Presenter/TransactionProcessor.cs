using System.Collections.Generic;

public class TransactionProcessor
{
    Dictionary<TransactionType, ITransactionHandler> Handlers { get; set; }

    public TransactionProcessor(Dictionary<TransactionType, ITransactionHandler> handlers)
    {
        Handlers = handlers;
    }

    public TransactionResult Process(Transaction transaction)
    {
        if (transaction == null)
            return TransactionResult.InvalidTransaction("Transaction is null");

        TransactionResult result;

        if (Handlers.TryGetValue(transaction.Type, out var handler) &&
            handler != null &&
            handler.CanHandle(transaction))
        {
            result = handler.Handle(transaction) 
                     ?? TransactionResult.InvalidTransaction("Handler returned null result.");
        }
        else
        {
            result = TransactionResult.InvalidTransaction(
                $"No handler found for transaction type {transaction.Type}");
        }

        transaction.FinalizeResult(result);
        GameEvents.Instance.OnTransactionComplete(new TransactionEventData(transaction));

        return result;
    }
}