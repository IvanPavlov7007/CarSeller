using Pixelplacement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class CarSpawnManager
{
    readonly List<Car> temporaryCars = new List<Car>();
    readonly Dictionary<Car, City.CityMarker> usedMarkers = new Dictionary<Car, City.CityMarker>();

    int nextCarsToHaveMinCount;
    int nextCarsToSpawnCount;

    CarSpawnConfig carSpawnConfig => G.Economy.Config.CarSpawnConfig;

    bool subscribed;

    public void SubscribeToEvents()
    {
        if (subscribed)
            return;

        GameEvents.Instance.OnProductDestroyed += OnProductDestroyed;
        subscribed = true;
    }

    public void UnsubscribeFromEvents()
    {
        if (!subscribed)
            return;

        GameEvents.Instance.OnProductDestroyed -= OnProductDestroyed;
        subscribed = false;
    }

    public void ReleaseCar(Car car)
    {
        if (car == null)
            return;

        temporaryCars.Remove(car);
        usedMarkers.Remove(car);
    }

    public void NewCarsRotation()
    {
        pickCarsCount();
        RemoveTemporaryCars();
        SpawnTemporaryCars();
    }

    public void CheckAndRefill()
    {
        Debug.Assert(nextCarsToHaveMinCount > 0,
            "CarSpawnManager not initialized properly. Make sure NewCarsRotation() is called at least once before CheckAndRefill().");

        if (temporaryCars.Count < nextCarsToHaveMinCount)
        {
            refill();
            pickCarsCount();
        }
    }

    void refill()
    {
        var markers = getCarSpawnMarkers();
        markers.RemoveAll(m => usedMarkers.Values.Contains(m));

        Debug.Assert(markers.Count >= nextCarsToSpawnCount - temporaryCars.Count,
            $"Not enough car markers in the city to spawn {nextCarsToSpawnCount - temporaryCars.Count} " +
            $"cars. Found only {markers.Count} available markers.");

        markers.Shuffle();

        int carsToSpawn = nextCarsToSpawnCount - temporaryCars.Count;

        Debug.Assert(carsToSpawn > 0);
        Debug.Assert(carsToSpawn <= markers.Count);

        spawnCarsAtMarkers(markers.Take(carsToSpawn).ToList());
    }

    void pickCarsCount()
    {
        nextCarsToHaveMinCount = Random.Range(carSpawnConfig.CarsToHaveMinCount, carSpawnConfig.CarsToSpawnMaxCount);
        nextCarsToSpawnCount = Random.Range(nextCarsToHaveMinCount + 1, carSpawnConfig.CarsToSpawnMaxCount + 1);
    }

    List<City.CityMarker> getCarSpawnMarkers()
    {
        return G.City.QueryMarkers("car").ToList();
    }

    void SpawnTemporaryCars()
    {
        var markers = getCarSpawnMarkers();

        // shuffle markers
        markers.Shuffle();

        spawnCarsAtMarkers(markers.Take(nextCarsToSpawnCount).ToList());
    }

    /// <summary>
    /// Old method, spawns cars from provided entries at provided markers
    /// </summary>
    void spawnCarsAtMarkers(List<City.CityMarker> markers, CarSpawnConfig.CarSpawnEntry[] carSpawnEntries)
    {
        Debug.Assert(markers.Count >= carSpawnEntries.Length,
            $"Not enough car markers in the city to spawn {carSpawnEntries.Length} cars. Found only {markers.Count} markers.");

        for (int i = 0; i < carSpawnEntries.Length; i++)
        {
            var carSpawnEntry = carSpawnEntries[i];
            var randomMarker = markers[i];

            if(randomMarker.PositionOnGraph == null)
            {
                Debug.LogWarning($"Marker {randomMarker.Id} has no position on graph, skipping car spawn.");
                continue;
            }
            var car = generateCar(randomMarker.PositionOnGraph.Value, carSpawnEntry);

            temporaryCars.Add(car);
            usedMarkers.Add(car, randomMarker);
        }
    }

    void spawnCarsAtMarkers(List<City.CityMarker> markers)
    {
        foreach (var marker in markers)
        {
            var carSpawnEntry = carSpawnConfig.GetWeightedRandomCarForRegion(marker.RegionId);

            if (marker.PositionOnGraph == null)
            {
                Debug.LogWarning($"Marker {marker.Id} has no position on graph, skipping car spawn.");
                continue;
            }
            var car = generateCar(marker.PositionOnGraph.Value, carSpawnEntry);

            temporaryCars.Add(car);
            usedMarkers.Add(car, marker);
        }
    }

    Car generateCar(CityPosition position, CarSpawnConfig.CarSpawnEntry carSpawnEntry)
    {
        var entity = CityEntitiesCreationHelper.CreateNewCar(
            carSpawnEntry.CarBaseConfig,
            carSpawnEntry.CarVariantConfig,
            position);
        return entity.Subject as Car;
    }

    void RemoveTemporaryCars()
    {
        foreach (var car in temporaryCars)
        {
            // TODO check that the are no leftovers in some registries
            G.ProductLifetimeService.DestroyProduct(car);
        }
        temporaryCars.Clear();
        usedMarkers.Clear();
    }

    void OnProductDestroyed(ProductDestroyedEventData eventData)
    {
        if (eventData?.Product is Car car)
        {
            ReleaseCar(car);
        }
    }
}