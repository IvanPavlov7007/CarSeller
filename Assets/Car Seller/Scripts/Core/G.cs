using Pixelplacement;
using UnityEngine;

/// <summary>
/// Global services locator
/// </summary>
public class G : Singleton<G>
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
        GameEvents.Instance.Reset();
        ResetGameState();
    }

    public void ResetGameState()
    {
        WorldManager.InitializeCity(CityConfig);
    }

}
