using System.Linq;
using UnityEngine;

public class GlobalUIMethods : MonoBehaviour
{
    public void OpenCity()
    {
        //hackPickRandomCar();
        hackCreatePlayerFigure();
        G.GameFlowController.EnterCity();
    }

    private void hackCreatePlayerFigure()
    {
        Warehouse warehouse = G.GameFlowController.CurrentWarehouse;
        if(warehouse == null)
            warehouse = (Warehouse)
            World.Instance.City.Entities.Keys.First(x => x.GetType() == typeof(Warehouse));
        var figure = new PlayerFigure();
        CityEntitiesCreationHelper.CreatePlayerFigure(figure,
            CityLocatorHelper.GetCityEntity(warehouse).Position);
        G.GameFlowController.TryControlPlayerFigure(figure, out _);

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
                    World.Instance.City.Entities.Keys.First(x => x.GetType() == typeof(Warehouse)));
    }
}