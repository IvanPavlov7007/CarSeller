using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "CityViewObjectBuilder", menuName = "Configs/View/CityViewObjectBuilder")]
public class CityViewObjectBuilder : ScriptableObject
{
    public GameObject carViewPrefab;
    public GameObject triggerPrefab;

    [Header("UI Prefabs")]
    public GameObject pinUIPrefab;
    public Sprite WarehouseIcon;
    public Sprite BuyerIcon;

    public CityViewObjectController BuildObject(object cityObject)
    {
        switch(cityObject)
        {
            case Car car:
                return buildCar(car);
            case Warehouse warehouse:
                return buildWarehouse(warehouse);
            case Buyer buyer:
                return BuildBuyer(buyer);
            default:
                Debug.LogError($"No builder for city object of type {cityObject.GetType().Name}");
                return null;
        }
    }

    public CityViewObjectController BuildBuyer(Buyer buyer)
    {
        var location = CityLocatorHelper.GetCityLocation(buyer);
        GameObject buyerGO = Instantiate(triggerPrefab, location.CityPosition.WorldPosition, Quaternion.identity);
        var viewController =
            buyerGO.AddComponent<CityViewObjectController>().Initialize(buyer, ViewObjectVisualState.Normal, true);
        buyerGO.AddComponent<ContentProvider>().Initialize(buyer);
        buyerGO.AddComponent<Interactable>();
        buyerGO.AddComponent<Triggerable>();

        CityUIBuilder.SetUpCityPin(viewController, pinUIPrefab, BuyerIcon);
        return viewController;
    }

    public CityViewObjectController buildCar(Car car)
    {
        GameObject carGO = Instantiate(carViewPrefab);
        
        var location = G.Instance.ProductLocationService.GetProductLocation(car) as City.CityLocation;

        var viewController = 
            carGO.AddComponent<CityViewObjectController>().Initialize(car);
        var rigidbody2D = carGO.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        carGO.AddComponent<ContentProvider>().Initialize(car);
        carGO.AddComponent<DragInteractable>().sortingOrder = 10;
        carGO.AddComponent<DragDisabler>();
        carGO.AddComponent<MovingPoint>().Initialize(location);
        var sr = carGO.GetComponentInChildren<SpriteRenderer>();
        sr.sprite = car.CarFrame.runtimeConfig.Icon;
        sr.color = car.CarFrame.runtimeConfig.FrameColor;
        carGO.AddComponent<ViewStateChanger>();
        return viewController;
    }

    public CityViewObjectController buildWarehouse(Warehouse warehouse)
    {
        var location = CityLocatorHelper.GetCityLocation(warehouse);
        GameObject warehouseViewGO = Instantiate(triggerPrefab, location.CityPosition.WorldPosition,Quaternion.identity);
        var viewController =
            warehouseViewGO.AddComponent<CityViewObjectController>().Initialize(warehouse);
        warehouseViewGO.AddComponent<Interactable>();
        warehouseViewGO.AddComponent<ContentProvider>().Initialize(warehouse);
        warehouseViewGO.AddComponent<Triggerable>();

        CityUIBuilder.SetUpCityPin(viewController, pinUIPrefab, WarehouseIcon);

        return viewController;
    }
}