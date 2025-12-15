
using Pixelplacement;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameFlowController
{
    G G=> G.Instance;

    GameSceneType currentSceneType;
    public enum GameSceneType
    {
        City,
        Warehouse
    }

    public void SetGameState(GameState newState)
    {
        var oldState = G.GameState;
        G.GameState = newState;
        GameEvents.Instance.OnGameStateChanged?.Invoke(new GameStateChangeEventData(oldState, newState));
    }

    public void EnterWarehouse(Warehouse warehouse)
    {
        if(currentSceneType == GameSceneType.Warehouse && WarehouseSceneManager.SceneWarehouseModel == warehouse)
            return;
        currentSceneType = GameSceneType.Warehouse;

        WarehouseSceneManager.SceneWarehouseModel = warehouse;
        G.Instance.InteractionManager = new WarehouseInteractionManager();
        SceneManager.LoadScene(warehouse.Config.SceneToLoad);
    }

    public void GetToTheCity()
    {
        if(currentSceneType == GameSceneType.City)
            return;
        currentSceneType = GameSceneType.City;
        G.Instance.InteractionManager = new CityInteractionManager();
        SceneManager.LoadScene(World.Instance.City.Config.SceneToLoad);
    }
}
