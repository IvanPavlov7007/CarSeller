using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

using Random = UnityEngine.Random;

public class BuyerManager : MonoBehaviour
{
    [Obsolete]
    public static Buyer CreateBuyer(Car car, CarSellOffer offer)
    {
        Warehouse warehouse = CityLocatorHelper.GetWarehouse(car);
        Debug.Assert(warehouse != null, "Warehouse should not be null when creating a buyer.");
        var warehouseLocation = CityLocatorHelper.GetCityEntity(warehouse);

        var randomBuyerMarker = G.City.GetRandomMarker("buyer", predicate: marker => marker.PositionOnGraph != null);

        if(randomBuyerMarker == null)
        {
            Debug.LogError("No valid buyer marker found in the city.");
            return null;
        }

        var pos = randomBuyerMarker.PositionOnGraph.Value;
        Buyer buyer = new Buyer(car.Name + "_buyer", createInfoText(offer));
        CityEntitiesCreationHelper.CreateBuyer(buyer, pos);
        return buyer;
    }

    [Obsolete]
    private static string createInfoText(CarSellOffer offer)
    {
        return $"Interested in buying your {offer.Car.Name} for ${offer.InitialOfferPrice:F2}.";
    }

    public static void CreateRandomBuyers()
    {
        var markers = G.City.QueryMarkers("buyer").ToList();
        foreach (var marker in markers)
        {
            var pos = marker.PositionOnGraph;
            if (pos == null)
            {
                Debug.LogWarning($"Buyer marker {marker.Name} does not have a valid position on graph.");
                continue;
            }
            Buyer buyer = new Buyer(marker.Name, "Just a random buyer.");
            CityEntitiesCreationHelper.CreateBuyer(buyer, pos.Value);
        }
    }

    public static CityArea InitializeArea()
    {
        var buyersPositions = G.City.QueryMarkers("buyer").Where(m => m.PositionOnGraph != null).Select(m => m.PositionOnGraph.Value).ToList();
        var spawnPoints = buyersPositions.Select(pos => new BuyerSpawnPoint { Position = pos }).ToList();
        BuyersPool buyersPool = new BuyersPool(spawnPoints);
        CityArea area = new CityArea(buyersPool);
        G.Area = area;
        return area;
    }

    public static BuyerManager CreateManager()
    {
        var ob = new GameObject("BuyerManager");
        var manager = ob.AddComponent<BuyerManager>();

        return manager;
    }

    private void Awake()
    {
        G.BuyerManager = this;
    }

    private void Update()
    {
        UpdateArea(G.Area, Time.deltaTime);
    }

    void UpdateArea(CityArea area, float deltaTime)
    {
        var buyersPool = area.BuyersPool;
        Debug.Assert(buyersPool != null, "BuyersPool should not be null when updating area.");

        if (Time.time - buyersPool.lastSpawnTime >= buyersPool.minSpawnInterval && 
            Random.value <= probabilityForATry(buyersPool.averageSpawnInterval,deltaTime))
        {
            TrySpawnBuyerAtRandomFreeSpawnPoint(buyersPool);
        }
    }

    float probabilityForATry(float averageTime, float deltaTime)
    {
        return 1f - Mathf.Exp(-deltaTime / averageTime);
    }

    void TrySpawnBuyerAtRandomFreeSpawnPoint(BuyersPool pool)
    {
        var freeSpawnPoints = pool.SpawnPoints.Where(sp => sp.buyer == null).ToList();
        if (freeSpawnPoints.Count == 0)
        {
            return;
        }
        var randomSpawnPoint = freeSpawnPoints[UnityEngine.Random.Range(0, freeSpawnPoints.Count)];
        Buyer buyer = new Buyer($"Buyer_{Guid.NewGuid()}", "A new buyer has arrived.");
        SpawnBuyerAtPosition(randomSpawnPoint.Position, randomSpawnPoint);
        pool.lastSpawnTime = Time.time;
    }

    public void SpawnBuyerAtPosition(CityPosition position, BuyerSpawnPoint spawnPoint)
    {
        Buyer buyer = new Buyer($"Buyer_{Guid.NewGuid()}", "A new buyer has arrived.");
        CityEntitiesCreationHelper.CreateBuyer(buyer, position);

        buyer.onBeingDestroyed += onBuyerRemove;
        if(spawnPoint != null)
            spawnPoint.buyer = buyer;
    }

    void onBuyerRemove(IDestroyable buyer)
    {
        G.Area.BuyersPool.SpawnPoints.Where(sp => sp.buyer == buyer).ToList().ForEach(sp => sp.buyer = null);
    }
}

public class Buyer : CityDestroyable, ILocatable
{
    public readonly string Name;
    public readonly string InfoText;
    public Buyer(string name, string infoText)
    {
        this.Name = name;
        this.InfoText = infoText;
    }
}