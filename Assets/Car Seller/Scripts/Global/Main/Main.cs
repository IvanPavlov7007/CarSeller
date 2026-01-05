using Pixelplacement;
using System.Collections;
using UnityEngine;

public class Main : Singleton<Main>
{
    [SerializeField] SimpleCarSpawnConfig carSpawnConfig;
    [SerializeField] Transform carSpawnPoint;

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
    }
}