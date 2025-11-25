
using UnityEngine;

[CreateAssetMenu(fileName = "CarPartViewBuilder", menuName = "Configs/View/Car Part View Builder")]
public class CarPartViewBuilder : ScriptableObject, IProductViewBuilder<GameObject>
{
    public GameObject baseWheelPrefab;
    public GameObject baseSpoilerPrefab;
    public GameObject BuildCar(Car car)
    {
        GameObject carGO = new GameObject(car.Name);
        foreach (var partLocation in car.GetProducts())
        {
            var slotData = car.carParts[partLocation as Car.CarPartLocation]?.partSlotData;
            var part = partLocation.Product.GetRepresentation(this);
            if (part != null && slotData != null)
            {
                part.transform.SetParent(carGO.transform);
                part.transform.localPosition = slotData.Value.LocalPosition;
                part.transform.localRotation = Quaternion.Euler(slotData.Value.LocalRotation);
                part.transform.localScale = slotData.Value.LocalScale;
            }
        }
        //Debug.LogError($"Building a car part({car.UniqueName}) of a car is not supported: ");
        return carGO;
    }

    public GameObject BuildCarFrame(CarFrame carFrame)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildEngine(Engine engine)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildSpoiler(Spoiler spoiler)
    {
        throw new System.NotImplementedException();
    }

    public GameObject BuildWheel(Wheel wheel)
    {
        throw new System.NotImplementedException();
    }
}
