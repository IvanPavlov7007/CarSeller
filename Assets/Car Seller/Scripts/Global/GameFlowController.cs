
using Pixelplacement;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Methods to control the game flow, holding the current game state
/// </summary>
public class GameFlowController
{
    public GameState GameState { get; private set; } = new NeutralGameState(null);
    public GameSceneType currentSceneType { get; private set; }

    
    [Serializable]
    public enum GameSceneType
    {
        City,
        Warehouse
    }

    #region GameState
    // Controlling GameState
    public void SetGameState(GameState newState)
    {
        var oldState = GameState;
        GameState = newState;
        Debug.Log($"GameFlowController: Game state changed from {oldState.GetType().Name} to {newState.GetType().Name}");
        GameEvents.Instance.OnGameStateChanged?.Invoke(new GameStateChangeEventData(oldState, newState));
    }

    #endregion

    /*
     * Game Logic Initialization - 2 ways:
     * 
     * 1) EnterWarehouse(..) or GetToTheCity(..) load respective logics automatically. 
     * Used for initialization from a not involved point like Intro Scene
     * 
     * 2) Initialize(..) When the game starts directly in a specific game scene
     * 
     */
    public void EnterWarehouse(Warehouse warehouse)
    {
        if(currentSceneType == GameSceneType.Warehouse && WarehouseSceneManager.SceneWarehouseModel == warehouse)
            return;
        setWarehouse(warehouse);
        SceneManager.LoadScene(warehouse.Config.Name);
    }

    public void GetToTheCity()
    {
        if(currentSceneType == GameSceneType.City)
            return;
        setCity();
        SceneManager.LoadScene(World.Instance.City.Config.SceneToLoad);
    }

    /// <summary>
    /// is called by G
    /// </summary>
    /// <param name="sceneEntrancePoint"></param>
    internal void Initialize(SceneEntrancePoint sceneEntrancePoint)
    {
        Debug.Assert(sceneEntrancePoint != null);

        switch (sceneEntrancePoint.gameSceneType)
        {
            case GameSceneType.City:
                setCity();
                break;
            case GameSceneType.Warehouse:
                var name = sceneEntrancePoint.specificName();
                var warehouse = World.Instance.WorldRegistry.GetByName<Warehouse>(name);
                Debug.Assert(warehouse != null, $"GameFlowController.Initialize: Warehouse with id {name} not found!");
                setWarehouse(warehouse);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void setWarehouse(Warehouse warehouse)
    {
        currentSceneType = GameSceneType.Warehouse;
        WarehouseSceneManager.SceneWarehouseModel = warehouse;
        G.Instance.InteractionManager = new WarehouseInteractionManager();
    }

    private void setCity()
    {
        currentSceneType = GameSceneType.City;
        G.Instance.InteractionManager = new CityInteractionManager();
    }
}
