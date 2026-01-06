using Pixelplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectablesManager : Singleton<CollectablesManager>
{
    Dictionary<ILocation, Collectable> collectables;
    List<ILocation> allLocations;

    int tooFew;
    float money;
    Action callback;

    public void Initialize(List<ILocation> locations, float money, Action callback, int tooFew)
    {
        allLocations = new List<ILocation>();
        allLocations.AddRange(locations);
        collectables = new Dictionary<ILocation, Collectable>(locations.Count);
        this.tooFew = tooFew;
        this.money = money;
        this.callback = callback;
        Fill(allLocations);
    }

    public void Fill(List<ILocation> locations)
    {
        foreach (var location in locations)
        {
            CreateCollectable(location, money, callback);
        }
    }

    public Collectable CreateCollectable(ILocation location, float money, Action additionalCallback)
    {
        if(collectables.ContainsKey(location))
        {
            Debug.LogWarning($"Collectable already exists at location {location}");
            return collectables[location];
        }
        var collectable = new Collectable(location, "Collectable", money, additionalCallback: additionalCallback);
        collectable.OnCollectedAdditionalCallback += () => OnCollectableCollected(collectable);
        collectables.Add(location, collectable);
        return collectable;
    }

    void OnCollectableCollected(Collectable collectable)
    {
        if (!collectables.ContainsKey(collectable.Location))
        {
            Debug.LogWarning("Collected collectable not found in manager.");
            return;
        }
        collectables.Remove(collectable.Location);
        
        var transactionLocation = new TransactionLocation(TransactionLocationType.WorldSpace, collectable.Location.CityPosition.WorldPosition);
        var rewardTransaction = new Transaction(
            TransactionType.Reward,
            new RewardTransactionData(collectable.MoneyAmount, null, transactionLocation)
        );
        G.TransactionProcessor.Process(rewardTransaction);

        CheckTooFew();
    }

    void CheckTooFew()
    {
        if(collectables.Count <= tooFew)
        {
            var availableLocations = allLocations.Except(collectables.Keys).ToList();
            Fill(availableLocations);
        }
    }
}