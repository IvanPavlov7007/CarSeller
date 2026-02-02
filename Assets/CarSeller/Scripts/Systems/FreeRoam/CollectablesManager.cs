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
        if (data.ReachedObject.Subject is Collectable collectable)
        {
            collect(collectable, data.ReachedObject);
            City.EntityLifetimeService.Destroy(collectable);
        }
    }


    //TODO make process possessions, actions and custom data as well in the future
    void collect(Collectable collectable, CityEntity cityEntity)
    {
        var transactionLocation = new TransactionFeedbackLocation(TransactionLocationType.WorldSpace, cityEntity.Position.WorldPosition);
        var rewardTransaction = new Transaction(
            TransactionType.Reward,
            new RewardTransactionData(collectable.MoneyAmount, null)
        );
        G.TransactionProcessor.Process(rewardTransaction, transactionLocation);
    }
}