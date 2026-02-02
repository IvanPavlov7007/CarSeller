using System;

[Serializable]
public class CurrencyRewardBundle : RewardBundle
{
    public int amount;
    public override Transaction CreateTransaction()
    {
        var rewardData = new RewardTransactionData(amount, null);
        return new Transaction(TransactionType.Reward, rewardData);
    }

    public override string GetRewardDescription()
    {
        return CTX_Menu_Tools.FormatPrice(amount);
    }
}