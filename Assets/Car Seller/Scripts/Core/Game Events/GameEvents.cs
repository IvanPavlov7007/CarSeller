using System.Collections;
using UnityEngine;
using Pixelplacement;
using System;

public class GameEvents
{
    public static GameEvents Instance = new GameEvents();

    public void Reset()
    {
        OnGamePaused = null;
        OnGameUnpaused = null;
        OnObjectWithPopupClicked = null;
        OnProductLocationChanged = null;
        OnProductCreated = null;
        OnProductDestroyed = null;
    }


    public Action OnGamePaused;
    public Action OnGameUnpaused;

    public Action<ObjectWithPopupMenuTest> OnObjectWithPopupClicked;


    public Action<ProductLocationChangedEventData> OnProductLocationChanged;
    public Action<ProductCreatedEventData> OnProductCreated;
    public Action<ProductDestroyedEventData> OnProductDestroyed;
}