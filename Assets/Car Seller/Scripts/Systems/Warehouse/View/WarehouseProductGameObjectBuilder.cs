using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseProductViewBuilder", menuName = "Configs/View/WarehouseProductViewBuilder")]
public class WarehouseProductGameObjectBuilder : ScriptableObject, IProductViewBuilder<GameObject>
{
    public CarPartGameObjectBuilder carPartViewBuilder;
    IProductViewComponentBuilder<WarehouseProductView> productViewComponentBuilder = new WarehouseProductViewComponentBuilder();


    public GameObject rectanglePrefab;

    public GameObject BuildCar(Car car)
    {
        GameObject carGO = new GameObject(car.Name);
        carGO.AddComponent<Rigidbody2D>();

        var frameGO = car.CarFrame.GetRepresentation(carPartViewBuilder);
        frameGO.transform.SetParent(carGO.transform);
        frameGO.transform.localPosition = Vector3.zero;

        foreach (var partLocation in car.carParts.Keys)
        {
            var slotData = partLocation.PartSlotRuntimeConfig.partSlotData;
            CarPartViewPlacementHelper.BuildCarPartAtPosition(partLocation, carGO.transform,carPartViewBuilder);
        }

        productViewComponentBuilder.BuildViewComponent(carGO, car);

        return carGO;
    }

    public GameObject BuildCarFrame(CarFrame carFrame)
    {
        Debug.LogError("WarehouseProductViewBuilder does not support building CarFrame views. " + carFrame.UniqueName);
        return null;
    }

    public GameObject BuildEngine(Engine engine)
    {
        var go = Instantiate(rectanglePrefab);
        var sr = go.GetComponent<SpriteRenderer>();
        go.name = engine.Name;
        sr.sprite = engine.runtimeConfig.Sprite;
        CollisionBuilder.InitializeCollision(sr);

        productViewComponentBuilder.BuildViewComponent(go, engine);
        return go;
    }

    public GameObject BuildSpoiler(Spoiler spoiler)
    {
        var go = Instantiate(rectanglePrefab);
        var sr = go.GetComponent<SpriteRenderer>();
        go.name = spoiler.Name;
        sr.sprite = spoiler.runtimeConfig.Sprite;
        sr.color = spoiler.runtimeConfig.Color;
        go.transform.localScale = Vector3.one * spoiler.runtimeConfig.Size;
        CollisionBuilder.InitializeCollision(sr);

        productViewComponentBuilder.BuildViewComponent(go, spoiler);
        return go;
    }

    public GameObject BuildWheel(Wheel wheel)
    {
        var go = Instantiate(rectanglePrefab);
        var sr = go.GetComponent<SpriteRenderer>();
        go.name = wheel.Name;
        sr.sprite = wheel.runtimeConfig.TopViewSprite;
        sr.color = wheel.runtimeConfig.Color;
        go.transform.localScale = Vector3.one * wheel.runtimeConfig.TopViewSize;
        CollisionBuilder.InitializeCollision(sr);

        productViewComponentBuilder.BuildViewComponent(go, wheel);
        return go;
    }


    public class WarehouseProductViewComponentBuilder : IProductViewComponentBuilder<WarehouseProductView>
    {
        public WarehouseProductView BuildViewComponent(GameObject gameObject, Product product)
        {
            Debug.Assert(gameObject != null, "gameObject != null");
            Debug.Assert(product != null, "product != null");


            WarehouseProductView view;

            if (product is Car)
            {
                view = gameObject.AddComponent<WarehouseCarView>();
            }
            else
            {
                view = gameObject.AddComponent<WarehouseProductView>();
            }
            view.Initialize(product, G.Instance.LocationService.GetProductLocation(product));
            initializeAdditionalComponents(gameObject, product);
            return view;
        }

        // Add any additional components based on product type if needed
        private void initializeAdditionalComponents(GameObject gameObject, Product product)
        {
            gameObject.AddComponent<DirectDragInteractable>();
            gameObject.AddComponent<ContentProvider>().Initialize(product);
            if (product is not Car)
            {
                gameObject.AddComponent<DragCollisionDisabler>();
                gameObject.AddComponent<DragSortingOrderChanger>();
            }
        }
    }
}