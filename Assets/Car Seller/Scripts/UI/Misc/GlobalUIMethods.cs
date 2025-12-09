using System.Linq;
using UnityEngine;

public class GlobalUIMethods : MonoBehaviour
{
    public void OpenCity()
    {
        G.Instance.GameFlowController.GetToTheCity();
    }

    public void StartGame()
    {
        G.Instance.GameFlowController.EnterWarehouse((Warehouse)
                    World.Instance.City.Locations.Keys.First(x => x.GetType() == typeof(Warehouse)));
    }
}