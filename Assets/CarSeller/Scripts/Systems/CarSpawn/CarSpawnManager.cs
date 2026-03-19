using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class CarSpawnManager
{
    readonly List<Car> temporaryCars = new List<Car>();
    readonly Dictionary<Car, City.CityMarker> usedMarkers = new Dictionary<Car, City.CityMarker>();

    // Cars spawned by SpatialGrid activation (NOT part of rotation cars)
    readonly HashSet<Car> cellSpawnedCars = new HashSet<Car>();
    readonly Dictionary<City.CityMarker, Car> markerToCellCar = new Dictionary<City.CityMarker, Car>();

    int nextCarsToHaveMinCount;
    int nextCarsToSpawnCount;

    //CarSpawnConfig carSpawnConfig => G.Economy.Config.CarSpawnConfig;

    bool subscribed;
    private int CarsToHaveMinCount = 10;
    private int CarsToSpawnMaxCount = 20;

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

        // If this car was spawned by a marker, free the marker mapping first.
        if (usedMarkers.TryGetValue(car, out City.CityMarker marker) && marker != null)
        {
            if (markerToCellCar.TryGetValue(marker, out Car mappedCar) && mappedCar == car)
            {
                markerToCellCar.Remove(marker);
            }
        }

        temporaryCars.Remove(car);
        usedMarkers.Remove(car);
        cellSpawnedCars.Remove(car);
    }

    public void activeCellsUpdated(UpdatedActiveCellsData data, CellWrapperManager cellWrapperManager)
    {
        if (data == null || !data.HasChanges)
            return;

        foreach (var cell in data.NewActiveCells)
        {
            populateCell(cellWrapperManager.GetCarSpawnWrapper(cell));
        }

        foreach (var cell in data.DisactivatedCells)
        {
            clearCell(cellWrapperManager.GetCarSpawnWrapper(cell));
        }
    }

    void populateCell(CarSpawnCellWrapper cellWrapper)
    {
        var markers = cellWrapper.GetMarkers().ToList();

        // Only use markers that are not already occupied by a cell-spawned car.
        markers.RemoveAll(m => m == null || markerToCellCar.ContainsKey(m));

        int desiredCarsCount = Mathf.Min(Mathf.FloorToInt(cellWrapper.density), markers.Count);
        if (desiredCarsCount <= 0)
            return;

        markers.Shuffle();

        for (int i = 0; i < desiredCarsCount; i++)
        {
            var marker = markers[i];
            if (marker.PositionOnGraph == null)
                continue;

            spawnCellCarAtPosition(marker.PositionOnGraph.Value, marker);
        }
    }

    void clearCell(CarSpawnCellWrapper cellWrapper)
    {
        // Despawn ONLY cars spawned by the cell activation system.
        foreach (var carEntity in cellWrapper.GetCars())
        {
            if (carEntity?.Subject is Car car && cellSpawnedCars.Contains(car))
            {
                DespawnCar(carEntity);
            }
        }
    }

    void spawnCellCarAtPosition(CityPosition position, City.CityMarker marker)
    {
        var car = G.SimplifiedCarsManager.CreateCarHidden(G.SimplifiedCarsCreationBuilder.RuntimeConfigs.Keys.First());
        CityEntitiesCreationHelper.MoveInExistingCar(car, position);

        cellSpawnedCars.Add(car);

        if (marker != null)
        {
            usedMarkers[car] = marker;
            markerToCellCar[marker] = car;
        }
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
        nextCarsToHaveMinCount = Random.Range(CarsToHaveMinCount, CarsToSpawnMaxCount);
        nextCarsToSpawnCount = Random.Range(nextCarsToHaveMinCount + 1, CarsToSpawnMaxCount + 1);
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

    void spawnCarsAtMarkers(List<City.CityMarker> markers)
    {
        foreach (var marker in markers)
        {
            if (marker.PositionOnGraph == null)
            {
                Debug.LogWarning($"Marker {marker.Id} has no position on graph, skipping car spawn.");
                continue;
            }

            SpawnCarAtPosition(marker.PositionOnGraph.Value, marker);
        }
    }

    // Rotation/temp cars (legacy path)
    public void SpawnCarAtPosition(CityPosition position, City.CityMarker marker)
    {
        var car = G.SimplifiedCarsManager.CreateCarHidden(G.SimplifiedCarsCreationBuilder.RuntimeConfigs.Keys.First());
        CityEntitiesCreationHelper.MoveInExistingCar(car, position);

        temporaryCars.Add(car);

        if (marker != null)
        {
            usedMarkers[car] = marker;
        }
    }

    void RemoveTemporaryCars()
    {
        foreach (var car in temporaryCars)
        {
            G.ProductLifetimeService.DestroyProduct(car);
        }

        temporaryCars.Clear();

        // Note: usedMarkers for temporary cars will be cleaned via OnProductDestroyed -> ReleaseCar.
        // Keeping usedMarkers here avoids losing marker mapping for cellSpawnedCars.
    }

    void OnProductDestroyed(ProductDestroyedEventData eventData)
    {
        if (eventData?.Product is Car car)
        {
            ReleaseCar(car);
        }
    }

    internal void DespawnCar(CityEntity car)
    {
        G.CityEntityLifetimeService.Destroy(car);
    }
}