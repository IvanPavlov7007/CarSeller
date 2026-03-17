using System.Collections.Generic;

public class CityArea
{
    public CityArea(BuyersPool pool)
    {
        BuyersPool = pool;
    }
    public BuyersPool BuyersPool { get; private set; }

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