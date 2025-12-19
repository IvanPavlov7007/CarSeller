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
                }
                break;
            case TransactionType.Reward:
                if(transaction.Result.Type == TransactionResultType.Success)
                {
                }
                break;
            default:
                break;
        }
    }


}