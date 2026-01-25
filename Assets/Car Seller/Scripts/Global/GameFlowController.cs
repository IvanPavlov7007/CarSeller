
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
    public GameSceneType CurrentSceneType { get; private set; }
    public Warehouse CurrentWarehouse { get; private set; }

    [Serializable]
    public enum GameSceneType
    {
        None,
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
    public void EnterWarehouse(Warehouse warehouse)
    {
        if(CurrentSceneType == GameSceneType.Warehouse && CurrentWarehouse == warehouse)
            return;
        SceneManager.LoadScene(warehouse.Config.Name);
    }

    public void GetToTheCity()
    {
        if(CurrentSceneType == GameSceneType.City)
            return;
        SceneManager.LoadScene(World.Instance.City.Config.SceneToLoad);
    }

    public void SetWarehouse(Warehouse warehouse)
    {
        CurrentSceneType = GameSceneType.Warehouse;
        CurrentWarehouse = warehouse;
        G.InteractionManager = new WarehouseInteractionManager();
    }

    public void SetCity()
    {
        CurrentSceneType = GameSceneType.City;
        G.InteractionManager = new CityInteractionManager();
    }
}
