using System.Collections.Generic;
using UnityEngine;

public class TransactionProcessor
{
    Dictionary<TransactionType, ITransactionHandler> Handlers { get; set; }

    public TransactionProcessor(Dictionary<TransactionType, ITransactionHandler> handlers)
    {
        Handlers = handlers;
    }


    /// <summary>
    /// Finds handler for the transaction, executes, saves result, and triggers transaction complete event.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public TransactionResult Process(Transaction transaction, TransactionFeedbackLocation location)
    {
        Debug.Assert(Handlers != null, "TransactionProcessor handlers dictionary is null.");

        if (transaction == null)
            return TransactionResult.InvalidTransaction("Transaction is null");

        if (location == null)
        {
            Debug.LogWarning("TransactionProcessor.Process called with null location. Defaulting to OmniDirectional.");
            location = TransactionFeedbackLocation.OmniDirectional;
        }

        TransactionResult result;

        if (Handlers != null &&
            Handlers.TryGetValue(transaction.Type, out var handler) &&
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
        GameEvents.Instance.OnTransactionComplete(new TransactionEventData(transaction, location));

        return result;
    }

    public TransactionResult Process(Transaction transaction)
    {
        return Process(transaction, TransactionFeedbackLocation.OmniDirectional);
    }
}