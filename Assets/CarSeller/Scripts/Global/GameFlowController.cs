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

    readonly CarActionResolver _carActionResolver = new CarActionResolver();

    [Serializable]
    public enum GameSceneType
    {
        None,
        City,
        Warehouse
    }

    #region GameState
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
        if (CurrentSceneType == GameSceneType.Warehouse && CurrentWarehouse == warehouse)
            return;
        SceneManager.LoadScene(warehouse.Config.Name);
    }

    public void EnterCity()
    {
        if (CurrentSceneType == GameSceneType.City)
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

    public bool TryDriveCar(Car car, out string reason)
    {
        var resolution = _carActionResolver.ResolveDrive(GameState, car);
        if (!resolution.Allowed)
        {
            reason = resolution.Reason;
            return false;
        }

        reason = null;
        SetGameState(resolution.NextState);
        return true;
    }
}

public readonly struct CarActionResolution
{
    public bool Allowed { get; }
    public string Reason { get; }
    public GameState NextState { get; }

    public CarActionResolution(bool allowed, GameState nextState, string reason)
    {
        Allowed = allowed;
        NextState = nextState;
        Reason = reason;
    }

    public static CarActionResolution Deny(string reason) => new CarActionResolution(false, null, reason);
    public static CarActionResolution Allow(GameState nextState) => new CarActionResolution(true, nextState, null);
}

public sealed class CarActionResolver
{
    public CarActionResolution ResolveDrive(GameState currentState, Car targetCar)
    {
        if (targetCar == null)
            return CarActionResolution.Deny("Car is null");

        // Example constraints (adjust to your rules):
        // if (!GameRules.CanDriveCar.Check(targetCar)) return Deny(...);
        // if (!CityLocatorHelper.IsInCity(targetCar)) return Deny("Car is not in city");

        if (currentState == null)
            return CarActionResolution.Allow(new FreeRoamGameState(targetCar));

        // If already focused, nothing to do (treat as deny or allow/no-op)
        if (currentState.FocusedCar == targetCar)
            return CarActionResolution.Deny("Already driving this car");

        switch (currentState)
        {
            case FreeRoamGameState:
                return CarActionResolution.Allow(new FreeRoamGameState(targetCar));

            case MissionGameState mission:
                // keep mission, just swap focus
                return CarActionResolution.Allow(new MissionGameState(targetCar, mission.CurrentMission));

            default:
                // fallback: preserve "drive" intent by moving into FreeRoam
                return CarActionResolution.Allow(new FreeRoamGameState(targetCar));
        }
    }
}