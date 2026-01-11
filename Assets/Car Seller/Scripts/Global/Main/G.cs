using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global services locator
/// </summary>
public sealed class G : Singleton<G>
{
    //Use only for reading
    //MODEL
    public CityConfig CityConfig;
    public EconomyConfig EconomyConfig;
    public WorldMissionsConfig WorldMissionsConfig;

    //References
    public static City City => World.Instance.City;
    public static Economy Economy => World.Instance.Economy;
    public static Player Player => World.Instance.Economy.Player;
    public static GameState GameState => Instance.GameFlowController.GameState;

    public static TransactionProcessor TransactionProcessor => Economy.TransactionProcessor;

    //MODEL AND VIEW MIXED (SORRY)
    public GameObject CityRoot { get; set; }

    //REPRESENTATION

    public GlobalCreationService GlobalCreationService = new GlobalCreationService();

    public ProductManager ProductManager = new ProductManager();
    public ProductLocationService ProductLocationService = new ProductLocationService();

    public GameFlowManager GameFlowManager;
    public GameFlowController GameFlowController = new GameFlowController();

    public WorldManager WorldManager = new WorldManager();

    public CarMechanicService CarMechanicService;

    public PlayerManager PlayerManager = new PlayerManager();
    

    //City
    public CityActionService CityActionService = new CityActionService();

    //Interaction
    public IInteractionManager InteractionManager;
    public MissionController MissionController;

    //VIEW
    //View builders
    //Warehouse
    public WarehouseProductGameObjectBuilder warehouseProductViewBuilder;
    public CarPartGameObjectBuilder carPartViewBuilder;

    //City
    public CityViewObjectBuilder cityViewObjectBuilder;
    public CityViewStreetsBuilder cityViewStreetsBuilder = new CityViewStreetsBuilder();

    protected override void OnRegistration()
    {
        base.OnRegistration();

        Initialize();

        GameEvents.Instance.Reset();
        ResetGameState();

        TryInitializeLogic();
    }

    private void Initialize()
    {
        GameFlowManager = new GameFlowManager();
        CarMechanicService = new CarMechanicService();
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

    public void ResetGameState()
    {
        WorldManager.InitializeWorld(CityConfig, EconomyConfig, WorldMissionsConfig);
    }

}
