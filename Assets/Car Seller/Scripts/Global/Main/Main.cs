using Pixelplacement;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Main : Singleton<Main>
{
    [SerializeField] SimpleCarSpawnConfig carSpawnConfig;
    [SerializeField] Transform carSpawnPoint;
    [SerializeField] WarehouseConfig carshopWarehouseConfig;

    private IEnumerator Start()
    {
        yield return testRoamingState();
    }

    private IEnumerator testRoamingState()
    {
        Debug.Assert(carSpawnConfig != null, "CarSpawnConfig is not assigned in Main.");
        Debug.Assert(carSpawnPoint != null, "CarSpawnPoint is not assigned in Main.");

        if (carSpawnConfig == null || carSpawnPoint == null)
            yield break;

        var location = G.City.GetEmptyLocation(G.City.GetClosestPosition(carSpawnPoint.position));
        var car = carSpawnConfig.GenerateCar(location);

        var roamingState = new FreeRoamGameState(car);
        G.Instance.GameFlowController.SetGameState(roamingState);

        var locations = G.City.QueryMarkers("cash")
            .Select(marker => G.City.GetEmptyLocation(marker.PositionOnGraph.Value) as ILocation).ToList();

        //CollectablesManager.Instance.Initialize(locations, 900f, null, 20);
        PoliceManager.Instance.CreatePolice();
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