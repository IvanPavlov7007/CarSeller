using UnityEngine;

public class SellHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Sell;

    public override TransactionResult Handle(Transaction transaction)
    {
        var sellData = transaction.Data as SellTransactionData;

        Debug.Assert(sellData != null, "SellHandler received a transaction with invalid data.");

        TransactionResult result = new 
        
        GameEvents.Instance.OnTransactionComplete(new TransactionEventData(transaction, resu));
    }
}