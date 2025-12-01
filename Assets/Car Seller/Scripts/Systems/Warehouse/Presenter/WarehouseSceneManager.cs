using Pixelplacement;
using Sirenix.OdinInspector;
using UnityEngine;


/// <summary>
/// Loads and manages warehouse in the warehouse scene
/// </summary>
//[ExecuteAlways]
public class WarehouseSceneManager : Singleton<WarehouseSceneManager>
{
    public static Warehouse SceneWarehouseModel { get; set; }

    public Transform emptyPosition;

    private void Awake()
    {
        if(SceneWarehouseModel == null)
        {
            Debug.LogError("Warehouse instance is not set in the WarehouseSceneManager");
            return;
        }
        initializeWarehouse();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnProductCreated += onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged += onProductLocationChanged;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnProductCreated -= onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged -= onProductLocationChanged;
    }

    private void Update()
    {
        if (emptyPosition != null)
        {
            SceneWarehouseModel.emptyProductLocation = new Warehouse.DimensionalPositionData
            { LocalPosition = emptyPosition.localPosition, LocalRotation = emptyPosition.localEulerAngles };
        }
        else
        {
            SceneWarehouseModel.emptyProductLocation = new Warehouse.DimensionalPositionData
            { LocalPosition = Vector3.up * 4f, LocalRotation = Vector3.zero };
        }
        
    }

    void initializeWarehouse()
    {
        if (SceneWarehouseModel == null)
        {
            Debug.LogWarning("Warehouse instance is not set");
            return;
        }
        foreach (var location in SceneWarehouseModel.products)
        {
            Debug.Assert(location.Product != null, "Product at the location is null");

            buildProductView(location.Product, location);
        }
    }

    void onNewProductCreated(ProductCreatedEventData data)
    {
        var location = G.Instance.LocationService.GetProductLocation(data.Product);
        if (SceneWarehouseModel == null)
        {
            Debug.LogError("Warehouse instance is not set");
            return;
        }
        if (location.Holder == SceneWarehouseModel)
        {
            buildProductView(data.Product, location);
        }
    }

    void onProductLocationChanged(ProductLocationChangedEventData data)
    {
        Debug.Assert(SceneWarehouseModel != null, "Warehouse instance is not set");

        if (data.NewLocation?.Holder == SceneWarehouseModel)
        {
            buildProductView(data.Product, data.NewLocation);
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
