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

    public bool TryControlPlayerFigure(PlayerFigure figure, out string reason)
    {
        var resolution = _carActionResolver.ResolveControlPlayerFigure(GameState, figure);
        if (!resolution.Allowed)
        {
            reason = resolution.Reason;
            return false;
        }

        reason = null;
        SetGameState(resolution.NextState);
        return true;
    }

    public bool TryDriveIntoCarFromPlayerFigure(Car car, out string reason)
    {
        if (car == null)
        {
            reason = "Car is null";
            return false;
        }

        var state = GameState;
        if (state?.PlayerFigure == null)
        {
            reason = "No PlayerFigure is currently controlled";
            return false;
        }

        // Remove player figure from the city (if it was spawned as a city entity).
        City.EntityLifetimeService.Destroy(state.PlayerFigure);

        // Now just drive the car as usual.
        return TryDriveCar(car, out reason);
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
        if (currentState.FocusedCar == targetCar && currentState.PlayerFigure == null)
            return CarActionResolution.Deny("Already driving this car");

        // Preserve PlayerFigure if present (some flows may want to keep it; but driving a car implies no figure).
        // The actual figure destruction is handled by GameFlowController.TryDriveIntoCarFromPlayerFigure.

        switch (currentState)
        {
            case FreeRoamGameState:
                return CarActionResolution.Allow(new FreeRoamGameState(targetCar));

            case MissionGameState mission:
                // keep mission, just swap focus
                return CarActionResolution.Allow(new MissionGameState(targetCar, mission.CurrentMission));

            case SellingGameState selling:
                return CarActionResolution.Allow(new SellingGameState(targetCar, selling.Buyer, selling.SellingWarehouse));

            case StealingGameState:
                return CarActionResolution.Allow(new FreeRoamGameState(targetCar));

            default:
                // fallback: preserve "drive" intent by moving into FreeRoam
                return CarActionResolution.Allow(new FreeRoamGameState(targetCar));
        }
    }

    public CarActionResolution ResolveControlPlayerFigure(GameState currentState, PlayerFigure figure)
    {
        if (figure == null)
            return CarActionResolution.Deny("PlayerFigure is null");

        // When controlling a player figure we keep focused car null (or whatever preserves your state type).
        // For now, default to FreeRoam.
        if (currentState == null)
            return CarActionResolution.Allow(new FreeRoamGameState(null, figure));

        switch (currentState)
        {
            case FreeRoamGameState:
                return CarActionResolution.Allow(new FreeRoamGameState(null, figure));

            case MissionGameState mission:
                return CarActionResolution.Allow(new MissionGameState(null, mission.CurrentMission, figure));

            case SellingGameState selling:
                return CarActionResolution.Allow(new SellingGameState(null, selling.Buyer, selling.SellingWarehouse, figure));

            case StealingGameState:
                return CarActionResolution.Allow(new FreeRoamGameState(null, figure));

            default:
                return CarActionResolution.Allow(new FreeRoamGameState(null, figure));
        }
    }
}