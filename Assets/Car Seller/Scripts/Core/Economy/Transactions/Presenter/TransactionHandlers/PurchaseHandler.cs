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

        TransactionResult result = null;

        if (transaction == null || transaction.Type != TransactionType.Purchase)
        {
            result = TransactionResult.InvalidTransaction("Invalid transaction: expected Purchase.");
        }

        var purchaseData = transaction?.Data as PurchaseTransactionData;
        if (result == null && purchaseData == null)
        {
            result = TransactionResult.InvalidTransaction("Invalid data: expected PurchaseTransactionData.");
        }

        var player = World.Instance.Economy.Player;
        if (result == null && player == null)
        {
            result = TransactionResult.InvalidTransaction("Player not found.");
        }

        if (result == null && purchaseData.Price > player.Money)
        {
            result = TransactionResult.InvalidTransaction(
                "Insufficient funds: price " + purchaseData.Price + ", available " + player.Money + ".");
        }

        if (result == null && purchaseData.Items == null)
        {
            result = TransactionResult.InvalidTransaction("No items specified for purchase.");
        }

        // TODO: check if items are available in the world for purchase

        if (result == null)
        {
            result = TransactionResult.Success();
        }

        if (result.Type == TransactionResultType.Success)
        {
            G.Instance.PlayerManager.SubtractPlayerMoney(purchaseData.Price);
            G.Instance.PlayerManager.AddPossessions(purchaseData.Items);
        }
        return result;
    }
}