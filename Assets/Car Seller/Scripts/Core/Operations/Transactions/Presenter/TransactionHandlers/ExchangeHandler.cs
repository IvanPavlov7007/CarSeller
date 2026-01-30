using UnityEngine;

public class ExchangeHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Exchange;
    public override TransactionResult Handle(Transaction transaction)
    {
        Debug.Assert(transaction != null && transaction.Type == TransactionType.Exchange,
            "RewardHandler received a non-exchange transaction.");

        if (transaction == null || transaction.Type != TransactionType.Exchange)
            return TransactionResult.InvalidTransaction("Invalid transaction: expected Exchange.");

        var exchangeData = transaction.Data as ExchangeTransactionData;
        if (exchangeData == null)
            return TransactionResult.InvalidTransaction("Invalid data: expected ExchangeTransactionData.");

        if (exchangeData.FromPlayer == null)
            return TransactionResult.InvalidTransaction("No car specified to give in exchange.");

        if (exchangeData.ToPlayer == null)
            return TransactionResult.InvalidTransaction("No car specified to receive in exchange.");

        // Build result first
        var result = TransactionResult.Success();

        // Side effects only if successful
        if (result.Type == TransactionResultType.Success)
        {
            G.PlayerManager.DeltaPlayerMoney(exchangeData.DeltaMoney);
            G.ProductLifetimeService.SwapProducts(exchangeData.FromPlayer,
                exchangeData.ToPlayer);
        }

        return result;
    }
}