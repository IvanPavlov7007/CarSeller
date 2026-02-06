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

    public static TransactionProcessor TransactionProcessor;
    public static ProcessRunner ProcessRunner;

    public static ICarWarehousePolicy CarWarehousePolicy;
    public static AcquisitionResolver AcquisitionResolver = new AcquisitionResolver();

    public static CarSpawnManager CarSpawnManager => Economy.CarSpawnManager;

    //MODEL AND VIEW MIXED (SORRY)
    public static GameObject CityRoot { get; set; }

    //REPRESENTATION

    public static ProductManager ProductManager = new ProductManager();

    public static ProductLifetimeService ProductLifetimeService = new ProductLifetimeService();
    public static OwnershipService OwnershipService = new OwnershipService();

    public static GameFlowManager GameFlowManager;
    public static GameFlowController GameFlowController = new GameFlowController();

    public static WorldManager WorldManager = new WorldManager();

    public static CarMechanicService CarMechanicService;

    public static PlayerManager PlayerManager = new PlayerManager();

    public static CarStripper CarStripper = new CarStripper();

    //City
    
    public static CityEntityLifetimeService CityEntityLifetimeService = new CityEntityLifetimeService();
    public static CityActionService CityActionService = new CityActionService();

    public static WarehouseEntryCooldownService WarehouseEntryCooldownService = new WarehouseEntryCooldownService();

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

        initializeTransactionProcessor();


        CarWarehousePolicy = new CarIntoWarehousePolicy();
        GameFlowManager = new GameFlowManager();
        CarMechanicService = new CarMechanicService();
        ProcessRunner = new ProcessRunner();
    }

    private static void initializeTransactionProcessor()
    {
        TransactionProcessor = new TransactionProcessor(new Dictionary<TransactionType, ITransactionHandler>
        {
            { TransactionType.Purchase, new PurchaseHandler() },
            { TransactionType.Sell, new SellHandler() },
            { TransactionType.Reward, new RewardHandler() },
            { TransactionType.Lose, new LoseHandler() },
            { TransactionType.Confiscate, new ConfiscateHandler() },
            { TransactionType.Steal, new StealHandler()   },
            { TransactionType.Exchange, new ExchangeHandler() },
            {TransactionType.StripCar, new StripCarHandler() },
            {TransactionType.PullCarFromWarehouse, new PullCarFromWarehouseHandler() },
            {TransactionType.PutProductsInWarehouse, new PutProductsInWarehouseHandler() },
        });
    }
}
