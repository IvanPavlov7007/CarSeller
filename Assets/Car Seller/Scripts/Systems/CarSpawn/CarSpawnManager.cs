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
        
        var carSpawnConfigs = G.Instance.Economy.Config.CarSpawnConfigs;
        foreach (var carSpawnConfig in carSpawnConfigs)
        {
            for (int i = 0; i < carSpawnConfig.TemporaryCarsCount; i++)
            {
                var randomMarker = G.City.GetRandomMarker(carSpawnConfig.MarkerTag);
                var location = G.City.GetEmptyLocation(randomMarker.PositionOnGraph.Value);
                Car car = G.Instance.ProductManager.CreateCar(
                    carSpawnConfig.CarBaseConfig,
                    carSpawnConfig.CarVariantConfig,
                    location);
                temporaryCars.Add(car);
            }
        }
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