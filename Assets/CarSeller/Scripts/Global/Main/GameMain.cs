using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameFlowController;

[Serializable]
public enum GameConfigMode
{
    CarShop,
    CarSteal,
    DisassembleStolenCars,
    SimplifiedGameplay
}

//TODO use strategy pattern to avoid large switch statements in resolvers
// Apply dictionaries or reflection where possible

/// <summary>
/// GameMains are the highest level game controllers, initializing world and game logic according to the selected game mode
/// don't talk to them from anywhere else!
/// </summary>
public abstract partial class GameMain
{
    static GameMain Instance;

    SceneMainResolver sceneMainResolver = new SceneMainResolver();
    ISceneMain sceneMain;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Main()
    {
        if (Instance != null)
            Instance.Destroy();

        GameMainResolver gameResolver = new GameMainResolver();
        Instance = gameResolver.Resolve(GameMainConfig.Instance.GameConfig);
        Instance.Initialize(GameMainConfig.Instance.GameConfig);
    }

    public virtual void Initialize(GameConfig gameConfig)
    {
        ResetStaticState();
        ResetData(gameConfig);
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    public void ResetData(GameConfig gameConfig)
    {
        InitializeWorld(gameConfig);
        InitializeLogic(gameConfig);
    }

    public static void GameReset()
    {
        Instance.ResetData(GameMainConfig.Instance.GameConfig);
    }

    public virtual void Destroy()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    public virtual void ResetStaticState()
    {
        GameEvents.Instance.Reset();
        G.Initialize(GameMainConfig.Instance.ViewBuilders);
    }
    public virtual void InitializeWorld(GameConfig gameConfig)
    {
        G.WorldManager.InitializeWorld(gameConfig.CityConfig, gameConfig.EconomyConfig, gameConfig.WorldMissionsConfig);
    }
    public virtual void InitializeLogic(GameConfig gameConfig)
    {

    }

    public virtual void AfterSceneLoad(ISceneMain sceneMain)
    {

    }

    void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Assert(scene != null);

        if (sceneMain != null)
            sceneMain.Exit();

        var entryPoint = GameObject.FindAnyObjectByType<SceneEntrancePoint>();
        if(entryPoint == null)
            sceneMain = null;
        else
            sceneMain = sceneMainResolver.Resolve(entryPoint);

        if (sceneMain != null)
        {

            sceneMain.Enter();
            AfterSceneLoad(sceneMain);
        }
    }
}