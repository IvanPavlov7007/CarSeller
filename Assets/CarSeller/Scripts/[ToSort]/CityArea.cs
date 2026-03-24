using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

public class CityArea
{
    public LinkedList<AreaLevel> areaLevels;
    public float currentXP;
    public const float XP_PER_PRICE = 0.05f;

    public LinkedListNode<AreaLevel> CurrentLevelNode;

    public AreaLevel CurrentLevel => CurrentLevelNode.Value;

    public CarsPool CarsPool => CurrentLevel.CarPool;
    public BuyersPool BuyersPool => CurrentLevel.BuyersPool;

    public AreaBuyerSystem buyersSystem { get; private set; }
    public string Id { get; internal set; }

    public CityArea(AreaCollection data)
    {
        Id = data.Id;
        initializeBuyersSpawnPoints(data);
        initializeLevels(data);
    }

    private void initializeBuyersSpawnPoints(AreaCollection areaData)
    {
        var buyersPositions = G.City.QueryMarkers("buyer",areaData.Id).Where(m => m.PositionOnGraph != null).Select(m => m.PositionOnGraph.Value).ToList();
        buyersSystem = new AreaBuyerSystem(
            buyersPositions.Select(pos => new BuyerSpawnPoint { Position = pos }).ToList());
    }

    private void initializeLevels(AreaCollection data)
    {
        areaLevels = new LinkedList<AreaLevel>(AreaLevel.createLevels(data.Levels));
        CurrentLevelNode = areaLevels.First;
    }
    
}

public class AreaLevel
{
    public readonly int Index;
    public readonly float XpToNextLevel;
    public readonly bool Final = false;

    public CarSpawnWeight[] CarSpawnWeights;
    public BuyerSpawnSpawnWeight[] BuyerSpawnWeights;

    public CarsPool CarPool;
    public BuyersPool BuyersPool;

    AreaLevel(LevelBalancing balancing, bool final)
    {
        Index = balancing.AreaLevel;
        XpToNextLevel = balancing.RequiredXP;
        Final = final;

        intializeCars(balancing);
        initializeBuyers(balancing);
    }

    private void intializeCars(LevelBalancing balancing)
    {
        CarSpawnWeights = balancing.CalculateCarSpawnWeightsForLevel();
        CarPool = new CarsPool(G.SimplifiedCarsManager.CreatePooledCarList(CarSpawnWeights));
    }

    private void initializeBuyers(LevelBalancing balancing)
    {
        BuyerSpawnWeights = balancing.CalculateBuyerSpawnWeightsForLevel();
        BuyersPool = new BuyersPool(BuyerManager.CreatePooledBuyerList(BuyerSpawnWeights));
    }

    public static AreaLevel[] createLevels(List<LevelBalancing> balancings)
    {
        AreaLevel[] result = new AreaLevel[balancings.Count];
        for (int i = 0; i < balancings.Count; i++)
        {
            result[i] = new AreaLevel(balancings[i], i == balancings.Count - 1);
        }
        return result;
    }

}

public class Pool<T>
{
    public Queue<T> Queue;
    public HashSet<T> AllItems;

    public Pool(List<T> list)
    {
        list.Shuffle();
        AllItems = new HashSet<T>(list);
        Queue = new Queue<T>(list);
    }

    public void shuffle()
    {
        var list = new List<T>(Queue);
        list.Shuffle();
        Queue = new Queue<T>(list);
    }
}

public class CarsPool : Pool<Car>
{
    public CarsPool(List<Car> list) : base(list)
    {
    }
}

public class BuyersPool : Pool<Buyer>
{
    public BuyersPool(List<Buyer> list) : base(list)
    {
    }
}




public class AreaBuyerSystem
{
    public float minSpawnInterval = 5f;
    public float averageSpawnInterval = 10f;
    public float lastSpawnTime = 0f;
    public List<BuyerSpawnPoint> SpawnPoints { get; private set; }

    public AreaBuyerSystem(List<BuyerSpawnPoint> spawnPoints)
    {
        SpawnPoints = spawnPoints;
    }
}

public class BuyerSpawnPoint
{
    public Buyer buyer;
    public CityPosition Position;
}

