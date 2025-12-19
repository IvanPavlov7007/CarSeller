using UnityEngine;

[CreateAssetMenu(fileName = "CityViewObjectBuilder", menuName = "Configs/View/CityViewObjectBuilder")]
public class CityViewObjectBuilder : ScriptableObject
{
    public GameObject carViewPrefab;
    public GameObject warehouseViewPrefab;

    public GameObject BuildObject(object cityObject)
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

    public GameObject BuildObjectDisabled(object cityObject)
    {
        var objGO = BuildObject(cityObject);
        if (objGO != null)
        {
            var spriteRenderers = objGO.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = Color.gray;
            }
        }
        return objGO;
    }

    public GameObject buildCar(Car car)
    {
        GameObject carGO = Instantiate(carViewPrefab);
        var location = G.Instance.LocationService.GetProductLocation(car) as City.CityLocation;
        carGO.AddComponent<ProductView>().Initialize(car, location);
        carGO.AddComponent<ContentProvider>().Initialize(car);
        carGO.AddComponent<DragInteractable>().sortingOrder = 10;
        carGO.AddComponent<MovingPoint>().Initialize(location);
        carGO.GetComponentInChildren<SpriteRenderer>().color = car.CarFrame.runtimeConfig.FrameColor;
        return carGO;
    }

    public GameObject buildWarehouse(Warehouse warehouse)
    {
        var location = World.Instance.City.Locations[warehouse];
        GameObject warehouseGO = Instantiate(warehouseViewPrefab, location.CityPosition.WorldPosition,Quaternion.identity);
        warehouseGO.AddComponent<Interactable>();
        warehouseGO.AddComponent<ContentProvider>().Initialize(warehouse);
        return warehouseGO;
    }
}