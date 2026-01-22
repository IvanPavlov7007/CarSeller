using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// global services locator
/// </summary>
public static class G
{

    public static GameConfig Config => GameMainConfig.Instance.GameConfig;
    public static CityConfig CityConfig => Config.CityConfig;
    public static EconomyConfig EconomyConfig => Config.EconomyConfig;
    public static WorldMissionsConfig WorldMissionsConfig => Config.WorldMissionsConfig;

    //References
    public static City City => World.Instance.City;
    public static Economy Economy => World.Instance.Economy;
    public static Player Player => World.Instance.Economy.Player;
    public static GameState GameState => GameFlowController.GameState;

    public static TransactionProcessor TransactionProcessor => Economy.TransactionProcessor;

    //MODEL AND VIEW MIXED (SORRY)
    public static GameObject CityRoot { get; set; }

    //REPRESENTATION

    public static GlobalCreationService GlobalCreationService = new GlobalCreationService();

    public static ProductManager ProductManager = new ProductManager();
    public static ProductLocationService ProductLocationService = new ProductLocationService();

    public static GameFlowManager GameFlowManager;
    public static GameFlowController GameFlowController = new GameFlowController();

    public static WorldManager WorldManager = new WorldManager();

    public static CarMechanicService CarMechanicService;

    public static PlayerManager PlayerManager = new PlayerManager();
    
    public static GameMain.InstantMain InstantMain = new GameMain.InstantMain();

    //City
    public static CityActionService CityActionService = new CityActionService();

    //Interaction
    public static IInteractionManager InteractionManager;
    public static MissionController MissionController;

    //VIEW
    //View builders
    //Warehouse
    public static WarehouseProductGameObjectBuilder warehouseProductViewBuilder => physicalWarehouseProductViewBuilder;

    public static MonolithProductGameObjectBuilder monolithWarehouseProductViewBuilder => viewBuildersConfig.monolithWarehouseProductViewBuilder;
    public static PhysicalProductGameObjectBuilder physicalWarehouseProductViewBuilder => viewBuildersConfig.physicalWarehouseProductViewBuilder;
    public static CarPartGameObjectBuilder carPartViewBuilder => viewBuildersConfig.carPartViewBuilder;

    //City
    public static CityViewObjectBuilder cityViewObjectBuilder => viewBuildersConfig.cityViewObjectBuilder;
    public static CityViewStreetsBuilder cityViewStreetsBuilder = new CityViewStreetsBuilder();

    static ViewBuildersConfig viewBuildersConfig;

    public static void Initialize( ViewBuildersConfig handlersConfig)
    {
        viewBuildersConfig = handlersConfig;

        GameFlowManager = new GameFlowManager();
        CarMechanicService = new CarMechanicService();
    }

    protected override void OnRegistration()
    {
        base.OnRegistration();

        Initialize();

        GameEvents.Instance.Reset();
        ResetGameState();
        InstantMain.AfterWorldInitialize();
        TryInitializeLogic();
    }
    public void ResetGameState()
    {
        WorldManager.InitializeWorld(CityConfig, EconomyConfig, WorldMissionsConfig);
    }

    /// <summary>
    /// If the game starts directly in a specific scene, initialize the logic accordingly
    /// </summary>
    private void TryInitializeLogic()
    {
        SceneEntrancePoint sceneEntrancePoint = FindAnyObjectByType<SceneEntrancePoint>();
        if(sceneEntrancePoint != null)
            GameFlowController.Initialize(sceneEntrancePoint);
    }
}
