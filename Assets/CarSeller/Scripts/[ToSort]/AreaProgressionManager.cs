using Pixelplacement;
using System.Linq;
using UnityEngine;

public class AreaProgressionManager : Singleton<AreaProgressionManager>
{
    public void ProgressCarSale(SellTransaction transaction)
    {
        var position = CityLocatorHelper.GetCityEntity(transaction.Buyer).Position;

        if (!G.City.TryGetCityAreaAt(position, out var area))
        {
            Debug.LogWarning("ProgressCarSale: no CityArea found at buyer position.");
            return;
        }

        progressArea(area, transaction.Price * CityArea.XP_PER_PRICE);
    }


    private void Update()
    {
        foreach (var area in G.Areas.Values)
            UpdateArea(area, Time.deltaTime);
    }

    void UpdateArea(CityArea area, float deltaTime)
    {
        var spawnSystem = area.buyersSystem;
        Debug.Assert(spawnSystem != null, "BuyersPool should not be null when updating area.");

        if (Time.time - spawnSystem.lastSpawnTime >= spawnSystem.minSpawnInterval &&
            Random.value <= probabilityForATry(spawnSystem.averageSpawnInterval, deltaTime))
        {
            G.BuyerManager.TrySpawnBuyerAtRandomFreeSpawnPoint(spawnSystem, area.CurrentLevel.BuyersPool);
        }
    }

    float probabilityForATry(float averageTime, float deltaTime)
    {
        return 1f - Mathf.Exp(-deltaTime / averageTime);
    }

    public void progressArea(CityArea area, float xp)
    {
        float initialXP = area.currentXP;
        var initialLevel = area.CurrentLevelNode.Value;
        if(!initialLevel.Final)
            progressRecursively(area,xp);

        var currentLevel = area.CurrentLevelNode.Value;

        GameEvents.Instance.onAreaProgressed?.Invoke(new AreaProgressEventData(
            area,
            initialXP,
            area.currentXP,
            initialLevel, currentLevel));
    }

    void progressRecursively(CityArea area, float xp)
    {
        var currentLevel = area.CurrentLevelNode.Value;
        float newXP = area.currentXP + xp;

        if (newXP >= currentLevel.XpToNextLevel)
        {
            float remainderXP = newXP - currentLevel.XpToNextLevel;

            area.currentXP = currentLevel.XpToNextLevel;

            if (tryUpgradeArea(area))
            {
                area.currentXP = 0f;
                progressRecursively(area, remainderXP);
            }
        }
        else
        {
            area.currentXP = newXP;
        }
    }

    bool tryUpgradeArea(CityArea area)
    {
        if (area.CurrentLevelNode.Next == null)
        {
            return false;
        }

        area.CurrentLevelNode = area.CurrentLevelNode.Next;
        return true;
    }
}
