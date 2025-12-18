using UnityEngine;

public class PurchaseHandler : TransactionHandler
{
    // CanHandle should only indicate whether this handler is responsible for the transaction type.
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Purchase;

    public override TransactionResult Handle(Transaction transaction)
    {
        // Programming error if the processor routed a non-purchase transaction here.
        Debug.Assert(transaction != null && transaction.Type == TransactionType.Purchase,
            "PurchaseHandler received a non-purchase transaction.");

        if (transaction == null || transaction.Type != TransactionType.Purchase)
        {
            return TransactionResult.InvalidTransaction("Invalid transaction: expected Purchase.");
        }

        var purchaseData = transaction.Data as PurchaseTransactionData;
        if (purchaseData == null)
        {
            return TransactionResult.InvalidTransaction("Invalid data: expected PurchaseTransactionData.");
        }

        var player = World.Instance.Economy.Player;
        if (player == null)
        {
            return TransactionResult.InvalidTransaction("Player not found.");
        }

        if (purchaseData.Price > player.Money)
        {
            return TransactionResult.InvalidTransaction(
                "Insufficient funds: price " + purchaseData.Price + ", available " + player.Money + ".");
        }

        if (purchaseData.Items == null)
        {
            return TransactionResult.InvalidTransaction("No items specified for purchase.");
        }

        // TODO: check if items are available in the world for purchase

        G.Instance.PlayerManager.SubtractPlayerMoney(purchaseData.Price);
        G.Instance.PlayerManager.AddPossessions(purchaseData.Items);

        return TransactionResult.Success();
    }
}