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
            new AreaLevel() { levelNumber = 1, xpToNextLevel = 100f },
            new AreaLevel() { levelNumber = 2, xpToNextLevel = 200f },
            new AreaLevel() { levelNumber = 3, xpToNextLevel = 300f },
        });
        CurrentLevelNode = areaLevels.First;
    }
    public BuyersPool BuyersPool { get; private set; }
}

public class AreaLevel
{
    public int levelNumber;
    public float xpToNextLevel;
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

public class CarSpawnAlgorithm
{
    

    void onCarDestroyed(Car car)
    {

    }
}