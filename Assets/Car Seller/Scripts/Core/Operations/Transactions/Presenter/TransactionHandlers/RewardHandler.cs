using UnityEngine;

public class RewardHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Reward;

    public override TransactionResult Handle(Transaction transaction)
    {
        // Programming error if the processor routed a non-reward transaction here.
        Debug.Assert(transaction != null && transaction.Type == TransactionType.Reward,
            "RewardHandler received a non-reward transaction.");

        if (transaction == null || transaction.Type != TransactionType.Reward)
            return TransactionResult.InvalidTransaction("Invalid transaction: expected Reward.");

        var rewardData = transaction.Data as RewardTransactionData;
        if (rewardData == null)
            return TransactionResult.InvalidTransaction("Invalid data: expected RewardTransactionData.");

        if (G.Player == null)
            return TransactionResult.InvalidTransaction("Player not found.");

        if (rewardData.Price < 0f)
            return TransactionResult.InvalidTransaction("Invalid reward amount.");

        if (rewardData.Price <= 0f && (rewardData.Items == null || rewardData.Items.Length == 0))
            return TransactionResult.InvalidTransaction("No reward specified.");

        // Build result first
        var result = TransactionResult.Success();

        // Side effects only if successful
        if (result.Type == TransactionResultType.Success)
        {
            if (rewardData.Price > 0f)
                G.PlayerManager.AddPlayerMoney(rewardData.Price);

            if (rewardData.Items != null && rewardData.Items.Length > 0)
                G.PlayerManager.AddPossessions(rewardData.Items);
        }

        return result;
    }
}