using Pixelplacement;

public class UI_FX_Manager : Singleton<UI_FX_Manager>
{
    private void OnEnable()
    {
        GameEvents.Instance.OnTransactionComplete += onTransactionComplete;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnTransactionComplete -= onTransactionComplete;
    }

    void onTransactionComplete(TransactionEventData data)
    {
        var transaction = data.Transaction;

        switch(transaction.Type)
        {
            case TransactionType.Purchase:
                if(transaction.Result.Type == TransactionResultType.Success)
                {
                }
                break;
            case TransactionType.Steal:
                if(transaction.Result.Type == TransactionResultType.Success)
                {
                }
                break;
            case TransactionType.Sell:
                if(transaction.Result.Type == TransactionResultType.Success)
                {
                    var transactionData = transaction.Data as SellTransactionData;
                    playMoneyEffect(transactionData.Price, transaction.Result.Location);
                }
                break;
            case TransactionType.Reward:
                if(transaction.Result.Type == TransactionResultType.Success)
                {
                    var transactionData = transaction.Data as RewardTransactionData;
                    playMoneyEffect(transactionData.Price, transaction.Result.Location);
                }
                break;
            default:
                break;
        }
    }

    void playMoneyEffect(float amount, TransactionLocation location)
    {
        if(location.Type == TransactionLocationType.WorldSpace)
        {
            WorldEffectsDisplayer.Instance.PlayMoneyEffectWorld(amount, location.Position);
        }
        else
        {
            WorldEffectsDisplayer.Instance.PlayMonetEffectScreen(amount, location.Position);
        }
    }


}