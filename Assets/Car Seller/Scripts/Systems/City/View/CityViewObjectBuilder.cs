using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "CityViewObjectBuilder", menuName = "Configs/View/CityViewObjectBuilder")]
public class CityViewObjectBuilder : ScriptableObject
{
    public GameObject carViewPrefab;
    public GameObject triggerPrefab;
    public GameObject policeUnitPrefab;

    //pure view
    public GameObject collectablePrefab;

    [Header("UI Prefabs")]
    public GameObject pinUIPrefab;
    public PinStyle WarehousePinStyle;
    public PinStyle BuyerPinStyle;

    CityUIBuilder CityUIBuilder = new CityUIBuilder();

    public CityViewObjectController BuildObject(object cityObject)
    {
        switch(cityObject)
        {
            case Car car:
                return buildCar(car);
            case Warehouse warehouse:
                return buildCityObject(warehouse);
            case Collectable collectable: // TODO ACHTUNG!!! generalize, since Collectable is a CityObject
                return BuildCollectable(collectable);
            case PoliceCityObject policeUnit: // TODO ACHTUNG!!! generalize, since PoliceUnit is a CityObject, but also moving
                return buildPoliceUnit(policeUnit);
            case CityObject co:
                return buildCityObject(co);
            default:
                Debug.LogError($"No builder for city object of type {cityObject.GetType().Name}");
                return null;
        }
    }

    public CityViewObjectController BuildCollectable(Collectable collectable)
    {
        var location = CityLocatorHelper.GetCityLocation(collectable);
        var pos = location.CityPosition.WorldPosition;

        GameObject collectableGO = Instantiate(triggerPrefab, pos, Quaternion.identity);
        var viewController =
            collectableGO.AddComponent<CityViewObjectController>().Initialize(collectable);
        collectableGO.AddComponent<ContentProvider>().Initialize(collectable);
        collectableGO.AddComponent<Interactable>();
        collectableGO.AddComponent<Triggerable>();

        Instantiate(collectablePrefab, pos, Quaternion.identity, collectableGO.transform);
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
        carGO.AddComponent<SpeedProviderFromCar>().Initialize(car);
        carGO.AddComponent<MovingPoint>().Initialize(location);
        var sr = carGO.GetComponentInChildren<SpriteRenderer>();
        sr.sprite = car.CarFrame.runtimeConfig.Icon;
        sr.color = car.CarFrame.runtimeConfig.FrameColor;
        carGO.AddComponent<ViewStateChanger>();
        return viewController;
    }


    //TODO make warehouse a city object and generalize this method
    public CityViewObjectController buildCityObject(ILocatable locatable)
    {
        var location = CityLocatorHelper.GetCityLocation(locatable);
        GameObject warehouseViewGO = Instantiate(triggerPrefab, location.CityPosition.WorldPosition,Quaternion.identity);
        var viewController =
            warehouseViewGO.AddComponent<CityViewObjectController>().Initialize(locatable);
        warehouseViewGO.AddComponent<Interactable>();
        warehouseViewGO.AddComponent<ContentProvider>().Initialize(locatable);
        warehouseViewGO.AddComponent<Triggerable>();

        PinStyle pinStyle = null;

        if (locatable is CityObject cityObject)
        {
            pinStyle = cityObject.PinStyle;
            if(cityObject is Buyer)
            {
                pinStyle = BuyerPinStyle;
            }
        }
        else if (locatable is Warehouse)
        {
            pinStyle = WarehousePinStyle;
        }


        if (pinStyle != null)
            CityUIBuilder.SetUpCityPin(viewController, pinUIPrefab, pinStyle);
        else
            Debug.LogWarning($"Couldn't resolve PinStyle for {locatable}.");

        return viewController;
    }

    public CityViewObjectController buildPoliceUnit(PoliceCityObject policeCityObject)
    {
        var data = policeCityObject.Data as PoliceUnit;
        //TODO fix the system: data and policeCityObject should be one object

        GameObject policeGO = Instantiate(policeUnitPrefab);

        var location = CityLocatorHelper.GetCityLocation(policeCityObject);

        var viewController =
            policeGO.AddComponent<CityViewObjectController>().Initialize(policeCityObject);
        var rigidbody2D = policeGO.AddComponent<Rigidbody2D>();
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        policeGO.AddComponent<ContentProvider>().Initialize(policeCityObject);
        policeGO.AddComponent<Interactable>().sortingOrder = 12;
        policeGO.AddComponent<MovingPointSimpleView>().Initialize(data.GraphMovement);
        var policeViewController = policeGO.AddComponent<PoliceStateViewController>();
        policeViewController.Initialize(data);
        policeGO.AddComponent<PoliceSpotlightVisionVisuals>().Intialize(data, policeViewController);
        policeGO.AddComponent<PoliceLightsViewController>().Initialize(policeViewController, policeGO.GetComponentInChildren<PoliceLightsVisuals>());
        policeGO.AddComponent<ViewStateChanger>();
        return viewController;
    }
}