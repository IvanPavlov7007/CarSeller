using UnityEngine;

public class SellHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Sell;

    public override TransactionResult Handle(Transaction transaction)
    {
        // Programming error if the processor routed a non-sell transaction here.
        Debug.Assert(transaction != null && transaction.Type == TransactionType.Sell,
            "SellHandler received a non-sell transaction.");

        TransactionResult result = null;

        if (transaction == null || transaction.Type != TransactionType.Sell)
        {
            result = TransactionResult.InvalidTransaction("Invalid transaction: expected Sell.");
        }

        var sellData = transaction?.Data as SellTransactionData;

        if (result == null && sellData == null)
        {
            result = TransactionResult.InvalidTransaction("Invalid data: expected SellTransactionData.");
        }

        var player = World.Instance.Economy.Player;
        if (result == null && player == null)
        {
            result = TransactionResult.InvalidTransaction("Player not found.");
        }

        var car = sellData?.Car;

        if (result == null && car == null)
        {
            result = TransactionResult.InvalidTransaction("No car specified for sale.");
        }

        if (result == null && !player.Owns(car))
        {
            result = TransactionResult.InvalidTransaction("Player does not own the car being sold.");
        }

        if (result == null)
        {
            result = TransactionResult.Success();
        }

        if (result.Type == TransactionResultType.Success)
        {
            G.PlayerManager.AddPlayerMoney(sellData.Price);
            G.ProductLifetimeService.DeleteProduct(car);
        }
        return result;
    }
}