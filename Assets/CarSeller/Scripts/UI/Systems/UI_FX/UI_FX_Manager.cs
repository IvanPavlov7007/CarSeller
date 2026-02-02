using Pixelplacement;
using UnityEngine;

public class UI_FX_Manager : Singleton<UI_FX_Manager>
{
    private void OnEnable() { GameEvents.Instance.OnTransactionComplete += onTransactionComplete; }
    private void OnDisable()
    {
        GameEvents.Instance.OnTransactionComplete -= onTransactionComplete;
    }

    void onTransactionComplete(TransactionEventData data)
    {
        var transaction = data.Transaction;
        if (transaction == null || transaction.Result == null)
            return;

        if (transaction.Result.Type != TransactionResultType.Success)
        {
            Debug.LogWarning($"Transaction failed as {transaction.Result.Type} with {transaction.Result.Data}");
            return;
        }

        switch (transaction.Type)
        {
            case TransactionType.Purchase:
                // TODO: add purchase-specific FX if needed
                break;

            case TransactionType.Steal:
                // TODO: add steal-specific FX if needed
                break;

            case TransactionType.Sell:
                {
                    var transactionData = transaction.Data as SellTransactionData;
                    if (transactionData != null)
                        playMoneyEffect(transactionData.Price, data.TransactionFeedbackLocation);
                    break;
                }

            case TransactionType.Reward:
                {
                    var transactionData = transaction.Data as RewardTransactionData;
                    if (transactionData != null)
                        playMoneyEffect(transactionData.Price, data.TransactionFeedbackLocation);
                    break;
                }

            default:
                break;
        }
    }

    void playMoneyEffect(float amount, TransactionFeedbackLocation location)
    {
        if (location == null)
        {
            Debug.LogWarning("Transaction location is null, defaulting to OmniDirectional.");
            location = TransactionFeedbackLocation.OmniDirectional;
        }

        if (location.Type == TransactionLocationType.WorldSpace)
        {
            UIEffectsDisplayer.Instance.PlayMoneyEffectWorld(amount, location.Position);
        }
        else
        {
            UIEffectsDisplayer.Instance.PlayMonetEffectScreen(amount, location.Position);
        }
    }
}
