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
    public GameState GameState;

    //MODEL AND VIEW MIXED (SORRY)
    public GameObject CityRoot { get; set; }

    //REPRESENTATION

    public ProductManager ProductManager = new ProductManager();
    public LocationService LocationService = new LocationService();

    public GameFlowManager GameFlowManager = new GameFlowManager();
    public GameFlowController GameFlowController = new GameFlowController();

    public WorldManager WorldManager = new WorldManager();

    public CarMechanicService CarMechanicService = new CarMechanicService();

    public PlayerManager PlayerManager = new PlayerManager();
    public TransactionProcessor TransactionProcessor;

    //City
    public CityActionService CityActionService = new CityActionService();

    //Interaction
    public IInteractionManager InteractionManager;

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
    }

    private void Initialize()
    {
        GameFlowManager = new GameFlowManager();

        CarMechanicService = new CarMechanicService();

        TransactionProcessor transactionProcessor = new TransactionProcessor(new Dictionary<TransactionType, ITransactionHandler>
        {
            { TransactionType.Purchase, new PurchaseHandler() },
            { TransactionType.Sell, new SellHandler() },
            { TransactionType.Reward, new RewardHandler() },
            { TransactionType.Lose, new LoseHandler() },
            { TransactionType.Confiscate, new ConfiscateHandler() },
            { TransactionType.Steal, new StealHandler()   }
        });
    }

    public void ResetGameState()
    {
        WorldManager.InitializeCity(CityConfig);
    }

}
