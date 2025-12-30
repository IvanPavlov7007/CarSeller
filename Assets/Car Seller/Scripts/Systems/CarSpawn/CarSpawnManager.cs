using Pixelplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public static class CarSpawnManager
{

    //ACHTUNG, static list might not be cleared on domain reloads
    static List<Car> temporaryCars = new List<Car>();
    static Dictionary<Car, City.CityMarker> usedMarkers = new Dictionary<Car, City.CityMarker>();


    static int nextCarsToHaveMinCount;
    static int nextCarsToSpawnCount;

    static CarSpawnConfig carSpawnConfig => G.Economy.Config.CarSpawnConfig;

    public static void ReleaseCar(Car car)
    {
        temporaryCars.Remove(car);
        usedMarkers.Remove(car);
    }

    public static void NewCarsRotation()
    {
        pickCarsCount();
        RemoveTemporaryCars();
        SpawnTemporaryCars();
    }

    public static void CheckAndRefill()
    {
        Debug.Assert(nextCarsToHaveMinCount > 0,
            "CarSpawnManager not initialized properly. Make sure NewCarsRotation() is called at least once before CheckAndRefill().");
        if (temporaryCars.Count <  nextCarsToHaveMinCount)
        {
            refill();
            pickCarsCount();
        }
    }

    private static void refill()
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

    private static void pickCarsCount()
    {
        nextCarsToHaveMinCount = Random.Range(carSpawnConfig.CarsToHaveMinCount, carSpawnConfig.CarsToSpawnMaxCount);
        nextCarsToSpawnCount = Random.Range(nextCarsToHaveMinCount + 1, carSpawnConfig.CarsToSpawnMaxCount + 1);
    }

    private static List<City.CityMarker> getCarSpawnMarkers()
    {
        return G.City.QueryMarkers("car").ToList();
    }

    private static void SpawnTemporaryCars()
    {

        var markers = getCarSpawnMarkers();

        //shuffle markers
        markers.Shuffle();

        spawnCarsAtMarkers(markers.Take(nextCarsToSpawnCount).ToList());
    }

    /// <summary>
    /// Old method, spawns cars from provided entries at provided markers
    /// </summary>
    /// <param name="markers"></param>
    /// <param name="carSpawnEntries"></param>
    private static void spawnCarsAtMarkers(List<City.CityMarker> markers, CarSpawnConfig.CarSpawnEntry[] carSpawnEntries)
    {
        Debug.Assert(markers.Count >= carSpawnEntries.Length,
            $"Not enough car markers in the city to spawn {carSpawnEntries.Length} cars. Found only {markers.Count} markers.");

        for (int i = 0; i < carSpawnEntries.Length; i++)
        {
            var carSpawnEntry = carSpawnEntries[i];
            var randomMarker = markers[i];
            var location = G.City.GetEmptyLocation(randomMarker.PositionOnGraph.Value);
            var car = generateCar(location, carSpawnEntry);

            temporaryCars.Add(car);
            usedMarkers.Add(car, randomMarker);
        }
    }

    private static void spawnCarsAtMarkers(List<City.CityMarker> markers)
    {
        foreach (var marker in markers)
        {
            var carSpawnEntry = carSpawnConfig.GetWeightedRandomCarForRegion(marker.RegionId);
            var location = G.City.GetEmptyLocation(marker.PositionOnGraph.Value);
            var car = generateCar(location, carSpawnEntry);

            temporaryCars.Add(car);
            usedMarkers.Add(car, marker);
        }
    }

    private static Car generateCar(City.CityLocation location, CarSpawnConfig.CarSpawnEntry carSpawnEntry)
    {
        Car car = G.Instance.ProductManager.CreateCar(
                    carSpawnEntry.CarBaseConfig,
                    carSpawnEntry.CarVariantConfig,
                    location);
        return car;
    }

    private static void RemoveTemporaryCars()
    {
        foreach (var car in temporaryCars)
        {
            //TODO check that the are no leftovers in some registries

            ProductDeletionService.DeleteProduct(car);
        }
        temporaryCars.Clear();
        usedMarkers.Clear();
    }
}