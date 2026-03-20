using JetBrains.Annotations;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaBalancingContent
{
    [SpreadsheetPage("Area1CarTypeWeights")]
    public List<AreaCarTypeSpawnWeightsPerLevel> Area1CarTypeSpawnWeightsPerLevelList;
    [SpreadsheetPage("Area1CarRarityWeights")]
    public List<AreaCarRaritySpawnWeightsPerLevel> Area1CarRaritySpawnWeightsPerLevelList;

    public CarSpawnWeight[] CalculateCarSpawnWeightsForLevel(int level)
    {
        return CalculateCarSpawnWeightsForLevel(
            Area1CarTypeSpawnWeightsPerLevelList.Find(x => x.AreaLevel == level),
            Area1CarRaritySpawnWeightsPerLevelList.Find(x => x.AreaLevel == level));
    }

    static CarSpawnWeight[] CalculateCarSpawnWeightsForLevel(
        AreaCarTypeSpawnWeightsPerLevel typeWeights,
        AreaCarRaritySpawnWeightsPerLevel levelWeights)
    {
        Debug.Assert(typeWeights.AreaLevel == levelWeights.AreaLevel);
        var result = new []
        {
            new CarSpawnWeight(new CarKind(CarType.Small,CarRarity.Common) , typeWeights.Small * levelWeights.Common),
            new CarSpawnWeight(new CarKind(CarType.Sedan,CarRarity.Common) , typeWeights.Sedan * levelWeights.Common),
            new CarSpawnWeight(new CarKind(CarType.Truck,CarRarity.Common) , typeWeights.Truck * levelWeights.Common),
            new CarSpawnWeight(new CarKind(CarType.Bike,CarRarity.Common) , typeWeights.Bike * levelWeights.Common),
            new CarSpawnWeight(new CarKind(CarType.Super,CarRarity.Common) , typeWeights.Super * levelWeights.Common),

            new CarSpawnWeight(new CarKind(CarType.Small,CarRarity.Uncommon) , typeWeights.Small * levelWeights.Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Sedan,CarRarity.Uncommon) , typeWeights.Sedan * levelWeights.Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Truck,CarRarity.Uncommon) , typeWeights.Truck * levelWeights.Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Bike,CarRarity.Uncommon) , typeWeights.Bike * levelWeights.Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Super,CarRarity.Uncommon) , typeWeights.Super * levelWeights.Uncommon),

            new CarSpawnWeight(new CarKind(CarType.Small,CarRarity.Epic) , typeWeights.Small * levelWeights.Epic),
            new CarSpawnWeight(new CarKind(CarType.Sedan,CarRarity.Epic) , typeWeights.Sedan * levelWeights.Epic),
            new CarSpawnWeight(new CarKind(CarType.Truck,CarRarity.Epic) , typeWeights.Truck * levelWeights.Epic),
            new CarSpawnWeight(new CarKind(CarType.Bike,CarRarity.Epic) , typeWeights.Bike * levelWeights.Epic),
            new CarSpawnWeight(new CarKind(CarType.Super,CarRarity.Epic) , typeWeights.Super * levelWeights.Epic),
        };
        return result;
    }
}

[Serializable]
public class AreaCarTypeSpawnWeightsPerLevel
{
    public int AreaLevel;
    public float Small;
    public float Sedan;
    public float Truck;
    public float Bike;
    public float Super;
}

[Serializable]
public class AreaCarRaritySpawnWeightsPerLevel
{
    public int AreaLevel;
    public float Common;
    public float Uncommon;
    public float Epic;
}

[CreateAssetMenu(fileName = "GameDatabaseContainer", menuName = "CarSeller/GameDatabaseContainer")]
public class GameDatabaseContainer : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField]
    AreaBalancingContent AreaBalancingContent;
    public AreaBalancingContent AreaBalancing => AreaBalancingContent;
}

public struct CarSpawnWeight
{
    public CarKind CarKind;
    public float Weight;

    public CarSpawnWeight(CarKind carKind, float weight)
    {
        this.CarKind = carKind;
        this.Weight = weight;
    }
}