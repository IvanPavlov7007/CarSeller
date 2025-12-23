using Pixelplacement;
using System;
using System.Collections.Generic;

public static class CarSpawnManager
{

    //ACHTUNG, static list might not be cleared on domain reloads
    static List<Car> temporaryCars = new List<Car>();
    public static void ReleaseCar(Car car)
    {
        temporaryCars.Remove(car);
    }

    public static void NewCarsRotation()
    {
        RemoveTemporaryCars();
        SpawnTemporaryCars();
    }

    private static void SpawnTemporaryCars()
    {
        
        var carSpawnConfig = G.Economy.Config.CarSpawnConfig;
        foreach (var carSpawnEntry in carSpawnConfig.GetRandomCarSpawnEntriesWithPuttingBack())
        {
            var randomMarker = G.City.GetRandomMarker("car");
            var location = G.City.GetEmptyLocation(randomMarker.PositionOnGraph.Value);
            var car = generateCar(location, carSpawnEntry);
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
    }
}