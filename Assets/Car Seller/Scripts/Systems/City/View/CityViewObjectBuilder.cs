using UnityEngine;

[CreateAssetMenu(fileName = "CityViewObjectBuilder", menuName = "Configs/View/CityViewObjectBuilder")]
public class CityViewObjectBuilder : SingletonScriptableObject<CityViewObjectBuilder>
{
    public GameObject carViewPrefab;
    public GameObject warehouseViewPrefab;

    public GameObject buildObject(object cityObject)
    {
        switch(cityObject)
        {
            case Car car:
                return buildCar(car);
            case Warehouse warehouse:
                return buildWarehouse(warehouse);
            default:
                Debug.LogError($"No builder for city object of type {cityObject.GetType().Name}");
                return null;
        }
    }

    public GameObject buildCar(Car car)
    {
        GameObject carGO = Instantiate(carViewPrefab);
        var location = G.Instance.LocationService.GetProductLocation(car) as City.CityProductLocation;
        carGO.AddComponent<ProductView>().Initialize(car, location);
        carGO.AddComponent<ContentProvider>().Initialize(car);
        carGO.AddComponent<DragInteractable>();
        carGO.AddComponent<MovingPoint>().Initialize(location.CityPosition);
        return carGO;
    }

    public GameObject buildWarehouse(Warehouse warehouse)
    {
        var position = World.Instance.City.Objects[warehouse];
        GameObject warehouseGO = Instantiate(warehouseViewPrefab, position.GetWorldPosition(),Quaternion.identity);
        return warehouseGO;
    }
}