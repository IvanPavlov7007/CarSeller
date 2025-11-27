using NUnit.Framework;
using UnityEngine;
using static Car;
using static UnityEngine.Rendering.GPUSort;

[CreateAssetMenu(fileName = "WarehouseProductViewBuilder", menuName = "Configs/View/WarehouseProductViewBuilder")]
public class WarehouseProductViewBuilder : ScriptableObject, IProductViewBuilder<GameObject>, IProductViewInitializer<WarehouseProductView>
{
    public CarPartViewBuilder carPartViewBuilder;

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
            //if hidden or not occupied, skip
            if (slotData.Hidden == true || partLocation.Product == null)
                continue;
            CarPartViewPlacementHelper.BuildCarPartAtPosition(partLocation, carGO.transform,carPartViewBuilder);
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
        CollisionBuilder.InitializeCollision(sr);
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
        CollisionBuilder.InitializeCollision(sr);
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
        CollisionBuilder.InitializeCollision(sr);
        InitializeView(go, wheel);
        return go;
    }

    public WarehouseProductView InitializeView(GameObject gameObject, Product product)
    {
        var warehouseProductView = gameObject.AddComponent<WarehouseProductView>();
        warehouseProductView.Initialize(product, World.Instance.Warehouse.GetEmptyLocation());
        return warehouseProductView;
    }
}