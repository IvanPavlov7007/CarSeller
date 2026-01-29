using System.Linq;
using UnityEngine;

public class GlobalUIMethods : MonoBehaviour
{
    public void OpenCity()
    {
        var car = G.GameState.FocusedCar;
        if (car == null || !CityLocatorHelper.IsInCity(car))
        {
            car =
            CityLocatorHelper.GetClosestCar(G.GameFlowController.CurrentWarehouse, out _);
        }

        G.GameState.FocusedCar = car;

        G.GameFlowController.EnterCity();
    }

    public void StartGame()
    {
        G.GameFlowController.EnterWarehouse((Warehouse)
                    World.Instance.City.Locations.Keys.First(x => x.GetType() == typeof(Warehouse)));
    }
}