using System.Linq;
using UnityEngine;

public class GlobalUIMethods : MonoBehaviour
{
    public void OpenCity()
    {
        G.GameFlowController.GetToTheCity();
    }

    public void StartGame()
    {
        G.GameFlowController.EnterWarehouse((Warehouse)
                    World.Instance.City.Locations.Keys.First(x => x.GetType() == typeof(Warehouse)));
    }
}