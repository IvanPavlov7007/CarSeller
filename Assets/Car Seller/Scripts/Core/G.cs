
using Pixelplacement;
using UnityEngine;

public class G : Singleton<G>
{
    public LocationService LocationService;
    public ProductManager ProductManager;

    //Views
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
