using System.Linq;
using UnityEngine;

public class GlobalUIMethods : MonoBehaviour
{
    public void OpenCity()
    {
        hackPickRandomCar();
        G.GameFlowController.EnterCity();
    }

    /// <summary>
    /// Don't use this method in production code!
    /// </summary>
    private void hackPickRandomCar()
    {
        var car = G.GameState.FocusedCar;
        if (car == null || !CityLocatorHelper.IsInCity(car))
        {
            car =
            CityLocatorHelper.GetClosestCar(G.GameFlowController.CurrentWarehouse, out _);
        }

        G.GameFlowController.TryDriveCar(car, out _);
    }

    public void StartGame()
    {
        G.GameFlowController.EnterWarehouse((Warehouse)
                    World.Instance.City.Locations.Keys.First(x => x.GetType() == typeof(Warehouse)));
    }
}