using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        GameMainResolver gameResolver = new GameMainResolver();
        Instance = gameResolver.Resolve(GameMainConfig.Instance.GameConfig);
        Instance.Initialize(GameMainConfig.Instance.GameConfig);
        SceneManager.sceneLoaded += Instance.onSceneLoaded;
    }

    public virtual void Initialize(GameConfig gameConfig)
    {
        ResetStaticState();
        InitializeWorld(gameConfig);
        InitializeLogic(gameConfig);
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
        Debug.Assert(SceneEntrancePoint.Instance != null, "SceneEntrancePoint instance is null on scene loaded.");
        
        if (sceneMain != null)
            sceneMain.Exit();

        var entryPoint = SceneEntrancePoint.Instance;
        sceneMain = sceneMainResolver.Resolve(entryPoint);
        
        sceneMain.Enter();

        AfterSceneLoad(sceneMain);
    }
}