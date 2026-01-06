using Pixelplacement;
using UnityEngine;

public class CarShopSceneManager : Singleton<CarShopSceneManager>
{
    public void ExitCarShop()
    {
        var state = G.GameState as FreeRoamGameState;
        if (state == null)
        {
            Debug.LogError("ExitCarShop: GameState is not FreeRoamGameState");
            return;
        }

        var car = state.FocusedCar;
        if (car == null)
        {
            Debug.LogError("ExitCarShop: FocusedCar is null");
            return;
        }

        state.NotifyExitedWarehouse();
        G.Instance.CarMechanicService.RideCarFromWarehouse(car, WarehouseSceneManager.SceneWarehouseModel);
    }
}