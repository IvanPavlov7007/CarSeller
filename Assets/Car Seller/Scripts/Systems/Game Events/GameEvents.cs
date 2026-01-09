using System.Collections;
using UnityEngine;
using Pixelplacement;
using System;

public abstract class GameEventData { }

public class GameEvents
{
    public static GameEvents Instance = new GameEvents();

    public void Reset()
    {
        OnGamePaused = null;
        OnGameUnpaused = null;
        OnProductLocationChanged = null;
        OnProductCreated = null;
        OnProductDestroyed = null;
        OnGameStateChanged = null;

        OnPlayerCancel = null;
        OnPlayerCaught = null;
        OnPlayerSucceed = null;
    }

    public Action OnGamePaused;
    public Action OnGameUnpaused;

    public Action<GameStateChangeEventData> OnGameStateChanged;

    public Action<ProductLocationChangedEventData> OnProductLocationChanged;
    public Action<ProductCreatedEventData> OnProductCreated;
    public Action<ProductDestroyedEventData> OnProductDestroyed;

    public Action<LocatableStateChangedEventData> OnLocatableStateChanged;
    public Action<LocatableCreatedEventData> OnLocatableCreated;
    public Action<LocatableDestroyedEventData> OnLocatableDestroyed;
    public Action<LocatableLocationChangedEventData> OnLocatableLocationChanged;

    public Action<PossesionChangeEventData> OnPlayerPossessionLose;
    public Action<PossesionChangeEventData> OnPlayerPossessionAcquired;
    public Action<PlayerMoneyChangeEventData> OnPlayerMoneyChanged;

    public Action<TransactionEventData> OnTransactionComplete;

    // Game Flow Events
    public Action<PlayerActionEventData> OnPlayerCancel;
    public Action<PlayerActionEventData> OnPlayerCaught;
    public Action<PlayerActionEventData> OnPlayerSucceed;

    
}