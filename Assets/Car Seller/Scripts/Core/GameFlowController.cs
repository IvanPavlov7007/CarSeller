
using Pixelplacement;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameFlowController
{
    public void EnterWarehouse(Warehouse warehouse)
    {
        WarehouseSceneManager.SceneWarehouseModel = warehouse;
        G.Instance.InteractionManager = new WarehouseInteractionManager();
        SceneManager.LoadScene(warehouse.Config.SceneToLoad);
    }

    public void GetToTheCity()
    {
        G.Instance.InteractionManager = new CityInteractionManager();
        SceneManager.LoadScene(World.Instance.City.Config.SceneToLoad);
    }
}