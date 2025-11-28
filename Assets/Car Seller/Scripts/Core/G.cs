using Pixelplacement;
using UnityEngine;

/// <summary>
/// Global services locator
/// </summary>
public class G : Singleton<G>
{
    //MODEL
    public LocationService LocationService;
    public ProductManager ProductManager;

    //REPRESENTATION
    
    public GameFlowController GameFlowController = new GameFlowController();

    //Interaction
    public IInteractionManager InteractionManager;

    //VIEW
    //View builders
    //Warehouse
    public WarehouseProductGameObjectBuilder warehouseProductViewBuilder;
    public CarPartGameObjectBuilder carPartViewBuilder;

    private void Awake()
    {
        ResetGameState();
        LocationService = new LocationService();
        ProductManager = new ProductManager();
    }

    public void ResetGameState()
    {
        World.Reset();
        GameEvents.Instance.Reset();
    }

}
