using UnityEngine;
using Pixelplacement;

public class AreaProgressionManager : Singleton<AreaProgressionManager>
{
    CityArea area => G.Area;

    public void ProgressCarSale(SellTransaction transaction)
    {
        progressArea(transaction.Price * CityArea.XP_PER_PRICE);
    }

    public void progressArea(float xp)
    {
        float initialXP = area.currentXP;
        var initialLevel = area.CurrentLevelNode.Value;
        if(!initialLevel.Final)
            progressRecursively(xp);

        var currentLevel = area.CurrentLevelNode.Value;

        GameEvents.Instance.onAreaProgressed?.Invoke(new AreaProgressEventData(
            area,
            initialXP,
            area.currentXP,
            initialLevel, currentLevel));
    }

    void progressRecursively(float xp)
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
                progressRecursively(remainderXP);
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
