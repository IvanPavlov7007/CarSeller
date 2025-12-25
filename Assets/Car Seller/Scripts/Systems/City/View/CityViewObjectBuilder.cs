using UnityEngine;

[CreateAssetMenu(fileName = "CityViewObjectBuilder", menuName = "Configs/View/CityViewObjectBuilder")]
public class CityViewObjectBuilder : ScriptableObject
{
    public GameObject carViewPrefab;
    public GameObject warehouseViewPrefab;
    public GameObject buyerViewPrefab;

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
        GameObject buyerGO = Instantiate(buyerViewPrefab);
        var location = CityPositionLocator.GetCityLocation(buyer);
        var viewController =
            buyerGO.AddComponent<CityViewObjectController>().Initialize(buyer, ViewObjectVisualState.Normal, true);
        buyerGO.AddComponent<ContentProvider>().Initialize(buyer);
        buyerGO.AddComponent<Interactable>();
        buyerGO.AddComponent<ViewStateChanger>();
        buyerGO.AddComponent<Triggerable>();
        return viewController;
    }

    public CityViewObjectController buildCar(Car car)
    {
        GameObject carGO = Instantiate(carViewPrefab);
        
        var location = G.Instance.ProductLocationService.GetProductLocation(car) as City.CityLocation;

        carGO.AddComponent<ProductView>().Initialize(car, location);
        var viewController = 
            carGO.AddComponent<CityViewObjectController>().Initialize(car);
        var rigidbody2D = carGO.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        carGO.AddComponent<ContentProvider>().Initialize(car);
        carGO.AddComponent<DragInteractable>().sortingOrder = 10;
        carGO.AddComponent<DragDisabler>();
        carGO.AddComponent<ViewStateChanger>();
        carGO.AddComponent<MovingPoint>().Initialize(location);
        carGO.GetComponentInChildren<SpriteRenderer>().color = car.CarFrame.runtimeConfig.FrameColor;
        return viewController;
    }

    public CityViewObjectController buildWarehouse(Warehouse warehouse)
    {
        var location = World.Instance.City.Locations[warehouse];
        GameObject warehouseGO = Instantiate(warehouseViewPrefab, location.CityPosition.WorldPosition,Quaternion.identity);
        var viewController =
            warehouseGO.AddComponent<CityViewObjectController>().Initialize(warehouse);
        warehouseGO.AddComponent<ViewStateChanger>();
        warehouseGO.AddComponent<Interactable>();
        warehouseGO.AddComponent<ContentProvider>().Initialize(warehouse);
        warehouseGO.AddComponent<Triggerable>();
        return viewController;
    }
}