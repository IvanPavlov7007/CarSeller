using UnityEngine;

public class RewardHandler : TransactionHandler
{
    public override bool CanHandle(Transaction transaction) => transaction.Type == TransactionType.Reward;

    public override TransactionResult Handle(Transaction transaction)
    {
        // Programming error if the processor routed a non-reward transaction here.
        Debug.Assert(transaction != null && transaction.Type == TransactionType.Reward,
            "RewardHandler received a non-reward transaction.");
        TransactionResult result = null;
        if (transaction == null || transaction.Type != TransactionType.Reward)
        {
            result = TransactionResult.InvalidTransaction("Invalid transaction: expected Reward.");
        }
        var rewardData = transaction?.Data as RewardTransactionData;
        if (result == null && rewardData == null)
        {
            result = TransactionResult.InvalidTransaction("Invalid data: expected RewardTransactionData.");
        }
        if (result == null && G.Player == null)
        {
            result = TransactionResult.InvalidTransaction("Player not found.");
        }
        if(result == null && rewardData.Price < 0f)
        {
            result = TransactionResult.InvalidTransaction("Invalid reward amount.");
        }
        if(result == null && rewardData.Price <= 0f && (rewardData.Items == null || rewardData.Items.Length == 0))
        {
            result = TransactionResult.InvalidTransaction("No reward specified.");
        }
        if (result == null)
        {
            result = TransactionResult.Success(rewardData.Location);
        }
        if (result.Type == TransactionResultType.Success)
        {
            if(rewardData.Price > 0f)
                G.Instance.PlayerManager.AddPlayerMoney(rewardData.Price);
            if(rewardData.Items != null && rewardData.Items.Length > 0)
                G.Instance.PlayerManager.AddPossessions(rewardData.Items);
        }
        return result;
    }
}