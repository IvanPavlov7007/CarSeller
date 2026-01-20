using Pixelplacement;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[Serializable]
public enum GameConfigMode
{
    CarShop,
    CarSteal,
    DisassembleStolenCars
}

public class Main : Singleton<Main>
{
    [SerializeField] SimpleCarSpawnConfig carSpawnConfig;
    [SerializeField] Transform carSpawnPoint;
    [SerializeField] WarehouseConfig carshopWarehouseConfig;

    private IEnumerator Start()
    {
        var config = G.Instance.GameConfig;
        switch (config.GameConfigMode)
        {
            case GameConfigMode.CarShop:
                yield return carShop();
                break;
            case GameConfigMode.CarSteal:
                yield return carSteal();
                break;
            case GameConfigMode.DisassembleStolenCars:
                yield return disassembleStolenCarsMode();
                break;
            default:
                break;
        }
        
        yield return null;
    }

    public void OnCityInitialize()
    {
        switch (G.Config.GameConfigMode)
        {
            case GameConfigMode.CarShop:
                break;
            case GameConfigMode.CarSteal:
                CarSpawnManager.CheckAndRefill();
                break;
            case GameConfigMode.DisassembleStolenCars:
                CarSpawnManager.CheckAndRefill();
                break;
            default:
                break;
        }
    }


    //Defined and used in G 
    public class InstantMain
    {
        public void AfterWorldInitialize()
        {
            switch (G.Config.GameConfigMode)
            {
                case GameConfigMode.CarShop:
                    break;
                case GameConfigMode.CarSteal:
                    CarSpawnManager.NewCarsRotation();
                    break;
                case GameConfigMode.DisassembleStolenCars:
                    CarSpawnManager.NewCarsRotation();
                    break;
                default:
                    break;
            }
        }
    }

    private IEnumerator carSteal()
    {
        yield return null;
    }

    private IEnumerator disassembleStolenCarsMode()
    {
        Debug.Assert(carSpawnConfig != null, "CarSpawnConfig is not assigned in Main.");
        Debug.Assert(carSpawnPoint != null, "CarSpawnPoint is not assigned in Main.");

        if (carSpawnConfig == null || carSpawnPoint == null)
            yield break;

        var location = G.City.GetEmptyLocation(G.City.GetClosestPosition(carSpawnPoint.position));
        var car = carSpawnConfig.GenerateCar(location);

        var roamingState = new FreeRoamGameState(car);
        G.Instance.GameFlowController.SetGameState(roamingState);


        yield return null;
    }

    private IEnumerator carShop()
    {
        Debug.Assert(carSpawnConfig != null, "CarSpawnConfig is not assigned in Main.");
        Debug.Assert(carSpawnPoint != null, "CarSpawnPoint is not assigned in Main.");

        if (carSpawnConfig == null || carSpawnPoint == null)
            yield break;

        var location = G.City.GetEmptyLocation(G.City.GetClosestPosition(carSpawnPoint.position));
        var car = carSpawnConfig.GenerateCar(location);

        var roamingState = new FreeRoamGameState(car);
        G.Instance.GameFlowController.SetGameState(roamingState);

        //CollectablesManager.Instance.Initialize(locations, 900f, null, 20);
        //PoliceManager.Instance.CreatePolice();
        //tryEnterCarShop(car);


        yield return null;
    }

    void tryEnterCarShop(Car car)
    {
        var carShopWarehouse = World.Instance.WorldRegistry.GetByConfig<Warehouse>(carshopWarehouseConfig)?.First();
        if (carShopWarehouse == null)
        {
            Debug.LogError("CarShop warehouse not found in WorldRegistry.");
            return;
        }
        if (G.Instance.CityActionService.PutCarInsideWarehouse
            (car, carShopWarehouse))
        {
            G.Instance.GameFlowController.EnterWarehouse(carShopWarehouse);
        }
        else
            Debug.LogError("Failed to put car inside carshop warehouse.");
    }
}