using Pixelplacement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AreaProgressionManager : MonoBehaviour
{
    CityArea area;


    void progressArea(float xp)
    {
        
    }

    void progressRecursively(float xp)
    {
        var currentLevel = area.CurrentLevelNode.Value;
        float newXP = area.currentXP + xp;
        if (newXP >= currentLevel.xpToNextLevel)
        {
            area.currentXP = currentLevel.xpToNextLevel;
            if (tryUpgradeArea(area, newXP - area.currentXP))
            {
                progressRecursively(newXP - area.currentXP);
            }
        }
        else
        {
            area.currentXP = newXP;
        }
    }

    bool tryUpgradeArea(CityArea area, float deltaXP)
    {
        if(area.CurrentLevelNode.Next == null)
        {
            return false;
        }
        area.CurrentLevelNode = area.CurrentLevelNode.Next;
        return true;
    }
}
