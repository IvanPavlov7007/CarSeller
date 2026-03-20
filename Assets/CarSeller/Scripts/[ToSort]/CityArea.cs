using System.Collections.Generic;

public class CityArea
{
    public LinkedList<AreaLevel> areaLevels;
    public float currentXP;
    public const float XP_PER_PRICE = 0.05f;

    public LinkedListNode<AreaLevel> CurrentLevelNode;

    public CityArea(BuyersPool pool)
    {
        BuyersPool = pool;
        initialize();
    }

    void initialize()
    {
        areaLevels = new LinkedList<AreaLevel>(
        new[]{
            new AreaLevel(0, 100f ),
            new AreaLevel(1,200f),
            new AreaLevel(2, 300f),
            new AreaLevel(3, 0f,true),
        });
        CurrentLevelNode = areaLevels.First;
    }

    public BuyersPool BuyersPool { get; private set; }
}

public class AreaLevel
{
    public readonly int Index;
    public readonly float XpToNextLevel;
    public readonly bool Final = false;
    public CarSpawnWeight[] CarSpawnWeights;
    public CarsPool CarPool;

    public AreaLevel(int index, float xpToNextLevel, bool final = false)
    {
        this.Index = index;
        this.XpToNextLevel = xpToNextLevel;
        this.Final = final;

        CarSpawnWeights = G.Config.GameDatabaseContainer.AreaBalancing.CalculateCarSpawnWeightsForLevel(index);
        CarPool = new CarsPool(G.SimplifiedCarsManager.CreatePooledCarList(CarSpawnWeights));
    }

}



public class BuyersPool
{
    public float minSpawnInterval = 5f;
    public float averageSpawnInterval = 10f;
    public float lastSpawnTime = 0f;
    public List<BuyerSpawnPoint> SpawnPoints { get; private set; }

    public BuyersPool(List<BuyerSpawnPoint> spawnPoints)
    {
        SpawnPoints = spawnPoints;
    }
}

public class BuyerSpawnPoint
{
    public Buyer buyer;
    public CityPosition Position;
}