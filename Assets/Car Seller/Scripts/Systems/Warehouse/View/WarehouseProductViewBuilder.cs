using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseProductViewBuilder", menuName = "Configs/View/WarehouseProductViewBuilder")]
public class WarehouseProductViewBuilder : ScriptableObject, IProductViewBuilder<GameObject>, IProductViewInitializer<WarehouseProductView>
{
    public CarPartViewBuilder carPartViewBuilder;

    public GameObject rectanglePrefab;

    public GameObject BuildCar(Car car)
    {
        GameObject carGO = new GameObject(car.Name);

        var frameGO = car.CarFrame.GetRepresentation(carPartViewBuilder);
        frameGO.transform.SetParent(carGO.transform);
        frameGO.transform.localPosition = Vector3.zero;

        foreach (var partLocation in car.GetProducts())
        {
            var slotData = car.carParts[partLocation as Car.CarPartLocation]?.partSlotData;
            if (slotData?.Hidden == true)
                continue;
            var part = partLocation.Product.GetRepresentation(carPartViewBuilder);
            if (part != null && slotData != null)
            {
                part.transform.SetParent(carGO.transform);
                part.transform.localPosition = slotData.Value.LocalPosition;
                part.transform.localRotation = Quaternion.Euler(slotData.Value.LocalRotation);
                part.transform.localScale = slotData.Value.LocalScale;
            }
        }

        InitializeView(carGO, car);

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
        InitializeView(go, engine);
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
        InitializeView(go, spoiler);
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
        InitializeView(go, wheel);
        return go;
    }

    public WarehouseProductView InitializeView(GameObject gameObject, Product product)
    {
        var warehouseProductView = gameObject.AddComponent<WarehouseProductView>();
        warehouseProductView.Initialize(product, null);
        return warehouseProductView;
    }
}