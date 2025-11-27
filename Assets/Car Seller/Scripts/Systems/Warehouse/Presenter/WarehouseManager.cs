using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;


// Loads and manages warehouse in the warehouse scene
[ExecuteAlways]
public class WarehouseManager : Singleton<WarehouseManager>
{
    public Warehouse Warehouse => World.Instance.Warehouse;

    public Transform emptyPosition;

    private void Start()
    {
        initializeWarehouse();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnProductCreated += onNewProductCreated;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnProductCreated -= onNewProductCreated;
    }

    private void Update()
    {
        World.Instance.Warehouse.emptyProductLocation = new Warehouse.DimensionalPositionData
        { LocalPosition =  emptyPosition.localPosition, LocalRotation = emptyPosition.localEulerAngles };
    }

    void initializeWarehouse()
    {
        if (Warehouse == null)
        {
            Debug.LogWarning("Warehouse instance is not set");
            return;
        }
        foreach (var location in Warehouse.products)
        {
            if (location.Product != null)
            {
                //var productObject = Instantiate(location.Product.Prefab, location.Position, Quaternion.identity);
                //productObject.GetComponent<ProductBehaviour>().Initialise(location.Product, location);
            }
        }
    }

    void onNewProductCreated(ProductCreatedEventData data)
    {
        var location = G.Instance.LocationService.GetProductLocation(data.Product);
        if (Warehouse == null)
        {
            Debug.LogError("Warehouse instance is not set");
            return;
        }
        if (location.Holder == Warehouse)
        {
            buildProductView(data.Product, location);
        }
    }

    void buildProductView(Product product, IProductLocation location)
    {
        var productViewGO = product.GetRepresentation(G.Instance.warehouseProductViewBuilder);
        var transform = productViewGO.transform;

        transform.SetParent(this.transform);
        var dimensionalData = (location as Warehouse.WarehouseProductLocation).Position;
        transform.localPosition = dimensionalData.LocalPosition;
        transform.localEulerAngles = dimensionalData.LocalRotation;
    }
}
