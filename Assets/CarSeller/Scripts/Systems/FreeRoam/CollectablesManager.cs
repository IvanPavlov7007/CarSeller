using Pixelplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectablesManager : Singleton<CollectablesManager>
{
    private void OnEnable()
    {
        GameEvents.Instance.OnTargetReached += onCityTargetReached;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnTargetReached -= onCityTargetReached;
    }

    void onCityTargetReached(CityTargetReachedEventData data)
    {
        if (data.ReachedObject is CollectableCityObject cityObject)
        {
            collect(cityObject.Data as Collectable, cityObject);
            cityObject.Destroy();
        }
    }


    //TODO make process possessions, actions and custom data as well in the future
    void collect(Collectable collectable, CityObject cityObject)
    {
        var transactionLocation = new TransactionFeedbackLocation(TransactionLocationType.WorldSpace, cityObject.Location.CityPosition.WorldPosition);
        var rewardTransaction = new Transaction(
            TransactionType.Reward,
            new RewardTransactionData(collectable.MoneyAmount, null)
        );
        G.TransactionProcessor.Process(rewardTransaction, transactionLocation);
    }
}