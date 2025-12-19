
using Pixelplacement;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Methods to control the game flow, holding the current game state
/// </summary>
public class GameFlowController
{
    public GameState GameState = new NeutralGameState();
    GameSceneType currentSceneType;
    public enum GameSceneType
    {
        City,
        Warehouse
    }

    public void SetGameState(GameState newState)
    {
        var oldState = GameState;
        GameState = newState;
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
